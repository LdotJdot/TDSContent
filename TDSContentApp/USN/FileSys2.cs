using EngineCore.Engine.Actions.USN;
using J2N.Text;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TDSContentApp.USN.Engine.Actions.USN;
using TDSContentApp.USN.Engine.Utils;
using TDSNET.Engine.Actions.USN;


namespace TDSContentApp.USN
{
    public class FileEntry
    {
        public ulong referenceNumber;
        public ulong parentReferenceNumber;
        public string fileName;

        public FileEntry(ulong referenceNumber, ulong parentReferenceNumbher, string fileName)
        {
            this.referenceNumber = referenceNumber;
            this.parentReferenceNumber = parentReferenceNumbher;
            this.fileName = string.IsInterned(fileName) ?? fileName;
        }
    }

    public class FileSys2
    {

        public NtfsUsnJournal ntfsUsnJournal;
        public Win32Api.USN_JOURNAL_DATA lastUsnStates;
        public Win32Api.USN_JOURNAL_DATA usnStates;
        public readonly string DriveName;
        public readonly string DriveFormat;

        public ConcurrentDictionary<ulong, FileEntry> files = new(-1, 10_0000);

        public FileSys2(string driveName,string driveFormat)
        {
            this.DriveName = driveName;
            this.DriveFormat = driveFormat;
            ntfsUsnJournal = new NtfsUsnJournal(new DriveInfoData() { Name = this.DriveName, DriveFormat = this.DriveFormat });
        }

        public IEnumerable<FileEntry> GetSubtree( ulong rootId)
        {
            if (!files.TryGetValue(rootId, out FileEntry rootNode))
            {
                return [];
            }

            return GetSubtreeRecursive(rootNode);
        }

        private IEnumerable<FileEntry> GetSubtreeRecursive( FileEntry node)
        {
            yield return node;

            foreach (var fileEntry in files.Values)
            {
                if (fileEntry.parentReferenceNumber == node.referenceNumber)
                {
                    foreach (var descendant in GetSubtreeRecursive(fileEntry))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public void InitialUsn()
        {
            CreateJournal();
            files.Clear();
            ntfsUsnJournal.GetNtfsVolumeAllentries(DriveName, out var usnRtnCode, files);
        }

        public string GetPath(ulong uid)
        {
            return GetPath(uid, null);
        }

        string GetPath(ulong uid, string path="")
        {
            StringBuilder sb = new StringBuilder();
            ulong currentUid = uid;

            while (currentUid != ulong.MaxValue)
            {
                if (!files.TryGetValue(currentUid, out FileEntry entry))
                {
                    return string.Empty;
                }

                if (path == null)
                {
                    path = entry.fileName;
                }
                else
                {
                    sb.Insert(0, $"{entry.fileName}\\");
                }

                currentUid = entry.parentReferenceNumber;
            }

            if (path != null)
            {
                sb.Append(path);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 掩码
        /// </summary>
        private uint reasonMask = Win32Api.USN_REASON_FILE_CREATE
                                | Win32Api.USN_REASON_FILE_DELETE
                                | Win32Api.USN_REASON_RENAME_NEW_NAME
                                | Win32Api.USN_REASON_DATA_TRUNCATION
                                | Win32Api.USN_REASON_DATA_EXTEND
                                | Win32Api.USN_REASON_DATA_OVERWRITE
                                | Win32Api.USN_REASON_NAMED_DATA_EXTEND
                                | Win32Api.USN_REASON_NAMED_DATA_OVERWRITE
                                | Win32Api.USN_REASON_NAMED_DATA_TRUNCATION
                                | Win32Api.USN_REASON_OBJECT_ID_CHANGE;



        public void DoWhileFileChanges4Index(List<Win32Api.UsnEntry> usnEntries)  //筛选USN状态改变
        {
            for (int i = 0; i < usnEntries.Count; i++)
            {
                var f = usnEntries[i];
                uint value = f.Reason & Win32Api.USN_REASON_RENAME_NEW_NAME;

                if (0 != value && files.Count > 0)
                {
                    if (files.ContainsKey(f.ParentFileReferenceNumber))
                    {
                        if (files.ContainsKey(f.FileReferenceNumber))
                        {
                            FileEntry frn = files[f.FileReferenceNumber];
                            frn.fileName = f.Name;
                            frn.parentReferenceNumber = f.ParentFileReferenceNumber;
                        }
                        else
                        {
                            FileEntry frn = new FileEntry(f.FileReferenceNumber,f.ParentFileReferenceNumber,f.Name);
                            files.TryAdd(frn.referenceNumber, frn);
                        }
                    }
                     continue;
                }

                value = f.Reason & Win32Api.USN_REASON_FILE_CREATE;
                if (0 != value)
                {
                    if (!files.ContainsKey(f.FileReferenceNumber) && !string.IsNullOrWhiteSpace(f.Name) && files.ContainsKey(f.ParentFileReferenceNumber))
                    {
                        FileEntry frn = new FileEntry(f.FileReferenceNumber, f.ParentFileReferenceNumber, f.Name);
                        files.TryAdd(frn.referenceNumber, frn);
                    }
                    continue;
                }

                value = f.Reason & Win32Api.USN_REASON_FILE_DELETE;
                if (0 != value && files.Count > 0)
                {
                    //Console.WriteLine($"delete {f.FileReferenceNumber} {f.Name}");
                    if (files.ContainsKey(f.FileReferenceNumber))
                    {
                        files.TryRemove(f.FileReferenceNumber,out _);
                    }
                    continue;
                }

                //Debug.WriteLine("USN Unknown reason");
            }
        }

        readonly object _usnLock = new();
        public List<Win32Api.UsnEntry> DoWhileFileChanges()  //筛选USN状态改变
        {
            try
            {
                lock (_usnLock)
                {
                    if (usnStates.UsnJournalID != 0)
                    {
                        if (lastUsnStates.Equals(Win32Api.USN_JOURNAL_DATA.Empty))
                        {
                            lastUsnStates = usnStates;
                        }
                        _ = ntfsUsnJournal.GetUsnJournalEntries(lastUsnStates, reasonMask, out List<Win32Api.UsnEntry> usnEntries, out Win32Api.USN_JOURNAL_DATA newUsnState);
                        
                        if (SaveJournalState(newUsnState))
                        {
                            lastUsnStates = newUsnState;
                        }
                        DoWhileFileChanges4Index(usnEntries);
                        return usnEntries;
                    }
                }
            }
            catch
            {

            }
            return [];
        }

        
        private void CreateJournal()
        {
            usnStates = new Win32Api.USN_JOURNAL_DATA();
            if (!SaveCurrentJournalState())
            {
                ntfsUsnJournal.CreateUsnJournal(1000 * 1024, 16 * 1024);  //尝试重建USN
                if (SaveCurrentJournalState())
                {
                }
            }
        }

        /// <summary>
        /// 查询并跟踪USN状态，更新后保存当前状态再继续跟踪
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool SaveCurrentJournalState()        //保存USN状态
        {
            try
            {
                Win32Api.USN_JOURNAL_DATA journalState = new Win32Api.USN_JOURNAL_DATA();
                NtfsUsnJournal.UsnJournalReturnCode rtn = ntfsUsnJournal.GetUsnJournalState(ref journalState);
                if (rtn == NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                {
                    if (SaveJournalState(journalState))
                    {
                        return true;
                    }
                }
            }
            catch {}
            
            return false;
        }

        /// 查询并跟踪USN状态，更新后保存当前状态再继续跟踪
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool SaveJournalState(Win32Api.USN_JOURNAL_DATA usnState)        //保存USN状态
        {

            try
            {
                usnStates = usnState;
                return true;
            }
            catch { }

            return false;
        }

        /// 查询并跟踪USN状态，更新后保存当前状态再继续跟踪
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool LoadJournalStateFromDisk()        //保存USN状态
        {

            try
            {              
                return true;
            }
            catch { }

            return false;
        }
    }
}