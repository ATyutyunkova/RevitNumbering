using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RevitNumbering.Core
{
    public sealed class NumberTemplate
    {
        private readonly List<Segment> _segments = new List<Segment>();

        private NumberTemplate()
        {
        }

        public bool IsValid { get; private set; }

        public string Error { get; private set; } = "";

        public static NumberTemplate Parse(string text)
        {
            text = text ?? "";
            var template = new NumberTemplate();
            var literal = new StringBuilder();
            var hasSequence = false;

            void FlushLiteral()
            {
                if (literal.Length == 0)
                {
                    return;
                }

                template._segments.Add(Segment.ForLiteral(literal.ToString()));
                literal.Clear();
            }

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '{')
                {
                    var end = text.IndexOf('}', i + 1);
                    if (end < 0)
                    {
                        return Failed("Незакрытая подстановка: пропущена «}».");
                    }

                    var token = text.Substring(i + 1, end - i - 1);
                    FlushLiteral();
                    i = end;

                    if (token.Equals("N", StringComparison.OrdinalIgnoreCase))
                    {
                        template._segments.Add(Segment.ForNumber(""));
                        hasSequence = true;
                    }
                    else if (token.StartsWith("N:", StringComparison.OrdinalIgnoreCase))
                    {
                        var format = token.Substring(2);
                        if (!CanFormat(format))
                        {
                            return Failed($"Некорректный формат числа «{format}».");
                        }

                        template._segments.Add(Segment.ForNumber(format));
                        hasSequence = true;
                    }
                    else if (token.Equals("Уровень", StringComparison.OrdinalIgnoreCase))
                    {
                        template._segments.Add(Segment.ForLevel());
                    }
                    else
                    {
                        return Failed($"Неизвестная подстановка «{{{token}}}». Доступны {{N}}, {{N:формат}} и {{Уровень}}.");
                    }
                }
                else if (c == '}')
                {
                    return Failed("Лишняя закрывающая скобка «}».");
                }
                else
                {
                    literal.Append(c);
                }
            }

            FlushLiteral();

            if (!hasSequence)
            {
                return Failed("Шаблон должен содержать подстановку {N}.");
            }

            template.IsValid = true;
            return template;
        }

        public string Format(int sequence, string levelName)
        {
            if (!IsValid)
            {
                throw new InvalidOperationException(Error);
            }

            var builder = new StringBuilder();
            foreach (var segment in _segments)
            {
                switch (segment.Kind)
                {
                    case SegmentKind.Literal:
                        builder.Append(segment.Text);
                        break;
                    case SegmentKind.Level:
                        builder.Append(levelName ?? "");
                        break;
                    case SegmentKind.Number:
                        builder.Append(sequence.ToString(segment.Text, CultureInfo.InvariantCulture));
                        break;
                }
            }

            return builder.ToString();
        }

        private static bool CanFormat(string format)
        {
            try
            {
                var first = 1.ToString(format, CultureInfo.InvariantCulture);
                var second = 2.ToString(format, CultureInfo.InvariantCulture);
                return first != second;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static NumberTemplate Failed(string error)
        {
            return new NumberTemplate { Error = error };
        }

        private enum SegmentKind
        {
            Literal,
            Level,
            Number
        }

        private sealed class Segment
        {
            public SegmentKind Kind { get; private set; }

            public string Text { get; private set; }

            public static Segment ForLiteral(string text)
            {
                return new Segment { Kind = SegmentKind.Literal, Text = text };
            }

            public static Segment ForLevel()
            {
                return new Segment { Kind = SegmentKind.Level, Text = "" };
            }

            public static Segment ForNumber(string format)
            {
                return new Segment { Kind = SegmentKind.Number, Text = format };
            }
        }
    }
}
