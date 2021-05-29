using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D11;

namespace Remote
{
    public class ScreenCapturer
    {
        System.Drawing.Size screenSize;
        Factory1 factory;
        Adapter1 adapter;
        SharpDX.Direct3D11.Device device;
        Output output;
        Output1 output1;
        OutputDuplication duplicatedOutput;

        Texture2D screenTexture;

        ImageCodecInfo codec = null;

        public ScreenCapturer()
        {
            factory = new Factory1();
            //Get first adapter
            adapter = factory.GetAdapter1(0);
            //Get device from adapter
            device = new SharpDX.Direct3D11.Device(adapter);
            //Get front buffer of the adapter
            output = adapter.GetOutput(0);
            output1 = output.QueryInterface<Output1>();

            Update();

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg")
                    this.codec = codec;
            }
        }

        public void Update()
        {
            // Width/Height of desktop to capture
            screenSize = Screen.PrimaryScreen.Bounds.Size;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = screenSize.Width,
                Height = screenSize.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            screenTexture = new Texture2D(device, textureDesc);

            if (duplicatedOutput != null) duplicatedOutput.Dispose();
            duplicatedOutput = output1.DuplicateOutput(device);
        }

        public byte[] CaptureScreen(int width, int height, int quality = 75)
        {
            var screenShot = GetScreen();
            if (screenShot == null) return null;

            using (var bitmap = new Bitmap(width, height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(screenShot, 0, 0, width, height);
                    screenShot.Dispose();
                }
                using (var ms = new MemoryStream())
                {
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                    bitmap.Save(ms, codec, ep);
                    return ms.ToArray();
                }
            }
        }

        public byte[] CaptureScreen(int quality = 75)
        {
            var bitmap = GetScreen();
            if (bitmap == null) return null;

            using (var ms = new MemoryStream())
            {
                EncoderParameters ep = new EncoderParameters();
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                bitmap.Save(ms, codec, ep);
                bitmap.Dispose();
                return ms.ToArray();
            }
        }

        public Bitmap GetScreen()
        {
            if (screenSize != Screen.PrimaryScreen.Bounds.Size)
            {
                Update();
                Debug.WriteLine("ResolutionChanged");
            }         

            try
            {
                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation duplicateFrameInformation;

                // Try to get duplicated frame within given time is ms // DEFAULT 5
                duplicatedOutput.AcquireNextFrame(5, out duplicateFrameInformation, out screenResource);

                // copy resource into memory that can be accessed by the CPU
                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                // Get the desktop capture texture
                var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                // Create Drawing.Bitmap
                var bitmap = new Bitmap(screenSize.Width, screenSize.Height, PixelFormat.Format32bppArgb);
                var boundsRect = new System.Drawing.Rectangle(0, 0, screenSize.Width, screenSize.Height);

                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var sourcePtr = mapSource.DataPointer;
                var destPtr = mapDest.Scan0;
                for (int y = 0; y < screenSize.Height; y++)
                {
                    // Copy a single line 
                    Utilities.CopyMemory(destPtr, sourcePtr, screenSize.Width * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                }

                // Release source and dest locks
                bitmap.UnlockBits(mapDest);
                device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                screenResource.Dispose();
                duplicatedOutput.ReleaseFrame();
                return bitmap;
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    Trace.TraceError(e.Message);
                    Trace.TraceError(e.StackTrace);
                }
                return null;
            }
        }
    }
}
