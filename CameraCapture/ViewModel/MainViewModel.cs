using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Input;
using CVCapturePanel.Service;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CVCapturePanel.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public delegate void ImageWithDetectionChangedEventHandler(object sender, Image<Bgr, byte> image);
        private string buttonContent = "Start";
        private Bitmap frame;
        private bool isStreaming;
        private WebCamService webCamService;

        /// <summary>
        ///     .ctor
        /// </summary>
        public MainViewModel(Action methodAction)
        {
            InitializeCommands();
            CloseAction = methodAction;
        }

        /// <summary>
        ///     Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand { get; set; }

        
        public ICommand ToggleCloseAppCommand { get; set; }

        public Action CloseAction { get; set; }

        public string ButtonContent
        {
            get => buttonContent;
            set
            {
                buttonContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Start webCam service button toogle
        /// </summary>
        public bool IsStreaming
        {
            get => isStreaming;
            set
            {
                isStreaming = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Property for View Image component notification
        /// </summary>
        public Bitmap Frame
        {
            get => frame;

            set
            {
                frame = value;
                OnPropertyChanged();
            }
        }
        
        public event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;

        private void InitializeWebCamService()
        {
            webCamService = new WebCamService();
            webCamService.ImageChanged += OnCameraImageChanged;
        }

        private void RaiseImageWithDetectionChangedEvent(Image<Bgr, byte> image)
        {
            ImageWithDetectionChanged?.Invoke(this, image);
        }

        private void OnCameraImageChanged(object sender, Image<Bgr, byte> image)
        {
            RaiseImageWithDetectionChangedEvent(image);
            Frame = image.Bitmap;
        }

        /// <summary>
        ///     Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            ToggleWebServiceCommand = new RelayCommand(TogglePlayerServiceExecute);
            ToggleCloseAppCommand = new RelayCommand(ToggleCloseApp);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void TogglePlayerServiceExecute()
        {
            if (webCamService == null)
            {
                InitializeWebCamService();
            }

            if (!webCamService.IsRunning)
            {
                IsStreaming = true;
                ButtonContent = "Stop";
                webCamService.RunServiceAsync();
            }
            else
            {
                IsStreaming = false;
                ButtonContent = "Start";
                webCamService.CancelServiceAsync();
                ClearFrame();
                webCamService.Dispose();
                webCamService = null;
            }
        }

        private void ClearFrame()
        {
            if (Frame != null)
            {
                Frame = null;
            }
        }
        
        private void ToggleCloseApp()
        {
            CloseAction?.Invoke();
            webCamService?.Dispose();
        }
    }
}