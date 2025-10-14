using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TDSAot.ViewModels;
using TDSContentApp;
using TDSContentCore;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        public MessageViewModel MessageData { get; } = new MessageViewModel();

        public DataViewModel Items { get; } = new DataViewModel();

        void UpdateData(IList<IFrnFileOrigin> data, int count)
        {
            Items.Bind(data);
            Items.SetDisplayCount(count);

            if(data.FirstOrDefault() is FrnResult)
            {
                Items.IsShowResults = true;
            }
            else
            {
                Items.IsShowResults = false;
            }

            if (count <= 1)
            {
                MessageData.Message = $"{count} item";
            }
            else
            {
                MessageData.Message = $"{count} items";
            }
        }
    }
}