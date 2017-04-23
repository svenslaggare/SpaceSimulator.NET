using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Effects;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.Common.Old
{
	/// <summary>
	/// Custom terrain vertex format
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct TerrainVertex
	{
		public Vector3 Position;
		public Vector2 TextureCoordinates;
		public Vector2 BoundsY;
	};

	/// <summary>
	/// Represents a terrain render
	/// </summary>
	public class TerrainRender : IDisposable
	{
		private readonly string heightMapFile;
		private readonly string layerMap0File;
		private readonly string layerMap1File;
		private readonly string layerMap2File;
		private readonly string layerMap3File;
		private readonly string layerMap4File;
		private readonly string blendMapFile;

		private readonly float heightScale;
		private readonly int heightMapWidth;
		private readonly int heightMapHeight;
		private readonly float cellSpacing;

		private static readonly int CellsPerPatch = 64;

		private TerrainEffect effect;
		private InputLayout inputLayout;

		private Buffer vertexBuffer;
		private Buffer indexBuffer;

		private ShaderResourceView layerMapArrayView;
		private ShaderResourceView blendMapView;
		private ShaderResourceView heightMapView;

		private readonly int numPatchVertices;
		private readonly int numPatchQuadFaces;

		private readonly int numPatchVerticesRows;
		private readonly int numPatchVerticesCols;

		private Material material;

		private Vector2[] patchBoundsY = null;
		private float[] heightMap = null;

		/// <summary>
		/// Creates a new terrain
		/// </summary>
		public TerrainRender(string heightMapFile, string layerMap0File, string layerMap1File, string layerMap2File,
			string layerMap3File, string layerMap4File, string blendMapFile,
			float heightScale, int heightMapWidth, int heightMapHeight, float cellSpacing)
		{
			this.material = new Material()
			{
				Ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Diffuse = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
				Specular = new Vector4(0.0f, 0.0f, 0.0f, 64.0f),
				Reflect = new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
			};

			this.heightMapFile = heightMapFile;
			this.layerMap0File = layerMap0File;
			this.layerMap1File = layerMap1File;
			this.layerMap2File = layerMap2File;
			this.layerMap3File = layerMap3File;
			this.layerMap4File = layerMap4File;
			this.blendMapFile = blendMapFile;

			this.heightScale = heightScale;
			this.heightMapWidth = heightMapWidth;
			this.heightMapHeight = heightMapHeight;
			this.cellSpacing = cellSpacing;

			// Divide heightmap into patches such that each patch has CellsPerPatch.
			this.numPatchVerticesRows = ((this.heightMapHeight - 1) / CellsPerPatch) + 1;
			this.numPatchVerticesCols = ((this.heightMapWidth - 1) / CellsPerPatch) + 1;

			this.numPatchVertices = this.numPatchVerticesRows * this.numPatchVerticesCols;
			this.numPatchQuadFaces = (this.numPatchVerticesRows - 1) * (this.numPatchVerticesCols - 1);

			this.World = Matrix.Identity;
		}

		/// <summary>
		/// Returns the width of the world
		/// </summary>
		public float Width
		{
			get { return (this.heightMapWidth - 1) * this.cellSpacing; }
		}

		/// <summary>
		/// Returns the depth of the world
		/// </summary>
		public float Depth
		{
			get { return (this.heightMapHeight - 1) * this.cellSpacing; }
		}

		/// <summary>
		/// The world matrix
		/// </summary>
		public Matrix World { get; set; }

		/// <summary>
		/// Returns the height at the given world position
		/// </summary>
		/// <param name="x">The x position</param>
		/// <param name="z">The y position</param>
		public float GetHeight(float x, float z)
		{
			// Transform from terrain local space to "cell" space.
			float c = (x + 0.5f * this.Width) / this.cellSpacing;
			float d = (z - 0.5f * this.Depth) / -this.cellSpacing;

			// Get the row and column we are in.
			int row = (int)Math.Floor(d);
			int col = (int)Math.Floor(c);

			// Grab the heights of the cell we are in.
			// A*--*B
			//  | /|
			//  |/ |
			// C*--*D
			float A = this.heightMap[row * this.heightMapWidth + col];
			float B = this.heightMap[row * this.heightMapWidth + col + 1];
			float C = this.heightMap[(row + 1) * this.heightMapWidth + col];
			float D = this.heightMap[(row + 1) * this.heightMapWidth + col + 1];

			// Where we are relative to the cell.
			float s = c - (float)col;
			float t = d - (float)row;

			// If upper triangle ABC.
			if (s + t <= 1.0f)
			{
				float uy = B - A;
				float vy = C - A;
				return A + s * uy + t * vy;
			}
			else // lower triangle DCB.
			{
				float uy = C - D;
				float vy = B - D;
				return D + (1.0f - s) * uy + (1.0f - t) * vy;
			}
		}

		/// <summary>
		/// Loads the heightmap
		/// </summary>
		private void LoadHeightmap()
		{
			this.heightMap = new float[this.heightMapWidth * this.heightMapHeight];

			using (var fileStream = new FileStream(this.heightMapFile, FileMode.Open))
			using (var reader = new BinaryReader(fileStream))
			{
				for (int i = 0; i < this.heightMap.Length; i++)
				{
					byte height = reader.ReadByte();
					this.heightMap[i] = (height / 255.0f) * this.heightScale;
				}
			}
		}

		/// <summary>
		/// Smooths the height map
		/// </summary>
		private void Smooth()
		{
			var newHeightMap = new float[this.heightMap.Length];

			for (int i = 0; i < this.heightMapHeight; i++)
			{
				for (int j = 0; j < this.heightMapWidth; j++)
				{
					newHeightMap[i * this.heightMapWidth + j] = this.Average(i, j);
				}
			}

			this.heightMap = newHeightMap;
		}

		/// <summary>
		/// Indicates if the given grid coordinates are valid
		/// </summary>
		private bool InBounds(int i, int j)
		{
			return 
				i >= 0 && i < (int)this.heightMapHeight
				&& j >= 0 && j < (int)this.heightMapWidth;
		}

		/// <summary>
		/// Returns the average height for the given gird coordinates
		/// </summary>
		private float Average(int i, int j)
		{
			// Function computes the average height of the ij element.
			// It averages itself with its eight neighbor pixels.  Note
			// that if a pixel is missing neighbor, we just don't include it
			// in the average--that is, edge pixels don't have a neighbor pixel.
			//
			// ----------
			// | 1| 2| 3|
			// ----------
			// |4 |ij| 6|
			// ----------
			// | 7| 8| 9|
			// ----------

			float avg = 0.0f;
			float num = 0.0f;

			for (int m = i - 1; m <= i + 1; ++m)
			{
				for (int n = j - 1; n <= j + 1; ++n)
				{
					if (this.InBounds(m, n))
					{
						avg += this.heightMap[m * this.heightMapWidth + n];
						num += 1.0f;
					}
				}
			}

			return avg / num;
		}

		/// <summary>
		/// Calculates the patch bounds in the y axis
		/// </summary>
		private void CalcAllPatchBoundsY()
		{
			this.patchBoundsY = new Vector2[this.numPatchQuadFaces];

			for (int i = 0; i < this.numPatchVerticesRows - 1; i++)
			{
				for (int j = 0; j < this.numPatchVerticesCols - 1; j++)
				{
					this.CalcPatchBoundsY(i, j);
				}
			}
		}

		/// <summary>
		/// Calculates the patch bounds in the y axis
		/// </summary>
		private void CalcPatchBoundsY(int i, int j)
		{
			//Scan the heightmap values this patch covers and compute the min/max height.
			int x0 = j * CellsPerPatch;
			int x1 = (j + 1) * CellsPerPatch;

			int y0 = i * CellsPerPatch;
			int y1 = (i + 1) * CellsPerPatch;

			float minY = float.MaxValue;
			float maxY = float.MinValue;
			for (int y = y0; y <= y1; ++y)
			{
				for (int x = x0; x <= x1; ++x)
				{
					int k = y * this.heightMapWidth + x;
					minY = Math.Min(minY, this.heightMap[k]);
					maxY = Math.Max(maxY, this.heightMap[k]);
				}
			}

			int patchId = i * (this.numPatchVerticesCols - 1) + j;
			this.patchBoundsY[patchId] = new Vector2(minY, maxY);
		}

		/// <summary>
		/// Creates the vertex buffer
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		private void CreateVertexBuffer(Device graphicsDevice)
		{ 
			var vertices = new TerrainVertex[this.numPatchVerticesRows * this.numPatchVerticesCols];

			float halfWidth = 0.5f * this.Width;
			float halfDepth = 0.5f * this.Depth;

			float patchWidth = this.Width / (this.numPatchVerticesCols - 1);
			float patchDepth = this.Depth / (this.numPatchVerticesRows - 1);
			float du = 1.0f / (this.numPatchVerticesCols - 1);
			float dv = 1.0f / (this.numPatchVerticesRows - 1);

			for (int i = 0; i < this.numPatchVerticesRows; ++i)
			{
				float z = halfDepth - i * patchDepth;
				for (int j = 0; j < this.numPatchVerticesCols; ++j)
				{
					float x = -halfWidth + j * patchWidth;
					vertices[i * this.numPatchVerticesCols + j] = new TerrainVertex()
					{
						Position = new Vector3(x, 0, z),
						TextureCoordinates = new Vector2(j * du, i * dv), //Stretch texture over grid.
					};
				}
			}

			//Store axis-aligned bounding box y-bounds in upper-left patch corner.
			for (int i = 0; i < this.numPatchVerticesRows - 1; ++i)
			{
				for (int j = 0; j < this.numPatchVerticesCols - 1; ++j)
				{
					int patchId = i * (this.numPatchVerticesCols - 1) + j;
					var vertex = vertices[i * this.numPatchVerticesCols + j];
					vertex.BoundsY = this.patchBoundsY[patchId];
					vertices[i * this.numPatchVerticesCols + j] = vertex;
				}
			}

			this.vertexBuffer = Buffer.Create(
				graphicsDevice,
				BindFlags.VertexBuffer,
				vertices);
		}

		/// <summary>
		/// Creates the index buffer
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		private void CreateIndexBuffer(Device graphicsDevice)
		{
			var indices = new int[this.numPatchQuadFaces * 4];

			//Iterate over each quad and compute indices.
			int k = 0;
			for (int i = 0; i < this.numPatchVerticesRows - 1; ++i)
			{
				for (int j = 0; j < this.numPatchVerticesCols - 1; ++j)
				{
					// Top row of 2x2 quad patch
					indices[k] = i * this.numPatchVerticesCols + j;
					indices[k + 1] = i * this.numPatchVerticesCols + j + 1;

					// Bottom row of 2x2 quad patch
					indices[k + 2] = (i + 1) * this.numPatchVerticesCols + j;
					indices[k + 3] = (i + 1) * this.numPatchVerticesCols + j + 1;

					k += 4; // next quad
				}
			}

			this.indexBuffer = Buffer.Create(
				graphicsDevice,
				BindFlags.IndexBuffer,
				indices);
		}

		/// <summary>
		/// Creates the textures
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="context">The device context</param>
		private void CreateTextures(Device graphicsDevice, DeviceContext context)
		{
			//Create the height map texture
			this.heightMapView = TextureHelpers.FromRaw(
				graphicsDevice,
				this.heightMapWidth,
				this.heightMapHeight,
				SharpDX.DXGI.Format.R32_Float,
				this.heightMap);

			//Layer map
			this.layerMapArrayView = TextureHelpers.LoadTextureArray(
				graphicsDevice,
				context,
				new string[] { this.layerMap0File, this.layerMap1File, this.layerMap2File, this.layerMap3File, this.layerMap4File });

			//Blend map
			using (var texture = TextureHelpers.FromFile(graphicsDevice, this.blendMapFile))
			{
				this.blendMapView = new ShaderResourceView(graphicsDevice, texture);
			}
		}

		/// <summary>
		/// Initializes the terrain
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="context">The device context</param>
		public void Initialize(Device graphicsDevice, DeviceContext context)
		{
			this.LoadHeightmap();
			this.Smooth();
			this.CalcAllPatchBoundsY();

			this.CreateVertexBuffer(graphicsDevice);
			this.CreateIndexBuffer(graphicsDevice);
			this.CreateTextures(graphicsDevice, context);

			this.effect = new TerrainEffect(graphicsDevice, "Content/Effects/Terrain.fx", "Light3");
			this.inputLayout = new InputLayout(graphicsDevice, this.effect.ShaderBytecode, new InputElement[]
			{
			    new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
				new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0),
				new InputElement("TEXCOORD", 1, SharpDX.DXGI.Format.R32G32_Float, 20, 0),
			});
		}

		/// <summary>
		/// Draws the terrain
		/// </summary>
		/// <param name="context">The device context</param>
		/// <param name="camera">The camera</param>
		/// <param name="lights">The lights</param>
		public void Draw(DeviceContext context, BaseCamera camera, DirectionalLight[] lights)
		{
			context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith4ControlPoints;
			context.InputAssembler.InputLayout = this.inputLayout;

			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<TerrainVertex>(), 0));
			context.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

			var viewProj = camera.View * camera.Projection;
			var worldPlanes = CameraHelpers.ExtractFrustumPlanes(viewProj);

			//Set per frame constants
			this.effect.SetViewProjection(viewProj);
			this.effect.SetEyePosition(camera.Position);
			this.effect.SetDirectionalLights(lights);
			this.effect.SetFogColor(Color.Silver.ToVector4());
			this.effect.SetFogStart(15.0f);
			this.effect.SetFogRange(175.0f);
			this.effect.SetMinTessDistance(20.0f);
			this.effect.SetMaxTessDistance(500.0f);
			this.effect.SetMinTessFactor(0.0f);
			this.effect.SetMaxTessFactor(6.0f);
			this.effect.SetTexelCellSpaceU(1.0f / this.heightMapWidth);
			this.effect.SetTexelCellSpaceV(1.0f / this.heightMapHeight);
			this.effect.SetWorldCellSpace(this.cellSpacing);
			this.effect.SetWorldFrustumPlanes(worldPlanes);

			this.effect.SetLayerMapArray(this.layerMapArrayView);
			this.effect.SetBlendMap(this.blendMapView);
			this.effect.SetHeightMap(this.heightMapView);

			this.effect.SetMaterial(this.material);

			for (int i = 0; i < this.effect.Technique.Description.PassCount; i++)
			{
				var pass = this.effect.Technique.GetPassByIndex(i);
				pass.Apply(context);
				context.DrawIndexed(this.numPatchQuadFaces * 4, 0, 0);
			}

			//The effect file sets tessellation stages, but it does not disable them.  So do that here
			//to turn off tessellation.
			context.HullShader.SetShader(null, null, 0);
			context.DomainShader.SetShader(null, null, 0);
		}

		/// <summary>
		/// Disposes the resources
		/// </summary>
		public void Dispose()
		{
			this.effect.Dispose();
			this.inputLayout.Dispose();

			this.vertexBuffer.Dispose();
			this.indexBuffer.Dispose();
			this.layerMapArrayView.Dispose();
			this.blendMapView.Dispose();
			this.heightMapView.Dispose();
		}
	}
}
