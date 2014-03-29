// Project: Rocket Commander, File: Asteroid.cs
// Namespace: RocketCommanderXna.Game, Class: Asteroid
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 379, Size of file: 11 KB
// Creation date: 06.11.2005 07:17
// Last modified: 03.12.2005 00:05
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Asteroid
	/// </summary>
	public class Asteroid
	{
		#region Variables
		/// <summary>
		/// Type of asteroid, must be in range of asteroidModels.Length.
		/// </summary>
		private int type;
		
		/// <summary>
		/// Absolute position in 3d space.
		/// Only public to improve performance for physics and calling
		/// position very often. Normally you should not make any member variables
		/// public, but you know, game developers don't care and do things anyway.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// Moving speed through space in relation of current rotation.
		/// Same argument here to make it public, used in physics and updated
		/// there.
		/// </summary>
		public Vector3 movement;
		
		/// <summary>
		/// Size of asteroid, will also influence mass.
		/// Last of the 3 public variables we need for physics.
		/// </summary>
		public float size;

		/// <summary>
		/// Helper for calculating collisionRadius with size.
		/// Don't ask me why the factor is so crazy, it is just a funny
		/// number around 0.9f, it doesn't really matter what number it is.
		/// Should be around 0.9-1.0 somewhere.
		/// Update: Not longer used for player collision with asteroids,
		/// only for asteroid to asteroid collisions.
		/// Player uses polygon based collision testing!
		/// </summary>
		public const float CollisionSizeCorrectionFactor = 0.9149594f;

		/// <summary>
		/// Collision radius for AsteroidManager.HandleAsteroidCollision(...)
		/// </summary>
		public float collisionRadius;

		/// <summary>
		/// Rotation of asteroid as yaw, pitch, roll factors.
		/// </summary>
		private Vector3 rotation;
		
		/// <summary>
		/// Rotation speed of yaw and pitch.
		/// Roll is not rotated (would look strange).
		/// </summary>
		public Vector2 rotationSpeed;

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
				// Tested for optimal performance, see below!

				// Build matrix ourselfs, matrix multiply is way too slow!
				// Our way is over 250% faster (10ns instead of 27ns)
				renderMatrix =
					Matrix.CreateRotationX(rotation.X) *
					Matrix.CreateRotationY(rotation.Y) *
					Matrix.CreateRotationZ(rotation.Z);

				// Scale renderMatrix like a 3x3 matrix
				renderMatrix.M11 *= size;// renderSize;
				renderMatrix.M12 *= size;// renderSize;
				renderMatrix.M13 *= size;// renderSize;
				renderMatrix.M21 *= size;// renderSize;
				renderMatrix.M22 *= size;// renderSize;
				renderMatrix.M23 *= size;// renderSize;
				renderMatrix.M31 *= size;// renderSize;
				renderMatrix.M32 *= size;// renderSize;
				renderMatrix.M33 *= size;// renderSize;

				// And finally set translation
				renderMatrix.M41 = position.X;
				renderMatrix.M42 = position.Y;
				renderMatrix.M43 = position.Z;

				/*old, works fine, but is a lot slower
				renderMatrix =
					Matrix.RotationYawPitchRoll(rotation.X, rotation.Y, rotation.Z) *
					Matrix.Scaling(renderSize, renderSize, renderSize) *
					Matrix.CreateTranslation(position);

				// Profiler results:
				// Profiler (Total time: 38ns)
				// Section: test rotation matrix calc (38ns, 100%): 
				//  optimized: 10ns (27,5%)
				//  normal method: 27ns (72,5%)
				 */

				return renderMatrix;
			} // get
		} // RenderMatrix
		#endregion

		#region Constructor
		/// <summary>
		/// Create asteroid of specific type at specific location.
		/// All other parameters (speed, rotation) are set randomly.
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPosition">Set position</param>
		/// <param name="objectSize">Object size</param>
		public Asteroid(int setType, Vector3 setPosition)//, float objectSize)
		{
			type = setType;
			position = setPosition;
			size = RandomHelper.GetRandomFloat(
				GameAsteroidManager.MinAsteroidSize,
				GameAsteroidManager.MaxAsteroidSize);
			//renderSize = objectSize * size;

			// Reduce size for physics a bit, outer limit is never filled.
			collisionRadius = size * CollisionSizeCorrectionFactor;

			// For every other type except 0 (sphere) reduce size a bit more!
			if (type > 0)
				collisionRadius *= CollisionSizeCorrectionFactor;

			// And use a even smaller radius for donut asteroids
			if (type == 4)
				collisionRadius *= CollisionSizeCorrectionFactor;

			// Make 60% really slow moving asteroids,
			// 35% normal moving and only 5% fast moving asteroids
			float speedFactor = RandomHelper.GetRandomFloat(-20, +20);
			Vector3 flyDirection = new Vector3(
				RandomHelper.GetRandomFloat(-1, +1),
				RandomHelper.GetRandomFloat(-1, +1),
				RandomHelper.GetRandomFloat(-1, +1));
			flyDirection.Normalize();
			movement = flyDirection * speedFactor;

			// Rotation is random, rotation speed is low
			float pi = (float)Math.PI;
			rotation = new Vector3(
				RandomHelper.GetRandomFloat(-pi, +pi),
				RandomHelper.GetRandomFloat(-pi, +pi),
				RandomHelper.GetRandomFloat(-pi, +pi));
			rotationSpeed = new Vector2(
				RandomHelper.GetRandomFloat(-0.25f, +0.6f),
				RandomHelper.GetRandomFloat(-0.75f, +1.0f));

			// Randomly create fast flying asteroids
			int highSpeedNumber = RandomHelper.globalRandomGenerator.Next(100);
			if (highSpeedNumber < 5)
			{
				movement *= 5;
				// Also give them a higher rotation speed
				rotationSpeed.Y = RandomHelper.GetRandomFloat(1.28f, 4.28f);
			} // if

			if (highSpeedNumber < 40)
			{
				movement *= 2;
			} // if (highSpeedNumber)

			// Make donuts asteroids very big!
			if (type == 4)
			{
				size += GameAsteroidManager.MaxAsteroidSize;
				// And reduce rotation for them to increase the chance of
				// flying through them without colliding.
				rotationSpeed = new Vector2(rotationSpeed.X / 3, rotationSpeed.Y / 4);
			} // if
		} // Asteroid(setPosition)
		#endregion

		#region Update movement
		/// <summary>
		/// Render matrix for asteroid with all the current parameters.
		/// </summary>
		public void UpdateMovement(float moveFactor)
		{
			// Apply rotation
			rotation.X += rotationSpeed.X * moveFactor;
			rotation.Y += rotationSpeed.Y * moveFactor;

			// Movement!
			position += movement * moveFactor;
		} // UpdateMovement(moveFactor)
		#endregion
	} // class Asteroid
} // namespace RocketCommanderXna.Game
