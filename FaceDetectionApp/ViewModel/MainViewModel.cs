using System;
using System.Drawing;
using System.Windows.Input;
using CVCapturePanel.Service;
using Emgu.CV;
using Emgu.CV.Structure;
using NLog;

namespace CVCapturePanel.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string buttonContent = "Start";
        private Bitmap frame;
        private bool isStreaming;
        private WebCameraService webCameraService;

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
        public ICommand ToggleCameraServiceCommand { get; set; }


        public ICommand ToggleCloseAppCommand { get; set; }

        public Action CloseAction { get; set; }

        public string ButtonContent
        {
            get => buttonContent;
            set => SetField(ref buttonContent, value);
        }

        /// <summary>
        ///     Start webCam service button toggle
        /// </summary>
        public bool IsStreaming
        {
            get => isStreaming;
            set => SetField(ref isStreaming, value);
        }

        /// <summary>
        ///     Property for View Image component notification
        /// </summary>
        public Bitmap Frame
        {
            get => frame;
            set => SetField(ref frame, value);
        }

        private void InitializeWebCamService()
        {
            webCameraService = new WebCameraService();
            webCameraService.ImageChanged += OnCameraImageChanged;
        }

        private void OnCameraImageChanged(object sender, Image<Bgr, byte> image)
        {
            Frame = image.Bitmap;
        }

        /// <summary>
        ///     Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            ToggleCameraServiceCommand = new RelayCommand(ToggleCameraServiceExecute);
            ToggleCloseAppCommand = new RelayCommand(ToggleCloseApp);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void ToggleCameraServiceExecute()
        {
            if (webCameraService == null)
            {
                InitializeWebCamService();
            }

            if (!webCameraService.IsRunning)
            {
                IsStreaming = true;
                ButtonContent = "Stop";
                webCameraService.RunServiceAsync();
                logger.Info("Video streaming is started!");
            }
            else
            {
                IsStreaming = false;
                ButtonContent = "Start";
                webCameraService.CancelServiceAsync();
                ClearFrame();
                webCameraService.Dispose();
                webCameraService = null;
                logger.Info("Video streaming stopped!");
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
            webCameraService?.Dispose();
        }
    }
}