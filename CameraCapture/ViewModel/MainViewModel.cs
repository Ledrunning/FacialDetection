using System.Drawing;
using System.Windows.Input;

namespace CameraCaptureWPF.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Bitmap _frame;

        private bool _isStreaming;


        /// <summary>
        ///     .ctor
        /// </summary>
        public MainViewModel()
        {
            InitializeServices();
            InitializeCommands();
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