using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using RevitNumbering.Core;
using RevitNumbering.Revit.Model;
using RevitNumbering.Revit.Mvvm;

namespace RevitNumbering.Revit.ViewModels
{
    public sealed class NumberingViewModel : ObservableObject, IDisposable
    {
        private static readonly IReadOnlyDictionary<string, string> DefaultTemplates =
            new Dictionary<string, string>
            {
                ["OST_Walls"] = "С-{Уровень}-{N:000}",
                ["OST_Doors"] = "Д-{Уровень}-{N:000}",
                ["OST_Windows"] = "О-{Уровень}-{N:000}",
                ["OST_Rooms"] = "П-{Уровень}-{N:000}"
            };

        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _searchTimer;
        private readonly DispatcherTimer _rebuildTimer;
        private readonly RelayCommand _refreshCommand;
        private readonly RelayCommand _applyCommand;

        private IReadOnlyList<ElementSnapshot> _allElements;
        private IReadOnlyList<ElementSnapshot> _scopedElements;
        private Document _loadedDocument;
        private NumberingPlan _plan;
        private HashSet<int> _conflictIds = new HashSet<int>();
        private CategoryDefinition _selectedCategory;
        private NumberingScope _scope;
        private string _templateText = "";
        private string _templateError = "";
        private string _searchText = "";
        private string _status = "";
        private bool _isBusy;
        private bool _initialized;
        private bool _pendingReload;
        private bool _disposed;

        public NumberingViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            _searchTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _searchTimer.Tick += OnSearchTimerTick;

            _rebuildTimer = new DispatcherTimer(DispatcherPriority.Background, _dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _rebuildTimer.Tick += OnRebuildTimerTick;

            RowsView = CollectionViewSource.GetDefaultView(Rows);
            RowsView.Filter = FilterRow;

            _refreshCommand = new RelayCommand(() => _ = ReloadAsync(), () => !IsBusy);
            _applyCommand = new RelayCommand(
                () => _ = ApplyAsync(),
                () => !IsBusy && _plan != null && _plan.Elements.Count > 0);

            AddinContext.DocumentTracker.ActiveDocumentChanged += OnActiveDocumentChanged;
        }

        public IReadOnlyList<CategoryDefinition> Categories => CategoryDefinition.All;

        public ObservableCollection<RowViewModel> Rows { get; } = new ObservableCollection<RowViewModel>();

        public ICollectionView RowsView { get; }

        public RelayCommand RefreshCommand => _refreshCommand;

        public RelayCommand ApplyCommand => _applyCommand;

        public CategoryDefinition SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (value == null || ReferenceEquals(_selectedCategory, value))
                {
                    return;
                }

                RememberTemplate();
                _selectedCategory = value;
                OnPropertyChanged();
                TemplateText = GetTemplate(value);
                _rebuildTimer.Stop();

                if (_initialized)
                {
                    _ = ReloadAsync();
                }
            }
        }

        public bool IsAllScope
        {
            get => _scope == NumberingScope.All;
            set
            {
                if (value)
                {
                    SetScope(NumberingScope.All);
                }
            }
        }

        public bool IsSelectionScope
        {
            get => _scope == NumberingScope.Selection;
            set
            {
                if (value)
                {
                    SetScope(NumberingScope.Selection);
                }
            }
        }

        public string TemplateText
        {
            get => _templateText;
            set
            {
                if (!SetProperty(ref _templateText, value ?? ""))
                {
                    return;
                }

                var template = NumberTemplate.Parse(_templateText);
                TemplateError = template.IsValid ? "" : template.Error;

                _rebuildTimer.Stop();
                if (template.IsValid)
                {
                    _rebuildTimer.Start();
                }
                else
                {
                    _plan = null;
                    _conflictIds.Clear();
                    Rows.Clear();
                    UpdateCommandState();
                }
            }
        }

        public string TemplateError
        {
            get => _templateError;
            private set => SetProperty(ref _templateError, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value ?? ""))
                {
                    return;
                }

                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    UpdateCommandState();
                }
            }
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            var settings = AddinContext.Settings;
            _selectedCategory = Categories.FirstOrDefault(
                                   c => c.Category.ToString() == settings.LastCategory)
                               ?? Categories[0];
            OnPropertyChanged(nameof(SelectedCategory));

            _scope = settings.Scope;
            OnPropertyChanged(nameof(IsAllScope));
            OnPropertyChanged(nameof(IsSelectionScope));

            _templateText = GetTemplate(_selectedCategory);
            OnPropertyChanged(nameof(TemplateText));

            _initialized = true;
            await ReloadAsync();
        }

        public void SaveOnClose()
        {
            RememberTemplate();
            AddinContext.Settings.Scope = _scope;
            AddinContext.SaveSettings();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            AddinContext.DocumentTracker.ActiveDocumentChanged -= OnActiveDocumentChanged;
            _searchTimer.Stop();
            _rebuildTimer.Stop();
        }

        private void SetScope(NumberingScope scope)
        {
            if (_scope == scope)
            {
                return;
            }

            _scope = scope;
            AddinContext.Settings.Scope = scope;
            AddinContext.SaveSettings();
            OnPropertyChanged(nameof(IsAllScope));
            OnPropertyChanged(nameof(IsSelectionScope));

            if (_initialized)
            {
                _ = ReloadAsync();
            }
        }

        private async Task ReloadAsync()
        {
            if (!_initialized || _disposed)
            {
                return;
            }

            if (IsBusy)
            {
                _pendingReload = true;
                return;
            }

            _rebuildTimer.Stop();
            IsBusy = true;

            try
            {
                var category = SelectedCategory;
                var scope = _scope;
                Logger.Info($"Загрузка: {category.Category}, {scope}");

                var payload = await AddinContext.Executor.InvokeAsync(app =>
                {
                    var uiDocument = app.ActiveUIDocument;
                    if (uiDocument == null)
                    {
                        return null;
                    }

                    AddinContext.DocumentTracker.SetActiveDocument(uiDocument.Document);
                    return new LoadPayload(
                        uiDocument.Document,
                        RevitElementLoader.Load(uiDocument, category, scope));
                }, TimeSpan.FromSeconds(30));

                if (payload == null)
                {
                    ClearForNoDocument();
                    return;
                }

                _loadedDocument = payload.Document;
                _allElements = payload.Result.All;
                _scopedElements = payload.Result.Scoped;

                RememberTemplate();
                AddinContext.Settings.LastCategory = category.Category.ToString();
                AddinContext.SaveSettings();

                RebuildPlan();
                Logger.Info($"Загружено: всего {_allElements.Count}, в охвате {_scopedElements.Count}");

                if (_scopedElements.Count == 0 && scope == NumberingScope.Selection)
                {
                    Status = "В модели не выбрано ни одного элемента категории.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось загрузить элементы", ex);
                Status = "Не удалось загрузить элементы: " + ex.Message;
            }
            finally
            {
                IsBusy = false;

                if (_pendingReload)
                {
                    _pendingReload = false;
                    _ = ReloadAsync();
                }
            }
        }

        private void RebuildPlan()
        {
            if (_scopedElements == null)
            {
                _plan = null;
                _conflictIds.Clear();
                Rows.Clear();
                UpdateCommandState();
                return;
            }

            var template = NumberTemplate.Parse(TemplateText);
            if (!template.IsValid)
            {
                TemplateError = template.Error;
                _plan = null;
                _conflictIds.Clear();
                Rows.Clear();
                Status = "Исправьте шаблон номера.";
                UpdateCommandState();
                return;
            }

            TemplateError = "";
            var toleranceFeet = Math.Max(0, AddinContext.Settings.RowToleranceMm) / 304.8;
            _plan = NumberingPlanner.Build(_scopedElements, template, toleranceFeet);

            var conflicts = ConflictDetector.Detect(_plan, _allElements);
            _conflictIds = new HashSet<int>(conflicts.Select(c => c.ElementId));
            var conflictsById = conflicts.ToDictionary(c => c.ElementId);

            Rows.Clear();
            foreach (var entry in _plan.Elements)
            {
                conflictsById.TryGetValue(entry.Element.ElementId, out var conflict);
                Rows.Add(new RowViewModel(entry, conflict));
            }

            Status = $"Элементов: {_plan.Elements.Count}; конфликтов: {_conflictIds.Count}.";
            UpdateCommandState();
        }

        private async Task ApplyAsync()
        {
            if (IsBusy || _plan == null)
            {
                return;
            }

            _rebuildTimer.Stop();
            RebuildPlan();
            if (_plan == null)
            {
                return;
            }

            var document = _loadedDocument;
            if (document == null
                || !document.IsValidObject
                || !ReferenceEquals(document, AddinContext.DocumentTracker.ActiveDocument))
            {
                Status = "Активный документ изменился. Нажмите «Обновить».";
                System.Windows.MessageBox.Show(
                    Status,
                    "Нумерация элементов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var entries = _plan.Elements
                .Where(e => !_conflictIds.Contains(e.Element.ElementId))
                .Where(e => e.NewValue != e.Element.CurrentValue)
                .ToList();
            var conflictCount = _conflictIds.Count;

            if (entries.Count == 0)
            {
                Status = conflictCount > 0
                    ? "Все изменения конфликтуют — ничего не записано."
                    : "Нет изменений для записи.";
                return;
            }

            IsBusy = true;
            ApplyResult result = null;

            try
            {
                var parameter = SelectedCategory.Parameter;
                result = await AddinContext.Executor.InvokeAsync(app =>
                {
                    var active = app.ActiveUIDocument;
                    if (active == null
                        || !ReferenceEquals(active.Document, document)
                        || !document.IsValidObject)
                    {
                        throw new InvalidOperationException("Активный документ изменился. Обновите данные.");
                    }

                    return RevitWriter.Apply(document, parameter, entries);
                });

                AddinContext.SaveSettings();
                var summary = BuildSummary(result, conflictCount);
                Status = summary;
                System.Windows.MessageBox.Show(
                    summary,
                    "Нумерация элементов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось применить нумерацию", ex);
                Status = "Не удалось применить нумерацию: " + ex.Message;
                System.Windows.MessageBox.Show(
                    Status,
                    "Нумерация элементов",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }

            if (result != null)
            {
                await ReloadAsync();
            }
        }

        private static string BuildSummary(ApplyResult result, int conflictCount)
        {
            var text = $"Записано: {result.Applied}.";
            if (result.Unchanged > 0)
            {
                text += $"{Environment.NewLine}Без изменений: {result.Unchanged}.";
            }

            if (conflictCount > 0)
            {
                text += $"{Environment.NewLine}Пропущено из-за конфликтов: {conflictCount}.";
            }

            if (result.Skipped.Count > 0)
            {
                text += $"{Environment.NewLine}Не удалось изменить: {result.Skipped.Count}.";
                foreach (var item in result.Skipped.Take(10))
                {
                    text += Environment.NewLine + "— " + item;
                }

                if (result.Skipped.Count > 10)
                {
                    text += Environment.NewLine + "…";
                }
            }

            return text;
        }

        private void OnActiveDocumentChanged(Document document)
        {
            if (_disposed)
            {
                return;
            }

            _dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed || !_initialized)
                {
                    return;
                }

                _plan = null;
                _conflictIds.Clear();
                Rows.Clear();
                _allElements = null;
                _scopedElements = null;
                _loadedDocument = null;
                UpdateCommandState();

                if (document == null || !document.IsValidObject)
                {
                    Status = "Нет активного документа.";
                    return;
                }

                _ = ReloadAsync();
            }));
        }

        private void ClearForNoDocument()
        {
            _loadedDocument = null;
            _allElements = null;
            _scopedElements = null;
            _plan = null;
            _conflictIds.Clear();
            Rows.Clear();
            Status = "Нет активного документа.";
            UpdateCommandState();
        }

        private bool FilterRow(object item)
        {
            return item is RowViewModel row
                   && ElementSearch.Matches(row.Element, row.NewValue, SearchText);
        }

        private void OnSearchTimerTick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            RowsView.Refresh();
        }

        private void OnRebuildTimerTick(object sender, EventArgs e)
        {
            _rebuildTimer.Stop();
            RebuildPlan();
        }

        private void UpdateCommandState()
        {
            _refreshCommand.RaiseCanExecuteChanged();
            _applyCommand.RaiseCanExecuteChanged();
        }

        private void RememberTemplate()
        {
            if (SelectedCategory == null)
            {
                return;
            }

            var key = SelectedCategory.Category.ToString();
            AddinContext.Settings.LastCategory = key;

            if (NumberTemplate.Parse(TemplateText).IsValid)
            {
                AddinContext.Settings.Templates[key] = TemplateText;
            }
        }

        private static string GetTemplate(CategoryDefinition category)
        {
            var key = category.Category.ToString();
            if (AddinContext.Settings.Templates.TryGetValue(key, out var template)
                && !string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            return DefaultTemplates.TryGetValue(key, out var fallback)
                ? fallback
                : "{Уровень}-{N:000}";
        }
    }
}
