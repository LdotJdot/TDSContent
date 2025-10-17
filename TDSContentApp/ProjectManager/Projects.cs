using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.USN.Engine.Utils;
using TDSContentCore.Engine;

namespace TDSContentApp.ProjectManager
{
    public class Projects:IDisposable
    {
        [JsonIgnore]
        internal IEnumerable<Project> this[string driverName]=> projects.TryGetValue(driverName, out var collection) ? collection.Values : Enumerable.Empty<Project>();

        [JsonInclude]
        internal ConcurrentDictionary<string, ConcurrentDictionary<string,Project>> projects { get; set; } = new();

        [JsonIgnore]
        public (string driveName, (ulong refNumber, string[] ext, string id)[] projects)[] Info=> projects.Select(p => (p.Key, p.Value.Values.Select(v => (v.FolderReferenceNumber, v.Extents, v.Id)).ToArray())).ToArray();

        private readonly Dictionary<string, IFileToStringConverter> _converters=new Dictionary<string, IFileToStringConverter>(StringComparer.OrdinalIgnoreCase);

        [JsonIgnore]
        public string[] Extensions=>_converters.Keys.ToArray();

        public Projects()
        {
            AddConverter(new Converter_PURETXT());
            AddConverter(new Converter_PDF());
            AddConverter(new Converter_DOCX());
            AddConverter(new Converter_PPTX());
            AddConverter(new Converter_DOCPPT());
            AddConverter(new Converter_Dwg());
            AddConverter(new Converter_Dxf());
        }


        public void AddConverter(IFileToStringConverter converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            var extensions = converter.Extension.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var ext in extensions)
            {
                var normalizedExt = ext.StartsWith(".") ? ext : "." + ext;
                _converters[normalizedExt] = converter;
            }
        }
        public void Initialize()
        {
            foreach(var projectsInDrive in projects.Values)
            {
                foreach(var project in projectsInDrive.Values)
                {
                    project.Initialize(_converters);
                }
            }
        }

      
        public Project AddProject(string path, string[] extent)
        {
            if (!Directory.Exists(path))
            {
                throw new Exception($"Folder not existed: {path}");
            }

            var refNum = GetUSNFromPath.GetPathReferenceNumber(path, out _);
            if(refNum == ulong.MaxValue)
            {
                throw new Exception($"Cannot access folder: {path}");
            }

            var driveName = Directory.GetDirectoryRoot(path);

            if(!projects.TryGetValue(driveName,out var collection))
            {              
                projects[driveName] = new ();
            }
            
            var newProject = new Project(path, extent);
            newProject.Initialize(_converters);
            projects[driveName].TryAdd(newProject.Id, newProject);
            return newProject;
        }

        public void AddChildFolder(string driveName, ulong referenceNumber,ulong parentReferenceNumber)
        {
            if (projects.TryGetValue(driveName, out var collection))
            {
                foreach (var project in collection.Values)
                {
                    if (project.Contains(parentReferenceNumber))
                    {
                        project.Add(referenceNumber);
                    }
                }
            }
            else
            {
                projects[driveName] = new ();
            }
        }

        public void AddSubFolder(string driveName, ulong referenceNumber,ulong parentReferenceNumber)
        {
            if (projects.TryGetValue(driveName, out var collection))
            {
                foreach (var project in collection.Values)
                {
                    if (project.Contains(parentReferenceNumber))
                    {
                        project.Add(referenceNumber);
                    }
                }
            }
            else
            {
                projects[driveName] = new();
            }
        }

        public void RemoveSubFolder(string driveName, ulong referenceNumber)
        {
            if (projects.TryGetValue(driveName, out var collection))
            {
                foreach (var project in collection.Values)
                {
                     project.Remove(referenceNumber);
                }
            }
            else
            {
                projects[driveName] = new();
            }
        }

        public void Remove(string driveName, ulong referenceNumber)
        {
            if (projects.TryGetValue(driveName, out var collection))
            {
                List<Project> removedProjects = new();
                foreach (var project in collection.Values)
                {
                    if (project.FolderReferenceNumber==referenceNumber)
                    {
                        removedProjects.Add(project);
                    }
                    
                    project.Remove(referenceNumber);
                }

                foreach(var proj in removedProjects)
                {
                    projects[driveName].TryRemove(proj.DriveName, out _);
                }
            }
            else
            {
                projects[driveName] = new ();
            }
        }

        /// <summary>
        /// Check if the file is in the project folders by parent folder reference number and extent
        /// </summary>
        /// <param name="driveName"></param>
        /// <param name="fileName"></param>
        /// <param name="parentReferenceNumber"></param>
        /// <returns></returns>
        public bool CheckEntry(string driveName, string fileName,ulong parentReferenceNumber)
        {
            var ext= System.IO.Path.GetExtension(fileName);
            if (projects.TryGetValue(driveName, out var collection))
            {
                foreach (var project in collection.Values)
                {
                    if (project.Contains(parentReferenceNumber) && project.ContainsExtent(ext))
                    {
                        return true;
                    }
                }
            }
            else
            {
                projects[driveName] = new ();
            }
            return false;
        }


        public bool Contains(string driveName, ulong referenceNumber)
        {
            if (projects.TryGetValue(driveName, out var collection))
            {
                foreach (var project in collection.Values)
                {
                    if (project.Contains(referenceNumber))
                    {
                        return true;
                    }
                }
            }
            else
            {
                projects[driveName] = new ();
            }
            return false;
        }

        public void Dispose()
        {
            foreach(var drive in projects.Values)
            {
                foreach(var project in drive.Values)
                {
                    project?.Dispose();
                }
            }
            projects.Clear();
            projects = null;


            foreach (var converter in _converters.Values)
            {
                converter?.Dispose();
            }
            _converters.Clear();
        }
    }
}
