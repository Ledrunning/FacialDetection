using System.Drawing;
using Emgu.CV.Structure;

namespace CVCapturePanel.Constants
{
    public static class ScreenText
    {
        public static readonly Point SourceType = new Point(10, 80);
        public static readonly Point DateAndTime = new Point(400, 80);
        public static readonly Point FrameRate = new Point(10, 400);
        public static readonly Bgr Green = new Bgr(0, 255, 0);
    }
}