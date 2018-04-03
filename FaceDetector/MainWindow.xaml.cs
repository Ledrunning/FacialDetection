using Emgu.CV;
using Emgu.CV.Structure;
using FaceDetector.Service;
using System.IO;
using System;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using Emgu.CV.CvEnum;
using System.Windows.Controls;

namespace FaceDetector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture capture = null;
        private VideoCapture cap = null;
        private bool captureInProgress;
        private bool capturingFlag;
        private Mat frame;
        private const int FPS = 33; //(1000 / 30);

        private readonly string[] cmbItems = { "Видео", "Захват камеры" };


        public MainWindow()
        {
            InitializeComponent();
            chooseInput.ItemsSource = cmbItems;
            chooseInput.SelectedIndex = 1;
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

        private void captureButton_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null && capturingFlag)
            {
                if (captureInProgress)
                { 
                    //stop the capture
                    captureButton.Content = "Старт";
                    capture.Stop();
                }
                else
                {
                    //start the capture
                    captureButton.Content = "Стоп";

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
                    cap = new VideoCapture(openFile.FileName);
                    cap.ImageGrabbed += VideoFrames;
                    cap.Start();
                }
                catch (FileFormatException errmsg)
                {
                    MessageBox.Show("Ошибка открытия файла", errmsg.Message,
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ReleaseData();
            this.Close();
        }

        private void VideoClose_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void chooseInput_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            //string text = ((ComboBoxItem)chooseInput.SelectedItem).Content.ToString();
            if (chooseInput.SelectedItem == "Захват камеры")
            {
                capturingFlag = true;
                captureButton.Content = "Старт";
            }
            else
            {
                capturingFlag = false;
                capture.Stop();
                captureButton.Content = "Стоп";
            }
                
        }

        #region Frames events
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

        private void VideoFrames(object sender, EventArgs e)
        {
            if (cap != null && cap.Ptr != IntPtr.Zero)
            {
                cap.Retrieve(frame, 0);

                Thread.Sleep(FPS);
                //  imageBox.Source = BitmapSourceConvert.ToBitmapSource(temp as Image);
                Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(frame as IImage)));
            }
        }
        #endregion
    }
}
