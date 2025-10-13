using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TDSAot.Utils;
using TDSContentCore;

namespace TDSAot
{
    public partial class MainWindow : Window
    {

    }

    public class HighlightTextConverter : IValueConverter
    {
        static readonly IBrush highlightBrush = Brush.Parse("#00BFFF");
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var frn = value as IFrnFileOrigin;


            var inlines = new InlineCollection();


            var textSource = frn.Details;

            if (MainWindow.keyword.Length == 0)
            {
                return inlines;
            }

            try
            {
                var results = TextMatch.FindTextMatches(textSource, MainWindow.keyword, maxResults: 5, contextChars: 20);


                foreach (var result in results)
                {
                    if (inlines.Count() > 0)
                    {
                        inlines.Add(new Run("\r\n"));
                    }

                    var extracted= textSource.Slice(result.ExtractStart,result.ExtractLength);

                    var text1= textSource.Slice(result.ExtractStart,result.KeywordIndex-result.ExtractStart);
                    var text2= MainWindow.keyword;
                    var text3 = textSource.Slice(result.KeywordIndex + result.KeywordLength, result.ExtractStart + result.ExtractLength - (result.KeywordIndex + result.KeywordLength));

                    var run1 = new Run($"... {text1.TrimStart().ToString().Replace("\r\n"," ").Replace("\r","").Replace("\n","").Replace("\t","")}");
                    var run2 = new Run(text2.ToString()) { Foreground = highlightBrush };
                    var run3=  new Run($"{text3.TrimEnd().ToString().Replace("\r\n", " ").Replace("\r", "").Replace("\n", "").Replace("\t", "")} ...");

                    inlines.Add(run1);
                    inlines.Add(run2);
                    inlines.Add(run3);
                }
            }
            catch
            {
                inlines.Clear();
                inlines.Add(new Run { Text = frn.FileName });
            }

            return inlines;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public readonly struct TextMatch
    {
        public int KeywordIndex { get; }    // 关键词位置
        public int KeywordLength { get; }   // 关键词长度
        public int ExtractStart { get; }    // 截取起始位置
        public int ExtractLength { get; }   // 截取长度

        private TextMatch(int keywordIndex, int keywordLength, int extractStart, int extractLength)
        {
            KeywordIndex = keywordIndex;
            KeywordLength = keywordLength;
            ExtractStart = extractStart;
            ExtractLength = extractLength;
        }


        public static List<TextMatch> FindTextMatches(ReadOnlySpan<char> text, ReadOnlySpan<char> keyword, int maxResults, int contextChars = 10)
        {
            var matches = new List<TextMatch>();
            if (keyword.IsEmpty) return matches;

            int index = 0;
            int count = 0;

            while (count < maxResults && index < text.Length)
            {
                // 在剩余的文本中搜索关键词
                ReadOnlySpan<char> remainingText = text.Slice(index);
                int foundIndex = remainingText.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);

                if (foundIndex == -1) break;

                int absoluteIndex = index + foundIndex; // 在原文中的绝对位置

                int start = Math.Max(0, absoluteIndex - contextChars);
                int end = Math.Min(text.Length, absoluteIndex + keyword.Length + contextChars);
                int length = end - start;

                matches.Add(new TextMatch(absoluteIndex, keyword.Length, start, length));

                count++;
                index = absoluteIndex + keyword.Length; // 移动到关键词后面继续搜索
            }

            return matches;
        }
    }
}