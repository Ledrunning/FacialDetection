using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Documents;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

#if !(__IOS__ || NETFX_CORE)

#endif


namespace CameraCaptureWPF.Service
{
    public class FaceDetection : WebCamService
    {
        public delegate void ImageChangedEventHandler(object sender, Image<Bgr, byte> image);
        public event ImageChangedEventHandler ImageDetectionChanged;

        private readonly string EyeHaarFileName = ApplicationConfiguration.EyeHaar;
        private readonly string FaceHaarFileName = ApplicationConfiguration.FaceHaar;
        private bool isStarted;

        private (List<Rectangle> facesList, List<Rectangle> eyesList) detectedFaces;

        public FaceDetection(bool isStarted)
        {
            ImageChanged += WebCamImageChanged;
            detectedFaces.facesList = new List<Rectangle>();
            detectedFaces.eyesList = new List<Rectangle>();
        }

        /// <summary>
        /// todo need to check!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="image"></param>
        private async void WebCamImageChanged(object sender, Image<Bgr, byte> image)
        {
            await DetectFaceAndEyesAsync(image);

            DrawRectangles(image);
            ImageDetectionChanged?.Invoke(this, image);
        }

        private Task DetectFaceAndEyesAsync(Image<Bgr, byte> image)
        {
            var faces = new List<Rectangle>();
            var eyes = new List<Rectangle>();
            return Task.Run(() =>
            {
                DetectFaceAndEyes(image, FaceHaarFileName, EyeHaarFileName, faces, eyes, out var time);
            });
        }

        private void DrawRectangles(Image<Bgr, byte> image)
        {
            foreach (var f in detectedFaces.facesList)
            {
                image.Draw(f, new Bgr(Color.Red), 3);
                
                foreach (var e in detectedFaces.eyesList)
                {
                    image.Draw(e, new Bgr(Color.Blue), 2);
                }
            }
        }

        #region EmguDetector

        public void DetectFaceAndEyes(
            IInputArray image, string faceFileName, string eyeFileName,
            List<Rectangle> faces, List<Rectangle> eyes,
            out long detectionTime)
        {
            Stopwatch watch;

            using (var iaImage = image.GetInputArray())
            {
#if !(__IOS__ || NETFX_CORE)
                if (iaImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                    using (var face = new CudaCascadeClassifier(faceFileName))
                    using (var eye = new CudaCascadeClassifier(eyeFileName))
                    {
                        face.ScaleFactor = 1.1;
                        face.MinNeighbors = 10;
                        face.MinObjectSize = Size.Empty;
                        eye.ScaleFactor = 1.1;
                        eye.MinNeighbors = 10;
                        eye.MinObjectSize = Size.Empty;
                        watch = Stopwatch.StartNew();
                        using (var gpuImage = new CudaImage<Bgr, byte>(image))
                        using (var gpuGray = gpuImage.Convert<Gray, byte>())
                        using (var region = new GpuMat())
                        {
                            face.DetectMultiScale(gpuGray, region);
                            var faceRegion = face.Convert(region);
                            faces.AddRange(faceRegion);
                            foreach (var f in faceRegion)
                                using (var faceImg = gpuGray.GetSubRect(f))
                                {
                                    //For some reason a clone is required.
                                    //Might be a bug of CudaCascadeClassifier in opencv
                                    using (var clone = faceImg.Clone(null))
                                    using (var eyeRegionMat = new GpuMat())
                                    {
                                        eye.DetectMultiScale(clone, eyeRegionMat);
                                        var eyeRegion = eye.Convert(eyeRegionMat);
                                        foreach (var e in eyeRegion)
                                        {
                                            var eyeRect = e;
                                            eyeRect.Offset(f.X, f.Y);
                                            eyes.Add(eyeRect);
                                        }
                                    }
                                }
                        }

                        watch.Stop();
                    }
                else
#endif
                    using (var face = new CascadeClassifier(faceFileName))
                    using (var eye = new CascadeClassifier(eyeFileName))
                    {
                        watch = Stopwatch.StartNew();

                        using (var ugray = new UMat())
                        {
                            CvInvoke.CvtColor(image, ugray, ColorConversion.Bgr2Gray);

                            //normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(ugray, ugray);

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel
                            var facesDetected = face.DetectMultiScale(
                                ugray,
                                1.1,
                                10,
                                new Size(20, 20));

                            faces.AddRange(facesDetected);

                            foreach (var f in facesDetected)
                                //Get the region of interest on the faces
                                using (var faceRegion = new UMat(ugray, f))
                                {
                                    var eyesDetected = eye.DetectMultiScale(
                                        faceRegion,
                                        1.1,
                                        10,
                                        new Size(20, 20));

                                    foreach (var e in eyesDetected)
                                    {
                                        var eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }
                        }

                        watch.Stop();
                    }

                detectionTime = watch.ElapsedMilliseconds;
            }
        }

        #endregion
    }
}