using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.ComponentModel;

namespace CameraCaptureWPF.Service
{
    /// <summary>
    ///     Class for camera call and async background works
    ///     EMGU version 3.2
    ///     Libs:
    ///     1.Emgu.CV
    ///     2.Emgu.CV.Structure
    ///     6.nvcuda.dll needed if have not Nvidia GPU on computer
    ///     All libs must to be copied into the bin folder
    /// </summary>
    public class WebCamService
    {
        public delegate void ImageChangedEventHandler(object sender, Image<Bgr, byte> image);

        private readonly VideoCapture capture;
        private BackgroundWorker webCamWorker;

        /// <summary>
        ///     Capture stream from camera
        ///     And init background workers todo crashing if camera does't exist
        /// </summary>
        public WebCamService()
        {
            try
            {
                capture = new VideoCapture();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            InitializeWorkers();
        }

        public bool IsRunning => webCamWorker?.IsBusy ?? false;

        public event ImageChangedEventHandler ImageChanged;

        /// <summary>
        ///     Async method for background work
        /// </summary>
        public void RunServiceAsync()
        {
            webCamWorker.RunWorkerAsync();
        }

        /// <summary>
        ///     Cancel Async method for background work
        /// </summary>
        public void CancelServiceAsync()
        {
            webCamWorker?.CancelAsync();
        }

        /// <summary>
        ///     Method for calling ImageChanged delegate
        /// </summary>
        /// <param name="image"></param>
        private void RaiseImageChangedEvent(Image<Bgr, byte> image)
        {
            ImageChanged?.Invoke(this, image);
        }

        /// <summary>
        ///     Method for background worker init
        /// </summary>
        private void InitializeWorkers()
        {
            webCamWorker = new BackgroundWorker();
            webCamWorker.WorkerSupportsCancellation = true;
            webCamWorker.DoWork += WbCamWorkerDoWork;
            webCamWorker.RunWorkerCompleted += WebCamWorkerCompleted;
        }

        /// <summary>
        ///     Draw image method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WbCamWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            while (!webCamWorker.CancellationPending)
                // Or _capture.Retrieve(frame, 0)
                RaiseImageChangedEvent(capture.QueryFrame().ToImage<Bgr, byte>());
        }

        private void WebCamWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}