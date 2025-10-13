using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentCore.Engine;

namespace TDSContentApp.Converters
{
    internal class Converter_PURETXT : IFileToStringConverter
    {
        public string Extension { get; } = ".txt";

        public string Convert(string path)
        {
            return File.ReadAllText(path);
        }

        public void Dispose()
        {
           
        }
    }
}
