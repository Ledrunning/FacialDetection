// <copyright file="DetectionManager.cs" company="ITU">
// ******************************************************
// GazeTrackingLibrary for ITU GazeTracker
// Copyright (C) 2010 Javier San Agustin  
// ------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation; either version 3 of the License, 
// or (at your option) any later version.
// This program is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
// General Public License for more details.
// You should have received a copy of the GNU General Public License 
// along with this program; if not, see http://www.gnu.org/licenses/.
// **************************************************************
// </copyright>
// <author>Javier San Agustin</author>
// <email>javier@itu.dk</email>
// <modifiedby>Martin Tall</modifiedby>
// <modifiedby>Adrian Voßkühler</modifiedby>

using System;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using aSee.ImageProcess.Detection.Eye;
using aSee.ImageProcess.Detection.Eyes;
using aSee.ImageProcess.Detection.Glint;
using aSee.ImageProcess.Detection.Pupil;
using aSee.ImageProcess.Log;
using aSee.Commons.Enum;
using aSee.Settings;
using aSee.ImageProcess.Utils;
using aSee.Devices;

namespace aSee.ImageProcess.Detection
{

    #region Includes

    #endregion

    public class DetectionManager
    {
        #region Variables

        private readonly GlintDetection glintDetectionLeft;
        private readonly GlintDetection glintDetectionRight;
        private readonly PupilDetection pupilDetectionLeft;
        private readonly PupilDetection pupilDetectionRight;

        private bool doEye = true;
        private bool doEyes = true;
        private EyesTracker eyestracker;
        private aSee.ImageProcess.Detection.Eye.Eyetracker eyetracker;
        private bool featuresLeftFound;
        private bool featuresRightFound;
        private int counter;
        private int missCounter;

        private Image<Gray, byte> inputLeftEye;
        private Image<Gray, byte> inputRightEye;

        private DateTime timeStampLastROIAdjustment;

        private int[] HistGram;

        private BlinkDetection blinkDetection;

        #endregion

        #region Constructor

        public DetectionManager()
        {
            //eyestracker = new EyesTracker();
            eyetracker = new aSee.ImageProcess.Detection.Eye.Eyetracker();
            blinkDetection = new BlinkDetection();

            pupilDetectionLeft = new PupilDetection(EyeEnum.Left);
            pupilDetectionRight = new PupilDetection(EyeEnum.Right);

            glintDetectionLeft = new GlintDetection(EyeEnum.Left);
            glintDetectionRight = new GlintDetection(EyeEnum.Right);
        }

        #endregion //CONSTRUCTION

        #region Get/Set properties

        public aSee.ImageProcess.Detection.Eye.Eyetracker Eyetracker
        {
            set { eyetracker = value; }
            get { return eyetracker; }
        }

        public GlintDetection GlintDetectionLeft
        {
            get { return glintDetectionLeft; }
        }

        public GlintDetection GlintDetectionRight
        {
            get { return glintDetectionRight; }
        }

        public PupilDetection PupilDetectionLeft
        {
            get { return pupilDetectionLeft; }
        }

        public PupilDetection PupilDetectionRight
        {
            get { return pupilDetectionRight; }
        }

        #endregion //PROPERTIES

        public void Threshold(Image<Gray, byte> imgTh)
        {
             HistGram = new int[256];

            //统计灰度级中每个像素在整幅图像中的个数
            for (int i = 0; i < imgTh.Size.Height; i++)
            {
                for (int j = 0; j < imgTh.Size.Width; j++)
                {
                    HistGram[imgTh.Data[i, j, 0]]++;
                }
            }
        }

        //不带眼镜处理阈值
        public int GetMinimumThreshold()
        {
            int Y, Iter = 0;
            double[] HistGramC = new double[256];           // 基于精度问题，一定要用浮点数来处理，否则得不到正确的结果
            double[] HistGramCC = new double[256];          // 求均值的过程会破坏前面的数据，因此需要两份数据
            for (Y = 0; Y < 256; Y++)
            {
                HistGramC[Y] = HistGram[Y];
                HistGramCC[Y] = HistGram[Y];
            }

            // 通过三点求均值来平滑直方图
            while (IsDimodal(HistGramCC) == false)                                        // 判断是否已经是双峰的图像了      
            {
                HistGramCC[0] = (HistGramC[0] + HistGramC[0] + HistGramC[1]) / 3;                 // 第一点
                for (Y = 1; Y < 255; Y++)
                    HistGramCC[Y] = (HistGramC[Y - 1] + HistGramC[Y] + HistGramC[Y + 1]) / 3;     // 中间的点
                HistGramCC[255] = (HistGramC[254] + HistGramC[255] + HistGramC[255]) / 3;         // 最后一点
                System.Buffer.BlockCopy(HistGramCC, 0, HistGramC, 0, 256 * sizeof(double));
                Iter++;
                if (Iter >= 1000) return -1;                                                   // 直方图无法平滑为双峰的，返回错误代码
            }
            // 阈值极为两峰之间的最小值 
            bool Peakfound = false;
            for (Y = 1; Y < 255; Y++)
            {
                if (HistGramCC[Y - 1] < HistGramCC[Y] && HistGramCC[Y + 1] < HistGramCC[Y]) Peakfound = true;
                if (Peakfound == true && HistGramCC[Y - 1] >= HistGramCC[Y] && HistGramCC[Y + 1] >= HistGramCC[Y])
                    return Y - 1;
            }
            return -1;
        }

        private bool IsDimodal(double[] HistGram)       // 检测直方图是否为双峰的
        {
            // 对直方图的峰进行计数，只有峰数位2才为双峰 
            int Count = 0;
            for (int Y = 1; Y < 255; Y++)
            {
                if (HistGram[Y - 1] < HistGram[Y] && HistGram[Y + 1] < HistGram[Y])
                {
                    Count++;
                    if (Count > 2) return false;
                }
            }
            if (Count == 2)
                return true;
            else
                return false;
        } //对于这种有较明显的双峰的图像，该算法还是能取得不错的效果的。


        public int Get1DMaxEntropyThreshold()
        {
            int X, Y, Amount = 0;
            double[] HistGramD = new double[256];
            double SumIntegral, EntropyBack, EntropyFore, MaxEntropy;
            int MinValue = 255, MaxValue = 0;
            int Threshold = 0;

            for (MinValue = 0; MinValue < 256 && HistGram[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && HistGram[MinValue] == 0; MaxValue--) ;
            if (MaxValue == MinValue) return MaxValue;          // 图像中只有一个颜色             
            if (MinValue + 1 == MaxValue) return MinValue;      // 图像中只有二个颜色

            for (Y = MinValue; Y <= MaxValue; Y++) Amount += HistGram[Y];        //  像素总数

            for (Y = MinValue; Y <= MaxValue; Y++) HistGramD[Y] = (double)HistGram[Y] / Amount + 1e-17;

            MaxEntropy = double.MinValue; ;
            for (Y = MinValue + 1; Y < MaxValue; Y++)
            {
                SumIntegral = 0;
                for (X = MinValue; X <= Y; X++) SumIntegral += HistGramD[X];
                EntropyBack = 0;
                for (X = MinValue; X <= Y; X++) EntropyBack += (-HistGramD[X] / SumIntegral * Math.Log(HistGramD[X] / SumIntegral));
                EntropyFore = 0;
                for (X = Y + 1; X <= MaxValue; X++) EntropyFore += (-HistGramD[X] / (1 - SumIntegral) * Math.Log(HistGramD[X] / (1 - SumIntegral)));
                if (MaxEntropy < EntropyBack + EntropyFore)
                {
                    Threshold = Y;
                    MaxEntropy = EntropyBack + EntropyFore;
                }
            }
            return Threshold;
        }

        #region Public methods - process image

        // This is the main image feature detection chain

        public bool BlinkDetection(Image<Gray, byte> img, Image<Gray, byte> prev, TrackData trackData, TrackData lastSample)
        {
            // discard the first frame
            if (prev.Width == 0)
            {
                return false;
            }
        
            bool isShook = blinkDetection.detectShook(img, prev);
            
            if (isShook)
            {             
                return false;
            }

            bool isBlinked = blinkDetection.detectOverall(img, prev);

            bool isLeftShook = false;
            bool isRightShook = false;
            bool isLeftEyeBlinked = blinkDetection.detectAlone(img, prev, trackData.LeftROI, lastSample.LeftROI, ref isLeftShook);
            bool isRightEyeBlinked = blinkDetection.detectAlone(img, prev, trackData.RightROI, lastSample.RightROI, ref isRightShook);

            if (isBlinked && isLeftShook || isBlinked && isRightShook)
            {
                isBlinked = false;
            }

            if (!isBlinked && isLeftEyeBlinked && isRightEyeBlinked)
            {
                isBlinked = true;
            }

            if (isBlinked)
            {
                aSee.ImageProcess.Detection.BlinkDetection.totalBlinks++;
            }

            return isBlinked;
        }

        public bool ProcessImage(Image<Gray, byte> input, TrackData trackData)
        {
            counter++;
            //Log.Performance.Now.IsEnabled = false;

            featuresLeftFound = false;
            featuresRightFound = false;

            #region Face detection

            #endregion

            #region Eyes region tracking (binocular)

            //// If binocular -> Track (head), (eye region), pupil, (glints)
            //if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Binocular)
            //{
            //    if (aSee.Settings.Settings.Instance.Processing.TrackingEyes && eyestracker.IsReady) 
            //    {
            //        if (doEyes && CameraControl.Instance.UsingUC480 == true && CameraControl.Instance.IsROISet == false) 
            //        {
            //            if (eyestracker.DetectEyes(input, trackData)) 
            //            {
            //               doEyes = false; // found both eyes
            //               CameraControl.Instance.ROI = trackData.EyesROI;
            //               TrackDB.Instance.Data.Clear();
            //               doEyes = false;
            //               doEye = true;
            //               return false;
            //            }
            //        }
            //    }
            //}

            #endregion

            #region Eye region tracking

            if (aSee.Settings.Settings.Instance.Processing.TrackingEye && doEye)
            {
                // Eye feature detector ready when haar cascade xml file loaded
                if (eyetracker.IsReady)
                {
                    if (eyetracker.DetectEyes(input, trackData))// will set left/right roi
                    {
                        missCounter = 0;
                        doEye = false;
                    }
                    else
                    {
                        // No eye/eys found
                        doEye = true;
                        missCounter++;

                        if (aSee.Devices.Camera.Instance.Device.IsSupportingROI && missCounter > aSee.Devices.Camera.Instance.Device.FPS / 3)
                            aSee.Devices.Camera.Instance.Device.ClearROI();

                        return false;
                    }
                }
            }

            #endregion

            #region Left eye

            string strPath;
            strPath = String.Format("..\\pLROI{0:d}.jpg", trackData.FrameNumber);

            // Set sub-roi, if eye feature detection was performed do nothing otherwise use values from previous frame 
            ApplyEstimatedEyeROI(EyeEnum.Left, trackData, input.Size);

            inputLeftEye = input.Copy(trackData.LeftROI);

            // Detect pupil
            Image<Gray, byte> RLGray = input.Copy();
            RLGray.Draw(trackData.LeftROI, new Gray(255), 1);

            strPath = String.Format("..\\pLEye{0:d}.jpg", trackData.FrameNumber);
           // inputLeftEye.Save(strPath);


            //是否带眼镜
            Threshold(inputLeftEye);
            if (!aSee.Settings.Settings.Instance.Processing.IsWearGlasses)
            {
                int ostuImage = GetMinimumThreshold();
                int newPupilThresholdLeft = ostuImage + aSee.Settings.Settings.Instance.Processing.AutoAddPupilThresholdLeft;
                aSee.Settings.Settings.Instance.Processing.PupilThresholdLeft = newPupilThresholdLeft;

                strPath = String.Format("..\\pLThreshold{0:d}.jpg", trackData.FrameNumber);
                
              //  inputLeftEye.ThresholdToZeroInv(new Gray(aSee.Settings.Settings.Instance.Processing.PupilThresholdLeft)).Save(strPath);
     
            }
            else
            {
                /////带眼镜时阈值处理方法 
                strPath = String.Format("..\\{0:d}Get1DMaxEntropyThreshold.jpg", trackData.FrameNumber);
                int ostuImage = Get1DMaxEntropyThreshold();
                //  inputLeftEye.ThresholdToZeroInv(new Gray(ostuImage)).Save(strPath);
                int newPupilThresholdLeft = ostuImage + aSee.Settings.Settings.Instance.Processing.AutoAddPupilThresholdLeft;

                aSee.Settings.Settings.Instance.Processing.PupilThresholdLeft = newPupilThresholdLeft;
            }
          
            
         
            if (pupilDetectionLeft.DetectPupil(inputLeftEye, trackData))
            {

                trackData.PupilDataLeft = pupilDetectionLeft.PupilData;
          
                // Detect glint(s)
                if (aSee.Settings.Settings.Instance.Processing.TrackingGlints)
                {
                    if (glintDetectionLeft.DetectGlints(inputLeftEye, pupilDetectionLeft.PupilData.Center))
                    {
                        trackData.GlintDataLeft = ConvertGlintsToAbsolute(glintDetectionLeft.GlintData, trackData.LeftROI);
                        featuresLeftFound = true;

                        PointF gf = new PointF();
                        gf.X = (float)trackData.GlintDataLeft.Glints.Centers[0].X;
                        gf.Y = (float)trackData.GlintDataLeft.Glints.Centers[0].Y;
                        RLGray.Draw(new CircleF(gf, 40), new Gray(255), 1);


                        gf.X = (float)trackData.GlintDataLeft.Glints.Centers[1].X;
                        gf.Y = (float)trackData.GlintDataLeft.Glints.Centers[1].Y;
                        RLGray.Draw(new CircleF(gf, 40), new Gray(255), 2);

                    }
                    else
                        trackData.GlintDataLeft = ConvertGlintsToAbsolute(glintDetectionLeft.GlintData, trackData.LeftROI);
                }
                else
                    featuresLeftFound = true;

            //    aSeeLogger.Logger.Instance.Debug(String.Format("{0:d}", DateTime.Now.Millisecond - start.Millisecond));

                // Convert values from subROI to whole absolute image space (ex. from 70x70 to 1280x1024)
                trackData.PupilDataLeft = ConvertPupilToAbsolute(EyeEnum.Left, pupilDetectionLeft.PupilData, trackData);

                PointF pf = new PointF();
                pf.X = (float)trackData.PupilDataLeft.Center.X;
                pf.Y = (float)trackData.PupilDataLeft.Center.Y;

                RLGray.Draw(new CircleF(pf, 20), new Gray(255), 2);

                strPath = String.Format("..\\pLR{0:d}.jpg", trackData.FrameNumber);

               // RLGray.Save(strPath);
            }

            #endregion

            #region Right eye

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Binocular)
            {
                ApplyEstimatedEyeROI(EyeEnum.Right, trackData, input.Size);
                
                inputRightEye = input.Copy(trackData.RightROI);

              //  Image<Gray, byte> rGray = input.Copy();

                RLGray.Draw(trackData.RightROI, new Gray(255), 1);

                strPath = String.Format("..\\pREye{0:d}.jpg", trackData.FrameNumber);

               // inputRightEye.Save(strPath);

                // Detect pupil
                //改变瞳孔阈值
                Threshold(inputRightEye);
                if (!aSee.Settings.Settings.Instance.Processing.IsWearGlasses)
                {
                    int ostuImage = GetMinimumThreshold();
                    int newPupilThresholdRight = ostuImage + aSee.Settings.Settings.Instance.Processing.AutoAddPupilThresholdRight;

                    aSee.Settings.Settings.Instance.Processing.PupilThresholdRight = newPupilThresholdRight;

                    strPath = String.Format("..\\pRThreshold{0:d}.jpg", trackData.FrameNumber);
                 //   inputRightEye.ThresholdToZeroInv(new Gray(aSee.Settings.Settings.Instance.Processing.PupilThresholdRight)).Save(strPath);
                }
                else
                {
                    //带眼镜时阈值处理方法 
                    strPath = String.Format("..\\{0:d}Get1DMaxEntropyThreshold.jpg", trackData.FrameNumber);
                    int ostuImage = Get1DMaxEntropyThreshold();
                    //inputRightEye.ThresholdToZeroInv(new Gray(ostuImage)).Save(strPath);
                    int newPupilThresholdRight = ostuImage + aSee.Settings.Settings.Instance.Processing.AutoAddPupilThresholdRight;

                    aSee.Settings.Settings.Instance.Processing.PupilThresholdRight = newPupilThresholdRight;
                }

             

                if (pupilDetectionRight.DetectPupil(inputRightEye, trackData))
                {
                    trackData.PupilDataRight = pupilDetectionRight.PupilData;
                  

                    // Detect glint(s)
                    if (aSee.Settings.Settings.Instance.Processing.TrackingGlints)
                    {
                        if (glintDetectionRight.DetectGlints(inputRightEye, pupilDetectionRight.PupilData.Center))
                        {
                            trackData.GlintDataRight = ConvertGlintsToAbsolute(glintDetectionRight.GlintData, trackData.RightROI);


                            PointF gf = new PointF();
                            gf.X = (float)trackData.GlintDataRight.Glints.Centers[0].X;
                            gf.Y = (float)trackData.GlintDataRight.Glints.Centers[0].Y;
                            RLGray.Draw(new CircleF(gf, 40), new Gray(255), 1);


                            gf.X = (float)trackData.GlintDataRight.Glints.Centers[1].X;
                            gf.Y = (float)trackData.GlintDataRight.Glints.Centers[1].Y;
                            RLGray.Draw(new CircleF(gf, 40), new Gray(255), 2);
                            
                            featuresRightFound = true;
                        }
                         else
                            trackData.GlintDataRight = ConvertGlintsToAbsolute(glintDetectionRight.GlintData, trackData.RightROI);
                    }
                    else
                        featuresRightFound = true;

                    trackData.PupilDataRight = ConvertPupilToAbsolute(EyeEnum.Right, pupilDetectionRight.PupilData, trackData);

                    PointF pf = new PointF();
                    pf.X = (float)trackData.PupilDataRight.Center.X;
                    pf.Y = (float)trackData.PupilDataRight.Center.Y;
                    RLGray.Draw(new CircleF(pf, 20), new Gray(255), 4);

                    strPath = String.Format("..\\pLR{0:d}.jpg", trackData.FrameNumber);

                 //  RLGray.Save(strPath);
                }
            }

          //  if (!doEye)
         //       RLGray.Save(strPath);

            #endregion

            #region ROI mode / state / update

            #region Monocular

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Monocular)
            {
                if (!featuresLeftFound)
                {
                    if (aSee.Settings.Settings.Instance.Processing.TrackingEye)
                    {
                        doEye = true;
                        if (aSee.Devices.Camera.Instance.Device.IsSettingROI && aSee.Devices.Camera.Instance.Device.IsROISet == false)
                            aSee.Devices.Camera.Instance.Device.ClearROI();
                    }
                    else
                        trackData.LeftROI = new Rectangle(new Point(0, 0), new Size(0, 0));
                }
                else
                {
                    trackData.LeftROI = SetROI(input.Size, trackData.PupilDataLeft.Center, trackData.PupilDataLeft.Diameter);
                    doEye = false;

                    // If using special camera, set roi and adjust EyeROIs
                    if (aSee.Devices.Camera.Instance.Device.IsSupportingROI)
                        if (aSee.Devices.Camera.Instance.Device.IsROISet == false)
                            CameraSetROI(trackData);
                        else
                        {
                            CenterROIOnPupil(trackData, EyeEnum.Left, input.Size);

                            // Re-center sub-ROIs, enuse that eyes stays within by margins
                            if (aSee.Devices.Camera.Instance.Device.IsSupportingROI && aSee.Devices.Camera.Instance.Device.IsROISet)
                                CameraCenterROI(trackData, input.Size);
                        }
                }
            }

            #endregion

            #region Binocular 

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Binocular)
            {
                // Nothing found, run eye classifier on next frame
                if (!featuresLeftFound || !featuresRightFound || trackData.LeftROI.IntersectsWith(trackData.RightROI))
                {
                    if (aSee.Settings.Settings.Instance.Processing.TrackingEye)
                        doEye = true;
                    else
                    {
                        trackData.LeftROI = new Rectangle(new Point(0, 0), new Size(0, 0));
                        trackData.RightROI = new Rectangle(new Point(0, 0), new Size(0, 0));
                    }
                }
                else
                {


                    trackData.LeftROI = SetROI(input.Size, trackData.PupilDataLeft.Center, trackData.PupilDataLeft.Diameter);
                    trackData.RightROI = SetROI(input.Size, trackData.PupilDataRight.Center, trackData.PupilDataRight.Diameter);

                    Image<Gray, byte> trackROIGray = input.Copy();
                    trackROIGray.Draw(trackData.LeftROI, new Gray(255), 1);
                    trackROIGray.Draw(trackData.RightROI, new Gray(255), 1);

                    strPath = String.Format("..\\trackROIGray{0:d}.jpg", trackData.FrameNumber);

                  //  trackROIGray.Save(strPath);




                    doEye = false;

                    // If using special camera, set roi and adjust EyeROIs
                    if (aSee.Devices.Camera.Instance.Device.IsSupportingROI)
                        if (aSee.Devices.Camera.Instance.Device.IsROISet == false)
                            CameraSetROI(trackData);
                        else
                        {
                            CenterROIOnPupil(trackData, EyeEnum.Left, input.Size);
                            CenterROIOnPupil(trackData, EyeEnum.Right, input.Size);

                            // Re-center sub-ROIs, enuse that eyes stays within by margins
                            if (aSee.Devices.Camera.Instance.Device.IsSupportingROI && aSee.Devices.Camera.Instance.Device.IsROISet)
                                CameraCenterROI(trackData, input.Size);
                        }
                }
            }

            #endregion

            #endregion

            //Performance.Now.Stamp("Processing all done");

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Binocular)
            {
                if (featuresRightFound)
                {
                    if (trackData.PupilRightDetected && trackData.GlintsRightDetected
                        && trackData.GlintDataRight.Glints.Count == 
                        aSee.Settings.Settings.Instance.Processing.NumberOfGlints
                       
                        && trackData.PupilLeftDetected && trackData.GlintsLeftDetected
                        && trackData.GlintDataLeft.Glints.Count == 
                        aSee.Settings.Settings.Instance.Processing.NumberOfGlints)
                        return true;
                    else
                        return false;
                           
                }
                else
                    return false;
            }
            else
            {
                if (featuresLeftFound)
                {
                    if (trackData.PupilLeftDetected && trackData.GlintsLeftDetected
                        && trackData.GlintDataLeft.Glints.Count == 
                        aSee.Settings.Settings.Instance.Processing.NumberOfGlints)
                        return true;
                    else
                        return false;

                }
                else
                    return false;
            }
        }

        public void Clear()
        {
            doEyes = true;
            doEye = true;
            TrackDB.Instance.Data.Clear();
        }

        #endregion //PUBLICMETHODS

        #region Private methods

        #region ROI

        private static void ApplyEstimatedEyeROI(EyeEnum eye, TrackData trackData, Size imageSize)
        {
            // If the feature detection was used the trackdata.LeftROI/RightROI will already be set (roi != 0)
            // If detector hasen't been used (eg. roi=0) then values from previous frame should be used
            // Update: We set the values on the trackdata object instead of returning a rectangle.

            var roi = new Rectangle(new Point(0, 0), new Size(imageSize.Width, imageSize.Height));

            switch (eye)
            {
                case EyeEnum.Left:

                    if (trackData.LeftROI.Width == 0)
                        trackData.LeftROI = TrackDB.Instance.GetLastEyeROI(EyeEnum.Left, imageSize);
                    break;

                case EyeEnum.Right:

                    if (trackData.RightROI.Width == 0)
                        trackData.RightROI = TrackDB.Instance.GetLastEyeROI(EyeEnum.Right, imageSize);
                    break;
            }

            Performance.Now.Stamp("ROI Estimated");
        }

        /// <summary>
        /// Set the ROI of an image around a central point given the radius, which would
        /// correspond to the radius of the inscribed circle (e.g a pupil). The method
        /// checks whether the ROI is actually within the limits of the image. If it's
        /// not, the ROI will not be set and the method return false
        /// </summary>
        /// <param name="image">Input image</param>
        /// <param name="center">Central point</param>
        /// <param name="radius">The radius of the roi.</param>
        /// <returns>True if succesfull, otherwise false.</returns>
        /// 
        private static Rectangle SetROI(Size imageSize, GTPoint center, double diameter)
        {
            var ROI = new Rectangle();

            double aspectRatio = imageSize.Width/(double) imageSize.Height;
            double r = 3 * diameter;//2.5//My Alter

            var roiSize = new Size((int) (aspectRatio*r), (int) (aspectRatio*r));

            //My Alter
            Rectangle rect = new Rectangle();
            rect.X = (int)Math.Round(center.X) - roiSize.Width/2;
            rect.Y = (int)Math.Round(center.Y) -  roiSize.Height/2;
            rect.Size = roiSize;

            if (rect.X < 0)
            {
                rect.X = 0;
            }

            if (rect.Y < 0)
            {
                rect.Y = 0;
            }

            if (center.X + roiSize.Width/2 > imageSize.Width)
            {
                rect.X = imageSize.Width - roiSize.Width-2;
                if (rect.X < 0)
                {
                    rect.X = 0;
                    rect.Width = imageSize.Width;
                }
            }


            if (center.Y + roiSize.Height/2 > imageSize.Height)
            {
                rect.Y = imageSize.Height - roiSize.Height-2;
                if (rect.Y < 0)
                {
                    rect.Y = 0;
                    rect.Height = imageSize.Height;
                }
            }

            ROI = rect;


            //if (center.X - roiSize.Width > 0 &&
            //    center.Y - roiSize.Height > 0 &&
            //    center.X + roiSize.Width/2 < imageSize.Width &&
            //    center.Y + roiSize.Height/2 < imageSize.Height)
            //{
            //    ROI = new Rectangle(
            //        (int) Math.Round(center.X) - roiSize.Width/2,
            //        (int) Math.Round(center.Y) - roiSize.Height/2,
            //        roiSize.Width,
            //        roiSize.Height);
            //}
            //else
            //    ROI = new Rectangle(new Point(0, 0), roiSize);
           

            return ROI;
        }

        private static void CenterROIOnPupil(TrackData trackData, EyeEnum eye, Size imageSize)
        {
            var roi = new Rectangle();

            switch (eye)
            {
                case EyeEnum.Left:

                    roi = trackData.LeftROI; // for size

                    if (roi.Width != 0)
                    {
                        roi.X = (int) trackData.PupilDataLeft.Center.X - roi.Width/2; // center it 
                        roi.Y = (int) trackData.PupilDataLeft.Center.Y - roi.Height/2;

                        if (roi.X > 0 && roi.Right < imageSize.Width &&
                            roi.Y > 0 && roi.Bottom < imageSize.Height)
                            trackData.LeftROI = roi; // ok, within image
                    }
                    break;

                case EyeEnum.Right:

                    roi = trackData.RightROI;

                    if (roi.Width != 0)
                    {
                        roi.X = (int) trackData.PupilDataRight.Center.X - roi.Width/2;
                        roi.Y = (int) trackData.PupilDataRight.Center.Y - roi.Height/2;

                        if (roi.X > 0 && roi.Right < imageSize.Width &&
                            roi.Y > 0 && roi.Bottom < imageSize.Height)
                            trackData.RightROI = roi;
                    }
                    break;
            }
        }

        #region Camera ROI methods

        private void CameraSetROI(TrackData td)
        {
            if (aSee.Devices.Camera.Instance.Device.IsROISet)
                return;

            #region Binocular 

            if (aSee.Settings.Settings.Instance.Processing.TrackingMethod == TrackingMethodEnum.RemoteBinocular)
            {
                // Only apply when we got both eyes
                if (td.LeftROI.Height == 0 || td.RightROI.Height == 0 || td.LeftROI.Y == 0 || td.RightROI.Y == 0)
                    return;

                if (Operations.Distance(new GTPoint(td.LeftROI.Location), new GTPoint(td.RightROI.Location)) < 200)
                    return;

                // The size of the ROI will greatly affect the tracking FPS
                var roi = new Rectangle();
                roi.Width = Convert.ToInt32(Operations.Distance(new GTPoint(td.LeftROI.Location), new GTPoint(td.RightROI.Location))*2.2);
                roi.Height = Convert.ToInt32(td.LeftROI.Height + td.RightROI.Height * 1.4);
                roi.X = Convert.ToInt32(td.LeftROI.X - td.LeftROI.Width*2);
                roi.Y = Convert.ToInt32((td.LeftROI.Y + td.RightROI.Y)/2 - 40);

                if (Operations.IsWithinBounds(
                    roi, 
                    new Rectangle(new Point(0, 0), 
                    new Size(aSee.Devices.Camera.Instance.Device.Width, aSee.Devices.Camera.Instance.Device.Height))))
                {
                    // Sometimes we have an issue here where the roi size is too small.. why?
                    if (roi.Width > 100)
                    {
                        aSee.Devices.Camera.Instance.Device.SetROI(roi);

                        var newLeft = new Rectangle(new Point(td.LeftROI.X - roi.X, td.LeftROI.Y - roi.Y),
                                                    new Size(td.LeftROI.Width, td.LeftROI.Height));

                        var newRight = new Rectangle(new Point(td.RightROI.X - roi.X, td.RightROI.Y - roi.Y),
                                                     new Size(td.RightROI.Width, td.RightROI.Height));

                        td.LeftROI = newLeft;
                        td.RightROI = newRight;

                        // Save full screen threshold to be (left+right)/2
                        if (aSee.Settings.Settings.Instance.Processing.TrackingMethod == TrackingMethodEnum.RemoteBinocular)
                            aSee.Settings.Settings.Instance.Processing.PupilThreshold = (aSee.Settings.Settings.Instance.Processing.PupilThresholdLeft + aSee.Settings.Settings.Instance.Processing.PupilThresholdRight) / 2;
                    }
                }
            }
            #endregion

            #region monocular

            else if (aSee.Settings.Settings.Instance.Processing.TrackingMethod == TrackingMethodEnum.RemoteMonocular)
            {
                if (td.LeftROI.Height == 0 || td.LeftROI.Y == 0)
                    return;

                var roi = new Rectangle();
                roi.Width = td.LeftROI.Width*5;
                roi.Height = td.LeftROI.Height*4;
                roi.X = Convert.ToInt32((td.LeftROI.X - td.LeftROI.Width/2));
                roi.Y = Convert.ToInt32((td.LeftROI.Y - td.LeftROI.Height/2));

                if (Operations.IsWithinBounds(
                    roi, 
                    new Rectangle(new Point(0, 0),
                    new Size(aSee.Devices.Camera.Instance.Device.Width, aSee.Devices.Camera.Instance.Device.Height))))
                {
                    // Sometimes we have an issue here where the roi size is too small.. why?
                    if (roi.Width > 100)
                    {
                        aSee.Devices.Camera.Instance.Device.SetROI(roi);

                        td.LeftROI = new Rectangle(new Point(td.LeftROI.X - roi.X, td.LeftROI.Y - roi.Y),
                                                   new Size(td.LeftROI.Width, td.LeftROI.Height));
                    }
                }
            }

            #endregion



        }
        
        public void CameraCenterROI(TrackData trackData, Size imgSize)
        {
            if(aSee.Devices.Camera.Instance.Device.IsSupportingROI == false && aSee.Devices.Camera.Instance.Device.IsROISet == false)
                return;

            if(aSee.Devices.Camera.Instance.DefaultHeight == imgSize.Height)
                return;

            // Chaning the ROI will start/stop the Thorlabs camera for a brief moment. Don't want to do it too often
            int minTimeSinceLastCall = 100; // How many milliseconds that must have ellapsed since last roi modification
            int minMovementX = 20;          // Minimum movement in X to trigger roi change
            int minMovementY = 20;          // Minimum movement in Y. 


            // Make sure we dont call this too ofter (Thorlabs will stop/start on roi change)
            if (timeStampLastROIAdjustment != null)
            {
                if (DateTime.Now.Subtract(timeStampLastROIAdjustment).Milliseconds < minTimeSinceLastCall)
                    return;
            }

            Rectangle newROI = aSee.Devices.Camera.Instance.ROI;

            #region Monocular (left)

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Monocular)
            {
                // Crude method for recentering the camera roi based on margins
                // This is the old method, see Binocular below for new 
                int heightMargin = aSee.Devices.Camera.Instance.ROI.Height/3;
                int widthMargin = aSee.Devices.Camera.Instance.ROI.Width/6;
                int adjustPixels = trackData.LeftROI.Width/3;

                if (trackData.PupilLeftDetected == false)
                    return;

                if (trackData.PupilDataLeft.Center.Y < heightMargin)
                    newROI.Y -= adjustPixels;

                else if (trackData.PupilDataLeft.Center.Y > imgSize.Height - heightMargin)
                    newROI.Y += adjustPixels;

                if (trackData.PupilDataLeft.Center.X < widthMargin)
                    newROI.X -= adjustPixels;

                else if (trackData.PupilDataLeft.Center.X > imgSize.Width - widthMargin)
                    newROI.X += adjustPixels;
            }

            #endregion

            #region Binocular (right)

            if (aSee.Settings.Settings.Instance.Processing.TrackingMode == TrackingModeEnum.Binocular)
            {
                if (trackData.PupilLeftDetected == false || trackData.PupilRightDetected == false)
                    return;

                // Get distances from vertial centerline
                int distYLeft  = Convert.ToInt32(imgSize.Height/2 - trackData.PupilDataLeft.Center.Y);
                int distYRight = Convert.ToInt32(imgSize.Height/2 - trackData.PupilDataRight.Center.Y);
                int adjustY = Math.Abs(distYLeft + distYRight/2);

                // Get distances from horizontal centerline
                int distXLeft =  Convert.ToInt32(Math.Abs(imgSize.Width/2 - trackData.PupilDataLeft.Center.X));
                int distXRight = Convert.ToInt32(Math.Abs(imgSize.Width/2 - trackData.PupilDataRight.Center.X));
                int adjustX = Math.Abs(distXRight-distXLeft)/2;

                // Don't do anything if either X/Y position hasn't changed more than N
                if (adjustY > minMovementY || adjustX > minMovementX)
                {
                    // Increase if avg Y pos is below mid horizontal line, else decrease Y
                    if (distYLeft < 0 || distYRight < 0)
                        newROI.Y += adjustY;
                    else
                        newROI.Y -= adjustY;

                    // If one eye is closer to middle we move opposite direction 
                    if (distXLeft > distXRight)
                        newROI.X -= Math.Abs(adjustX);
                    else 
                        newROI.X += Math.Abs(adjustX);
                }
                else
                {
                    return;
                }

                #region Old method..

                // Crude method for recentering the camera roi based on margins
                //int heightMargin = GTHardware.Camera.Instance.ROI.Height/3;
                //int widthMargin = GTHardware.Camera.Instance.ROI.Width/6;
                //int adjustPixels = trackData.LeftROI.Width/3;

                //int diffY = Convert.ToInt32(imgSize.Height/2 - trackData.PupilDataLeft.Center.Y);
                //newROI.Y += diffY; 

                //if (trackData.PupilDataRight.Center.Y < heightMargin || trackData.PupilDataLeft.Center.Y < heightMargin)
                //    newROI.Y += adjustY;   //-= heightMargin/4;

                //else if (trackData.PupilDataRight.Center.Y > imgSize.Height - heightMargin ||
                //         trackData.PupilDataLeft.Center.Y > imgSize.Height - heightMargin)
                //    newROI.Y += adjustY;  // += heightMargin / 4;

                //if (trackData.PupilDataRight.Center.X < widthMargin || trackData.PupilDataLeft.Center.X < widthMargin)
                //    newROI.X -= widthMargin/4;

                //else if (trackData.PupilDataRight.Center.X > imgSize.Width - widthMargin ||
                //         trackData.PupilDataLeft.Center.X > imgSize.Width - widthMargin)
                //    newROI.X += widthMargin/4;
                #endregion
            }

            #endregion

            aSee.Devices.Camera.Instance.Device.SetROI(newROI);
            timeStampLastROIAdjustment = DateTime.Now;
        }

        #endregion

        #endregion

        #region Convert local to absolute position

        private static PupilData ConvertPupilToAbsolute(EyeEnum eye, PupilData pupilData, TrackData trackData)
        {
            var eyeROI = new Rectangle();
            if (eye == EyeEnum.Left)
                eyeROI = trackData.LeftROI;
            else
                eyeROI = trackData.RightROI;

            pupilData.Center.X += eyeROI.X;
            pupilData.Center.Y += eyeROI.Y;
            pupilData.Blob.CenterOfGravity = new GTPoint(pupilData.Blob.CenterOfGravity.X + eyeROI.X,
                                                         pupilData.Blob.CenterOfGravity.Y + eyeROI.Y);
            return pupilData;
        }

        private static GlintData ConvertGlintsToAbsolute(GlintData input, Rectangle ROI)
        {
            foreach (GTPoint t in input.Glints.Centers)
            {
                if (System.Math.Abs(t.X) > 0.001 && System.Math.Abs(t.Y) > 0.001)
                {
                    t.X += ROI.X;
                    t.Y += ROI.Y;
                }
                //input.Glints.blobs[i].CentroidX = input.Glints.blobs[i].CentroidX + ROI.X;
                //input.Glints.blobs[i].CentroidY = input.Glints.blobs[i].CentroidY + ROI.Y;
            }

            return input;
        }

        #endregion

        #endregion
    }
}