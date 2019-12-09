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

        private readonly IList<VideoSource> list = new List<VideoSource>();
        private readonly IDialogService dialog = new DialogService();
        private Bitmap frame;

        private bool isStreaming;

        private VideoPlayingService videoPlayingService;
        private string videoSourceEntry;
        private readonly WebCamService webCamService = new WebCamService();

        /// <summary>
        ///     .ctor
        /// </summary>
        public MainViewModel()
        {
            InitializeServices();
            InitializeCommands();
            FillComboBox();
        }

        public CollectionView Video { get; private set; }

        public string OpenSource { get; } = "Open video";
        public string CloseSource { get; } = "Close video";
        public string Exit { get; } = "Exit";

        public string SelectSource { get; set; } = "Select source";

        public string ButtonContent { get; set; } = "Start";

        public string VideoSourceEntry
        {
            get => videoSourceEntry;
            set
            {
                if (videoSourceEntry == value) return;
                videoSourceEntry = value;
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
            ;
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
            list.Add(new VideoSource("Video"));
            list.Add(new VideoSource("Camera capture"));
            Video = new CollectionView(list);
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
            if (!webCamService.IsRunning)
            {
                IsStreaming = true;
                ButtonContent = "Start";
                webCamService.RunServiceAsync();
            }
            else
            {
                IsStreaming = false;
                ButtonContent = "Stop";
                webCamService.CancelServiceAsync();
            }
        }

        private void ToogleCloseApp()
        {
        }

        private void ToogleOpenVideo()
        {
            if (dialog.OpenFileDialog())
            {
            }
        }
    }
}