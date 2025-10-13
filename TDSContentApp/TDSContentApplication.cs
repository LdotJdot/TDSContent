using Lum.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.ProjectManager;
using TDSContentApp.USN;
using TDSContentCore.Engine;

namespace TDSContentApp
{
    public partial  class TDSContentApplication :IDisposable
    {
        ILoggerProvider loggerProvider = new LogProvider();

        static Lazy<TDSContentApplication> instance = new Lazy<TDSContentApplication>(()=>new TDSContentApplication());

        public static TDSContentApplication Instance => instance.Value;

        Projects projects;

        List<FileSys2> fileSysList = new(); 

        private TDSContentApplication()
        {

        }

        public IEnumerable<string> SupportFormat => [];

        public Projects Projects => projects;

        private bool disposedValue;

        internal static readonly string CurrentFolder = Directory.GetCurrentDirectory();
        const string FILESYSCAHNAME = "index.cah";
        static readonly string FILESYSCACHEPATH=Path.Combine(CurrentFolder, FILESYSCAHNAME);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DumpUSNToDisk()
        {
            DiskDataCache.DumpToDisk(fileSysList, FILESYSCACHEPATH);

        }
        public void Dispose()
        {
            DumpUSNToDisk();
            DumpProjectsToDisk();
            projects.Dispose();

            disposedValue = true;            
        }
    }
}
