using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.Models
{
	/// <summary>
	/// Represents a model loader
	/// </summary>
	public static class ModelLoader
	{
		private readonly static Regex vertexCountRegex = new Regex("VertexCount: ([0-9]+)");
		private readonly static Regex triangleCountRegex = new Regex("TriangleCount: ([0-9]+)");

		/// <summary>
		/// Parses the given float
		/// </summary>
		/// <param name="str">The float to parse</param>
		private static float ParseFloat(string str)
		{
			return float.Parse(
				str,
				NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
				System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Loads a model from the given stream
		/// </summary>
		/// <param name="stream">The stream to load from</param>
		/// <param name="vertices">The loaded vertices</param>
		/// <param name="indices">The loaded indices</param>
		/// <returns>True if loaded else false</returns>
		public static bool Load(Stream stream, out GeometryVertex[] vertices, out int[] indices)
		{
			using (var reader = new StreamReader(stream))
			{
				var vertexCountLine = vertexCountRegex.Match(reader.ReadLine());
				var triangleCountLine = triangleCountRegex.Match(reader.ReadLine());

				if (vertexCountLine.Success && triangleCountLine.Success)
				{
					var vertexCount = int.Parse(vertexCountLine.Groups[1].Value);
					var triangleCount = int.Parse(triangleCountLine.Groups[1].Value);

					vertices = new GeometryVertex[vertexCount];
					indices = new int[triangleCount * 3];

					//Skip two lines
					reader.ReadLine();
					reader.ReadLine();

					//Parse the vertices
					for (int i = 0; i < vertexCount; i++)
					{
                        var vertexLine = reader.ReadLine().Split(' ');
                        if (vertexLine.Length == 6)
                        {
                            var vertex = new GeometryVertex()
                            {
                                Position = new Vector3(
                                    ParseFloat(vertexLine[0]),
                                    ParseFloat(vertexLine[1]),
                                    ParseFloat(vertexLine[2])),
                                Normal = new Vector3(
                                    ParseFloat(vertexLine[3]),
                                    ParseFloat(vertexLine[4]),
                                    ParseFloat(vertexLine[5]))
                            };

                            vertices[i] = vertex;
						}
						else
						{
							return false;
						}
					}

					//Skip line
					reader.ReadLine();

					//Skip two lines
					reader.ReadLine();
					reader.ReadLine();

					//Parse the triangles (indices)
					for (int i = 0; i < triangleCount; i++)
					{
						var triangleLine = reader.ReadLine().Split(' ');

						if (triangleLine.Length == 3)
						{
							indices[i * 3 + 0] = int.Parse(triangleLine[0]);
							indices[i * 3 + 1] = int.Parse(triangleLine[1]);
							indices[i * 3 + 2] = int.Parse(triangleLine[2]);
						}
						else
						{
							return false;
						}
					}

					return true;
				}
			}

			vertices = null;
			indices = null;
			return false;
		}
	}
}
