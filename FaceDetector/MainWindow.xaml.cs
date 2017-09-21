using Emgu.CV;
using Emgu.CV.Structure;
using FaceDetector.Service;
using System;
using System.Windows;

namespace FaceDetector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture _capture = null;
        private bool _captureInProgress;
        private Mat _frame;
        
        public MainWindow()
        {
            InitializeComponent();

            // Инит EMGUU 

            CvInvoke.UseOpenCL = false;
            try
            {
                _capture = new VideoCapture();
                // Подписываемся на событие
                _capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }

           
            _frame = new Mat();
           
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);

                //var temp = _frame.ToImage<Bgr, byte>().Bitmap;
  
                imageBox.Image = _frame;

                //  imageBox.Source = BitmapSourceConvert.ToBitmapSource(temp as Image);
            }
        }

        private void captureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_capture != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Content = "Start Capture";
                    _capture.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Content = "Stop";

                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void ReleaseData()
        {
            if (_capture != null)
                _capture.Dispose();
        }

     
    }
}
