using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.ComponentModel;

namespace CameraCaptureWPF.Service
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
        private VideoCapture capture;
        private BackgroundWorker webCamWorker;

        public event ImageChangedEventHandler ImageChanged;

        public delegate void ImageChangedEventHandler(object sender, Image<Bgr, Byte> image);

        public bool IsRunning => webCamWorker?.IsBusy ?? false;

        /// <summary>
        /// Async method for background work
        /// </summary>
        public void RunServiceAsync()
        {
            webCamWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Cancel Async method for background work
        /// </summary>
        public void CancelServiceAsync()
        {
            webCamWorker?.CancelAsync();
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
            capture = new VideoCapture();
            InitializeWorkers();
        }

        /// <summary>
        /// Method for background worker init
        /// </summary>
        private void InitializeWorkers()
        {
            webCamWorker = new BackgroundWorker();
            webCamWorker.WorkerSupportsCancellation = true;
            webCamWorker.DoWork += WbCamWorkerDoWork;
            webCamWorker.RunWorkerCompleted += WebCamWorkerCompleted;
        }

        /// <summary>
        /// Draw image method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WbCamWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!webCamWorker.CancellationPending)
            {
                // Or _capture.Retrieve(frame, 0)
                RaiseImageChangedEvent(capture.QueryFrame().ToImage<Bgr,Byte>());
            }
        }

        private void WebCamWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}