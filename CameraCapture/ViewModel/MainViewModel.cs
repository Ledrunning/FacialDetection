﻿using System.Collections.Generic;
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
        private readonly IList<VideoSource> list = new List<VideoSource>();
        private FaceDetection _faceDetectionService;
        private Bitmap _frame;

        private bool _isStreaming;

        private ICommand _toggleWebServiceCommand;
        private string _videoSourceEntry;

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

        public string OpenSource { get; } = "Открыть видео";
        public string CloseSource { get; } = "Закрыть видио";
        public string Exit { get; } = "Выход";

        public string VideoSourceEntry
        {
            get => _videoSourceEntry;
            set
            {
                if (_videoSourceEntry == value) return;
                _videoSourceEntry = value;
                OnPropertyChanged("VideoSourceEntry");
            }
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

        private void FillComboBox()
        {
            list.Add(new VideoSource("Видео"));
            list.Add(new VideoSource("Захват камеры"));
            Video = new CollectionView(list);
        }

        private void InitializeServices()
        {
            _faceDetectionService = new FaceDetection(true);
            _faceDetectionService.ImageDetectionChanged += OnImageDetectionChanged;
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
            _toggleWebServiceCommand = new RelayCommand(ToggleWebServiceExecute);
            //_togglePhotoShootServiceCommand = new RelayCommand(TogglePhotoShootServiceExecute);
            //_toogleHelpCallCommand = new RelayCommand(ToogleHelpServiceExecute);
        }

        /// <summary>
        ///     Service From WebCamService
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