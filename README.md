# jpeg.tools

This is a C# wrapper on jpegtran interpretation (libjpeg). This project targets mainly on jpeg size reducing task.

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
