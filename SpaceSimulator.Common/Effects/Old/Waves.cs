using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.Old
{
	/// <summary>
	/// Represents waves
	/// </summary>
	public class Waves
	{
		private int numRows;
		private int numCols;

		private int vertexCount;
		private int triangleCount;

		private float k1;
		private float k2;
		private float k3;

		private float timeStep;
		private float spatialStep;

		private float totalTime;

		private Vector3[] prevSolution;
		private Vector3[] currentSolution;
		private Vector3[] normals;
		private Vector3[] tangentX;

		/// <summary>
		/// Creates waves
		/// </summary>
		public Waves()
		{

		}

		/// <summary>
		/// Returns the triangle count
		/// </summary>
		public int TriangleCount
		{
			get { return this.triangleCount; }
		}

		/// <summary>
		/// Returns the vertex count
		/// </summary>
		public int VertexCount
		{
			get { return this.vertexCount; }
		}

		/// <summary>
		/// Returns the number of rows
		/// </summary>
		public int RowCount
		{
			get { return this.numRows; }
		}

		/// <summary>
		/// Returns the number of cols
		/// </summary>
		public int ColumnCount
		{
			get { return this.numCols; }
		}

		/// <summary>
		/// Returns the width of the waves grid
		/// </summary>
		public float Width
		{
			get { return this.numCols * this.spatialStep; }
		}

		/// <summary>
		/// Returns the depth of the waves grid
		/// </summary>
		public float Depth
		{
			get { return this.numRows * this.spatialStep; }
		}

		/// <summary>
		/// Returns the solution at the given grid point
		/// </summary>
		/// <param name="i">The grid point</param>
		public Vector3 GetSolution(int i)
		{
			return this.currentSolution[i];
		}

		/// <summary>
		/// Returns the normal at the given grid point
		/// </summary>
		/// <param name="i">The grid point</param>
		public Vector3 GetNormal(int i)
		{
			return this.normals[i];
		}

		/// <summary>
		/// Initializes the waves
		/// </summary>
		/// <param name="m">The number of rows</param>
		/// <param name="n">The number of columns</param>
		/// <param name="dx">The delta x</param>
		/// <param name="dt">The delta y</param>
		/// <param name="speed">The speed</param>
		/// <param name="damping">The damping factor</param>
		public void Inititalize(int m, int n, float dx, float dt, float speed, float damping)
		{
			this.numRows = m;
			this.numCols = n;

			this.vertexCount = m * n;
			this.triangleCount = (m - 1) * (n - 1) * 2;

			this.timeStep = dt;
			this.spatialStep = dx;

			float d = damping * dt + 2.0f;
			float e = (speed * speed) * (dt * dt) / (dx * dx);
			this.k1 = (damping * dt - 2.0f) / d;
			this.k2 = (4.0f - 8.0f * e) / d;
			this.k3 = (2.0f * e) / d;

			this.prevSolution = new Vector3[m * n];
			this.currentSolution = new Vector3[m * n];
			this.normals = new Vector3[m * n];
			this.tangentX = new Vector3[m * n];

			float halfWidth = (n - 1) * dx * 0.5f;
			float halfDepth = (m - 1) * dx * 0.5f;
			for (int i = 0; i < m; ++i)
			{
				float z = halfDepth - i * dx;
				for (int j = 0; j < n; ++j)
				{
					float x = -halfWidth + j * dx;

					this.prevSolution[i * n + j] = new Vector3(x, 0.0f, z);
					this.currentSolution[i * n + j] = new Vector3(x, 0.0f, z);
					this.normals[i * n + j] = new Vector3(0.0f, 1.0f, 0.0f);
					this.tangentX[i * n + j] = new Vector3(1.0f, 0.0f, 0.0f);
				}
			}
		}

		/// <summary>
		/// Updates the waves
		/// </summary>
		/// <param name="elapsed">The elapsed time since the last frame</param>
		public void Update(TimeSpan elapsed)
		{
			// Accumulate time.
			this.totalTime += (float)elapsed.TotalMilliseconds;

			// Only update the simulation at the specified time step.
			if (this.totalTime >= this.timeStep)
			{
				// Only update interior points; we use zero boundary conditions.
				for (int i = 1; i < this.numRows - 1; ++i)
				{
					for (int j = 1; j < this.numCols - 1; ++j)
					{
						// After this update we will be discarding the old previous
						// buffer, so overwrite that buffer with the new update.
						// Note how we can do this inplace (read/write to same element) 
						// because we won't need prev_ij again and the assignment happens last.

						// Note j indexes x and i indexes z: h(x_j, z_i, t_k)
						// Moreover, our +z axis goes "down"; this is just to 
						// keep consistent with our row indices going down.

						this.prevSolution[i * this.numCols + j].Y =
							this.k1 * this.prevSolution[i * this.numCols + j].Y +
							this.k2 * this.currentSolution[i * this.numCols + j].Y +
							this.k3 * (this.currentSolution[(i + 1) * this.numCols + j].Y +
								 this.currentSolution[(i - 1) * this.numCols + j].Y +
								 this.currentSolution[i * this.numCols + j + 1].Y +
								 this.currentSolution[i * this.numCols + j - 1].Y);
					}
				}

				// We just overwrote the previous buffer with the new data, so
				// this data needs to become the current solution and the old
				// current solution becomes the new previous solution.
				var tmp = this.currentSolution;
				this.currentSolution = this.prevSolution;
				this.prevSolution = tmp;

				this.totalTime = 0;
			}

			//Compute the normals
			for (int i = 1; i < this.numRows - 1; ++i)
			{
				for (int j = 1; j < this.numCols - 1; ++j)
				{
					float l = this.currentSolution[i * this.numCols + j - 1].Y;
					float r = this.currentSolution[i * this.numCols + j + 1].Y;
					float t = this.currentSolution[(i - 1) * this.numCols + j].Y;
					float b = this.currentSolution[(i + 1) * this.numCols + j].Y;

					var normal = new Vector3(-r + l, 2.0f * this.spatialStep, b - t);
					normal.Normalize();
					this.normals[i * this.numCols + j] = normal;

					var tangent = new Vector3(2.0f * this.spatialStep, r - l, 0.0f);
					tangent.Normalize();
					this.tangentX[i * this.numCols + j] = tangent;
				}
			}
		}

		/// <summary>
		/// Disturbes the given grid point
		/// </summary>
		/// <param name="i">The row</param>
		/// <param name="j">The column</param>
		/// <param name="magnitude">The magnitude</param>
		public void Disturb(int i, int j, float magnitude)
		{
			float halfMag = 0.5f * magnitude;

			// Disturb the ijth vertex height and its neighbors.
			this.currentSolution[i * this.numCols + j].Y += magnitude;
			this.currentSolution[i * this.numCols + j + 1].Y += halfMag;
			this.currentSolution[i * this.numCols + j - 1].Y += halfMag;
			this.currentSolution[(i + 1) * this.numCols + j].Y += halfMag;
			this.currentSolution[(i - 1) * this.numCols + j].Y += halfMag;
		}
	}
}
