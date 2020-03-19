using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Input;
using CameraCaptureWPF.Helpers;
using CameraCaptureWPF.Service;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CameraCaptureWPF.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public delegate void ImageWithDetectionChangedEventHandler(object sender, Image<Bgr, byte> image);

        private readonly IDialogService dialog = new DialogService();
                private readonly IList<VideoSource> sourceList = new List<VideoSource>();
        private readonly WebCamService webCamService = new WebCamService();
        private string buttonContent = "Start";
        private Bitmap frame;

        private bool isStreaming;
        private string selectedVideoSource;

        private VideoPlayingService videoPlayingService;

        /// <summary>
        ///     .ctor
        /// </summary>
        public MainViewModel(Action methodAction)
        {
            InitializeServices();
            InitializeCommands();
            FillComboBox();
            CloseAction = methodAction;
        }

        public CollectionView Video { get; private set; }
        public Action CloseAction { get; set; }
        public string OpenSource { get; } = "Open video";
        public string CloseSource { get; } = "Close video";
        public string Exit { get; } = "Exit";

        public string SelectSource { get; set; } = "Select source";

        public string ButtonContent
        {
            get => buttonContent;
            set
            {
                buttonContent = value;
                OnPropertyChanged();
            }
        }

        public string SelectedVideoSource
        {
            get => selectedVideoSource;
            set
            {
                if (selectedVideoSource == value)
                {
                    return;
                }

                selectedVideoSource = value;
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

            set => SetField(ref frame, value);
        }

        /// <summary>
        ///     Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand { get; private set; }

        public ICommand ToogleOpenVideoCommand { get; private set; }

        public ICommand ToogleCloseAppCommand { get; private set; }

        public event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;

        private void InitializeWebCamService()
        {
            webCamService.ImageChanged += WebCamServiceOnImageChanged;
        }

        private void RaiseImageWithDetectionChangedEvent(Image<Bgr, byte> image)
        {
            ImageWithDetectionChanged?.Invoke(this, image);
        }

        private void WebCamServiceOnImageChanged(object sender, Image<Bgr, byte> image)
        {
            //RaiseImageWithDetectionChangedEvent(image);
            Frame = image.Bitmap;
        }

        private void FillComboBox()
        {
            sourceList.Add(new VideoSource("Video"));
            sourceList.Add(new VideoSource("Camera capture"));
            Video = new CollectionView(sourceList);
        }

        private void InitializeServices()
        {
            videoPlayingService = new VideoPlayingService();
            videoPlayingService.VideoFramesChangeEvent += VideoPlayingServiceVideoFramesChangeEvent;
            InitializeWebCamService();
        }

        private void VideoPlayingServiceVideoFramesChangeEvent(object sender, Mat frame)
        {
            videoPlayingService.PlayVideo(dialog.FilePath);
        }

        private void OnImageDetectionChanged(object sender, Image<Bgr, byte> image)
        {
            Frame = image.Bitmap;
        }

        /// <summary>
        ///     Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            ToggleWebServiceCommand = new RelayCommand(ToggleWebServiceExecute);
            ToogleOpenVideoCommand = new RelayCommand(ToogleOpenVideo);
            ToogleCloseAppCommand = new RelayCommand(ToogleCloseApp);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void ToggleWebServiceExecute()
        {
            if (SelectedVideoSource == sourceList[1].Name)
            {
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
                }
            }
        }

        private void ToogleCloseApp()
        {
            CloseAction?.Invoke();
            if (webCamService != null)
            {
                webCamService.Dispose();
            }
        }

        private void ToogleOpenVideo()
        {
            if (dialog.OpenFileDialog())
            {

            }
        }
    }
}