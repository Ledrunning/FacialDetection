using System.Configuration;

namespace CameraCaptureWPF
{
    public static class ApplicationConfiguration
    {
        public static string EyeHaar = ConfigurationManager.AppSettings["eyeHaar"];
        public static string FaceHaar = ConfigurationManager.AppSettings["faceHaar"];
    }
}