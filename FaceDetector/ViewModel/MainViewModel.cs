using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows;
using System.Windows.Threading;
using System.Media;
using System.Diagnostics;
namespace FaceDetector.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Bitmap _frame;

        private bool _isStreaming;
        /// <summary>
        /// Start webCam service button toogle
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
        /// Property for View Image component notification
        /// </summary>
        public Bitmap Frame
        {
            get => _frame;

            set => SetField(ref _frame, value);
        }

        private ICommand _toggleWebServiceCommand;
        /// <summary>
        /// Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand
        {
            get => _toggleWebServiceCommand;
        }


        /// <summary>
        /// .ctor
        /// </summary>
        public MainWindowViewModel()
        {
            InitializeServices();
            InitializeCommands();
        }

        private void InitializeServices()
        {
            //_faceDetectionService = new FaceDetectionService();
            //_photoShootService = new PhotoShootService();
            //_faceDetectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;
        }

        /// <summary>
        /// Initialize all commands from main view model
        /// </summary>
        private void InitializeCommands()
        {
            _toggleWebServiceCommand = new RelayCommand(ToggleWebServiceExecute);
            _togglePhotoShootServiceCommand = new RelayCommand(TogglePhotoShootServiceExecute);
            _toogleHelpCallCommand = new RelayCommand(ToogleHelpServiceExecute);
        }

        /// <summary>
        /// Service From WebCamService 
        /// </summary>
        private void ToggleWebServiceExecute()
        {

            if (!_faceDetectionService.IsRunning)
            {
                IsStreaming = true;
                _faceDetectionService.RunServiceAsync();
            }
            else
            {
                IsStreaming = false;
                _faceDetectionService.CancelServiceAsync();
            }
        }


    }
}
