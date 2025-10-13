using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.ProjectManager;
using TDSContentApp.USN.Engine.Utils;
using TDSContentApp.Utils;
using TDSContentCore.Engine;

namespace TDSContentApp
{        

    public partial class TDSContentApplication:IDisposable
    {
        public void RemoveCategory(string category)
        {
            foreach(var projs in projects.projects.Values)
            {
                if (projs.TryRemove(category, out var target))
                {
                    target.Destory();
                }
            }
        }

        public void DeleteEntry(string driveName,ulong parentReferenceNumber,ulong referenceNumber)
        {
            foreach(var project in projects[driveName])
            {
                if(project.Contains(parentReferenceNumber))
                {
                    project?.eng?.DeleteFile(referenceNumber);
                }
            }
        }       
    }
}
