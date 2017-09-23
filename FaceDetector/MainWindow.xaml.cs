using Emgu.CV;
using Emgu.CV.Structure;
using FaceDetector.Service;
using System.IO;
using System;
using System.Windows;
using Microsoft.Win32;

namespace FaceDetector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture capture = null;
        private bool captureInProgress;
        private Mat frame;
        
        public MainWindow()
        {
            InitializeComponent();

            // Инит EMGUU 

            CvInvoke.UseOpenCL = false;
            try
            {
                capture = new VideoCapture();
                // Подписываемся на событие
                capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }

           
            frame = new Mat();
           
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame, 0);

                //var temp = frame.ToImage<Bgr, byte>().Bitmap;

                //imageBox.Image = frame;

                Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(frame as IImage)));
                
            }
        }

        private void captureButton_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                if (captureInProgress)
                {  //stop the capture
                    captureButton.Content = "Start Capture";
                    capture.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Content = "Stop";

                    capture.Start();
                }

                captureInProgress = !captureInProgress;
            }
        }

        private void ReleaseData()
        {
            if (capture != null)
                capture.Dispose();
        }

              
        private void VideoOpen_Click(object sender, RoutedEventArgs e)
        { 
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = "c:\\";
            openFile.Filter = "Video Files |*.mp4";
            openFile.RestoreDirectory = true;

            //if (capture == null)
            //{


            if (openFile.ShowDialog() == true)
            {

                try
                {
                    capture = new VideoCapture(openFile.FileName);
                    capture.ImageGrabbed += VideoFrames;
                    capture.Start();
                }
                catch (FileFormatException errmsg)
                {
                    MessageBox.Show("Ошибка открытия файла", errmsg.Message,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void VideoFrames(object sender, EventArgs e)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame, 0);

                //var temp = _frame.ToImage<Bgr, byte>().Bitmap;

                //imageBox.Image = frame;

                //  imageBox.Source = BitmapSourceConvert.ToBitmapSource(temp as Image);
                Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(frame as IImage)));
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            capture.Dispose();
            this.Close();
        }
    }
}
