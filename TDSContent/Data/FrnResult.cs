using Avalonia.Media.Imaging;
using System;
using System.IO;
using TDSContentApp;
using TDSContentApp.USN;
using TDSContentCore.Engine;

namespace TDSContentCore
{
    public class FrnResult: IFrnFileOrigin
    {
        Lucene.Net.Documents.Document doc;

        public FrnResult(Lucene.Net.Documents.Document doc)
        {
            this.doc = doc;
        }



        public string FileName => Path.GetFileName(FilePath);

        public string FilePath => TDSContentApplication.Instance.GetPath(doc?.Get(TDSContentEngine.DRIVERNAME), doc?.Get(TDSContentEngine.FILEREFERENCENUMBER));

        public ReadOnlySpan<char> Details => doc != null ? doc.Get(TDSContentEngine.FILECONTENT).AsSpan() : Span<char>.Empty;

        public Bitmap? icon => FileIconService.GetIcon(FilePath);

        public string FileInfo => string.Join(" ",getFileInfoStr(this),$"Updated on {doc.Get(TDSContentEngine.FILEUPDATETIME)}");

        public static string getFileInfoStr(FrnResult f)
        {
            var path = f.FilePath;

            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var fileInfo = new FileInfo(f.FilePath);

            if (fileInfo.Exists)
            {
                return FormatBytes(fileInfo.Length);
            }
            else
            {
                return null;
            }
        }

        private static string FormatBytes(long bytes)
        {
            return bytes switch
            {
                < 1024L => $"{bytes:0.###} B",
                < 1048576L => $"{(bytes / 1024.0):0.###} KB",
                < 1073741824L => $"{(bytes / 1048576.0):0.###} MB",
                < 1099511627776L => $"{(bytes / 1073741824.0):0.###} GB",
                < 1125899906842624L => $"{(bytes / 1099511627776.0):0.###} TB",
                _ => $"{(bytes / 1125899906842624.0):0.###} PB"
            };
        }
    }
}
