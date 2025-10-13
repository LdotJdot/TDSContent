using Avalonia.Controls.Documents;
using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace TDSContentCore
{
    public interface IFrnFileOrigin
    {       

        public string FileName { get; }
        public string FilePath { get; }

        public Bitmap? icon { get; }

        public string FileInfo{ get; }

        public ReadOnlySpan<char> Details { get; }

        public bool HasDetail => Details.Length > 0;

    }
}
