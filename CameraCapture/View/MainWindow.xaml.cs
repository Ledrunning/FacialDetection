using System;
using System.IO;
using System.Threading;
using System.Windows;
using Emgu.CV;
using Microsoft.Win32;

namespace CameraCaptureWPF.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture capture;
        private VideoCapture cap;
        private bool captureInProgress;
        private bool capturingFlag;
        private readonly Mat frame;
        private const int Fps = 33; //(1000 / 30);

        public MainWindow()
        {
            InitializeComponent();
            //chooseInput.ItemsSource = _cmbItems;
            //chooseInput.SelectedIndex = 1;

            // Инит EMGUU
            //CvInvoke.UseOpenCL = false;
            //try
            //{
            //    _capture = new VideoCapture();
            //    // Подписываемся на событие
            //    _capture.ImageGrabbed += ProcessFrame;
            //}
            //catch (NullReferenceException excpt)
            //{
            //    MessageBox.Show(excpt.Message);
            //}

            //_frame = new Mat();
        }

        private void CaptureButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (capture != null && capturingFlag)
            {
                if (captureInProgress)
                {
                    //stop the capture
                    //captureButton.Content = "Старт";
                    capture.Stop();
                }
                else
                {
                    //start the capture
                    //captureButton.Content = "Стоп";

                    capture.Start();
                }

                captureInProgress = !captureInProgress;
            }
        }

        private void FreeVideoCapture()
        {
            capture?.Dispose();
        }

        private void VideoOpenOnClick(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void OpenFile()
        {
            var openFile = new OpenFileDialog();
            openFile.InitialDirectory = "c:\\";
            openFile.Filter = "Video Files |*.mp4";
            openFile.RestoreDirectory = true;

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

        private void MenuItemOnClick(object sender, RoutedEventArgs e)
        {
            FreeVideoCapture();
            this.Close();
        }

        private void VideoCloseOnClick(object sender, RoutedEventArgs e)
        {
            FreeVideoCapture();
        }

        private void InputStreamOnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if ((string)chooseInput.SelectedItem == _cmbItems[0])
            //{
            //    _capturingFlag = true;
            //    captureButton.Content = "Старт";
            //}
            //else
            //{
            //    _capturingFlag = false;
            //    _capture.Stop();
            //    captureButton.Content = "Стоп";
            //}
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame, 0);
               // Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }

        private void VideoFrames(object sender, EventArgs e)
        {
            if (cap != null && cap.Ptr != IntPtr.Zero)
            {
                cap.Retrieve(frame, 0);

                Thread.Sleep(Fps);
               // Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }
    }
}