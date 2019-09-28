using Emgu.CV;
using System;
using System.Threading;

namespace CameraCaptureWPF.Service
{
    public class VideoPlayingService : IDisposable
    {
        public delegate void VideoFrameChanged(object sender, Mat frame);

        private const int Fps = 33; //(1000 / 30);
        private readonly Mat frame;
        private VideoCapture cap;
        private IDialogService dialog;

        public VideoPlayingService()
        {
            frame = new Mat();
        }

        public void Dispose()
        {
            frame?.Dispose();
            cap?.Dispose();
            cap.ImageGrabbed -= ImageGrabbed;
        }

        public event VideoFrameChanged VideoFramesChangeEvent;

        public void PlayVideo(string filePath)
        {
            try
            {
                cap = new VideoCapture(filePath);
                cap.ImageGrabbed += ImageGrabbed;
                cap.Start();
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
            if (cap != null && cap.Ptr != IntPtr.Zero)
            {
                cap.Retrieve(frame, 0);

                Thread.Sleep(Fps);
                VideoFramesChangeEvent?.Invoke(sender, frame);
            }
        }
    }
}