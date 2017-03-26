using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Jpeg.Tools.UnitTests
{
    public class JpegToolTest
    {
        [Test]
        public void Transform_FileToFile()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));
            var input = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(input, jpeg);

            if (JpegTools.Transform(input, output, copy: false, optimize: true))
            {
                var res = File.ReadAllBytes(output);
                Assert.That(res.Length, Is.LessThan(jpeg.Length));
            }
            else
                Assert.Fail();

            Assert.IsTrue(IsJpegImage(output));
            File.Delete(input);
            File.Delete(output);
        }

        [Test]
        public void Transform_FileToFileGrayScale()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));
            var input = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(input, jpeg);

            if (JpegTools.Transform(input, output, false, true, grayscale: true))
            {
                var res = File.ReadAllBytes(output);
                Assert.That(res.Length, Is.LessThan(jpeg.Length));
            }
            else
                Assert.Fail();

            Assert.IsTrue(IsJpegImage(output));
            File.Delete(input);
            File.Delete(output);
        }

        [Test]
        public void Transform_MemoryToFile()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());

            if (JpegTools.Transform(jpeg, output, false, true))
            {
                var res = File.ReadAllBytes(output);
                Assert.That(res.Length, Is.LessThan(jpeg.Length));
            }
            else
                Assert.Fail();
            Assert.IsTrue(IsJpegImage(output));
            File.Delete(output);
        }


        [Test]
        public void Transform_MemoryToMemory()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));

            if (JpegTools.Transform(jpeg, out byte[] res, copy: false, optimize: true))
            {
                Assert.That(res.Length, Is.LessThan(jpeg.Length));
            }
            else
                Assert.Fail();

            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(output, res);
            Assert.IsTrue(IsJpegImage(output));
            File.Delete(output);
        }

        [Test]
        public void Transform_MemoryToMemory_CorruptedInput()
        {
            var nonjpeg = new byte[100];

            var retval = JpegTools.Transform(nonjpeg, out byte[] res, copy: false, optimize: true);
            
            Assert.IsFalse(retval);
        }

        [Test]
        public void Transform_MemoryToFile_CorruptedInput()
        {
            var nonjpeg = new byte[100];
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());

            var retval = JpegTools.Transform(nonjpeg, output, copy: false, optimize: true);

            Assert.IsFalse(retval);
        }

        [Test]
        public void Transform_FileToFile_CorruptedInput()
        {
            var nonjpeg = new byte[100];
            var input = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(input, nonjpeg);

            var retval = JpegTools.Transform(input, output, copy: false, optimize: true);

            Assert.IsFalse(retval);
        }

        [Test]
        public void Transform_MemoryToMemory_Arithmetics()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));

            if (JpegTools.Transform(jpeg, out byte[] res, copy: false, optimize: true))
                if (JpegTools.Transform(jpeg, out byte[] resArith, copy: false, optimize: false, arithmetic: true))
                {
                    Assert.That(jpeg.Length, Is.GreaterThan(res.Length));
                    Assert.That(res.Length, Is.GreaterThan(resArith.Length));
                }
                else
                    Assert.Fail();

            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());
            File.WriteAllBytes(output, res);
            Assert.IsTrue(IsJpegImage(output));
            File.Delete(output);
        }

        [Test, Explicit]
        public void Transform_MemoryToMemoryOverrun()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));
            byte[] res;
            for (var i = 0; i < 1000; i++)
            {
                if (!JpegTools.Transform(jpeg, out res, false, true))
                {
                    Assert.Fail();
                }
            }
            Assert.Pass();

        }

        [Test, Explicit]
        public void Transform_MemoryToFileOverrun()
        {
            var jpeg = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tu.jpg"));
            var output = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetRandomFileName());

            for (var i = 0; i < 1000; i++)
            {
                if (!JpegTools.Transform(jpeg, output, false, true))
                {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(IsJpegImage(output));
            File.Delete(output);
        }


        bool IsJpegImage(string filename)
        {
            try
            {
                using (var img = System.Drawing.Image.FromFile(filename))
                {
                    return img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
        }
    }
}
