using System;
using System.Threading;
using Emgu.CV;

namespace CVCapturePanel.Service
{
    /// <summary>
    ///     Video player service for AVI and MPEG4 formats
    /// </summary>
    public class VideoPlayingService : IDisposable
    {
        public delegate void VideoFrameChanged(object sender, Mat frame);

        private const int Fps = 33; //(1000 / 30);
        private VideoCapture capture;
        private IDialogService dialog;
        private Mat frame;

        public VideoPlayingService()
        {
            frame = new Mat();
        }

        public bool IsPlaying { get; private set; }

        public void Dispose()
        {
            if (capture != null)
            {
                capture.Stop();
                capture.ImageGrabbed -= ImageGrabbed;
                capture?.Dispose();
                capture = null;
            }

            frame?.Dispose();
            frame = null;
        }

        public event VideoFrameChanged VideoFramesChangeEvent;

        public void PlayVideo(string filePath)
        {
            try
            {
                capture = new VideoCapture(filePath);
                capture.ImageGrabbed += ImageGrabbed;
                capture.Start();
                IsPlaying = true;
            }
            catch (Exception e)
            {
                dialog.ShowMessage(e.Message);
            }
        }

        public void StopPlaying()
        {
            try
            {
                Dispose();
                IsPlaying = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ImageGrabbed(object sender, EventArgs e)
        {
            if (frame == null)
            {
                frame = new Mat();
            }

            if (capture != null && capture.Ptr != IntPtr.Zero)
            {
                capture.Retrieve(frame);

                Thread.Sleep(Fps);
                VideoFramesChangeEvent?.Invoke(sender, frame);
            }
        }
    }
}