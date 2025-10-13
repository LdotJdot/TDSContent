using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TDSContentCore;


namespace TDSAot.Utils
{
    internal enum FileActionType
    {
        Open,
        OpenFolder,
        Delete,
        CopyFile,
        CopyPath
    }

    internal class FileAction
    {
        private Action<IFrnFileOrigin?>? updateRecord;

        public FileAction(Action<IFrnFileOrigin?>? updateRecord = null)
        {
            this.updateRecord = updateRecord;
        }

        internal void Execute(IFrnFileOrigin[] files, FileActionType action)
        {
            switch (action)
            {
                case FileActionType.Open:
                    foreach(var file in files)
                    {
                        Open(file);
                        updateRecord?.Invoke(file);
                    }
                    return;

                case FileActionType.OpenFolder:
                    foreach (var file in files)
                    {
                        OpenFolder(file);
                        updateRecord?.Invoke(file);
                    }
                    return;

                case FileActionType.Delete:
                    Delete(files);
                    return;

                default:
                    return;
            }
        }

        private void Delete(IFrnFileOrigin[] files)
        {
            if (files != null && files.Length > 0)
            {
                var pathes = files.Select(o=>o.FilePath);
                if (Message.ShowYesNo("Delete?", string.Join("\r\n", pathes)))
                {
                    foreach (var path in pathes)
                    {
                        try
                        {
                            FileAttributes attr = File.GetAttributes(path);
                            if (attr == FileAttributes.Directory)
                            {
                                Directory.Delete(path, true);
                            }
                            else
                            {
                                File.Delete(path);
                            }
                        }
                        catch (Exception ex)
                        {
                            Message.ShowWaringOk("Delete failed", ex.Message);
                        }
                    }
                }
            }
        }

        private void OpenFolder(IFrnFileOrigin file)
        {
            if (file != null)
            {
                string path = string.Empty;
                try
                {
                    path = Path.GetDirectoryName(file.FilePath);

                    ExplorerFile(file.FilePath);
                    updateRecord?.Invoke(file);
                }
                catch (Exception ex)
                {
                    Message.ShowWaringOk("Open failed", ex.Message);
                }
            }
        }

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);

        private static void ExplorerFile(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                return;

            if (Directory.Exists(filePath))
                Process.Start(@"explorer.exe", "/select,\"" + filePath + "\"");
            else
            {
                IntPtr pidlList = ILCreateFromPathW(filePath);
                if (pidlList != IntPtr.Zero)
                {
                    try
                    {
                        Marshal.ThrowExceptionForHR(SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
                    }
                    finally
                    {
                        ILFree(pidlList);
                    }
                }
            }
        }

        private static void Open(IFrnFileOrigin file)
        {
            if (!(file == null))
            {
                string path = string.Empty;
                //识别
                try
                {
                    path = file.FilePath;
                }
                catch (Exception ex)
                {
                    Message.ShowWaringOk("Open failed", ex.Message);
                }

                var ext = Path.GetExtension(file.FileName);
                if (ext.Length == 0)
                {
                    if (Directory.Exists(path))
                    {
                        try
                        {
                            Process.Start("explorer.exe", path);
                        }
                        catch (Exception ex)
                        {
                            Message.ShowWaringOk("Open failed", ex.Message);
                        }
                    }
                    else
                    {
                        Message.ShowWaringOk("Open failed", "Path not existed.");
                    }
                }
                else
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            Process p = new System.Diagnostics.Process();
                            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.FileName = path;
                            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                            p.Start();
                        }
                        catch (Exception ex)
                        {
                            Message.ShowWaringOk("Open failed", ex.Message);
                        }
                    }
                    else
                    {
                        Message.ShowWaringOk("Open failed", "Path not existed.");
                    }
                }
            }
        }
    }

    public class ShellFileOpener
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            int nShowCmd);

        private const int SW_SHOWNORMAL = 1;

        public static void OpenFileWithShell(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                throw new FileNotFoundException("Path not existed.", filePath);
            }

            // 使用ShellExecute打开文件
            ShellExecute(IntPtr.Zero, "open", filePath, null, null, SW_SHOWNORMAL);
        }
    }
}