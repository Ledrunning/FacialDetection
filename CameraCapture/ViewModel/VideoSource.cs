namespace CameraCaptureWPF.ViewModel
{
    public class VideoSource
    {
        public VideoSource(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}