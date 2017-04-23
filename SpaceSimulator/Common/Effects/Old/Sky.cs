using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SpaceSimulator.Common.Camera;
using SpaceSimulator.Common.Models;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace SpaceSimulator.Common.Old
{
	/// <summary>
	/// Represents a sky vertex
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct SkyVertex
	{
		public Vector3 Position;
	}

	/// <summary>
	/// Represents sky map
	/// </summary>
	public class Sky : IDisposable
	{
		private readonly Buffer vertexBuffer;
		private readonly Buffer indexBuffer;

		private readonly SkyVertex[] vertices;
		private readonly int[] indices;

		private readonly ShaderResourceView cubeMapView;
		private readonly SkyEffect skyEffect;
		private InputLayout inputLayout;

		/// <summary>
		/// Creates a new sky map
		/// </summary>
		/// <param name="graphicsDevice">The graphics device</param>
		/// <param name="cubeMapFileName">The path of the cube map</param>
		/// <param name="skySpehereRadius">The sky sphere radius</param>
		public Sky(Device graphicsDevice, string cubeMapFileName, float skySpehereRadius)
		{
			using (var texture = TextureHelpers.FromFile(graphicsDevice, cubeMapFileName))
			{
				this.cubeMapView = new ShaderResourceView(graphicsDevice, texture);
			}

			GeometryVertex[] sphereVertices = null;
			GeometryGenerator.CreateSphere(skySpehereRadius, 30, 30, out sphereVertices, out this.indices);

			this.vertices = new SkyVertex[sphereVertices.Length];
			for (int i = 0; i < this.vertices.Length; i++)
			{
				this.vertices[i] = new SkyVertex() { Position = sphereVertices[i].Position };
			}

			this.vertexBuffer = Buffer.Create(
				graphicsDevice,
				BindFlags.VertexBuffer,
				this.vertices);

			this.indexBuffer = Buffer.Create(
				graphicsDevice,
				BindFlags.IndexBuffer,
				this.indices);

			this.skyEffect = new SkyEffect(graphicsDevice, "Content/Effects/Sky.fx", "SkyTech");

			this.inputLayout = new InputLayout(graphicsDevice, this.skyEffect.ShaderBytecode, new[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0)
            });
		}

		/// <summary>
		/// Returns the cube map view
		/// </summary>
		public ShaderResourceView CubeMapView
		{
			get { return this.cubeMapView; }
		}

		/// <summary>
		/// Draws the skies
		/// </summary>
		/// <param name="context">The device context</param>
		/// <param name="camera">The camera</param>
		public void Draw(DeviceContext context, BaseCamera camera)
		{
			//Center Sky about eye in world space
			var translation = Matrix.Translation(camera.Position);
			var worldViewProj = translation * camera.View * camera.Projection;

			//Set constants
			this.skyEffect.SetWorldViewProjection(worldViewProj);
			this.skyEffect.SetCubeMap(this.cubeMapView);

			//Draw
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, Utilities.SizeOf<SkyVertex>(), 0));
			context.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
			context.InputAssembler.InputLayout = inputLayout;
			context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

			for (int i = 0; i < this.skyEffect.Technique.Description.PassCount; i++)
			{
				var pass = this.skyEffect.Technique.GetPassByIndex(i);
				pass.Apply(context);
				context.DrawIndexed(this.indices.Length, 0, 0);
			}
		}

		/// <summary>
		/// Disposes resources
		/// </summary>
		public void Dispose()
		{
			this.vertexBuffer.Dispose();
			this.indexBuffer.Dispose();
			this.cubeMapView.Dispose();
			this.skyEffect.Dispose();
			this.inputLayout.Dispose();
		}
	}
}
