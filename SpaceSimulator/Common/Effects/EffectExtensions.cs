using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Contains extension methods for the Effect class
	/// </summary>
	public static class EffectExtensions
	{
		/// <summary>
		/// Sets the given struct variable
		/// </summary>
		/// <typeparam name="T">The type of the struct</typeparam>
		/// <param name="variable">The variable to set</param>
		/// <param name="value">The value</param>
		public static void SetStruct<T>(this EffectVariable variable, T value) where T : struct
		{
			using (var buffer = new DataStream(Utilities.SizeOf<T>(), false, true))
			{
				buffer.Write(value);
				buffer.Position = 0;
				variable.SetRawValue(buffer, (int)buffer.Length);
			}
		}

		/// <summary>
		/// Sets the given struct array variable
		/// </summary>
		/// <typeparam name="T">The type of the struct</typeparam>
		/// <param name="variable">The variable to set</param>
		/// <param name="value">The array of values</param>
		public static void SetStructArray<T>(this EffectVariable variable, T[] values) where T : struct
		{
			using (var buffer = new DataStream(Utilities.SizeOf<T>() * values.Length, false, true))
			{
				for (int i = 0; i < values.Length; i++)
				{
					buffer.Write(values[i]);
				}

				buffer.Position = 0;
				variable.SetRawValue(buffer, (int)buffer.Length);
			}
		}
	}
}
