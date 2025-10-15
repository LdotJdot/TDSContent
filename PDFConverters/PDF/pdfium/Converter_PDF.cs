using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentCore.Engine;

namespace TDSContentApp.Converters
{
    public class Converter_PDF : IFileToStringConverter
    {
        public Converter_PDF()
        {
            PDFiumCore.fpdfview.FPDF_InitLibrary();
        }

        int bufferSize = 10240;

        readonly object lk = new object();

        public string Convert(string path)
        {
            lock (lk)
            {
                return Convert_PDFium(path);
            }
        }

        public string Convert_PDFium(string path)
        {

            var doc = PDFiumCore.fpdfview.FPDF_LoadDocument(path, string.Empty);


            var pageCount = PDFiumCore.fpdfview.FPDF_GetPageCount(doc);
            var sb = new StringBuilder(512);

            for (int i = 0; i < pageCount; i++)
            {

                var page = PDFiumCore.fpdfview.FPDF_LoadPage(doc, i);
                if (page == null)
                {
                    Debug.WriteLine($"无法加载页面: {path} - 页面 {i}");
                    continue;
                }

                var textPage = PDFiumCore.fpdf_text.FPDFTextLoadPage(page);
                if (textPage == null)
                {
                    Debug.WriteLine($"无法加载文本页面: {path} - 页面 {i}");
                    continue;
                }

                var textBuffer = new ushort[bufferSize];

                int charsExtracted = PDFiumCore.fpdf_text.FPDFTextGetText(textPage, 0, bufferSize, ref textBuffer[0]);
                try
                {
                    var text = new string(textBuffer.Select(o => (char)o).ToArray(), 0, charsExtracted);
                    sb.AppendLine(text);// 将 ushort 数组转换为字符串
                }
                catch
                {
                }
                finally
                {
                    PDFiumCore.fpdf_text.FPDFTextClosePage(textPage);
                    PDFiumCore.fpdfview.FPDF_ClosePage(page);
                }
            }

            PDFiumCore.fpdfview.FPDF_CloseDocument(doc);
            //Console.WriteLine(sb.ToString());
            return sb.ToString();
        }

        public string Extension { get; } = ".PDF";
                   

        public void Dispose()
        {
            PDFiumCore.fpdfview.FPDF_DestroyLibrary();
        }
    }
}
