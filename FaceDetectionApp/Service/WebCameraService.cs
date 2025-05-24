using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using CVCapturePanel.Constants;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using NLog;

namespace CVCapturePanel.Service
{
    /// <summary>
    ///     Class for camera call and async background works
    ///     EMGU version 3.2 -> has been ported to CV 4.1.1 version. 
	///     current EMGU version is CV 4.1.1
    ///     Libs:
    ///     1.Emgu.CV
    ///     2.Emgu.CV.Structure
    ///     6.nvcuda.dll is required if your computer does not have any Nvidia GPUs on it
    ///     All libraries must be copied to the bin folder
    /// </summary>
    public class WebCameraService : IDisposable
    {
        public delegate void ImageChangedEventHandler(object sender, Image<Bgr, byte> image);

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private VideoCapture capture;

        private CascadeClassifier cascadeClassifier;
        private BackgroundWorker webCamWorker;

        /// <summary>
        ///     Capture stream from camera
        ///     And init background workers todo crashing if camera doesn't exist
        /// </summary>
        public WebCameraService()
        {
            InitializeWorkers();
            InitializeClassifier();
        }

        private VideoCapture Capture
        {
            set => capture = value;
            get
            {
                try
                {
                    if (capture == null)
                    {
                        capture = new VideoCapture();

                        if (!capture.IsOpened)
                        {
                            capture.Start();
                        }
                        else
                        {
                            capture.Stop();
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Video capture failed! {e}", e);
                }

                return capture;
            }
        }

        /// <summary>
        ///     Flag when service is running
        /// </summary>
        public bool IsRunning => webCamWorker?.IsBusy ?? false;

        public void Dispose()
        {
            webCamWorker.DoWork -= OnWebCameraDoWork;
            webCamWorker.CancelAsync();
            Capture?.Dispose();
            Capture = null;
            webCamWorker?.Dispose();
        }

        private void InitializeClassifier()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = Path.GetDirectoryName(assembly.Location);

            if (path != null)
            {
                cascadeClassifier = new CascadeClassifier(Path.Combine(path, "haarcascade_frontalface_default.xml"));
            }
            else
            {
                logger.Error("Could not find haarcascade xml file");
            }
        }

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
            webCamWorker.DoWork += OnWebCameraDoWork;
        }

        /// <summary>
        ///     Draw image method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWebCameraDoWork(object sender, DoWorkEventArgs e)
        {
            if (webCamWorker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            while (!webCamWorker.CancellationPending)
                // Or _capture.Retrieve(frame, 0)
            {
                var frameRate = Capture.GetCaptureProperty(CapProp.Fps); //get fps
                var sourceType = Capture.CaptureSource;
                var image = Capture.QueryFrame().ToImage<Bgr, byte>();

                DetectFaces(image);

                SetBackgroundText(image,
                    $"Source: {sourceType}",
                    ScreenText.SourceType,
                    ScreenText.Green);

                SetBackgroundText(image,
                    $"{DateTime.Now}",
                    ScreenText.DateAndTime,
                    ScreenText.Green, 0.5);

                SetBackgroundText(image,
                    $"{frameRate} fps",
                    ScreenText.FrameRate,
                    ScreenText.Green);

                RaiseImageChangedEvent(image);
            }
        }

        private void DetectFaces(Image<Bgr, byte> image)
        {
            var grayFrame = image.Convert<Gray, byte>();
            var faces = cascadeClassifier.DetectMultiScale(grayFrame,
                FaceDetectionConstants.ScaleFactors,
                FaceDetectionConstants.MinNeighbor,
                Size.Empty); //the actual face detection happens here
            foreach (var face in faces)
            {
                image.Draw(face, new Bgr(FaceDetectionConstants.RectangleColor),
                    FaceDetectionConstants
                        .RectangleThickness); //the detected face(s) is highlighted here using a box that is drawn around it/them
            }
        }

        private static void SetBackgroundText(IInputOutputArray image, string text, Point point, Bgr textColor,
            double fontScale = 1.0)
        {
            CvInvoke.PutText(
                image,
                text,
                point,
                FontFace.HersheyComplex,
                fontScale,
                textColor.MCvScalar);
        }
    }
}