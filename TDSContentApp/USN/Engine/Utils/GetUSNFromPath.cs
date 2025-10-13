using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TDSContentApp.USN.Engine.Utils
{
    internal class GetUSNFromPath
    {
        internal static ulong GetPathReferenceNumber(string path,out string err)
        {
            err= string.Empty;
            try
            {
                return GetUSNFromPath.GetPathReferenceNumber(path,5,100);
            }
            catch (Exception ex)
            {
                err=ex.Message;
                return ulong.MaxValue;
            }
        }

        private static ulong GetPathReferenceNumber(string path, int retryCount, int retryDelayMs)
        {
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                IntPtr hFile = NativeMethods.CreateFile(
                    path,
                    NativeMethods.GENERIC_READ,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE | NativeMethods.FILE_SHARE_DELETE,
                    IntPtr.Zero,
                    NativeMethods.OPEN_EXISTING,
                    NativeMethods.FILE_FLAG_BACKUP_SEMANTICS,
                    IntPtr.Zero);

                if (hFile != NativeMethods.INVALID_HANDLE_VALUE)
                {
                    try
                    {
                        if (NativeMethods.GetFileInformationByHandle(hFile, out var info))
                        {
                            return ((ulong)info.nFileIndexHigh << 32) | info.nFileIndexLow;
                        }
                        else
                        {
                            throw new System.ComponentModel.Win32Exception(
                                Marshal.GetLastWin32Error());
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseHandle(hFile);
                    }
                }
                else
                {
                    // 文件句柄创建失败，可能是文件被占用
                    if (attempt < retryCount - 1)
                    {
                        Thread.Sleep(retryDelayMs);
                    }
                    else
                    {
                        throw new System.ComponentModel.Win32Exception(
                            Marshal.GetLastWin32Error());
                    }
                }
            }

            throw new Exception("Failed to get file ID after multiple attempts.");
        }
    }

    // 定义 NativeMethods 类
    // 定义 NativeMethods 类
    static class NativeMethods
    {
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint FILE_SHARE_READ = 1;
        internal const uint FILE_SHARE_WRITE = 2;
        internal const uint FILE_SHARE_DELETE = 4;

        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000; // 添加此标志以打开目录

        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);


        // 定义 Win32 结构与 P/Invoke
        [StructLayout(LayoutKind.Sequential)]
        internal struct BY_HANDLE_FILE_INFORMATION
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint dwVolumeSerialNumber;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint nNumberOfLinks;
            public uint nFileIndexHigh;
            public uint nFileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetFileInformationByHandle(
            IntPtr hFile,
            out BY_HANDLE_FILE_INFORMATION lpFileInformation);
    }
}
