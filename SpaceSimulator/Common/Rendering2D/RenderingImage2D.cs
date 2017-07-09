using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

namespace SpaceSimulator.Common.Rendering2D
{
    /// <summary>
    /// Represents an image managed by the <see cref="RenderingManager2D"/> class.
    /// </summary>
    public class RenderingImage2D : IRenderingResource2D
    {
        private readonly int stride;
        private readonly BitmapProperties bitmapProperties;
        private readonly DataStream dataStream;

        private Bitmap currentBitmap;

        /// <summary>
        /// Returns the size of the image
        /// </summary>
        public Size2 Size { get; }

        /// <summary>
        /// Creates a new image from the given file
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        public RenderingImage2D(string fileName)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(fileName))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                this.bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(
                    Format.B8G8R8A8_UNorm,
                    SharpDX.Direct2D1.AlphaMode.Premultiplied));
                this.Size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                this.stride = bitmap.Width * sizeof(int);
                this.dataStream = new DataStream(bitmap.Height * stride, true, true);

                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                // Convert all pixels 
                var width = bitmap.Width;
                var height = bitmap.Height;

                //for (int y = 0; y < height; y++)
                //{
                //    int offset = bitmapData.Stride * y;
                //    for (int x = 0; x < width; x++)
                //    {
                //        // Not optimized 
                //        byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                //        byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                //        byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                //        byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                //        int rgba = R | (G << 8) | (B << 16) | (A << 24);
                //        this.dataStream.Write(rgba);
                //    }
                //}

                Utilities.CopyMemory(this.dataStream.DataPointer, bitmapData.Scan0, width * height * sizeof(int));

                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Creates a new image from the given memory pointer
        /// </summary>
        /// <param name="bitmapProperties">The properties for the image</param>
        /// <param name="size">The size of the image</param>
        /// <param name="dataStream">A pointer to the memory</param>
        public RenderingImage2D(BitmapProperties bitmapProperties, Size2 size, DataStream dataStream)
        {
            this.stride = sizeof(int) * size.Width;
            this.bitmapProperties = bitmapProperties;
            this.Size = size;
            this.dataStream = dataStream;
        }

        /// <summary>
        /// Indicates if the internal resources has been bound to a device context
        /// </summary>
        public bool HasBoundResources => this.currentBitmap != null;

        /// <summary>
        /// Applies the internal resource to the given function.
        /// </summary>
        /// <remarks>It is possible that the resource is null.</remarks>
        /// <param name="fn">The function to apply</param>
        public void ApplyResource(Action<Bitmap> fn)
        {
            fn(this.currentBitmap);
        }

        /// <summary>
        /// Updates the internal image to the given device context
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        public void Update(DeviceContext deviceContext)
        {
            if (this.currentBitmap != null)
            {
                this.currentBitmap.Dispose();
            }

            this.dataStream.Position = 0;
            this.currentBitmap = new Bitmap(deviceContext, this.Size, this.dataStream, this.stride, this.bitmapProperties);
        }

        /// <summary>
        /// Draws the image at the given position
        /// </summary>
        /// <param name="deviceContext">The device context</param>
        /// <param name="position">The position</param>
        public void Draw(DeviceContext deviceContext, Vector2 position)
        {
            deviceContext.DrawBitmap(
                this.currentBitmap,
                1.0f,
                SharpDX.Direct2D1.InterpolationMode.Linear,
                new RectangleF(0, 0, this.Size.Width, this.Size.Height),
                Matrix.Translation(position.X, position.Y, 0));
        }

        public void Dispose()
        {
            this.dataStream.Dispose();

            if (this.currentBitmap != null)
            {
                this.currentBitmap.Dispose();
            }
        }
    }

}
