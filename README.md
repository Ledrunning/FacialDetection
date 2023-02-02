# Simple WPF MVVM Desktop application for facial detection in real time from web camera via Emgu
## This application use Emgu 3.x framework from https://www.emgu.com/wiki/index.php/Main_Page
To run app you should put these libraries into the bin folder (Debug or Release)
I used EMGU CV x64 version, so I put these libraries along this path *\bin\Debug\x64*
* concrt140.dll
* cvextern.dll
* msvcp140.dll
* opencv_videoio_ffmpeg411_64.dll.dll
* vcruntime140.dll

And don't forget about haarcascade_frontalface_default.xml just set copy to output directory in your project.

Logger is available in this project it writes all error and information data into a file.
You should find it in the bin folder and it called Logs

## For darcula or metro themes I used WPFToolkit and WPFToolkit.DataVisualization (JenniLe, Shimmy)
To change themes manually you should comment old and uncomment required theme in App.xaml f.e Metro Dark Theme:

<details> 
  <summary>Theme settings in Application.Resources</summary>
  
          ```xml
  
          ```
  
</details>

## UI Layuot: 

![Screenshot](uilayout.jpg)
