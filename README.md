# Simple WPF MVVM Desktop application for facial detection in real time from web camera via Emgu
## This application use Emgu 3.x framework
To run app you should put these libraries into the bin folder (Debug or Release)
* concrt140.dll
* cvextern.dll
* msvcp140.dll
* opencv_ffmpeg320_64.dll
* vcruntime140.dll

And don't forget about haarcascade_frontalface_default.xml just set copy to output directory in your project.

Logger is available in this project it writes all error and information data into a file.
You should find it in the bin folder and it called Logs

## For darcula or metro themes I used WPFToolkit and WPFToolkit.DataVisualization (JenniLe, Shimmy)
To change themes manually you should comment old and uncomment required theme in App.xaml f.e Metro Dark Theme:

```xml
      <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- IG Theme -->
                <ResourceDictionary Source="Themes/IG/IG.MSControls.Core.Implicit.xaml" />
                <ResourceDictionary Source="Themes/IG/IG.MSControls.Toolkit.Implicit.xaml" />

                <!-- Metro Theme -->
                <!--<ResourceDictionary Source="Themes/Metro/Metro.MSControls.Core.Implicit.xaml" />
                <ResourceDictionary Source="Themes/Metro/Metro.MSControls.Toolkit.Implicit.xaml" /> -->

                <!-- MetroDark Theme -->
                <!--<ResourceDictionary Source="Themes/MetroDark/MetroDark.MSControls.Core.Implicit.xaml" />
                <ResourceDictionary Source="Themes/MetroDark/MetroDark.MSControls.Toolkit.Implicit.xaml" /> -->
            </ResourceDictionary.MergedDictionaries>

            <!-- <SolidColorBrush x:Key="BackgroundKey" Color="#FFFFFF" /> Color="#FF181818" -->

            <!-- Dark Theme -->
            <SolidColorBrush x:Key="BackgroundKey" Color="#FFFFFF" />

            <Style x:Key="HeaderTextBlockStyle" TargetType="TextBlock">
                <Setter Property="FontSize" Value="22" />
                <Setter Property="FontFamily" Value="Segoe UI" />
                <Setter Property="Foreground" Value="#FF00AADE" />
            </Style>

            <Style x:Key="SubHeaderTextBlockStyle" TargetType="TextBlock">
                <Setter Property="FontSize" Value="18" />
                <Setter Property="FontFamily" Value="Segoe UI" />
                <Setter Property="Foreground" Value="#FF00AADE" />
            </Style>
        </ResourceDictionary>
    </Application.Resources>
```

## UI Layuot: 

![Screenshot](uilayout.jpg)
