using System;
using System.Drawing;
using Microsoft.Kinect;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.IO;
using Media = System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImaginativeUniversal.Kinect
{
    public static class KinectImageExtensions
    {


        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);


        /// <summary>
        /// Converts Kinect byte array to Bitmap.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The pixel format.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this byte[] data, int width, int height
            , PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        /// <summary>
        /// Converts Kinect byte array to Bitmap.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this short[] data, int width, int height
            , PixelFormat format)
        {
            var bitmap = new Bitmap(width, height, format);

            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        /// <summary>
        /// Convert Kinect byte array to BitmapSource.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="format">The format.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static Media.Imaging.BitmapSource ToBitmapSource(this byte[] data
            , Media.PixelFormat format, int width, int height)
        {
            return Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }

        public static Media.Imaging.BitmapSource ToBitmapSource(this short[] data
        , Media.PixelFormat format, int width, int height)
        {
            return Media.Imaging.BitmapSource.Create(width, height, 96, 96
                , format, null, data, width * format.BitsPerPixel / 8);
        }

        // bitmap methods

        /// <summary>
        /// Gets Bitmap image from Kinect ColorImageFrame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this ColorImageFrame image, PixelFormat format)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmap(image.Width, image.Height
                , format);
        }

        /// <summary>
        /// Gets Bitmap image from Kinect DepthImageFrame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this DepthImageFrame image, PixelFormat format)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new short[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmap(image.Width, image.Height
                , format);
        }

        /// <summary>
        /// Gets Bitmap image from Kinect ColorImageFrame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this ColorImageFrame image)
        {
            return image.ToBitmap(PixelFormat.Format32bppRgb);
        }

        public static Bitmap ToBitmap(this DepthImageFrame image)
        {
            return image.ToBitmap(PixelFormat.Format16bppRgb565);
        }

        // bitmapsource methods

        /// <summary>
        /// Gets Bitmap image from Kinect ColorImageFrame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Media.Imaging.BitmapSource ToBitmapSource(this ColorImageFrame image)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new byte[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmapSource(Media.PixelFormats.Bgr32, image.Width, image.Height);
        }

        /// <summary>
        /// Gets Bitmap image from Kinect DepthImageFrame.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static Media.Imaging.BitmapSource ToBitmapSource(this DepthImageFrame image)
        {
            if (image == null || image.PixelDataLength == 0)
                return null;
            var data = new short[image.PixelDataLength];
            image.CopyPixelDataTo(data);
            return data.ToBitmapSource(Media.PixelFormats.Bgr555, image.Width, image.Height);
        }

        /// <summary>
        /// Converts Kinect byte array to transparent BitmapSource.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static Media.Imaging.BitmapSource ToTransparentBitmapSource(this byte[] data
            , int width, int height)
        {
            return data.ToBitmapSource(Media.PixelFormats.Bgra32, width, height);
        }


        /// <summary>
        /// Converts Bitmap to BitmapSource.
        /// </summary>
        /// <param name="bitmap">The bitmap image.</param>
        /// <returns></returns>
        public static Media.Imaging.BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null) return null;
            IntPtr ptr = bitmap.GetHbitmap();
            var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            ptr,
            IntPtr.Zero,
            Int32Rect.Empty,
            Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(ptr);
            return source;
        }

        /// <summary>
        /// Converts BitmapSource to Bitmap.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this Media.Imaging.BitmapSource source)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new Media.Imaging.PngBitmapEncoder();
                enc.Frames.Add(Media.Imaging.BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }


        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor)
        {
            return sensor.RenderLivePlayer(Color.Transparent, Color.White);
        }

        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <param name="foreColor">A foreground color for player outline.  The default transparent uses original rgb colors.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor, Color foreColor)
        {
            return sensor.RenderLivePlayer(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, foreColor, Color.White);
        }

        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <param name="foreColor">A foreground color for player outline.  Transparent uses original rgb colors.</param>
        /// <param name="backgroundColor">A background color.  Transparent uses original rgb colors.  White is the default.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor, Color foreColor, Color backgroundColor)
        {
            return sensor.RenderLivePlayer(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, foreColor, backgroundColor);
        }

        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <param name="depthWidth">Width of the depth.</param>
        /// <param name="depthHeight">Height of the depth.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor, int depthWidth, int depthHeight)
        {
            return sensor.RenderLivePlayer(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, Color.Transparent, Color.White);
        }

        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <param name="depthWidth">Width of the depth.</param>
        /// <param name="depthHeight">Height of the depth.</param>
        /// <param name="foreColor">A foreground color for player outline.  Transparent uses original rgb colors.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor, int depthWidth, int depthHeight, Color foreColor)
        {
            return sensor.RenderLivePlayer(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, foreColor, Color.White);
        }


        /// <summary>
        /// Renders the live player.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <param name="depthWidth">Width of the depth.</param>
        /// <param name="depthHeight">Height of the depth.</param>
        /// <param name="foreColor">A foreground color for player outline.  Transparent uses original rgb colors.</param>
        /// <param name="backgroundColor">A background color.  Transparent uses original rgb colors.  White is the default.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderLivePlayer(this KinectSensor sensor, int depthWidth, int depthHeight, Color foreColor, Color backgroundColor)
        {
            if (sensor.DepthStream.IsEnabled == false || sensor.SkeletonStream.IsEnabled == false || sensor.ColorStream.IsEnabled == false)
                throw new InvalidOperationException("Open all the data streams before calling this method.");

            WriteableBitmap target = new WriteableBitmap(depthWidth, depthHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            var targetRect = new System.Windows.Int32Rect(0, 0, depthWidth, depthHeight);

            sensor.AllFramesReady += (s, e) =>
            {
                bool isTransparent = (foreColor == Color.Transparent);
                bool isBackgroundTransparent = (backgroundColor == Color.Transparent);
                bool showBackground = (backgroundColor != Color.White);
                byte R = foreColor.R;
                byte G = foreColor.G;
                byte B = foreColor.B;
                byte backR = backgroundColor.R;
                byte backG = backgroundColor.G;
                byte backB = backgroundColor.B;
                DepthImagePixel[] depthBits = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                ColorImagePoint[] colorCoordinates = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                byte[] colorBits = new byte[sensor.ColorStream.FramePixelDataLength];
                byte[] output = new byte[depthWidth * depthHeight * 4];
                int colorStride = sensor.ColorStream.FrameBytesPerPixel * sensor.ColorStream.FrameWidth;

                using (var depthFrame = e.OpenDepthImageFrame())
                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (depthFrame != null && colorFrame != null)
                    {

                        depthFrame.CopyDepthImagePixelDataTo(depthBits);
                        colorFrame.CopyPixelDataTo(colorBits);
                        int outputIndex = 0;

                        if (colorCoordinates == null)
                            colorCoordinates =
                            new ColorImagePoint[depthFrame.PixelDataLength];

                        sensor.CoordinateMapper.MapDepthFrameToColorFrame(depthFrame.Format, depthBits,
                                                        colorFrame.Format,
                                                        colorCoordinates);

                        for (int depthIndex = 0;
                            depthIndex < depthBits.Length;
                            depthIndex++, outputIndex += 4)
                        {

                            var playerIndex = depthBits[depthIndex].PlayerIndex;

                            var colorPoint = colorCoordinates[depthIndex];

                            var colorPixelIndex = (colorPoint.X * colorFrame.BytesPerPixel) +
                                                (colorPoint.Y * colorStride);

                            if ((playerIndex > 0 && isTransparent) || (playerIndex == 0 && isBackgroundTransparent))
                            {
                                output[outputIndex] = colorBits[colorPixelIndex + 0];
                                output[outputIndex + 1] = colorBits[colorPixelIndex + 1];
                                output[outputIndex + 2] = colorBits[colorPixelIndex + 2];
                            }
                            else if (playerIndex > 0)
                            {
                                output[outputIndex] = B;
                                output[outputIndex + 1] = G;
                                output[outputIndex + 2] = R;
                            }
                            else if (showBackground)
                            {
                                output[outputIndex] = backB;
                                output[outputIndex + 1] = backG;
                                output[outputIndex + 2] = backR;
                            }
                            //alpha channel
                            output[outputIndex + 3] = (playerIndex > 0 || showBackground) ? (byte)255 : (byte)0;

                        }
                        target.WritePixels(targetRect, output,
                                        depthFrame.Width * 4, 0);

                    }

                }

            };
            return target;
        }

        /// <summary>
        /// Renders the predator thermal vision view.
        /// </summary>
        /// <param name="sensor">The sensor.</param>
        /// <returns></returns>
        public static WriteableBitmap RenderPredatorView(this KinectSensor sensor)
        {
            if (sensor.DepthStream.IsEnabled == false || sensor.SkeletonStream.IsEnabled == false || sensor.ColorStream.IsEnabled == false)
                throw new InvalidOperationException("Open all the data streams before calling this method.");

            WriteableBitmap target = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            var targetRect = new System.Windows.Int32Rect(0, 0, sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight);

            sensor.AllFramesReady += (s, e) =>
            {
                DepthImagePixel[] depthBits = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                ColorImagePoint[] colorCoordinates = new ColorImagePoint[sensor.DepthStream.FramePixelDataLength];
                byte[] colorBits = new byte[sensor.ColorStream.FramePixelDataLength];
                byte[] output = new byte[sensor.DepthStream.FrameWidth * sensor.DepthStream.FrameHeight * 4];
                int colorStride = sensor.ColorStream.FrameBytesPerPixel * sensor.ColorStream.FrameWidth;

                using (var depthFrame = e.OpenDepthImageFrame())
                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (depthFrame != null && colorFrame != null)
                    {

                        depthFrame.CopyDepthImagePixelDataTo(depthBits);
                        colorFrame.CopyPixelDataTo(colorBits);
                        int outputIndex = 0;

                        if (colorCoordinates == null)
                            colorCoordinates =
                            new ColorImagePoint[depthFrame.PixelDataLength];

                        sensor.CoordinateMapper.MapDepthFrameToColorFrame(depthFrame.Format, depthBits,
                                                        colorFrame.Format,
                                                        colorCoordinates);

                        for (int depthIndex = 0;
                            depthIndex < depthBits.Length;
                            depthIndex++, outputIndex += 4)
                        {
                            var playerIndex = depthBits[depthIndex].PlayerIndex;
                            var depth = depthBits[depthIndex].Depth;
                            var colorPoint = colorCoordinates[depthIndex];
                            var colorPixelIndex = (colorPoint.X * colorFrame.BytesPerPixel) +
                                                (colorPoint.Y * colorStride);

                            if (playerIndex > 0)
                            {
                                output[outputIndex] = 0;
                                output[outputIndex + 1] = Convert.ToByte(depth % 255);
                                output[outputIndex + 2] = Convert.ToByte(depth % 120 + 134);
                                output[outputIndex + 3] = 255;
                            }

                            if (playerIndex == 0)
                            {
                                var b = colorBits[colorPixelIndex];
                                output[outputIndex] = b < (byte)100.0 ? (byte)0 : b;
                                output[outputIndex + 1] = b < (byte)100.0 ? (byte)0 : (byte)70;
                                output[outputIndex + 2] = 0;
                                output[outputIndex + 3] = 250;
                            }




                        }
                        target.WritePixels(targetRect, output,
                                        depthFrame.Width * 4, 0);

                    }

                }

            };
            return target;
        }

    }
}
