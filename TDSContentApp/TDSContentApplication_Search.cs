using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentCore.Engine;

namespace TDSContentApp
{
    public partial class TDSContentApplication:IDisposable
    {
      
        public IList<Lucene.Net.Documents.Document> ShowAll()
        {
            var list = new List<Lucene.Net.Documents.Document>(128);
            foreach (var projects in projects.projects.Values)
            {
                foreach(var project in projects.Values)
                {
                    list.AddRange(project?.eng?.ListAllDocuments() ?? []);
                }
            }
            return list;
        }

        public IList<Lucene.Net.Documents.Document> SearchFile(string keywords, SearchMode mode, int maxCount)
        {
            var list = new List<Lucene.Net.Documents.Document>(128);
            foreach (var projects in projects.projects.Values)
            {
                foreach (var project in projects.Values)
                {                    
                    list.AddRange(project?.eng?.Search(keywords, mode, maxCount) ?? []);
                }
            }
            return list;
        }

    }
}
