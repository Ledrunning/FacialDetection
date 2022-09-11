using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Input;
using CVCapturePanel.Helpers;
using CVCapturePanel.Model;
using CVCapturePanel.Service;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CVCapturePanel.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public delegate void ImageWithDetectionChangedEventHandler(object sender, Image<Bgr, byte> image);

        private readonly IDialogService dialog = new DialogService();
        private readonly IList<VideoSource> sourceList = new List<VideoSource>();
        private string buttonContent = "Start";
        private Bitmap frame;

        private bool isStreaming;
        private string selectedVideoSource;

        private VideoPlayingService videoPlayingService;
        private WebCamService webCamService;

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

        /// <summary>
        ///     Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand { get; set; }

        public ICommand ToogleOpenVideoCommand { get; set; }

        public ICommand ToogleCloseAppCommand { get; set; }

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

        private bool IsDialogOpened { get; set; }
        private string VideoFilePath { get; set; }

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
        }

        private void VideoPlayingServiceVideoFramesChangeEvent(object sender, Mat frame)
        {
            Frame = frame.Bitmap;
        }

        /// <summary>
        ///     Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            ToggleWebServiceCommand = new RelayCommand(TogglePlayerServiceExecute);
            ToogleOpenVideoCommand = new RelayCommand(ToogleOpenVideo);
            ToogleCloseAppCommand = new RelayCommand(ToogleCloseApp);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void TogglePlayerServiceExecute()
        {
            if (SelectedVideoSource == sourceList[1].Name && !IsDialogOpened)
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
            else if (SelectedVideoSource == sourceList[0].Name && IsDialogOpened)
            {
                if (!videoPlayingService.IsPlaying)
                {
                    IsStreaming = true;
                    ButtonContent = "Stop";
                    //ToogleOpenVideo();
                    videoPlayingService.PlayVideo(VideoFilePath);
                }
                else
                {
                    IsStreaming = false;
                    ButtonContent = "Start";
                    videoPlayingService.VideoFramesChangeEvent -= VideoPlayingServiceVideoFramesChangeEvent;
                    videoPlayingService.Dispose();
                    videoPlayingService.StopPlaying();
                    IsDialogOpened = false;
                    ClearFrame();
                }
            }
        }

        private void ClearFrame()
        {
            if (Frame != null)
            {
                Frame = null;
            }
        }

        private void ToogleOpenVideo()
        {
            IsDialogOpened = dialog.OpenFileDialog();
            VideoFilePath = dialog.FilePath;
        }

        private void ToogleCloseApp()
        {
            CloseAction?.Invoke();
            if (webCamService != null)
            {
                webCamService.Dispose();
                videoPlayingService.Dispose();
            }
        }
    }
}