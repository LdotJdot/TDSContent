using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DocumentFormat.OpenXml.Vml;
using System;
using System.Collections.Generic;
using System.IO;
using TDSContentApp;
using TDSContentCore;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        public void BindIndexProjects()
        {
            var list=new List<IFrnFileOrigin>();

            foreach(var project in TDSContentApplication.Instance.Projects?.Info ?? [])
            {
                foreach(var info in project.projects)
                {
                    list.Add(new IndexProjects(info.refNumber, project.driveName, info.ext, info.category));
                }
            }
                        
            UpdateData(list, list.Count);
            Dispatcher.UIThread.Invoke(scrollViewer.ScrollToHome);
        }

        class IndexProjects : IFrnFileOrigin
        {
            ulong fileReferenceNumber;
            string driverName;
            string[] ext;
            string category;
            public IndexProjects(ulong fileReferenceNumber, string driverName, string[] ext,string category)
            {
                this.fileReferenceNumber = fileReferenceNumber;
                this.driverName = driverName;
                this.ext = ext??[];
                this.category = category;
            }

            public string FileName=> category;

            public string FilePath => TDSContentApplication.Instance.GetPath(driverName, fileReferenceNumber);

            public Bitmap? icon=> FileIconService.GetIcon(FilePath);

            public string FileInfo => string.Join(',',ext);

            public ReadOnlySpan<char> Details => ReadOnlySpan<char>.Empty;
        }
    }
}