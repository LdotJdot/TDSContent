using ReactiveUI;
using System;

namespace TDSAot.ViewModels
{
    public class MessageViewModel : ReactiveObject
    {
        private string _msg = "Hello, TDS!";
        private string _watermark = "Hello, TDS!";
        private double _progress = 0;
        private bool _progressVisible = false;
        public MessageViewModel()
        {
            Message = $"Initialized at {DateTime.Now}";
        }



        public string Message
        {
            get => _msg;
            set => this.RaiseAndSetIfChanged(ref _msg, value);
        }

        public string Watermark
        {
            get => _watermark;
            set => this.RaiseAndSetIfChanged(ref _watermark, value);
        }
        
        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public bool ProgressVisible
        {
            get => _progressVisible;
            set => this.RaiseAndSetIfChanged(ref _progressVisible, value);
        }

        

    }
}