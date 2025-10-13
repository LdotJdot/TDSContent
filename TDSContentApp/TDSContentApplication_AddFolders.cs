using Lum.Log;
using Microsoft.Extensions.Logging;
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

        public void CheckCategoryNameExisted(string category)
        {
            projects.CheckCategoryNameExisted(category);
        }
        public void AddCategory(string folderPath, string categoryName,IEnumerable<string> searchPatterns,Action markAsStarted, Action<long> addTotal, Action taskIncrement, Action markAsCompleted)
        {
            // There is no need to dispose the logger.
            var logger = loggerProvider.CreateLogger($"Index-{categoryName}");
            try
            {
                markAsStarted?.Invoke();

                if (Directory.Exists(folderPath))
                {

                    var project = projects.AddProject(folderPath, searchPatterns.ToArray(), categoryName);

                    var filesPath = FileHelper.GetAllFiles(folderPath, searchPatterns).ToArray();

                    addTotal?.Invoke(filesPath.LongLength);

                    logger.LogInformation($"Index-{categoryName}");

                    long count = 0;
                    Parallel.ForEach(filesPath, filePath =>
                    {
                        var id = GetUSNFromPath.GetPathReferenceNumber(filePath, out _);
                        if (id != ulong.MaxValue)
                        {
                            try
                            {
                                project?.eng?.IndexFile(filePath, id);
                                if(Interlocked.Increment(ref count) % 100 == 0)
                                {
                                    project?.eng?.Commit();
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"{filePath}\r\n{ex.Message}");
                            }
                        }
                        taskIncrement?.Invoke();

                    });

                    project?.eng?.Commit();

                }
            }
            catch (Exception ex)
            {
                Message.ShowErrorOk(ex.Message);
                logger.LogError(ex.Message);

            }
            finally
            {

                markAsCompleted?.Invoke();
            }
        }

        // make sure the filePath in the projects folders before call this method
        private void AddFileEntry(string filePath)
        {
            if (!Path.Exists(filePath)) return;

            var referenceNumber = GetUSNFromPath.GetPathReferenceNumber(filePath,out _);
            var parentReferenceNumber = GetUSNFromPath.GetPathReferenceNumber(Path.GetDirectoryName(filePath) ?? string.Empty, out _);
            var root = Path.GetPathRoot(filePath) ?? string.Empty;
            var ext = Path.GetExtension(filePath) ?? string.Empty;

            if (referenceNumber != ulong.MaxValue)
            {
                foreach(var project in projects[root])
                {
                    if (project.Contains(parentReferenceNumber) && project.ContainsExtent(ext))
                    {
                       project.eng?.IndexFile(filePath, referenceNumber);
                       project.eng?.Commit();
                    }
                }
            }
        }     
    }
}
