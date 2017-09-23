using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Drawing;
using aSee;
using aSee.Devices;

namespace FaceDetector
{
    class BlinkDetection
    {
        #region Varibles

        public static int totalBlinks = 0;
        
        #endregion


        #region Constructor

        public BlinkDetection()
        {
        }

        #endregion

        #region public methods

        public bool detectShook(Image<Gray, byte> img, Image<Gray, byte> prev)
        {
            // if the ROI of current image is setted up, then save the ROI and reset it
            Rectangle imgROI = new Rectangle(0, 0, 0, 0);
            if (img.IsROISet)
            {
                imgROI = img.ROI;
                CvInvoke.cvResetImageROI(img);
            }

            // if the ROI of previous image is setted up, then save the ROI and reset it
            Rectangle prevROI = new Rectangle(0, 0, 0, 0);
            if (prev.IsROISet)
            {
                prevROI = prev.ROI;
                CvInvoke.cvResetImageROI(prev);
            }


            Image<Gray, byte> diff = new Image<Gray, byte>(img.Width, img.Height);
            CvInvoke.cvSub(img, prev, diff, IntPtr.Zero);           

            // if the ROI of current iamge and previous image are resetted up, then set them again to keep unchangged by the method
            if (img.IsROISet)
            {
                CvInvoke.cvSetImageROI(img, imgROI);
            }
            if (prev.IsROISet)
            {
                CvInvoke.cvSetImageROI(prev, prevROI);
            }

            CvInvoke.cvThreshold(diff, diff, 50, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);

            StructuringElementEx element = new StructuringElementEx(5, 5, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);          
            //CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);
            CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 7);


            IntPtr comp = new IntPtr();    
            MemStorage storage = new MemStorage();
            int nc = CvInvoke.cvFindContours(diff, storage, ref comp, StructSize.MCvContour,
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // when the numbers of connection regions are two huge, then a shaking is detected
            if (nc > 8)
            {
                return true;
            }

            // also, when one of the connected regions has too big width or too big height, then a shaking is detected
            Seq<Point> compSeq = new Seq<Point>(comp, null);
            while (compSeq != null && comp.ToInt32() != 0)
            {
                Rectangle r = CvInvoke.cvBoundingRect(compSeq, 1);

                if (r.Width >= 200 || r.Height >= 120 || Math.Abs(r.Width - r.Height) >= 100)
                {
                    return true;
                }

                compSeq = compSeq.HNext;
            }
            
            return false;
        }

        public bool detectOverall(Image<Gray, byte> img, Image<Gray, byte> prev)
        {
            Rectangle imgROI = new Rectangle(0, 0, 0, 0);
            if (img.IsROISet)
            {
                imgROI = img.ROI;
                CvInvoke.cvResetImageROI(img);
            }

            Rectangle prevROI = new Rectangle(0, 0, 0, 0);
            if (prev.IsROISet)
            {
                prevROI = prev.ROI;
                CvInvoke.cvResetImageROI(prev);
            }

            // step 1: compute diff = img - prev
            Image<Gray, byte> diff = new Image<Gray, byte>(img.Width, img.Height);
            CvInvoke.cvSub(img, prev, diff, IntPtr.Zero);

            if (img.IsROISet)
            {
                CvInvoke.cvSetImageROI(img, imgROI);
            }
            if (prev.IsROISet)
            {
                CvInvoke.cvSetImageROI(prev, prevROI);
            }

            // step 2: threshold
            CvInvoke.cvThreshold(diff, diff, 100, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);

            // step 3: do close and open operations
            StructuringElementEx element = new StructuringElementEx(5, 5, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);
            CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 3);

            // step 4: compute connection regions
            IntPtr comp = new IntPtr();    
            MemStorage storage = new MemStorage();
            int nc = CvInvoke.cvFindContours(diff, storage, ref comp, StructSize.MCvContour,
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // only when the number of connection regions equals 2, a blink is detected.
            if (nc != 2)
            {
                return false;
            }

            Seq<Point> compSeq = new Seq<Point>(comp, null);
            Rectangle r1 = CvInvoke.cvBoundingRect(compSeq, 1);
            Rectangle r2 = CvInvoke.cvBoundingRect(compSeq.HNext, 1);

            // Eliminate interference
            if (Math.Abs(r1.Y - r2.Y) > 20 || Math.Abs(r1.Width - r2.Width) > 20 || Math.Abs(r1.Height - r2.Height) > 20)
            {
                return false;
            }

            // Eliminate the possibility of noses pair
            if (Math.Abs(r1.X - r2.X) < 200)
            {
                return false;
            }

            return true;
        }

        public bool detectAlone(Image<Gray, byte> img, Image<Gray, byte> prev, Rectangle eyeROI, Rectangle prevEyeROI, ref Boolean isShook)
        {
            if (eyeROI.Width == 0 || prevEyeROI.Width == 0)
            {
                return false;
            }

            // when the difference between two eyeROI is huge, then it is probably caused by shaking
            int axisGap = 10;
            if (System.Math.Abs(eyeROI.X - prevEyeROI.X) > axisGap || System.Math.Abs(eyeROI.Y - prevEyeROI.Y) > axisGap)
            {
                isShook = true;
                return false;
            }

            int widthGap = 10;
            if (System.Math.Abs(eyeROI.Width - prevEyeROI.Height) > widthGap)
            {
                isShook = true;
                return false;
            }

            // Enlarge the ROI to make the algorithm more stable
            float widthFactor = 1.2F;
            int newX = (int)(eyeROI.X - (widthFactor - 1) * eyeROI.Width / 2);
            int newY = (int)(eyeROI.Y - (widthFactor - 1) * eyeROI.Height / 2);
            int newWidth = (int)(widthFactor * eyeROI.Width);
            int newHeight = (int)(widthFactor * eyeROI.Height);

            // Boundary judgment
            if (newX < 0)
            {
                newX = 0;
            }
            if (newY < 0)
            {
                newY = 0;
            }
            if (newX + newWidth > img.Width)
            {
                newX = img.Width - newWidth;
            }
            if (newY + newHeight > img.Height)
            {
                newY = img.Height - newHeight;
            }

            Rectangle newROI = new Rectangle(new Point(newX, newY), new Size(newWidth, newHeight));

            Image<Gray, byte> diff = new Image<Gray, byte>(img.Width, img.Height);

            CvInvoke.cvSetImageROI(img, newROI);
            CvInvoke.cvSetImageROI(prev, newROI);
            CvInvoke.cvSetImageROI(diff, newROI);

            CvInvoke.cvSub(img, prev, diff, IntPtr.Zero);

            CvInvoke.cvThreshold(diff, diff, 100, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_BINARY);
    
            StructuringElementEx element = new StructuringElementEx(5, 5, 3, 3, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_CLOSE, 1);
            CvInvoke.cvMorphologyEx(diff, diff, IntPtr.Zero, element, Emgu.CV.CvEnum.CV_MORPH_OP.CV_MOP_OPEN, 3);

            IntPtr comp = new IntPtr();//存放检测到的图像块的首地址
            MemStorage storage = new MemStorage();
            int nc = CvInvoke.cvFindContours(diff, storage, ref comp, StructSize.MCvContour,
                Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_CCOMP, Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, new Point(0, 0));

            // save eye region and previous eye region for latter use
            Image<Gray, byte> eye = img.Copy();
            Image<Gray, byte> prevEye = prev.Copy();

            CvInvoke.cvResetImageROI(img);
            CvInvoke.cvResetImageROI(prev);
            CvInvoke.cvResetImageROI(diff);


            if (nc != 1)
            {
                return false;
            }

            Rectangle rect = CvInvoke.cvBoundingRect(comp, 1);
            rect.X = rect.X + newROI.X;
            rect.Y = rect.Y + newROI.Y;

            if (rect.X < eyeROI.X || rect.Y < eyeROI.Y)
            {
                return false;
            }
            if (rect.X + rect.Width > eyeROI.X + eyeROI.Width || rect.Y + rect.Height > eyeROI.Y + eyeROI.Height)
            {
                return false;
            }

            /// reduce the effct of shaking
            byte pixelThreshold = 100;
            int sumThresholdLow = 100;
            int sumThresholdUp = 600;

            int sumPixelEye = 0;
            for (int i = 0; i < eye.Width; i++)
            {
                for (int j = 0; j < eye.Height; j++)
                {
                    if (eye.Data[i, j, 0] < pixelThreshold)
                        sumPixelEye++;
                }
            }

            int sumPixelPrevEye = 0;
            for (int i = 0; i < prevEye.Width; i++)
            {
                for (int j = 0; j < prevEye.Height; j++)
                {
                    if (prevEye.Data[i, j, 0] < pixelThreshold)
                        sumPixelPrevEye++;
                }
            }


            if (sumPixelEye > sumThresholdLow && sumPixelEye < sumThresholdUp && sumPixelPrevEye > sumThresholdLow && sumPixelPrevEye < sumThresholdUp)
            {                
                isShook = true;
                return false;
            }
            /// 

            return true;
        }

        #endregion
    }
}
