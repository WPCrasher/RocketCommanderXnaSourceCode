// Project: Rocket Commander, File: SmallAsteroids.cs
// Namespace: RocketCommanderXna.Game, Class: SmallAsteroids
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 103, Size of file: 2,40 KB
// Creation date: 29.11.2005 20:20
// Last modified: 29.11.2005 20:26
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Special class for smaller asteroids we render seperately without
	/// shaders and without any collision, rotation, movement speed, etc.
	/// </summary>
	public class SmallAsteroids
	{
		#region Variables
		/// <summary>
		/// Type of smaller asteroids, 0-2.
		/// </summary>
		private int type;

		/// <summary>
		/// Absolute position in 3d space.
		/// </summary>
		private Vector3 position;

		/// <summary>
		/// Render size is precalculated size for rendering.
		/// </summary>
		private float renderSize;

		/// <summary>
		/// Precalculated renderMatrix, used for rendering, must be updated by the
		/// physics themself when anything changes. Call RenderMatrix.
		/// </summary>
		private Matrix renderMatrix;
		#endregion

		#region Properties
		/// <summary>
		/// Type
		/// </summary>
		/// <returns>Int</returns>
		public int Type
		{
			get
			{
				return type;
			} // get
		} // Type

		/// <summary>
		/// Position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 Position
		{
			get
			{
				return position;
			} // get
		} // Position
		#endregion

		#region Get render matrix
		/// <summary>
		/// Render matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public Matrix RenderMatrix
		{
			get
			{
				return renderMatrix;
			} // get
		} // RenderMatrix
		#endregion

		#region Constructor
		/// <summary>
		/// Create small asteroids group of specific type at specific location.
		/// Size is randomized a bit, rest is given.
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPosition">Set position</param>
		/// <param name="objectSize">Object size</param>
		public SmallAsteroids(int setType, Vector3 setPosition)//, float objectSize)
		{
			type = setType;
			position = setPosition;
			renderSize = //objectSize *
				(GameAsteroidManager.SmallAsteroidSize +
				RandomHelper.GetRandomFloat(-2.5f, +5.0f));

			// Calculate render matrix (never changes)
			renderMatrix =
				Matrix.CreateRotationX(RandomHelper.GetRandomFloat(0, 3)) *
				Matrix.CreateRotationY(RandomHelper.GetRandomFloat(-1.6f, +1.5f)) *
				Matrix.CreateRotationZ(RandomHelper.GetRandomFloat(0, 2)) *
				Matrix.CreateScale(renderSize, renderSize, renderSize) *
				Matrix.CreateTranslation(position);
		} // SmallAsteroids(setType, setPosition, objectSize)
		#endregion
	} // class SmallAsteroids
} // namespace RocketCommanderXna.Game
