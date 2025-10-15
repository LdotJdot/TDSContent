using ACadSharp;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TDSContentCore.Engine;

namespace TDSContentApp.Converters
{
    public class Converter_Dwg:IFileToStringConverter
    {       
        public string Extension { get; } = ".DWG";

        public string Convert(string filepath)
        {
            return GetDwgTxt(filepath);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        string GetDwgTxt(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var doc = DwgReader.Read(fs);
            if (doc == null) throw new Exception($"Failed to open dwg file.");

            var sb = new StringBuilder(256);            
            Recruit(doc.Entities, sb);
            Recruit(doc, sb);
            return sb.ToString();
        }

        void Recruit(IEnumerable<Entity> entities, StringBuilder sb)
        {            
            foreach (var en in entities)
            {
                try
                {
                    if (en is IText txt)
                    {
                        sb.AppendLine(txt.Value);
                    }
                    else if (en is Dimension d)
                    {
                        sb.AppendLine(d.Text);

                    }
                    else if (en is Block block)
                    {
                        Recruit(block.Document.Entities, sb);
                    }
                }
                catch
                {

                }
            }
        }
        void Recruit(CadDocument doc, StringBuilder sb)
        {
            foreach(var vp in doc.VPorts)
            {
                sb.AppendLine($"视口:{vp.Name}");
            }

            foreach (var l in doc.Layers)
            {
                sb.AppendLine($"图层:{l.Name}");
            }
        }
    }
}
