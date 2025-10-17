using System;
using System.Text;

namespace DotMaysWind.Office.helper
{
    internal static class StringHelper
    {
        internal static string GetString(bool isUnicode, byte[] data)
        {
            if (isUnicode)
            {
                return Encoding.Unicode.GetString(data);
            }
            else
            {
                return Encoding.GetEncoding("Windows-1252").GetString(data);
            }
        }

        internal static string ReplaceString(string origin)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < origin.Length; i++)
            {
                if (origin[i] == '\r')
                {
                    sb.Append("\r\n");
                    continue;
                }

                if (origin[i] >= ' ')
                {
                    sb.Append(origin[i]);
                }
            }

            return sb.ToString();
        }
    }
}