﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Jpeg.Tools
{
    public static class JpegTools
    {
        private const string dll = "native.libjpeg.dll";
        private static readonly Destructor _finalise;
        private static IntPtr _lib;

        static JpegTools()
        {
            var dllFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dll);
            _finalise = new Destructor();

            if (!File.Exists(dllFile))
            {
                if (IntPtr.Size == 8)
                    File.WriteAllBytes(dllFile, Resources.native_libjpeg_x64);
                else
                    File.WriteAllBytes(dllFile, Resources.native_libjpeg_x86);
            }
            _lib = LoadLibrary(dllFile);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <param name="output"></param>
        /// <param name="outSize"></param>
        /// <param name="copy"></param>
        /// <param name="optimize"></param>
        /// <param name="progressive"></param>
        /// <param name="grayscale"></param>
        /// <param name="trim"></param>
        /// <param name="arithmetic"></param>
        /// <param name="scale_m">Scaling factor m, currently supported scale factors are M/N with all M from 1 to 16, where N is the source DCT size, which is 8 for baseline JPEG</param>
        /// <param name="scale_n">Scalling factor n, Currently supported scale factors are M/N with all M from 1 to 16, where N is the source DCT size, which is 8 for baseline JPEG</param>
        /// <param name="crop">Enable cropping</param>
        /// <param name="cropx">Crop x</param>
        /// <param name="cropy">Crop y</param>
        /// <param name="cropw">Crop width</param>
        /// <param name="croph">Crop Height</param>
        /// <returns></returns>
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern byte OptimizeMemoryToMemory([In] byte[] input, [In] uint size, [In, Out] byte[] output, [Out] out uint outSize, byte copy, byte optimize, byte progressive, byte grayscale, byte trim, byte arithmetic, byte scale_m, byte scale_n, byte crop, int cropx, int cropy, int cropw, int croph);

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
        /// <param name="scaleFactorM">M/N, where M is in [1,16] any value outside of this range leads to ignoring scaling</param>
        /// <param name="scaleFactorN">M/N, where N is the source DCT size</param>
        /// <returns></returns>
        static public bool Transform(byte[] jpegInMemory, out byte[] resultingJpeg, bool copy = false, bool optimize = true, bool progressive = false, bool grayscale = false, bool trim = false, bool arithmetic = false, byte scaleFactorM=0, byte scaleFactorN=8, bool crop = false, int cropx =-1, int cropy =-1, int cropw =-1, int croph=-1)
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
                    (byte)(arithmetic ? 1 : 0),
                    scaleFactorM,
                    scaleFactorN,
                    (byte)(crop ? 1 : 0),
                    cropx,
                    cropy,
                    cropw,
                    croph);

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
