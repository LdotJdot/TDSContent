using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO;
using netDxf;
using netDxf.Collections;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TDSContentCore.Engine;
using Dimension = netDxf.Entities.Dimension;
using MText = netDxf.Entities.MText;

namespace TDSContentApp.Converters
{
    public class Converter_Dxf:IFileToStringConverter
    {

        public string Extension { get; } = ".DXF";

        public string Convert(string filepath)
        {
            return GetDxfTxt(filepath);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        string GetDxfTxt(string filePath)
        {
            using var fs=new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite|FileShare.Delete);
            var doc = DxfDocument.Load(fs);
            if (doc == null) throw new Exception($"Failed to open dxf file.");
            var sb = new StringBuilder(256);

            Recruit(doc,sb);

 
            return sb.ToString();
        }
   

        void Recruit(DxfDocument doc, StringBuilder sb)
        {
            var entities = doc.Entities;

            foreach (var txt in entities.Texts)
            {
                sb.AppendLine(txt.Value);
            }

            foreach (var mtxt in entities.MTexts)
            {
                sb.AppendLine(mtxt.Value);
            }

            foreach (var d in entities.Dimensions)
            {
                sb.AppendLine(d.UserText);
            }

            foreach(var l in doc.Layers)
            {
                sb.AppendLine(l.Name);
            }

            Recruit(doc.Blocks, sb);
        }

        void Recruit(BlockRecords block, StringBuilder sb)
        {
            foreach (var bl in block)
            {
                var entities = bl.Entities;
                foreach (var en in entities)
                {
                    if (en is Text txt)
                    {
                        sb.AppendLine(txt.Value);
                    }
                    else if (en is MText mtxt)
                    {
                        sb.AppendLine(mtxt.Value);
                    }
                    else if (en is Dimension d)
                    {
                        sb.AppendLine(d.UserText);
                    }
                }
            }
        }
    }
}
