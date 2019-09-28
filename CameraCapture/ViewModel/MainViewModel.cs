﻿using CameraCaptureWPF.Helpers;
using CameraCaptureWPF.Service;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Input;

namespace CameraCaptureWPF.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IList<VideoSource> list = new List<VideoSource>();
        private VideoPlayingService videoPlayingService;
        private Bitmap frame;
        private IDialogService dialog = new DialogService();

        private bool _isStreaming;

        private ICommand toggleWebServiceCommand;
        private ICommand toogleVideoOpen;
        private ICommand toogleAppClose;
        private string videoSourceEntry;

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

        public string VideoSourceEntry
        {
            get => videoSourceEntry;
            set
            {
                if (videoSourceEntry == value) return;
                videoSourceEntry = value;
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
            get => frame;

            set => SetField(ref frame, value);
        }

        /// <summary>
        ///     Property for webCam service
        /// </summary>
        public ICommand ToggleWebServiceCommand => toggleWebServiceCommand;

        public ICommand ToogleOpenVideoCommand => toogleVideoOpen;

        public ICommand ToogleCloseAppCommand => toogleAppClose;

        private void FillComboBox()
        {
            list.Add(new VideoSource("Видео"));
            list.Add(new VideoSource("Захват камеры"));
            Video = new CollectionView(list);
        }

        private void InitializeServices()
        {
            videoPlayingService = new VideoPlayingService();
            videoPlayingService.VideoFramesChangeEvent += VideoPlayingServiceVideoFramesChangeEvent;
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
            toggleWebServiceCommand = new RelayCommand(ToggleWebServiceExecute);
            toogleVideoOpen = new RelayCommand(ToogleOpenVideo);
            toogleAppClose = new RelayCommand(ToogleCloseApp);
        }

        /// <summary>
        ///     Service From WebCamService
        /// </summary>
        private void ToggleWebServiceExecute()
        {
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