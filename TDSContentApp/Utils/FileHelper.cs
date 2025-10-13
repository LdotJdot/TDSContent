using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDSContentApp.Utils
{
    internal class FileHelper
    {
        internal static IEnumerable<string> GetTopFiles(string path, IEnumerable<string> exts)
        {
            IEnumerable<string> files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                foreach(var ext in exts)
                {
                    if (Path.GetExtension(file).Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return file;
                        break;
                    }
                }
            }
        }

        internal static IEnumerable<string> GetAllFiles(string path, IEnumerable<string> exts)
        {
            IEnumerable<string> files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                foreach(var ext in exts)
                {
                    if (Path.GetExtension(file).Equals(ext, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return file;
                        break;
                    }
                }
            }
        }

        internal static IEnumerable<string> GetTopFolders(string path)
        {
            return  Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);

        }

        internal static IEnumerable<string> GetAllFolders(string path)
        {
            return Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);

        }
    }
}
