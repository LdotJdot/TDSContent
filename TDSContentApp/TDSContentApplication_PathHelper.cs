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
        public string GetPath(string driverName,string referenceNumber)
        {
            if(!string.IsNullOrEmpty(driverName) && ulong.TryParse(referenceNumber, out var reference))
            {
                return GetPath(driverName, reference);
            }
            return string.Empty;
        }

        public string GetPath(string driverName,ulong referenceNumber)
        {
           return  fileSysList
                .FirstOrDefault(o => o.DriveName.Equals(driverName, StringComparison.OrdinalIgnoreCase))
                ?.GetPath(referenceNumber) ?? string.Empty;

        }

    }
}
