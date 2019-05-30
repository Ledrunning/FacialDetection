using System;
using System.Threading;
using Emgu.CV;

namespace CameraCaptureWPF.Service
{
    public class VideoPlayingService : IDisposable
    {
        public delegate void VideoFrameChanged(object sender, Mat frame);

        private const int Fps = 33; //(1000 / 30);
        private readonly Mat _frame;
        private VideoCapture _cap;
        private IDialogService dialog;

        public VideoPlayingService()
        {
            _frame = new Mat();
        }

        public void Dispose()
        {
            _frame?.Dispose();
            _cap?.Dispose();
            _cap.ImageGrabbed -= ImageGrabbed;
        }

        public event VideoFrameChanged VideoFramesChangeEvent;

        public void PlayVideo(string filePath)
        {
            try
            {
                _cap = new VideoCapture(filePath);
                _cap.ImageGrabbed += ImageGrabbed;
                _cap.Start();
            }
            catch (Exception errmsg)
            {
                dialog.ShowMessage(errmsg.Message);
            }
        }

        public void StopPlaying()
        {
            try
            {
                Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ImageGrabbed(object sender, EventArgs e)
        {
            if (_cap != null && _cap.Ptr != IntPtr.Zero)
            {
                _cap.Retrieve(_frame, 0);

                Thread.Sleep(Fps);
                VideoFramesChangeEvent?.Invoke(sender, _frame);
            }
        }
    }
}