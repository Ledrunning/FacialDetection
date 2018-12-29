using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.ComponentModel;

namespace FaceDetector.Service
{
    /// <summary>
    /// Class for camera call and async background works
    /// EMGU version 3.2
    /// Libs:
    /// 1.Emgu.CV
    /// 2.Emgu.CV.Structure
    /// 6.nvcuda.dll needed if have not Nvidia GPU on computer
    /// All libs must to be copied into the bin folder
    /// </summary>
    public class WebCamService
    {
        private VideoCapture _capture;
        private BackgroundWorker _webCamWorker;

        public event ImageChangedEventHndler ImageChanged;

        public delegate void ImageChangedEventHndler(object sender, Image<Bgr, Byte> image);

        public bool IsRunning => _webCamWorker?.IsBusy ?? false;

        /// <summary>
        /// Async method for background work
        /// </summary>
        public void RunServiceAsync()
        {
            _webCamWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Cancel Async method for background work
        /// </summary>
        public void CancelServiceAsync()
        {
            _webCamWorker?.CancelAsync();
        }

        /// <summary>
        /// Method for calling ImageChanged delegate
        /// </summary>
        /// <param name="image"></param>
        private void RaiseImageChangedEvent(Image<Bgr, Byte> image)
        {
            ImageChanged?.Invoke(this, image);
        }

        /// <summary>
        /// Capture stream from camera
        /// And init background workers
        /// </summary>
        public WebCamService()
        {
            _capture = new VideoCapture();
            InitializeWorkers();
        }

        /// <summary>
        /// Method for background worker init
        /// </summary>
        private void InitializeWorkers()
        {
            _webCamWorker = new BackgroundWorker();
            _webCamWorker.WorkerSupportsCancellation = true;
            _webCamWorker.DoWork += _webCamWorker_DoWork;
            _webCamWorker.RunWorkerCompleted += _webCamWorker_RunWorkerCompleted;
        }

        /// <summary>
        /// Draw image method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _webCamWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_webCamWorker.CancellationPending)
            {
                // Or _capture.Retrieve(frame, 0)
                RaiseImageChangedEvent(_capture.QueryFrame().ToImage<Bgr,Byte>());
            }
        }

        private void _webCamWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}