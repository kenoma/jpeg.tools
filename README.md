# jpeg.tools

This is a C# wrapper on jpegtran interpretation (libjpeg). This project targets mainly on lossles jpeg size reducing task. It's ~20 times faster than pure managed implementation with <a href="https://bitmiracle.com/libjpeg/">LibJpeg.NET</a>.

Usage
```
  if (JpegTools.Transform(jpeg, out byte[] optimized, copy: false, optimize: true))
  {
    //do smth useful 
  }
  else
  {
    //report on error or use origin
  }
```
