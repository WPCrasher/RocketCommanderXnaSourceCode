// Project: Rocket Commander, File: PhysicsAsteroidManager.cs
// Namespace: RocketCommanderXna.Game, Class: PhysicsAsteroidManager
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 1362, Size of file: 50,96 KB
// Creation date: 19.01.2006 22:46
// Last modified: 19.01.2006 23:26
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Physics asteroid manager.
	/// Derived from BaseAsteroidManager and adds all Physics
	/// (handle player collision, handle sector Physics, etc.).
	/// </summary>
	public class PhysicsAsteroidManager : BaseAsteroidManager
	{
		#region Constructor
		/// <summary>
		/// Create asteroid manager
		/// </summary>
		public PhysicsAsteroidManager(Level setLevel)
			: base(setLevel)
		{
			/*obs
			// Load all whosh sounds
			for (int num = 0; num < NumOfWhoshSounds; num++)
			{
				whoshSounds[num] = new Sample("Whosh" + (num + 1) + ".wav", 10);
			} // for (num)
			sideHitSound = new Sample("SideHit.wav");
			 */
		} // PhysicsAsteroidManager(setLevel)
		#endregion

		#region Player asteroid collision
		/// <summary>
		/// Player asteroid collision, used to check if player collided with
		/// any asteroid nearby. Returns a value between 0 and 1, 0 means
		/// no collision has occurred and 1.0 means we have done a frontal crash
		/// and will lose a lot of hitpoints. Side hits do hurt do, but are not
		/// as fatal. This method will also play the side hit and whosh sounds.
		/// </summary>
		/// <returns>0 if no collision happened, 1 for a frontal collision.
		/// Everything in between is just some strafe collision (doesn't kill us).
		/// </returns>
		public float PlayerAsteroidCollision()
		{
			try
			{
				Vector3 camPos = BaseGame.CameraPos;
				Vector3 camDir = new Vector3(0, 0, -1);
				camDir = Vector3.TransformNormal(camDir, BaseGame.ViewMatrix);
				Vector3 nextCamPos1 = camPos +
					camDir * 10.0f;
				Vector3 nextCamPos2 = camPos +
					camDir * (10.0f + Player.Speed *
					BaseGame.MoveFactorPerSecond *
					Player.MovementSpeedPerSecond);
				float damageFactor = 0.0f;
				float remToPlayWhoshVolume = 0.0f;

				// Helpers for intersection test
				Vector3 v = new Vector3(0, 0, 1);
				Matrix m = BaseGame.InverseViewMatrix;
				// Transform the screen space pick ray into 3D space
				Vector3 rayDir = new Vector3(
					v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
					v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
					v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33);
				Vector3 rayOrigin = new Vector3(
					m.M41, m.M42, m.M43);

				for (int z = -1; z <= +1; z++)
					//for (int y = -1; y <= +1; y++)
						for (int x = -1; x <= +1; x++)
						{
							List<Asteroid> thisSector = sectorAsteroids[
								MiddleSector + z, MiddleSector + x];
							for (int num = 0; num < thisSector.Count; num++)
							{
								Asteroid asteroid = thisSector[num];

								// Check distance to camera
								float distance = (asteroid.position - camPos).Length();

								// If distance is smaller than collision radius, we could have
								// a hit. Now do polygon based collision testing.
								if (distance <= asteroid.size * 0.825157789f)// * 1.25f)
								{
									// First of all do an intersection test to see if we
									// really hit something here.
									Matrix invWorldMatrix =
										Matrix.Invert(asteroid.RenderMatrix);
									Vector3 rayWorldPos =
										Vector3.Transform(rayOrigin, invWorldMatrix);
									Vector3 rayWorldDir =
										Vector3.TransformNormal( rayDir, invWorldMatrix);
									rayWorldDir.Normalize();

									/*obs, not supported in XNA
									// Use D3DMesh.Intersect's capability to determinate if
									// our rocket (represented as a ray) hits the asteroid mesh.
									IntersectInformation closestHit;
									if (asteroidModels[asteroid.Type].D3DMesh.Intersect(
										rayWorldPos, rayWorldDir, out closestHit))
									{
										// Check if we hit near the camera, else discard.
										// Use speed to see how much we can still move to avoid
										// collision!
										if (closestHit.Dist > 15.0f + Player.Speed *
											Player.MovementSpeedPerSecond *
											BaseGame.MoveFactorPerSecond * 2.0f)
										{
											// To far away, skip this collision!
											continue;
										} // if (closestHit.Dist)
									} // if (asteroidModels[asteroid.Type].D3DMesh.Intersect)
									else
									{
										// Is this donut asteroid? Then check if we just fly
										// through it.
										if (asteroid.Type == 4 &&
											asteroid.movement.LengthSq() > 0 &&
											distance <= 25.0f + Player.Speed *
											Player.MovementSpeedPerSecond *
											BaseGame.MoveFactorPerSecond * 1.25f)
										{
											// Give player additional 4000 points,
											// thats really a lot!
											Player.score += 4000;
											// Add text message
											Player.SetItemMessage(Texts.FlyThroughBonus);
											// Mark this one as already handled, don't add score
											// anymore.
											asteroid.movement = Vector3.Zero;
										} // if (asteroid.Type)

										// No real collision here, just in bounding sphere
										continue;
									} // else
									 */

									// Find out how bad it is, add 25% to make collision hit
									// hurt much more than just flying by very closely.
									float thisDamageFactor =
										// Use fixed value, always hurt player the same way,
										// We just can't say how the player collided without poly checking
										0.175f + 0.25f * Math.Max(0,
										1.0f - (distance / (asteroid.collisionRadius * 1.025f)));

									// Big asteroids to more damage than smaller ones
									thisDamageFactor *=
										0.5f + 0.5f * (asteroid.size / MaxAsteroidSize);

									// Check highest damage factor
									if (thisDamageFactor > damageFactor)
										damageFactor = thisDamageFactor;

									// Play side hit sound.
									// Play only once (asteroid is killed after that).
									Sound.Play(Sound.Sounds.SideHit);

									// Remember to show hit direction effect
									// Get direction from 3d data (similar to sound
									// calculation below).
									Vector3 distVector = asteroid.position - camPos;
									distVector = Vector3.TransformNormal(
										distVector, BaseGame.ViewMatrix);
									SetHitDirectionEffect(
										(float)Math.Atan2(distVector.X, distVector.Y));

									// Lets rumble a bit if we are hurt
									Input.GamePadRumble(
										0.125f + 0.4f * damageFactor,
										0.25f + 0.45f * damageFactor);
										//0.25f + damageFactor / 2.0f,
										//0.45f + 0.55f * damageFactor);

									// In bomb mode, we have 1 free hit
									if (Player.numberOfBombItems > 0)
									{
										Player.numberOfBombItems--;
										damageFactor = 0.0f;
									} // if (Player.numberOfBombItems)

									// Always remove asteroid we hit!
									// This way we hit it only once.
									thisSector.RemoveAt(num--);
								} // if (distance)

								float distanceSoon =
									(asteroid.position - nextCamPos1).Length();
								// Check if some asteroids are getting really close
								if (distanceSoon < MaxAsteroidSize * 3.5f)
								{
									// Determinate whosh by checking if any asteroid comes nearer
									// than SectorDepth	then check if the asteroid is not longer
									// closing in (but moving away from us).
									// Now play sound, this should only happen once per asteroid!

									// Find out if the asteroid is not longer moving closer
									if (distanceSoon <
										(asteroid.position - nextCamPos2).Length())
									{
										// Play one of the whosh sounds
										float loudness = 1.0f -
											((distanceSoon - asteroid.collisionRadius * 1.5f) /
											(MaxAsteroidSize * 3.5f -
											asteroid.collisionRadius * 1.5f));
										loudness *= 1.25f;
										if (loudness > 1.0f)
											loudness = 1.0f;
										if (loudness > 0.1f)
										{
											// Lets rumble a bit to indicate flying near
											if (loudness > 0.8f)
												Input.GamePadRumble(0.05f, 0.15f);

											// Get panning from 3d data
											Vector3 distVector = asteroid.position - nextCamPos1;
											distVector = Vector3.TransformNormal(
												distVector, BaseGame.ViewMatrix);
											float panning =
												distVector.X / MaxAsteroidSize * 4.0f;
											if (panning < -0.7f)
												panning = -0.7f;
											else if (panning > 0.7f)
												panning = 0.7f;

											// Play whosh sound
											float newVolume = 0.5f + 0.5f * loudness;
											if (newVolume > remToPlayWhoshVolume)
												remToPlayWhoshVolume = newVolume;

											if (distance <= asteroid.collisionRadius * 2.5f)
											{
												// Make camera wobbel (use smaller amounts)
												Player.SetCameraWobbel(0.33f * (1.0f -
													(distance / (asteroid.collisionRadius * 2.0f))));
											} // if (distance)

											// Add a little score for flying so close ^^
											if (BaseGame.TotalFrames % 20 == 0)
												Player.score += (int)BaseGame.ElapsedTimeThisFrameInMs;
										} // if (loudness)
									} // if (distanceSoon)
								} // if (distanceSoon)
							} // for (num)
						} // for for for (int)

				// Max. check every 10 frames, else we have to many whosh sounds!
				if (remToPlayWhoshVolume > 0 &&
					BaseGame.TotalFrames % 5 == 0)
					Sound.PlayWhosh(remToPlayWhoshVolume);

				return damageFactor;
			} // try
			catch (Exception ex)
			{
				// Note: This happened a few times in the beta, no reason to crash
				// the whole game if anything simple here fails (physics or sound).
				Log.Write("Failed to execute PlayerAsteroidCollision: " +
					ex.ToString());
				return 0.0f;
			} // catch
		} // PlayerAsteroidCollision()
		
		/// <summary>
		/// Set hit direction effect
		/// </summary>
		/// <param name="setDirection">Direction</param>
		protected virtual void SetHitDirectionEffect(float setDirection)
		{
			// Implement in derived class
		} // SetHitDirectionEffect(setDirection)

		/// <summary>
		/// Kill all inner sector asteroids
		/// </summary>
		public void KillAllInnerSectorAsteroids()
		{
			// Kill all asteroids in inner sectors (3x3)
			for (int z = -1; z <= +1; z++)
				for (int x = -1; x <= +1; x++)
				{
					// Kill all asteroids there
					sectorAsteroids[MiddleSector + z, MiddleSector + x].Clear();

					// Also kill all inner small asteroids
					sectorSmallerAsteroids[SmallMiddleSector + z,
						SmallMiddleSector + x].Clear();
				} // for for for (int)
		} // KillAllInnerSectorAsteroids()
		#endregion

		#region Handle sector physics
		/// <summary>
		/// Handle sector physics. See below for more explanations and
		/// first simpler versions in the unit test region.
		/// 
		/// Will handle all collisions inside a given sector. We will not
		/// check against the whole universe, which is way to slow.
		/// Just checking this sector is very fast, but does not cover any
		/// collisions at the borders (which occur VERY often, the first
		/// version of this method ignored the borders and sucked big time).
		/// For this reason we will check all neighbouring sectors too!
		/// Note: All asteroids should have an updated position here.
		/// The position will be corrected if a collision occurs and next
		/// frame the asteroids will move into the reflected direction.
		/// </summary>
		protected override void HandleSectorPhysics(
			int checkX, //int checkY,
			int checkZ,
			int cameraSectorPosX, //int cameraSectorPosY,
			int cameraSectorPosZ)
		{
			List<Asteroid> thisSectorAsteroids =
				sectorAsteroids[checkZ, checkX];

			// No asteroids in this sector? Then no collision can occur.
			if (thisSectorAsteroids.Count == 0)
				return;

			// 2 modes: inside sector and at border.
			// If we are inside a simple inner check is totally fine.
			// If we are at any border of the current sector, we check
			// all surrounding sectors and include other asteroids in our search!
			// Note: I've also pfusched in the out of sector check inside this loop,
			// this is very ugly but saves a good amount of performance!
			for (int num = 0; num < thisSectorAsteroids.Count; num++)
			{
				Asteroid asteroid = thisSectorAsteroids[num];

				// Get position in percentage (we use this variable several times)
				float xp = asteroid.position.X / SectorWidth;
				float zp = asteroid.position.Z / SectorDepth;

				// Get sector position, use asteroid pos and round it.
				int ix = (int)Math.Round(xp);
				int iz = (int)Math.Round(zp);

				// Calculate border distance -0.5 means left border, -0.4 to +0.4
				// means we are inside the sector and +0.5 means right border.
				float borderX = xp - ix;
				float borderZ = zp - iz;

				// Substract current position from sector position
				ix = ix - cameraSectorPosX + MiddleSector;
				iz = iz - cameraSectorPosZ + MiddleSector;

				// Bounding checks
				if (ix < 0)
					ix = 0;
				if (iz < 0)
					iz = 0;
				if (ix >= NumberOfSectors)
					ix = NumberOfSectors - 1;
				if (iz >= NumberOfSectors)
					iz = NumberOfSectors - 1;

				// Change asteroid sector if it is not longer in the correct sector.
				if (ix != checkX ||
					iz != checkZ)
				{
					// Remove from old list and reuse old num again.
					thisSectorAsteroids.RemoveAt(num--);

					// Add to new sector, which fits our current position.
					sectorAsteroids[iz, ix].Add(asteroid);
				} // if (ix)

				// Full loop for all possible neighbour configurations is only used
				// if we are at any border. In most cases we don't ever check any
				// neighbour sectors. If we have to, that will be a lot of crazy
				// code and checks to optimize every last bit out of this loops!
				if (borderX > -0.4f && borderX < 0.4f &&
					borderZ > -0.4f && borderZ < 0.4f)
				{
					// Only check this sector! Crosscheck with any other asteroid in
					// this sector.
					//foreach (Asteroid otherAsteroid in thisSectorAsteroids)
					for (int asteroidNum = 0; asteroidNum < thisSectorAsteroids.Count; asteroidNum++)
					{
						Asteroid otherAsteroid = thisSectorAsteroids[asteroidNum];

						if (asteroid != otherAsteroid)
						{
							float maxAllowedDistance =
								otherAsteroid.collisionRadius +
								asteroid.collisionRadius;

							// Distance smaller than max. allowed distance?
							if ((otherAsteroid.position -
								asteroid.position).LengthSquared() <
								maxAllowedDistance * maxAllowedDistance)
							{
								HandleAsteroidCollision(asteroid, otherAsteroid);
							} // if (otherAsteroid.position)
						} // if (asteroid)
					} // for
				} // if (borderX)
				else
				{
					// Check neighbour sectors too.
					// Go through list of asteroids in surrounding sectors.
					// Please also note the order we go through the multidimensional
					// lists, by using z, y, x we can let the cpu cache memory
					// accesses better.
					for (int z = checkZ - 1; z <= checkZ + 1; z++)
						if (z >= 0 && z < NumberOfSectors)
							//for (int y = checkY - 1; y <= checkY + 1; y++)
							//	if (y >= 0 && y < NumberOfSectors)
							for (int x = checkX - 1; x <= checkX + 1; x++)
								if (x >= 0 && x < NumberOfSectors &&
									sectorIsVisible[z, x])
								{
									// Check each of this other asteroids
									//foreach (Asteroid otherAsteroid in sectorAsteroids[z, x])
									for (int otherAsteroidNum = 0;
										otherAsteroidNum < sectorAsteroids[z, x].Count;
										otherAsteroidNum++)
									{
										Asteroid otherAsteroid = sectorAsteroids[z, x][otherAsteroidNum];

										if (asteroid != otherAsteroid)
										{
											float maxAllowedDistance =
												otherAsteroid.collisionRadius +
												asteroid.collisionRadius;

											// Distance smaller than max. allowed distance?
											if ((otherAsteroid.position -
												asteroid.position).LengthSquared() <
												maxAllowedDistance * maxAllowedDistance)
											{
												HandleAsteroidCollision(asteroid, otherAsteroid);
											} // if (otherAsteroid.position)
										} // if (asteroid)
									} // for
								} // for if for if for if (x)
				} // else
			} // for (num)
		} // HandleSectorPhysics(checkX, checkY, checkZ)

		/// <summary>
		/// Handle asteroid collision, called from HandleSectorPhysics.
		/// </summary>
		/// <param name="asteroid">Asteroid</param>
		/// <param name="otherAsteroid">Other asteroid</param>
		protected void HandleAsteroidCollision(
			Asteroid asteroid, Asteroid otherAsteroid)
		{
			float maxAllowedDistance =
				otherAsteroid.collisionRadius + asteroid.collisionRadius;

			// Calculate distance and max. allowed distance
			float distance =
				(otherAsteroid.position - asteroid.position).Length();

			// Collision detected, correct both asteroid positions first.
			// Middle is collision point (aprox. because we just use spheres)
			Vector3 middle =
				otherAsteroid.position *
				(asteroid.collisionRadius / maxAllowedDistance) +
				asteroid.position *
				(otherAsteroid.collisionRadius / maxAllowedDistance);

			// Calculate current relative positions to middle
			Vector3 positionRel = asteroid.position - middle;
			positionRel.Normalize();
			Vector3 otherPositionRel = otherAsteroid.position - middle;
			otherPositionRel.Normalize();

			// Put both circles outside of the collision
			// Add 1% to add a little distance between collided objects!
			otherAsteroid.position = middle +
				otherPositionRel * otherAsteroid.collisionRadius * 1.015f;
			asteroid.position = middle +
				positionRel * asteroid.collisionRadius * 1.015f;

			float asteroidSpeed = asteroid.movement.Length();
			float otherAsteroidSpeed = otherAsteroid.movement.Length();

			// Mass = size*size (size*size*size is too much)
			float asteroidMass =
				asteroid.size * asteroid.size;// *asteroid.size;
			float otherAsteroidMass =
				otherAsteroid.size * otherAsteroid.size;// *otherAsteroid.size;
			float bothMasses = asteroidMass + otherAsteroidMass;

			// Calculate force with speed * mass (mass=size)
			float asteroidForce = asteroidSpeed * asteroidMass;
			float otherAsteroidForce = otherAsteroidSpeed * otherAsteroidMass;
			float bothForces = asteroidForce + otherAsteroidForce;

			// Copy over normals
			Vector3 asteroidNormal = positionRel;
			Vector3 otherAsteroidNormal = otherPositionRel;

			// Normalize movement
			Vector3 asteroidDirection = asteroid.movement;
			// Update for Xna: Make sure we always got valid values,
			// (0, 0, 0).Normalize will produce NaN!
			if (asteroidDirection != Vector3.Zero)
				asteroidDirection.Normalize();
			Vector3 otherAsteroidDirection = otherAsteroid.movement;
			if (otherAsteroidDirection != Vector3.Zero)
				otherAsteroidDirection.Normalize();

			// Get collision strenth (1 if pointing in same direction,
			// 0 if 90 degrees) for both asteroids.
			float asteroidCollisionStrength = Math.Abs(Vector3.Dot(
				asteroidDirection, asteroidNormal));
			float otherAsteroidCollisionStrength = Math.Abs(Vector3.Dot(
				otherAsteroidDirection, otherAsteroidNormal));

			Vector3 asteroidReflection =
				ReflectVector(asteroidDirection, asteroidNormal);
			Vector3 otherAsteroidReflection =
				ReflectVector(otherAsteroidDirection, otherAsteroidNormal);

			// Make sure the strength is calculated correctly
			// We have also to correct the reflection vector if the length was 0,
			// use the normal vector instead.
			if (asteroidDirection.Length() <= 0.01f)
			{
				asteroidCollisionStrength = otherAsteroidCollisionStrength;
				asteroidReflection = asteroidNormal;
			} // if (asteroidDirection.Length)
			if (otherAsteroidDirection.Length() <= 0.01f)
			{
				otherAsteroidCollisionStrength = asteroidCollisionStrength;
				otherAsteroidReflection = otherAsteroidNormal;
			} // if (otherAsteroidDirection.Length)

			// Ok, now the complicated part, everything above was really easy!
			asteroid.movement = asteroidReflection *
				// So, first we have to reflect our current movement speed.
				// This will be scaled to 1-strength to only account the reflection
				// amount (imagine a collision with a static wall). In most cases
				// Strength is close to 1 and this reflection will be very small.
				((1 - asteroidCollisionStrength) * asteroidSpeed +
				// And finally we have to add the impuls, which is calculated
				// by the formular ((m1-m2)*v1 + 2*m2*v2)/(m1+m2), see
				// http://de.wikipedia.org/wiki/Sto%C3%9F_%28Physik%29 for more help.
				(asteroidCollisionStrength *
				(Math.Abs(asteroidMass - otherAsteroidMass) * asteroidSpeed +
				(2 * otherAsteroidMass * otherAsteroidSpeed)) / bothMasses));

			// Same for other asteroid, just with asteroid and otherAsteroid
			// inverted.
			otherAsteroid.movement = otherAsteroidReflection *
				// Same as above.
				((1 - otherAsteroidCollisionStrength) * otherAsteroidSpeed +
				(otherAsteroidCollisionStrength *
				(Math.Abs(otherAsteroidMass - asteroidMass) * otherAsteroidSpeed +
				(2 * asteroidMass * asteroidSpeed)) / bothMasses));

			asteroid.rotationSpeed.Y = (asteroid.rotationSpeed.Y +
				RandomHelper.GetRandomFloat(-0.75f, +1.0f)) / 2.0f;
			otherAsteroid.rotationSpeed.Y = (otherAsteroid.rotationSpeed.Y +
				RandomHelper.GetRandomFloat(-0.75f, +1.0f)) / 2.0f;
		} // HandleAsteroidCollision(asteroid, otherAsteroid)

		/// <summary>
		/// Reflect first vector to a surface with a normal vector.
		/// Basically will move away from normal and invert the current direction.
		/// </summary>
		/// <param name="reflectVector">Vector we want to reflect.</param>
		/// <param name="normal">Normal we use for reflection.</param>
		/// <returns>Reflected vector (same length as vec)</returns>
		private static Vector3 ReflectVector(Vector3 vec, Vector3 normal)
		{
			// Reflect and rescale to old length (looks like shader code ^^)
			// Note: All vectors should be normalized.
			return (vec - normal * 2.0f * Vector3.Dot(vec, normal));
		} // ReflectVector(vec, normal)
		#endregion

		#region Unit Testing
#if DEBUG
		#region Asteroid physics tests using AsteroidManager
		/// <summary>
		/// Asteroid physics using asteroid manager tests
		/// </summary>
		public class AsteroidPhysicsUsingAsteroidManagerTests
		{
			/// <summary>
			/// Asteroid manager
			/// </summary>
			PhysicsAsteroidManager asteroidManager = null;
			/// <summary>
			/// Asteroid model
			/// </summary>
			Model asteroidModel = null;

			#region Add asteroidModel
			/// <summary>
			/// Add asteroid
			/// </summary>
			/// <param name="pos">Position</param>
			/// <param name="size">Size</param>
			/// <param name="objectSize">Object size</param>
			/// <param name="movement">Movement</param>
			private void AddAsteroid(Vector3 pos,
				float size, float objectSize, Vector3 movement)
			{
				Asteroid asteroid = new Asteroid(0, pos);//, objectSize);
				asteroid.movement = movement;
				asteroid.size = size;
				asteroid.collisionRadius = size *
					Asteroid.CollisionSizeCorrectionFactor;
				//obs: asteroid.renderSize = size * objectSize;
				asteroidManager.sectorAsteroids[
					MiddleSector, MiddleSector].Add(asteroid);
			} // AddAsteroid(pos, size, objectSize)
			#endregion

			#region Setup scene
			/// <summary>
			/// Setup scene
			/// </summary>
			/// <param name="sceneNumber">Scene number</param>
			private void SetupScene(int sceneNumber)
			{
				// Kill all
				for (int z = 0; z < NumberOfSectors; z++)
					//for (int y = 0; y < NumberOfSectors; y++)
						for (int x = 0; x < NumberOfSectors; x++)
						{
							asteroidManager.sectorAsteroids[z, x] = new List<Asteroid>();
						} // for for for (int)

				switch (sceneNumber)
				{
					case 0:
						// Just add 2 colliding asteroids
						AddAsteroid(new Vector3(-50, 0, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(20, 0, 0));
						AddAsteroid(new Vector3(+50, 0, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(-20, 0, 0));
						break;

					case 1:
						// Collide fast moving asteroid with still one
						// Use same sizes and masses for 100% impluse transfer.
						AddAsteroid(new Vector3(-50, 0, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(40, 0, 0));
						AddAsteroid(new Vector3(+50, 0, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(0, 0, 0));
						break;

					case 2:
						// Collide with crazy movement values
						AddAsteroid(new Vector3(-35, +19, 8) * (-2),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(-35, +19, 8));
						AddAsteroid(new Vector3(+15, 40, 14) * (-2),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(+15, 40, 14));
						break;

					case 3:
						// Collide with up/down values
						AddAsteroid(new Vector3(0, 0, -50),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(0, 0, 40));
						AddAsteroid(new Vector3(0, 0, +70),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(0, 0, -20));
						break;

					case 4:
						// Collide with up/down values
						AddAsteroid(new Vector3(0, -50, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(0, 40, 0));
						AddAsteroid(new Vector3(0, +70, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(0, -20, 0));
						break;

					case 5:
						// Add 2 colliding asteroids with different sizes
						AddAsteroid(new Vector3(-50, 0, 0),
							2.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(20, 0, 0));
						AddAsteroid(new Vector3(+50, 0, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(-20, 0, 0));
						break;

					case 6:
						// Add 2 colliding asteroids with different sizes
						AddAsteroid(new Vector3(-50, +100, 0),
							2.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(20, -40, 0));
						AddAsteroid(new Vector3(+50, +100, 0),
							12.5f, asteroidModel.ObjectDownscaledSize,
							new Vector3(-20, -40, 0));
						break;
				} // switch
			} // SetupScene(sceneNumber)
			#endregion

			#region Test asteroid manager physics. Press 1-7 to start physics scenes.
			/// <summary>
			/// Test asteroid manager physics. Press 1-7 to start physics scenes.
			/// </summary>
			public void TestAsteroidPhysicsSmallScene()
			{
				TestGame.Start("TestAsteroidPhysicsSmallScene",
					delegate
					{
						asteroidManager = new GameAsteroidManager(
							Level.LoadAllLevels()[0]);
						asteroidModel = asteroidManager.asteroidModels[0];
						//new Model("Asteroid1.X");

						SetupScene(0);
					},
					delegate
					{
						BaseGame.GlowShader.Start();

						// Render sky cube map as our background.
						BaseGame.skyCube.RenderSky(1.0f, BaseGame.SkyBackgroundColor);

						BaseGame.camera.FreeCamera = true;
						BaseGame.EnableAlphaBlending();

						// Press 1-7 to start physics scenes.
						if (Input.KeyboardKeyJustPressed(Keys.D1))
							SetupScene(0);
						if (Input.KeyboardKeyJustPressed(Keys.D2))
							SetupScene(1);
						if (Input.KeyboardKeyJustPressed(Keys.D3))
							SetupScene(2);
						if (Input.KeyboardKeyJustPressed(Keys.D4))
							SetupScene(3);
						if (Input.KeyboardKeyJustPressed(Keys.D5))
							SetupScene(4);
						if (Input.KeyboardKeyJustPressed(Keys.D6))
							SetupScene(5);
						if (Input.KeyboardKeyJustPressed(Keys.D7))
							SetupScene(6);

						for (int z = 0; z < NumberOfSectors; z++)
							//for (int y = 0; y < NumberOfSectors; y++)
								for (int x = 0; x < NumberOfSectors; x++)
								{
									// Update all positions for this sector
									foreach (Asteroid asteroid in
										asteroidManager.sectorAsteroids[z, x])
										asteroid.UpdateMovement(BaseGame.MoveFactorPerSecond);

									// Handle physics (only calculate 50% per frame)
									//always: if ((z + x) % 2 == BaseGame.TotalFrames % 2)
										asteroidManager.HandleSectorPhysics(x, z,
											0, 0);
								} // asteroidModel

						// Make sure we start the mesh render manager for this frame.
						// It will copy all models from last frame over to this frame.
						//obs, handled in render now! BaseGame.MeshRenderManager.Init();

						// Render physics asteroids ourselfs
						for (int z = 0; z < NumberOfSectors; z++)
							//for (int y = 0; y < NumberOfSectors; y++)
								for (int x = 0; x < NumberOfSectors; x++)
									//if (x <= 1 && y <= 1 && z <= 1)
									if (asteroidManager.sectorIsVisible[z, x])
										foreach (Asteroid asteroid in
											asteroidManager.sectorAsteroids[z, x])
										{
											asteroidModel.Render(asteroid.RenderMatrix);
										} // asteroidModel
					});
			} // TestAsteroidPhysicsSmallScene()
			#endregion
		} // class AsteroidPhysicsUsingAsteroidManagerTests
		#endregion
#endif
		#endregion
	} // class PhysicsAsteroidManager
} // namespace RocketCommanderXna.Game
