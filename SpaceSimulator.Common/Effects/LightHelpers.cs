using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SpaceSimulator.Common.Effects
{
	/// <summary>
	/// Represents a directional light
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectionalLight
	{
		public Vector4 Ambient;
		public Vector4 Diffuse;
		public Vector4 Specular;

		public Vector3 Direction;
		private float Pad;
	}
	//[StructLayout(LayoutKind.Explicit)]
	//public struct DirectionalLight
	//{
	//	[FieldOffset(0)]
	//	public Vector4 Ambient;
	//	[FieldOffset(16)]
	//	public Vector4 Diffuse;
	//	[FieldOffset(32)]
	//	public Vector4 Specular;

	//	[FieldOffset(48)]
	//	public Vector3 Direction;
	//	[FieldOffset(60)]
	//	private float Pad;
	//}

	/// <summary>
	/// Represents a point light
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct PointLight
	{
		public Vector4 Ambient;
		public Vector4 Diffuse;
		public Vector4 Specular;

		public Vector3 Position;
		public float Range;

		public Vector3 Att;
		private float Pad;
	}
	//[StructLayout(LayoutKind.Explicit)]
	//public struct PointLight
	//{
	//	[FieldOffset(0)]
	//	public Vector4 Ambient;
	//	[FieldOffset(16)]
	//	public Vector4 Diffuse;
	//	[FieldOffset(32)]
	//	public Vector4 Specular;

	//	[FieldOffset(48)]
	//	public Vector3 Position;
	//	[FieldOffset(60)]
	//	public float Range;

	//	[FieldOffset(64)]
	//	public Vector3 Att;
	//	[FieldOffset(76)]
	//	private float Pad;
	//}
	
	/// <summary>
	/// Represents a spot light
	/// </summary>
	/// 	
	[StructLayout(LayoutKind.Sequential)]
	public struct SpotLight
	{
		public Vector4 Ambient;
		public Vector4 Diffuse;
		public Vector4 Specular;

		public Vector3 Position;
		public float Range;

		public Vector3 Direction;
		public float Spot;

		public Vector3 Att;
		private float Pad;
	}
	//[StructLayout(LayoutKind.Explicit)]
	//public struct SpotLight
	//{
	//	[FieldOffset(0)]
	//	public Vector4 Ambient;
	//	[FieldOffset(16)]
	//	public Vector4 Diffuse;
	//	[FieldOffset(32)]
	//	public Vector4 Specular;

	//	[FieldOffset(48)]
	//	public Vector3 Position;
	//	[FieldOffset(60)]
	//	public float Range;

	//	[FieldOffset(64)]
	//	public Vector3 Direction;
	//	[FieldOffset(76)]
	//	public float Spot;

	//	[FieldOffset(80)]
	//	public Vector3 Att;
	//	[FieldOffset(92)]
	//	private float Pad;
	//}

	/// <summary>
	/// Represents a material
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Material
	{
		public Vector4 Ambient;
		public Vector4 Diffuse;
		public Vector4 Specular; // w = SpecPower
		public Vector4 Reflect;
	}
	//[StructLayout(LayoutKind.Explicit)]
	//public struct Material
	//{
	//	[FieldOffset(0)]
	//	public Vector4 Ambient;

	//	[FieldOffset(16)]
	//	public Vector4 Diffuse;

	//	[FieldOffset(32)]
	//	public Vector4 Specular; // w = SpecPower

	//	[FieldOffset(48)]
	//	public Vector4 Reflect;
	//}
}
