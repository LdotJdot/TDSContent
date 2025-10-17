using System;

namespace DotMaysWind.Office
{
    public interface IWordFile
    {
        string ParagraphText { get; }

        string HeaderAndFooterText { get; }

        string CommentText { get; }

        string FootnoteText { get; }

        string EndnoteText { get; }
    }
}