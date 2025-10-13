using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Text.Json;
using TDSContentApp.ProjectManager;


namespace TDSContentApp
{

    public partial class TDSContentApplication : IDisposable
    {
        public string[] Extensions => projects?.Extensions ?? [];

        string projectPath => Path.Combine(CurrentFolder, "projects.json");

        public void DumpProjectsToDisk()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(projects);
            System.IO.File.WriteAllText(projectPath, json);
        }



        public void Initialize(bool tryFromDisk)
        {
            Projects? projs = null;
            try
            {
                if (tryFromDisk)
                {
                    projs = System.Text.Json.JsonSerializer.Deserialize<Projects>(File.ReadAllText(projectPath));
                }
            }
            catch
            {
            }
            finally
            {
                if(projs != null)
                {
                    projects = projs;
                }
                else
                {
                    projects = new Projects();  // reset the projects
                }
                projects.Initialize();
            }
        }
    }
}
