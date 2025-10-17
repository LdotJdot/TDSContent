using DotMaysWind.Office;
using TDSContentCore.Engine;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace TDSContentApp.Converters
{
    public class Converter_DOCPPT : IFileToStringConverter
    {
        public string Extension { get; } = ".DOC|.PPT";

        public Converter_DOCPPT()
        {
        }

        public string Convert(string path)
        {
            return ExtractText(path).ToString();
        }

        public void Dispose()
        {

        }

        public static string ExtractText(string filePath)
        {
            var file = OfficeFileFactory.CreateOfficeFile(filePath);
            if (file is IWordFile wordFile)
            {
                return wordFile.ParagraphText;
            }
            else if (file is IPowerPointFile pptFile)
            {
                return pptFile.AllText;
            }
            else
            {
                throw new Exception("Cannot extract content from this file.");
            }


        }


    }
}
