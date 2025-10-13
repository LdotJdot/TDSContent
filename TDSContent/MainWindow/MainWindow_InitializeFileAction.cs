using Avalonia.Controls;
using System.Collections.Generic;
using TDSAot.Utils;
using TDSContentCore;

namespace TDSAot
{
    public partial class MainWindow : Window
    {
        private FileAction fileAction;

        private void InitializeFileAction()
        {
            fileAction = new FileAction();
        }

        private void Execute(IFrnFileOrigin[] file, FileActionType action)
        {
            if (file == null || file.Length == 0) { return; }

            fileAction.Execute(file, action);
        }
    }
}