using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Represents a custom includer for Effect files
	/// </summary>
	public class IncludeEffect : Include
	{
		private readonly string effectsDirectory;
		private IDisposable shadow;

		/// <summary>
		/// Creates a new effects includer
		/// </summary>
		/// <param name="effectsDirectory">The effects directory</param>
		public IncludeEffect(string effectsDirectory = "Content/Effects/")
		{
			this.effectsDirectory = effectsDirectory;
		}

		/// <summary>
		/// Closes the given stream
		/// </summary>
		/// <param name="stream">The stream</param>
		public void Close(Stream stream)
		{
			stream.Close();
		}

		/// <summary>
		/// Opens the given stream
		/// </summary>
		/// <param name="type">The type to include</param>
		/// <param name="fileName">The name of the file</param>
		/// <param name="parentStream">The parent stream</param>
		public Stream Open(IncludeType type, string fileName, Stream parentStream)
		{
			return new FileStream(effectsDirectory + fileName, FileMode.Open);
		}

		public IDisposable Shadow
		{
			get
			{
				return this.shadow;
			}
			set
			{
				this.shadow = value;
			}
		}

		public void Dispose()
		{
			if (this.shadow != null)
			{
				this.shadow.Dispose();
			}
		}
	}
}
