using System.Collections.Generic;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;

namespace CameraCaptureWPF.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Bitmap _frame;

        private bool _isStreaming;

        private CollectionView _videoSource;
        private string __videoSourceEntry;

        IList<VideoSource> list = new List<VideoSource>();

        private void FillComboBox()
        {
            list.Add(new VideoSource("Видео"));
            list.Add(new VideoSource("Захват камеры"));
            _videoSource = new CollectionView(list);
        }

        public CollectionView Video
        {
            get { return _videoSource; }
        }

        public string OpenSource { get; } = "Открыть видео";
        public string CloseSource { get; } = "Закрыть видио";
        public string Exit { get; } = "Выход";

        public string VideoSourceEntry
        {
            get { return __videoSourceEntry; }
            set
            {
                if (__videoSourceEntry == value) return;
                __videoSourceEntry = value;
                OnPropertyChanged("VideoSourceEntry");
            }
        }

        /// <summary>
        ///     .ctor
        /// </summary>
        public MainViewModel()
        {
            InitializeServices();
            InitializeCommands();
            FillComboBox();
        }

        /// <summary>
        ///     Start webCam service button toogle
        /// </summary>
        public bool IsStreaming
        {
            get => _isStreaming;
            set
            {
                _isStreaming = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Property for View Image component notification
        /// </summary>
        public Bitmap Frame
        {
            get => _frame;

            set => SetField(ref _frame, value);
        }

        /// <summary>
        ///     Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand { get; }

        private void InitializeServices()
        {
            //_faceDetectionService = new FaceDetectionService();
            //_photoShootService = new PhotoShootService();
            //_faceDetectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;
        }

        /// <summary>
        ///     Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            //_toggleWebServiceCommand = new RelayCommand<>(ToggleWebServiceExecute);
            //_togglePhotoShootServiceCommand = new RelayCommand(TogglePhotoShootServiceExecute);
            //_toogleHelpCallCommand = new RelayCommand(ToogleHelpServiceExecute);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void ToggleWebServiceExecute()
        {
            //if (!_faceDetectionService.IsRunning)
            //{
            //    IsStreaming = true;
            //    _faceDetectionService.RunServiceAsync();
            //}
            //else
            //{
            //    IsStreaming = false;
            //    _faceDetectionService.CancelServiceAsync();
            //}
        }
    }
}