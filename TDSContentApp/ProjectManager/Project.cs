using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.USN.Engine.Utils;
using TDSContentApp.Utils;
using TDSContentCore.Engine;

namespace TDSContentApp.ProjectManager
{
    
    public class Project:IDisposable
    {
        internal TDSContentEngine? eng { get; private set; } 

        public ulong FolderReferenceNumber { get; set; }=ulong.MaxValue;

        public string DriveName { get; set; } = string.Empty;

        public string[] Extents { get; set; } = [];

        public string Id { get; set; } = string.Empty;
        HashSet<ulong> folderReferenceNumbers { get; set; } = new();

        // Constructor for deserialization
        public Project()
        {

        }

        public Project Initialize(Dictionary<string, IFileToStringConverter> _converters)
        {
            if(string.IsNullOrWhiteSpace(Id))
            {
                Id = Guid.NewGuid().ToString("N");
            }
            eng = new TDSContentEngine($"index/{Id}/index_data", _converters);
            return this;
        }

        public Project(string path, string[] extent)
        {
            IndexFolder(path, extent);
        }

        public bool Remove(ulong folderReferenceNumber)
        {
           return folderReferenceNumbers.Remove(folderReferenceNumber);
        }

        public void Add(ulong folderReferenceNumber)
        {
            folderReferenceNumbers.Add(folderReferenceNumber);
        }

        public bool ContainsExtent(string ext)
        {
            return Extents.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
        public bool Contains(ulong referenceNumber)
        {
            return folderReferenceNumbers.Contains(referenceNumber);
        }

        public void IndexFolder(string folderPath, string[] extent)
        {
            if (Directory.Exists(folderPath))
            {
                var refNum= GetUSNFromPath.GetPathReferenceNumber(folderPath, out _);
                if(refNum!=ulong.MaxValue)
                {
                    this.FolderReferenceNumber = refNum;
                    this.DriveName = Directory.GetDirectoryRoot(folderPath);                    
                    this.Extents = extent;
                    this.folderReferenceNumbers = new HashSet<ulong>();
                }
                else
                {
                    throw new Exception($"Cannot access folder: {folderPath}");
                }                               

                folderReferenceNumbers.Add(refNum);

                foreach (var folder in FileHelper.GetAllFolders(folderPath))
                {
                    var id = GetUSNFromPath.GetPathReferenceNumber(folder, out _);
                    if (id != ulong.MaxValue)
                    {
                        folderReferenceNumbers.Add(id);
                    }
                }
            }
        }
                
        public void Dispose()
        {
            eng?.Dispose();
            eng = null;
        }

        public void Destory()
        {
            eng?.Dispose();
            try
            {
                if(!string.IsNullOrWhiteSpace(Id))
                {
                    Directory.Delete($"index/{Id}", true);
                }
            }catch{ }
            eng = null;
        }
    }
}
