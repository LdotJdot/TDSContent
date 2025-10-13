using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.USN.Engine.Utils;
using TDSContentApp.Utils;
using TDSContentCore.Engine;

namespace TDSContentApp
{

    public partial class TDSContentApplication:IDisposable
    {
        public (string path, string[] exts)? GetCategoryInfo(string category)
        {
            var result=projects.GetCategoryInfo(category);
            if (result != null)
            {
                var path= GetPath(result.Value.driveName,result.Value.referenceNumber);
                return (path, result.Value.extents);
            }
            else
            {
                return null;
            }

        }

    }
}
