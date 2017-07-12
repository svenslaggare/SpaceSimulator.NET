using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.Models
{
	/// <summary>
	/// The vertex format used by the geometry
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct GeometryVertex
	{
		/// <summary>
		/// The position
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// The normal
		/// </summary>
		public Vector3 Normal;

		/// <summary>
		/// The tangent
		/// </summary>
		public Vector3 Tangent;

		/// <summary>
		/// The texture coordinates
		/// </summary>
		public Vector2 TextureCoordinates;

		/// <summary>
		/// Creates a new geometry vertex
		/// </summary>
		public GeometryVertex(
			float px, float py, float pz,
			float nx, float ny, float nz,
			float tx, float ty, float tz,
			float u, float v)
		{
			this.Position = new Vector3(px, py, pz);
			this.Normal = new Vector3(nx, ny, nz);
			this.Tangent = new Vector3(tx, ty, tz);
			this.TextureCoordinates = new Vector2(u, v);
		}
	}

	/// <summary>
	/// Represents a geometry generator
	/// </summary>
	public static class GeometryGenerator
	{
		/// <summary>
		/// Creates a m x n grid
		/// </summary>
		/// <param name="width">The width of the grid</param>
		/// <param name="depth">The depth of the grid</param>
		/// <param name="m">The number of rows</param>
		/// <param name="n">The number of columns</param>
		/// <param name="vertices">The created vertices</param>
		/// <param name="indices">The created indices</param>
		public static void CreateGrid(float width, float depth, int m, int n, out GeometryVertex[] vertices, out int[] indices)
		{
			int vertexCount = m * n;
			int faceCount = (m - 1) * (n - 1) * 2;

			// Create the vertices.
			float halfWidth = 0.5f * width;
			float halfDepth = 0.5f * depth;

			float dx = width / (n - 1);
			float dz = depth / (m - 1);

			float du = 1.0f / (n - 1);
			float dv = 1.0f / (m - 1);

			vertices = new GeometryVertex[vertexCount];

			for (int i = 0; i < m; i++)
			{
				var z = halfDepth - i * dz;
				for (int j = 0; j < n; ++j)
				{
					float x = -halfWidth + j * dx;
                    var vertex = new GeometryVertex()
                    {
                        Position = new Vector3(x, 0.0f, z),
                        Normal = new Vector3(0.0f, 1.0f, 0.0f),
                        Tangent = new Vector3(1.0f, 0.0f, 0.0f),
                        TextureCoordinates = new Vector2(j * du, i * dv) // Stretch texture over grid.
                    };

                    vertices[i * n + j] = vertex;
				}
			}

			//Create the indices.
			indices = new int[faceCount * 3]; // 3 indices per face

			// Iterate over each quad and compute indices.
			int k = 0;
			for (int i = 0; i < m - 1; i++)
			{
				for (int j = 0; j < n - 1; j++)
				{
					indices[k] = i * n + j;
					indices[k + 1] = i * n + j + 1;
					indices[k + 2] = (i + 1) * n + j;

					indices[k + 3] = (i + 1) * n + j;
					indices[k + 4] = i * n + j + 1;
					indices[k + 5] = (i + 1) * n + j + 1;

					k += 6; // next quad
				}
			}
		}
	
		/// <summary>
		/// Creates a box
		/// </summary>
		/// <param name="width">The width of the box</param>
		/// <param name="height">The height of the box</param>
		/// <param name="depth">The depth of the box</param>
		/// <param name="vertices">The created vertices</param>
		/// <param name="indices">The created indices</param>
		public static void CreateBox(float width, float height, float depth, out GeometryVertex[] vertices, out int[] indices)
		{
			//Create the vertices
			vertices = new GeometryVertex[24];

			float w2 = 0.5f * width;
			float h2 = 0.5f * height;
			float d2 = 0.5f * depth;

			// Fill in the front face vertex data.
			vertices[0] = new GeometryVertex(-w2, -h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			vertices[1] = new GeometryVertex(-w2, +h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			vertices[2] = new GeometryVertex(+w2, +h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
			vertices[3] = new GeometryVertex(+w2, -h2, -d2, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

			// Fill in the back face new GeometryVertex data.
			vertices[4] = new GeometryVertex(-w2, -h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);
			vertices[5] = new GeometryVertex(+w2, -h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			vertices[6] = new GeometryVertex(+w2, +h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			vertices[7] = new GeometryVertex(-w2, +h2, +d2, 0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);

			// Fill in the top face new GeometryVertex data.
			vertices[8] = new GeometryVertex(-w2, +h2, -d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			vertices[9] = new GeometryVertex(-w2, +h2, +d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			vertices[10] = new GeometryVertex(+w2, +h2, +d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
			vertices[11] = new GeometryVertex(+w2, +h2, -d2, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

			// Fill in the bottom face new GeometryVertex data.
			vertices[12] = new GeometryVertex(-w2, -h2, -d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);
			vertices[13] = new GeometryVertex(+w2, -h2, -d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			vertices[14] = new GeometryVertex(+w2, -h2, +d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			vertices[15] = new GeometryVertex(-w2, -h2, +d2, 0.0f, -1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);

			// Fill in the left face new GeometryVertex data.
			vertices[16] = new GeometryVertex(-w2, -h2, +d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f);
			vertices[17] = new GeometryVertex(-w2, +h2, +d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
			vertices[18] = new GeometryVertex(-w2, +h2, -d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f);
			vertices[19] = new GeometryVertex(-w2, -h2, -d2, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f);

			// Fill in the right face new GeometryVertex data.
			vertices[20] = new GeometryVertex(+w2, -h2, -d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f);
			vertices[21] = new GeometryVertex(+w2, +h2, -d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
			vertices[22] = new GeometryVertex(+w2, +h2, +d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);
			vertices[23] = new GeometryVertex(+w2, -h2, +d2, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f);

			// Create the indices.
			indices = new int[36];

			// Fill in the front face index data
			indices[0] = 0; indices[1] = 1; indices[2] = 2;
			indices[3] = 0; indices[4] = 2; indices[5] = 3;

			// Fill in the back face index data
			indices[6] = 4; indices[7] = 5; indices[8] = 6;
			indices[9] = 4; indices[10] = 6; indices[11] = 7;

			// Fill in the top face index data
			indices[12] = 8; indices[13] = 9; indices[14] = 10;
			indices[15] = 8; indices[16] = 10; indices[17] = 11;

			// Fill in the bottom face index data
			indices[18] = 12; indices[19] = 13; indices[20] = 14;
			indices[21] = 12; indices[22] = 14; indices[23] = 15;

			// Fill in the left face index data
			indices[24] = 16; indices[25] = 17; indices[26] = 18;
			indices[27] = 16; indices[28] = 18; indices[29] = 19;

			// Fill in the right face index data
			indices[30] = 20; indices[31] = 21; indices[32] = 22;
			indices[33] = 20; indices[34] = 22; indices[35] = 23;
		}
	
		/// <summary>
		/// Creates a sphere
		/// </summary>
		/// <param name="radius">The radius of the sphere</param>
		/// <param name="sliceCount">The slice count</param>
		/// <param name="stackCount">The stack count</param>
		/// <param name="vertices">The created vertices</param>
		/// <param name="indices">The created indices</param>
		public static void CreateSphere(float radius, int sliceCount, int stackCount, out GeometryVertex[] vertices, out int[] indices)
		{
			var sphereVertices = new List<GeometryVertex>();
			var sphereIndices = new List<int>();

			//Compute the vertices stating at the top pole and moving down the stacks.
			var topVertex = new GeometryVertex(0.0f, +radius, 0.0f, 0.0f, +1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
			var bottomVertex = new GeometryVertex(0.0f, -radius, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
			sphereVertices.Add(topVertex);

			float phiStep = MathUtil.Pi / stackCount;
			float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

			//Compute vertices for each stack ring (do not count the poles as rings).
			for (int i = 1; i <= stackCount - 1; ++i)
			{
				float phi = i * phiStep;

				// Vertices of ring.
				for (int j = 0; j <= sliceCount; ++j)
				{
					float theta = j * thetaStep;

					GeometryVertex vertex;

					//spherical to cartesian
					vertex.Position.X = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
					vertex.Position.Y = radius * (float)Math.Cos(phi);
					vertex.Position.Z = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);

					//Partial derivative of P with respect to theta
					vertex.Tangent.X = -radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
					vertex.Tangent.Y = 0.0f;
					vertex.Tangent.Z = +radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
					vertex.Tangent.Normalize();

					vertex.Normal = vertex.Position;
					vertex.Normal.Normalize();

					vertex.TextureCoordinates.X = theta / MathUtil.TwoPi;
					vertex.TextureCoordinates.Y = phi / MathUtil.Pi;

					sphereVertices.Add(vertex);
				}
			}

			sphereVertices.Add(bottomVertex);

			//Compute indices for top stack.  The top stack was written first to the vertex buffer
			//and connects the top pole to the first ring.
			for (int i = 1; i <= sliceCount; ++i)
			{
				sphereIndices.Add(0);
				sphereIndices.Add(i + 1);
				sphereIndices.Add(i);
			}

			// Compute indices for inner stacks (not connected to poles).
			// Offset the indices to the index of the first vertex in the first ring.
			// This is just skipping the top pole vertex.
			int baseIndex = 1;
			int ringVertexCount = sliceCount + 1;
			for (int i = 0; i < stackCount - 2; ++i)
			{
				for (int j = 0; j < sliceCount; ++j)
				{
					sphereIndices.Add(baseIndex + i * ringVertexCount + j);
					sphereIndices.Add(baseIndex + i * ringVertexCount + j + 1);
					sphereIndices.Add(baseIndex + (i + 1) * ringVertexCount + j);

					sphereIndices.Add(baseIndex + (i + 1) * ringVertexCount + j);
					sphereIndices.Add(baseIndex + i * ringVertexCount + j + 1);
					sphereIndices.Add(baseIndex + (i + 1) * ringVertexCount + j + 1);
				}
			}

			// Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
			// and connects the bottom pole to the bottom ring.

			//South pole vertex was added last.
			int southPoleIndex = (int)sphereVertices.Count - 1;

			//Offset the indices to the index of the first vertex in the last ring.
			baseIndex = southPoleIndex - ringVertexCount;

			for (int i = 0; i < sliceCount; ++i)
			{
				sphereIndices.Add(southPoleIndex);
				sphereIndices.Add(baseIndex + i);
				sphereIndices.Add(baseIndex + i + 1);
			}

			vertices = sphereVertices.ToArray();
			indices = sphereIndices.ToArray();
		}

		/// <summary>
		/// Creates a cylinder
		/// </summary>
		/// <param name="bottomRadius">The bottom radius</param>
		/// <param name="topRadius">The top radius</param>
		/// <param name="height">The height</param>
		/// <param name="sliceCount">The slice count</param>
		/// <param name="stackCount">The stack count</param>
		/// <param name="vertices">The created vertices</param>
		/// <param name="indices">The created indices</param>
		public static void CreateCylinder(
            float bottomRadius, float topRadius, float height, int sliceCount, int stackCount,
			out GeometryVertex[] vertices, out int[] indices)
		{
			var cylinderVertices = new List<GeometryVertex>();
			var cylinderIndices = new List<int>();

			// Build Stacks.
			float stackHeight = height / stackCount;

			//Amount to increment radius as we move up each stack level from bottom to top.
			float radiusStep = (topRadius - bottomRadius) / stackCount;

			int ringCount = stackCount + 1;

			// Compute vertices for each stack ring starting at the bottom and moving up.
			for (int i = 0; i < ringCount; ++i)
			{
				float y = -0.5f * height + i * stackHeight;
				float r = bottomRadius + i * radiusStep;

				// vertices of ring
				float dTheta = 2.0f * MathUtil.Pi / sliceCount;
				for (int j = 0; j <= sliceCount; ++j)
				{
					GeometryVertex vertex;

					float c = (float)Math.Cos(j * dTheta);
					float s = (float)Math.Sin(j * dTheta);

					vertex.Position = new Vector3(r * c, y, r * s);

					vertex.TextureCoordinates.X = (float)j / sliceCount;
					vertex.TextureCoordinates.Y = 1.0f - (float)i / stackCount;

					// Cylinder can be parameterized as follows, where we introduce v
					// parameter that goes in the same direction as the v tex-coord
					// so that the bitangent goes in the same direction as the v tex-coord.
					//   Let r0 be the bottom radius and let r1 be the top radius.
					//   y(v) = h - hv for v in [0,1].
					//   r(v) = r1 + (r0-r1)v
					//
					//   x(t, v) = r(v)*cos(t)
					//   y(t, v) = h - hv
					//   z(t, v) = r(v)*sin(t)
					// 
					//  dx/dt = -r(v)*sin(t)
					//  dy/dt = 0
					//  dz/dt = +r(v)*cos(t)
					//
					//  dx/dv = (r0-r1)*cos(t)
					//  dy/dv = -h
					//  dz/dv = (r0-r1)*sin(t)

					// This is unit length.
					vertex.Tangent = new Vector3(-s, 0.0f, c);

					float dr = bottomRadius - topRadius;
					var bitangent = new Vector3(dr * c, -height, dr * s);

					var normal = Vector3.Cross(vertex.Tangent, bitangent);
					normal.Normalize();
					vertex.Normal = normal;

					cylinderVertices.Add(vertex);
				}
			}

			// Add one because we duplicate the first and last vertex per ring
			// since the texture coordinates are different.
			int ringVertexCount = sliceCount + 1;

			// Compute indices for each stack.
			for (int i = 0; i < stackCount; ++i)
			{
				for (int j = 0; j < sliceCount; ++j)
				{
					cylinderIndices.Add(i * ringVertexCount + j);
					cylinderIndices.Add((i + 1) * ringVertexCount + j);
					cylinderIndices.Add((i + 1) * ringVertexCount + j + 1);

					cylinderIndices.Add(i * ringVertexCount + j);
					cylinderIndices.Add((i + 1) * ringVertexCount + j + 1);
					cylinderIndices.Add(i * ringVertexCount + j + 1);
				}
			}

			BuildCylinderTopCap(bottomRadius, topRadius, height, sliceCount, stackCount, cylinderVertices, cylinderIndices);
			BuildCylinderBottomCap(bottomRadius, topRadius, height, sliceCount, stackCount, cylinderVertices, cylinderIndices);

			vertices = cylinderVertices.ToArray();
			indices = cylinderIndices.ToArray();
		}

		/// <summary>
		/// Builds the top cylinder cap
		/// </summary>
		/// <param name="bottomRadius">The bottom radius</param>
		/// <param name="topRadius">The top radius</param>
		/// <param name="height">The height</param>
		/// <param name="sliceCount">The slice count</param>
		/// <param name="stackCount">The stack count</param>
		/// <param name="vertices">The vertices list</param>
		/// <param name="indices">The indices list</param>
		private static void BuildCylinderTopCap(float bottomRadius, float topRadius, float height, int sliceCount, int stackCount,
			IList<GeometryVertex> vertices, IList<int> indices)
		{
			int baseIndex = vertices.Count;

			float y = 0.5f * height;
			float dTheta = 2.0f * MathUtil.Pi / sliceCount;

			// Duplicate cap ring vertices because the texture coordinates and normals differ.
			for (int i = 0; i <= sliceCount; ++i)
			{
				float x = topRadius * (float)Math.Cos(i * dTheta);
				float z = topRadius * (float)Math.Sin(i * dTheta);

				// Scale down by the height to try and make top cap texture coord area
				// proportional to base.
				float u = x / height + 0.5f;
				float v = z / height + 0.5f;

				vertices.Add(new GeometryVertex(x, y, z, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, u, v));
			}

			// Cap center vertex.
			vertices.Add(new GeometryVertex(0.0f, y, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 0.5f));

			// Index of center vertex.
			int centerIndex = vertices.Count - 1;

			for (int i = 0; i < sliceCount; ++i)
			{
				indices.Add(centerIndex);
				indices.Add(baseIndex + i + 1);
				indices.Add(baseIndex + i);
			}
		}

		/// <summary>
		/// Builds the bottom cylinder cap
		/// </summary>
		/// <param name="bottomRadius">The bottom radius</param>
		/// <param name="topRadius">The top radius</param>
		/// <param name="height">The height</param>
		/// <param name="sliceCount">The slice count</param>
		/// <param name="stackCount">The stack count</param>
		/// <param name="vertices">The vertices list</param>
		/// <param name="indices">The indices list</param>
		private static void BuildCylinderBottomCap(float bottomRadius, float topRadius, float height, int sliceCount, int stackCount,
			IList<GeometryVertex> vertices, IList<int> indices)
		{
			//Build bottom cap.
			int baseIndex = vertices.Count;
			float y = -0.5f * height;

			//vertices of ring
			float dTheta = 2.0f * MathUtil.Pi / sliceCount;
			for (int i = 0; i <= sliceCount; ++i)
			{
				float x = bottomRadius * (float)Math.Cos(i * dTheta);
				float z = bottomRadius * (float)Math.Sin(i * dTheta);

				//Scale down by the height to try and make top cap texture coord area
				//proportional to base.
				float u = x / height + 0.5f;
				float v = z / height + 0.5f;

				vertices.Add(new GeometryVertex(x, y, z, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, u, v));
			}

			//Cap center vertex.
			vertices.Add(new GeometryVertex(0.0f, y, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.5f, 0.5f));

			// Cache the index of center vertex.
			int centerIndex = vertices.Count - 1;

			for (int i = 0; i < sliceCount; ++i)
			{
				indices.Add(centerIndex);
				indices.Add(baseIndex + i);
				indices.Add(baseIndex + i + 1);
			}
		}

		/// <summary>
		/// Creates a fullscreen quad
		/// </summary>
		/// <param name="vertices">The created vertices</param>
		/// <param name="indices">The created indices</param>
		public static void CreateFullscreenQuad(out GeometryVertex[] vertices, out int[] indices)
		{
			vertices = new GeometryVertex[4];
			indices = new int[6];

			// Position coordinates specified in NDC space.
			vertices[0] = new GeometryVertex(
				-1.0f, -1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				0.0f, 1.0f);

			vertices[1] = new GeometryVertex(
				-1.0f, +1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				0.0f, 0.0f);

			vertices[2] = new GeometryVertex(
				+1.0f, +1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				1.0f, 0.0f);

			vertices[3] = new GeometryVertex(
				+1.0f, -1.0f, 0.0f,
				0.0f, 0.0f, -1.0f,
				1.0f, 0.0f, 0.0f,
				1.0f, 1.0f);

			indices[0] = 0;
			indices[1] = 1;
			indices[2] = 2;

			indices[3] = 0;
			indices[4] = 2;
			indices[5] = 3;
		}
	}
}
