using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Lucene.Net.Index;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentCore.Engine;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace TDSContentApp.Converters
{
    public class Converter_DOCX : IFileToStringConverter
    {
        public string Extension { get; } = ".DOCX";

        public Converter_DOCX()
        {
        }


        public string Convert(string path)
        {
            var str= ExtractText(path).ToString();
            return str;
        }

        public void Dispose()
        {

        }

        public static StringBuilder ExtractText(string filename)
        {
            var sb = new StringBuilder();
            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(filename, false))
            {
                // Assign a reference to the existing document body.  
                Body? body = wordDocument?.MainDocumentPart?.Document?.Body;

                foreach (var p in wordDocument?.MainDocumentPart?.WordprocessingCommentsPart?.Comments ?? [])
                {
                    if (!string.IsNullOrWhiteSpace(p.InnerText))
                    {
                        sb.AppendLine(p.InnerText);
                    }
                }

                if (body != null)
                {
                    foreach (var p in body.Elements<Paragraph>())
                    {
                        if (!string.IsNullOrWhiteSpace(p.InnerText))
                        {
                            sb.AppendLine(p.InnerText);
                        }
                    }

                    // Extract text from tables
                    foreach (Table table in body.Elements<Table>())
                    {
                        foreach (TableRow row in table.Elements<TableRow>())
                        {
                            foreach (TableCell cell in row.Elements<TableCell>())
                            {
                                foreach (Paragraph p in cell.Elements<Paragraph>())
                                {
                                    if (!string.IsNullOrWhiteSpace(p.InnerText))
                                    {
                                        sb.AppendLine(p.InnerText);
                                    }
                                }
                            }
                        }
                    }


                    // Extract text from text boxes
                    foreach (var sdt in body.Elements<SdtElement>())
                    {
                        foreach (Paragraph p in sdt.Descendants<Paragraph>())
                        {
                            if (!string.IsNullOrWhiteSpace(p.InnerText))
                            {
                                sb.AppendLine(p.InnerText);
                            }
                        }
                    }
                }
                wordDocument.Dispose();
            }

            return sb;
        }

    
    }
}
