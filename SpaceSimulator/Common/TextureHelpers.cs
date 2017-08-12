using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common
{
	/// <summary>
	/// Contains helper methods for textures
	/// </summary>
	public static class TextureHelpers
	{
        /// <summary>
        /// Loads a texture from a bitmap
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="bitmap">The bitmap</param>
        /// <param name="textureDescription">The texture description</param>
        public static Texture2D FromBitmap(Device graphicsDevice, System.Drawing.Bitmap bitmap, Texture2DDescription? textureDescription = null)
        {
            var bitmapFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            var bitmapRectangle = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            if (bitmap.PixelFormat != bitmapFormat)
            {
                bitmap = bitmap.Clone(bitmapRectangle, bitmapFormat);
            }

            var data = bitmap.LockBits(bitmapRectangle, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmapFormat);

            var description = textureDescription ?? new Texture2DDescription()
            {
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                OptionFlags = ResourceOptionFlags.None,
            };

            description.Width = bitmap.Width;
            description.Height = bitmap.Height;
            description.ArraySize = 1;
            description.MipLevels = 1;
            description.SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0);

            var texture = new Texture2D(graphicsDevice, description, new DataRectangle(data.Scan0, data.Stride));

            bitmap.UnlockBits(data);
            return texture;
        }

        /// <summary>
        /// Loads a texture from a file
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="fileName">The path of the file</param>
        /// <param name="textureDescription">The texture description</param>
        public static Texture2D FromFile(Device graphicsDevice, string fileName, Texture2DDescription? textureDescription = null)
        {
            using (var bitmap = new System.Drawing.Bitmap(fileName))
            {
                return FromBitmap(graphicsDevice, bitmap, textureDescription);
            }
        }

        /// <summary>
        /// Loads the given array of textures into an array
        /// </summary>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="context">The context</param>
        /// <param name="textureNames">The name of the texture files</param>
        /// <param name="format">The format to use</param>
        public static ShaderResourceView LoadTextureArray(
            Device graphicsDevice,
            DeviceContext context,
            string[] textureNames,
            SharpDX.DXGI.Format format = SharpDX.DXGI.Format.B8G8R8A8_UNorm)
        {
            int numTextures = textureNames.Length;

            //Load the textures
            var textures = textureNames
                .Select(textureName =>
                {
                    //var loadInfo = ImageLoadInformation.Default;
                    //loadInfo.Usage = ResourceUsage.Staging;
                    //loadInfo.BindFlags = BindFlags.None;
                    //loadInfo.Format = format;
                    //loadInfo.MipFilter = FilterFlags.Linear;
                    //loadInfo.Filter = FilterFlags.None;
                    //loadInfo.CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write;
                    //loadInfo.FirstMipLevel = 0;

                    //return Texture2D.FromFile<Texture2D>(
                    //    graphicsDevice,
                    //    textureName,
                    //    loadInfo);

                    var textureDescription = new Texture2DDescription()
                    {
                        Usage = ResourceUsage.Staging,
                        Format = format,
                        CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write
                    };

                    return FromFile(graphicsDevice, textureName, textureDescription);
                })
                .ToArray();

            var elementDescription = textures[0].Description;
            var textureArrayDescription = new Texture2DDescription()
            {
                Width = elementDescription.Width,
                Height = elementDescription.Height,
                MipLevels = elementDescription.MipLevels,
                Format = elementDescription.Format,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = numTextures
            };

            //Create the texture array
            using (var textureArray = new Texture2D(graphicsDevice, textureArrayDescription))
            {
                //Update the texture array
                for (int textureIndex = 0; textureIndex < numTextures; textureIndex++)
                {
                    var texture = textures[textureIndex];
                    int mipLevels = texture.Description.MipLevels;

                    for (int mipLevel = 0; mipLevel < mipLevels; mipLevel++)
                    {
                        var subResource = context.MapSubresource(texture, mipLevel, MapMode.Read, MapFlags.None);
                        context.UpdateSubresource(
                            subResource,
                            textureArray,
                            Texture2D.CalculateSubResourceIndex(mipLevel, textureIndex, mipLevels));
                        context.UnmapSubresource(texture, mipLevel);
                    }
                }

                //Dispose the loaded textures
                foreach (var texture in textures)
                {
                    texture.Dispose();
                }

                //Create the texture array resource view
                return new ShaderResourceView(graphicsDevice, textureArray, new ShaderResourceViewDescription()
                {
                    Format = textureArray.Description.Format,
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                    {
                        MostDetailedMip = 0,
                        MipLevels = textureArray.Description.MipLevels,
                        FirstArraySlice = 0,
                        ArraySize = textureArray.Description.ArraySize
                    }
                });
            }
        }

        /// <summary>
        /// Creates a textue from raw data
        /// </summary>
        /// <typeparam name="T">The type of the data. This should match the format parameter.</typeparam>
        /// <param name="graphicsDevice">The graphics device</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="format">The format of the data</param>
        /// <param name="data">The raw data</param>
        public static ShaderResourceView FromRaw<T>(Device graphicsDevice, int width, int height, SharpDX.DXGI.Format format, T[] data)
			where T : struct
		{
			var description = new Texture2DDescription()
			{
				Width = width,
				Height = height,
				MipLevels = 1,
				ArraySize = 1,
				Format = format,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource
			};

			using (var stream = DataStream.Create(data, true, false))
			{
				var dataRectangle = new DataRectangle(
					stream.DataPointer,
					description.Width * (int)SharpDX.DXGI.FormatHelper.SizeOfInBytes(description.Format));

				using (var texture = new Texture2D(graphicsDevice, description, dataRectangle))
				{
					return new ShaderResourceView(graphicsDevice, texture);
				}
			}
		}

		/// <summary>
		/// Creates a random texture
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="random">The random generator</param>
		/// <param name="numRandomValues">The number of random values</param>
		public static ShaderResourceView CreateRandomTexture(Device graphicsDevice, Random random = null, int numRandomValues = 1024)
		{
			if (random == null)
			{
				random = new Random();
			}

			//Create the random values
			var randomValues = new Vector4[numRandomValues];
			for (int i = 0; i < numRandomValues; i++)
			{
				randomValues[i] = random.NextVector4(new Vector4(-1), new Vector4(1));
			}

			var description = new Texture1DDescription()
			{
				Width = numRandomValues,
				MipLevels = 1,
				ArraySize = 1,
				Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource
			};

			using (var stream = DataStream.Create(randomValues, true, false))
			{
				using (var texture = new Texture1D(graphicsDevice, description, stream))
				{
					return new ShaderResourceView(graphicsDevice, texture);
				}
			}
		}
	}
}
