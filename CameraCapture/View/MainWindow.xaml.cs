using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CameraCaptureWPF.ViewModel;
using Emgu.CV;
using Microsoft.Win32;

namespace CameraCaptureWPF.View
{
    /// <summary>
    ///     Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Fps = 33; //(1000 / 30);
        private readonly Mat frame;
        private VideoCapture cap;
        private VideoCapture capture;
        private bool captureInProgress;
        private bool capturingFlag;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel(Close);
            DataContext = vm;
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
            //FreeVideoCapture();
            //this.Close();
        }

        private void VideoCloseOnClick(object sender, RoutedEventArgs e)
        {
            FreeVideoCapture();
        }

        private void InputStreamOnSelectionChanged(object sender, SelectionChangedEventArgs e)
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
                capture.Retrieve(frame);
                // Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }

        private void VideoFrames(object sender, EventArgs e)
        {
            if (cap != null && cap.Ptr != IntPtr.Zero)
            {
                cap.Retrieve(frame);

                Thread.Sleep(Fps);
                // Dispatcher.Invoke(new Action(() => imageBox.Source = BitmapSourceConvert.ToBitmapSource(_frame as IImage)));
            }
        }
    }
}