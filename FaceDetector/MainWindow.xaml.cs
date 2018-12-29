using Emgu.CV;
using FaceDetector.Service;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace FaceDetector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture _capture;
        private VideoCapture _cap;
        private bool _captureInProgress;
        private bool _capturingFlag;
        private readonly Mat _frame;
        private const int Fps = 33; //(1000 / 30);
        private CascadeClassifier _haar;

        private readonly string[] _cmbItems = { "Видео", "Захват камеры" };

        public MainWindow()
        {
            InitializeComponent();
            chooseInput.ItemsSource = _cmbItems;
            chooseInput.SelectedIndex = 1;

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

        private void captureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_capture != null && _capturingFlag)
            {
                if (_captureInProgress)
                {
                    //stop the capture
                    captureButton.Content = "Старт";
                    _capture.Stop();
                }
                else
                {
                    //start the capture
                    captureButton.Content = "Стоп";

                    _capture.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void FreeVideoCapture()
        {
            _capture?.Dispose();
        }

        private void VideoOpen_Click(object sender, RoutedEventArgs e)
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
                    _cap = new VideoCapture(openFile.FileName);
                    _cap.ImageGrabbed += VideoFrames;
                    _cap.Start();
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
            FreeVideoCapture();
            this.Close();
        }

        private void VideoClose_Click(object sender, RoutedEventArgs e)
        {
            FreeVideoCapture();
        }

        private void chooseInput_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((string)chooseInput.SelectedItem == _cmbItems[0])
            {
                _capturingFlag = true;
                captureButton.Content = "Старт";
            }
            else
            {
                _capturingFlag = false;
                _capture.Stop();
                captureButton.Content = "Стоп";
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);
                Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }

        private void VideoFrames(object sender, EventArgs e)
        {
            if (_cap != null && _cap.Ptr != IntPtr.Zero)
            {
                _cap.Retrieve(_frame, 0);

                Thread.Sleep(Fps);
                Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }
    }
}