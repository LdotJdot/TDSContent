using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TDSContentApp.Converters;
using TDSContentApp.USN;
using TDSContentApp.USN.Engine.Utils;
using TDSContentApp.Utils;
using TDSContentCore.Engine;
using TDSNET.Engine.Actions.USN;

namespace TDSContentApp
{

    public partial class TDSContentApplication : IDisposable
    {
        public bool TryLoadUSNFromDickCache()
        {
            var res = DiskDataCache.TryLoadFromDisk(FILESYSCACHEPATH);
            if (res == null)
            {
                return false;
            }
            else
            {
                fileSysList = res;
                return true;
            }
        }

        public void SetDisk((string driveName, string driveFormat)[] driveInfo)
        {
            fileSysList.Clear();

            foreach (var drive in driveInfo)
            {
                var fs2 = new FileSys2(drive.driveName, drive.driveFormat);
                fs2.InitialUsn();
                fileSysList.Add(fs2);
            }
        }

        public async void UpdateIndexAsync(Action markAsStarted, Action<long> addTotal, Action taskIncrement, Action markAsCompleted)
        {
            await Task.Run(() =>
            {
                markAsStarted?.Invoke();
                foreach (var fileSys in fileSysList)
                {
                    var results = fileSys.DoWhileFileChanges();
                    addTotal?.Invoke(results.Count());


                    for (int i = 0; i < results.Count(); i++)
                    {
                        try
                        {
                            var entry = results[i];
                            if (                                
                                i + 1 < results.Count()
                                && entry.FileReferenceNumber==results[i+1].FileReferenceNumber
                                && (entry.Reason | Win32Api.USN_REASON_CLOSE) == results[i + 1].Reason
                                )
                            {
                                continue; // 动作合并
                            }


                            if (entry.IsFile)
                            {
                                if (!projects.CheckEntry(fileSys.DriveName, entry.Name, entry.ParentFileReferenceNumber))
                                {
                                    continue;
                                }

                                Debug.Write(entry.Name + ": " + entry.FileReferenceNumber + ": " + Convert.ToString(entry.Reason, 2).PadLeft(32,'0'));

                                if (Win32Api.USN_REASON_OBJECT_ID_CHANGE == entry.Reason)
                                {
                                    //单独考虑
                                    DeleteEntry(fileSys.DriveName, entry.ParentFileReferenceNumber, entry.FileReferenceNumber);
                                    Debug.Write(" :maybe id changed caused by microsoft word savw\r\n");
                                }
                                else if ((entry.Reason & Win32Api.USN_REASON_FILE_DELETE) != 0)
                                {
                                    DeleteEntry(fileSys.DriveName, entry.ParentFileReferenceNumber, entry.FileReferenceNumber);
                                    Debug.Write(" :delete\r\n");
                                }
                                else if ((entry.Reason & Win32Api.USN_REASON_FILE_CREATE) != 0)
                                {
                                    Debug.Write(" :create\r\n");
                                    AddFileEntry(fileSys.GetPath(entry.FileReferenceNumber));
                                }
                                else if (
                                    ((entry.Reason & Win32Api.USN_REASON_DATA_TRUNCATION) != 0)
                                    ||
                                    ((entry.Reason & Win32Api.USN_REASON_DATA_OVERWRITE) != 0)
                                    ||
                                    ((entry.Reason & Win32Api.USN_REASON_DATA_EXTEND) != 0)
                                    ||
                                    ((entry.Reason & Win32Api.USN_REASON_NAMED_DATA_EXTEND) != 0)
                                    ||
                                    ((entry.Reason & Win32Api.USN_REASON_NAMED_DATA_OVERWRITE) != 0)
                                    ||
                                    ((entry.Reason & Win32Api.USN_REASON_NAMED_DATA_TRUNCATION) != 0))
                                {
                                    Debug.Write(" :update\r\n");
                                    AddFileEntry(fileSys.GetPath(entry.FileReferenceNumber));
                                }
                                else if ((Win32Api.USN_REASON_RENAME_NEW_NAME & entry.Reason) != 0)
                                {
                                    if (projects.Contains(fileSys.DriveName, entry.ParentFileReferenceNumber))
                                    {
                                        AddFileEntry(fileSys.GetPath(entry.FileReferenceNumber));
                                    }
                                    else
                                    {
                                        // if the parent folder is not in the project, then a "move out" happened and should delete it
                                        // LdotJdot :) 
                                        DeleteEntry(fileSys.DriveName, entry.ParentFileReferenceNumber, entry.FileReferenceNumber);
                                    }
                                    Debug.Write(" :rename or moved \r\n");
                                }
                                else
                                {
                                    Debug.Write(" :unknow \r\n");
                                }

                            }
                            else if (entry.IsFolder)
                            {
                                if (!projects.Contains(fileSys.DriveName, entry.ParentFileReferenceNumber) && !projects.Contains(fileSys.DriveName, entry.FileReferenceNumber))
                                {
                                    continue;
                                }
                                Debug.WriteLine(entry.Name + ": " + entry.FileReferenceNumber + ": " + Convert.ToString(entry.Reason, 2).PadLeft(32, '0'));

                                if ((entry.Reason & Win32Api.USN_REASON_FILE_DELETE) != 0)
                                {
                                    projects.Remove(fileSys.DriveName, entry.FileReferenceNumber);
                                }
                                else if ((entry.Reason & Win32Api.USN_REASON_FILE_CREATE) != 0)
                                {
                                    projects.AddChildFolder(fileSys.DriveName, entry.FileReferenceNumber, entry.ParentFileReferenceNumber);
                                }
                                else if ((Win32Api.USN_REASON_RENAME_NEW_NAME & entry.Reason) != 0)
                                {
                                    if (!projects.Contains(fileSys.DriveName, entry.ParentFileReferenceNumber))
                                    {
                                        // if the parent folder is not in the project, then a "move out" happened and should delete it
                                        // all the files related to this folder will be removed in the Remove function
                                        // LdotJdot :)
                                        
                                        projects.RemoveSubFolder(fileSys.DriveName, entry.FileReferenceNumber);
                                        projects.Remove(fileSys.DriveName, entry.FileReferenceNumber);

                                        // get all the entry from usn
                                        foreach (var frn in fileSys.GetSubtree(entry.FileReferenceNumber))
                                        {
                                            DeleteEntry(fileSys.DriveName, frn.parentReferenceNumber, frn.referenceNumber);
                                        }
                                    }
                                    else
                                    {
                                        // the folder should be treated as move in
                                        // considering subfolder only
                                        // LdotJdot :)

                                        // get all the entry from usn
                                        var subTree = fileSys.GetSubtree(entry.FileReferenceNumber);

                                        foreach (var frn in subTree)
                                        {
                                            var path = fileSys.GetPath(frn.referenceNumber);

                                            if (Directory.Exists(path))
                                            {
                                                projects.AddSubFolder(fileSys.DriveName, frn.referenceNumber, frn.parentReferenceNumber);
                                            }
                                            else if (File.Exists(path))
                                            {
                                                AddFileEntry(path);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                        finally
                        {
                            taskIncrement?.Invoke();
                        }
                    }
                }
                markAsCompleted?.Invoke();
            });
        }
    }
}
