using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Jpeg.Tools
{
    public static class JpegTools
    {
        private const string dll = "native.libjpeg.dll";
        private static readonly Destructor _finalise;
        private static IntPtr _lib;

        static JpegTools()
        {
            var subfolder = IntPtr.Size==8 ? "x64" : "x86";
            _finalise = new Destructor();
            //Log.Info($"Trying to load {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subfolder, dll)}...");
            _lib = LoadLibrary(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subfolder), dll));
            if (_lib == IntPtr.Zero)
            {
                throw new DllNotFoundException(dll);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte OptimizeFileToFile([In] string inpfilename, [In] string outfilename, byte copy, byte optimize, byte progressive, byte grayscale, byte trim, byte arithmetic);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte OptimizeMemoryToFile([In] byte[] input, int size, [In] string outfilename, byte copy, byte optimize, byte progressive, byte grayscale, byte trim, byte arithmetic);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte OptimizeMemoryToMemory([In] byte[] input, [In] uint size, [In, Out] byte[] output, [Out] out uint outSize, byte copy, byte optimize, byte progressive, byte grayscale, byte trim, byte arithmetic);

        /// <summary>
        /// Tries the optimize JPEG.
        /// </summary>
        /// <param name="inpFilename">The input filename.</param>
        /// <param name="outFilename">The output filename.</param>
        /// <param name="copy">The copy.</param>
        /// <param name="optimize">if set to <c>true</c> [optimize].</param>
        /// <param name="progressive">if set to <c>true</c> [progressive].</param>
        /// <param name="grayscale">if set to <c>true</c> [grayscale].</param>
        /// <param name="trim">if set to <c>true</c> [trim].</param>
        /// <param name="arithmetic">if set to <c>true</c> [arithmetic].</param>
        /// <returns></returns>
        static public bool Transform(string inpFilename, string outFilename, bool copy = false, bool optimize = true, bool progressive = false, bool grayscale = false, bool trim = false, bool arithmetic = false)
        {
            if (!File.Exists(inpFilename))
                return false;

            if (_lib == null || _lib == IntPtr.Zero)
                return false;
            try
            {
                return OptimizeFileToFile(inpFilename, outFilename,
                    (byte)(copy ? 1 : 0),
                    (byte)(optimize ? 1 : 0),
                    (byte)(progressive ? 1 : 0),
                    (byte)(grayscale ? 1 : 0),
                    (byte)(trim ? 1 : 0),
                    (byte)(arithmetic ? 1 : 0)) == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Tries the optimize JPEG.
        /// </summary>
        /// <param name="jpegInMemory">The JPEG in memory.</param>
        /// <param name="outFilename">The out filename.</param>
        /// <param name="copy">The copy.</param>
        /// <param name="optimize">if set to <c>true</c> [optimize].</param>
        /// <param name="progressive">if set to <c>true</c> [progressive].</param>
        /// <param name="grayscale">if set to <c>true</c> [grayscale].</param>
        /// <param name="trim">if set to <c>true</c> [trim].</param>
        /// <param name="arithmetic">if set to <c>true</c> [arithmetic].</param>
        /// <returns></returns>
        static public bool Transform(byte[] jpegInMemory, string outFilename, bool copy = false, bool optimize = true, bool progressive = false, bool grayscale = false, bool trim = false, bool arithmetic = false)
        {
            if (_lib == null || _lib == IntPtr.Zero)
                return false;

            if (jpegInMemory == null)
                return false;

            if (jpegInMemory.Length == 0)
                return false;
            try
            {
                return OptimizeMemoryToFile(jpegInMemory, jpegInMemory.Length, outFilename,
                    (byte)(copy ? 1 : 0),
                    (byte)(optimize ? 1 : 0),
                    (byte)(progressive ? 1 : 0),
                    (byte)(grayscale ? 1 : 0),
                    (byte)(trim ? 1 : 0),
                    (byte)(arithmetic ? 1 : 0)) == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Tries the optimize JPEG.
        /// </summary>
        /// <param name="jpegInMemory">The JPEG in memory.</param>
        /// <param name="resultingJpeg">The resulting JPEG.</param>
        /// <param name="copy">The copy.</param>
        /// <param name="optimize">if set to <c>true</c> [optimize].</param>
        /// <param name="progressive">if set to <c>true</c> [progressive].</param>
        /// <param name="grayscale">if set to <c>true</c> [grayscale].</param>
        /// <param name="trim">if set to <c>true</c> [trim].</param>
        /// <param name="arithmetic">if set to <c>true</c> [arithmetic].</param>
        /// <returns></returns>
        static public bool Transform(byte[] jpegInMemory, out byte[] resultingJpeg, bool copy = false, bool optimize = true, bool progressive = false, bool grayscale = false, bool trim = false, bool arithmetic = false)
        {
            resultingJpeg = null;

            if (_lib == null || _lib == IntPtr.Zero)
                return false;

            if (jpegInMemory == null)
                return false;

            if (jpegInMemory.Length == 0)
                return false;
            try
            {
                var tmp = new byte[jpegInMemory.Length];
                uint outSize = 0;
                var res = OptimizeMemoryToMemory(jpegInMemory, (uint)jpegInMemory.Length, tmp, out outSize,
                    (byte)(copy ? 1 : 0),
                    (byte)(optimize ? 1 : 0),
                    (byte)(progressive ? 1 : 0),
                    (byte)(grayscale ? 1 : 0),
                    (byte)(trim ? 1 : 0),
                    (byte)(arithmetic ? 1 : 0));

                if (outSize > jpegInMemory.Length)
                    return false;

                if (res == 1)
                {
                    var outp = new byte[outSize];
                    Array.Copy(tmp, outp, outSize);
                    resultingJpeg = outp;
                }
                return res == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        ///     An helper to implement "destructor" for static class
        /// </summary>
        private sealed class Destructor
        {
            ~Destructor()
            {
                if (_lib != null && _lib != IntPtr.Zero)
                    FreeLibrary(_lib);
            }
        }
    }
}
