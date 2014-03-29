// Project: Rocket Commander, File: SpaceCamera.cs
// Namespace: RocketCommanderXna.Game, Class: SpaceCamera
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 389, Size of file: 11,15 KB
// Creation date: 06.11.2005 03:03
// Last modified: 11.03.2006 02:05
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RocketCommanderXna.Game;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Space camera like in space sims (e.g. descent) or shooters.
	/// Move around with aswd or cursor keys and rotate with the mouse.
	/// Based on Quaternions to perform all the rotations required.
	/// Note: Handles also some game logic, which has to do with the
	/// rocket controlling in HandlePlayerInput.
	/// </summary>
	public class SpaceCamera : GameComponent
	{
		#region Variables
		/// <summary>
		/// Default camera up vector (used for Matrix.LookAt calls)
		/// </summary>
		private static readonly Vector3 DefaultCameraUp =
			new Vector3(0.0f, 0.0f, -1.0f);

		/// <summary>
		/// Current camera position.
		/// </summary>
		private Vector3 pos;

		/// <summary>
		/// Move directions for controling the camera.
		/// X is for moving left/right, Y moves up/down and Z goes in/out.
		/// </summary>
		private enum MoveDirections
		{
			/// <summary>
			/// X direction (move left/right)
			/// </summary>
			X,
			/// <summary>
			/// Y direction (move up/down)
			/// </summary>
			Y,
			/// <summary>
			/// Z direction (move in/out)
			/// </summary>
			Z,
		} // enum MoveDirections

		/// <summary>
		/// Rotation axis for controling the camera.
		/// Pitch is for moving head up/down,
		/// Roll rotates around nose
		/// and Yaw is the rotation to left/right.
		/// </summary>
		private enum RotationAxis
		{
			/// <summary>
			/// Pitch is for moving head up/down.
			/// </summary>
			Pitch,
			/*not supported here, makes controlling simpler
			/// <summary>
			/// Roll rotates around nose.
			/// </summary>
			Roll,
			 */
			/// <summary>
			/// Yaw is the rotation to left/right.
			/// </summary>
			Yaw
		} // enum RotationAxis

		/*obs
		/// <summary>
		/// Quaternion used for rotation.
		/// </summary>
		private Quaternion quaternion = Quaternion.Identity;
		 */

		/// <summary>
		/// Yaw and pitch rotations
		/// </summary>
		private float yawRotation = MathHelper.Pi,
			pitchRotation = 0;

		/// <summary>
		/// Set pitch rotation
		/// </summary>
		/// <param name="setPitchRotation">Set pitch rotation</param>
		public void SetPitchRotation(float setPitchRotation)
		{
			pitchRotation = setPitchRotation;
		} // SetPitchRotation(setPitchRotation)

		/// <summary>
		/// Camera modes
		/// </summary>
		public enum CameraModes
		{
			/// <summary>
			/// Default mode for menu, just rotating around.
			/// </summary>
			MenuMode,
			/// <summary>
			/// In game using the rocket controls (always flying forwards).
			/// </summary>
			InGame,
			/// <summary>
			/// Free camera mode, allows to freely rotate and move around,
			/// much cooler than the InGame mode for testing and stuff.
			/// </summary>
			FreeCamera,
		} // enum CameraModes

		/// <summary>
		/// Current camera mode.
		/// </summary>
		private CameraModes cameraMode = CameraModes.FreeCamera;//.MenuMode;

		/// <summary>
		/// Keys for moving around. Assigned from settings!
		/// </summary>
		private Keys moveLeftKey,
			moveRightKey,
			moveForwardKey,
			moveBackwardKey;//,
			//unused: rollLeftKey,
			//rollRightKey;

		/// <summary>
		/// Default to normal mouse sensibility, can be changed
		/// from 0.5 to 2.0.
		/// </summary>
		private float mouseSensibility = 1.0f;

		/// <summary>
		/// Rotation matrix, used in UpdateViewMatrix.
		/// </summary>
		private Matrix rotMatrix = Matrix.Identity;
		/// <summary>
		/// Rotation matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public Matrix RotationMatrix
		{
			get
			{
				return rotMatrix;
			} // get
		} // RotationMatrix

		/// <summary>
		/// Movement vector
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 MovementVector
		{
			get
			{
				Matrix invMatrix = Matrix.Invert(rotMatrix);
				return Vector3.TransformNormal(new Vector3(0, 0, 1), invMatrix);
			} // get
		} // MovementVector

		/// <summary>
		/// Get rotated movement vector
		/// </summary>
		/// <param name="relVector">Rel vector</param>
		/// <returns>Vector 3</returns>
		public Vector3 GetRotatedMovementVector(Vector3 relVector)
		{
			Matrix invMatrix = Matrix.Invert(rotMatrix);
			relVector.Normalize();
			return Vector3.TransformNormal(relVector, invMatrix);
		} // GetRotatedMovementVector(relVector)
		#endregion

		#region Properties
		/*
		/// <summary>
		/// Quaternion rotation
		/// </summary>
		/// <returns>Quaternion</returns>
		public Quaternion QuaternionRotation
		{
			get
			{
				return quaternion;
			} // get
			set
			{
				quaternion = value;
			} // set
		} // QuaternionRotation
		 */

		/// <summary>
		/// Get current x axis with help of the current view matrix.
		/// </summary>
		/// <returns>Vector 3</returns>
		static public Vector3 XAxis
		{
			get
			{
				// Get x column
				return new Vector3(
					BaseGame.ViewMatrix.M11,
					BaseGame.ViewMatrix.M21,
					BaseGame.ViewMatrix.M31);
			} // get
		} // XAxis

		/// <summary>
		/// Get current y axis with help of the current view matrix.
		/// </summary>
		/// <returns>Vector 3</returns>
		static public Vector3 YAxis
		{
			get
			{
				// Get y column
				return new Vector3(
					BaseGame.ViewMatrix.M12,
					BaseGame.ViewMatrix.M22,
					BaseGame.ViewMatrix.M32);
			} // get
		} // YAxis

		/// <summary>
		/// Get current z axis with help of the current view matrix.
		/// </summary>
		/// <returns>Vector 3</returns>
		static public Vector3 ZAxis
		{
			get
			{
				// Get z column
				return new Vector3(
					BaseGame.ViewMatrix.M13,
					BaseGame.ViewMatrix.M23,
					BaseGame.ViewMatrix.M33);
			} // get
		} // ZAxis

		/// <summary>
		/// Set if we are in game or not. Write only property.
		/// </summary>
		/// <returns>Bool</returns>
		public bool InGame
		{
			get
			{
				return cameraMode == CameraModes.InGame;
			} // get
			set
			{
				if (value)
					cameraMode = CameraModes.InGame;
				else
					cameraMode = CameraModes.MenuMode;
			} // set
		} // InGame

		/// <summary>
		/// Free camera
		/// </summary>
		/// <returns>Bool</returns>
		public bool FreeCamera
		{
			get
			{
				return cameraMode == CameraModes.FreeCamera;
			} // get
			set
			{
				if (value == true)
					cameraMode = CameraModes.FreeCamera;
				else
					cameraMode = CameraModes.InGame;
			} // set
		} // FreeCamera

		/// <summary>
		/// Position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 Position
		{
			get
			{
				return pos;
			} // get
		} // Position
		#endregion

		#region Constructor
		/// <summary>
		/// Create space camera
		/// </summary>
		/// <param name="setCameraPos">Set camera pos</param>
		public SpaceCamera(BaseGame game, Vector3 setCameraPos)
			: base(game)
		{
			pos = setCameraPos;

			// Assign keys. Warning: This is VERY slow, never use it
			// inside any render loop (getting Settings, etc.)!
			moveLeftKey = GameSettings.Default.MoveLeftKey;
			moveRightKey = GameSettings.Default.MoveRightKey;
			moveForwardKey = GameSettings.Default.MoveForwardKey;
			moveBackwardKey = GameSettings.Default.MoveBackwardKey;
			//rollLeftKey = GameSettings.Default.RollLeftKey;
			//rollRightKey = GameSettings.Default.RollRightKey;

			// Also assign mouse sensibility
			mouseSensibility = 2.5f -
				2.0f * GameSettings.Default.ControllerSensibility;
			if (mouseSensibility < 0.5f)
				mouseSensibility = 0.5f;
		} // SpaceCamera(game, setCameraPos)

		/// <summary>
		/// Create space camera
		/// </summary>
		/// <param name="setCameraPos">Set camera pos</param>
		/// <param name="setLookPos">Set look pos</param>
		public SpaceCamera(BaseGame game, Vector3 setCameraPos,
			Vector3 setLookPos)
			: this(game, setCameraPos)
		{
			// Calculate rotation quaternion from look pos
			SetLookAt(setCameraPos, setLookPos, DefaultCameraUp);
		} // SpaceCamera(game, setCameraPos, setLookPos)

		/*obs
		/// <summary>
		/// Create space camera
		/// </summary>
		/// <param name="setCameraPos">Set camera pos</param>
		/// <param name="setRotationQuaternion">Set rotation quaternion</param>
		public SpaceCamera(BaseGame game, Vector3 setCameraPos,
			Quaternion setRotationQuaternion)
			: this(game, setCameraPos)
		{
			quaternion = setRotationQuaternion;
		} // SpaceCamera(game, setCameraPos, setRotationQuaternion)
		 */
		#endregion

		#region Set position
		/// <summary>
		/// Set position
		/// </summary>
		/// <param name="setCameraPos">Set camera position</param>
		public void SetPosition(Vector3 setCameraPos)
		{
			pos = setCameraPos;
		} // SetPosition(setCameraPos)
		#endregion

		#region Helper method to get quaternion from LookAt matrix (private)
		/// <summary>
		/// Helper method to get quaternion from LookAt matrix.
		/// </summary>
		/// <param name="setCamPos">Set cam pos</param>
		/// <param name="setLookPos">Set look pos</param>
		/// <param name="setUpVector">Set up vector</param>
		public void SetLookAt(
			Vector3 setCamPos, Vector3 setLookPos, Vector3 setUpVector)
		{
			pos = setCamPos;

			/*obs
			// Build look at matrix and get the quaternion from that
			quaternion = Quaternion.CreateFromRotationMatrix(
				Matrix.CreateLookAt(pos, setLookPos, setUpVector));
			 */
		} // SetLookAt(setCamPos, setLookPos, setUpVector)
		#endregion

		#region Rotate methods
		/// <summary>
		/// Rotate around pitch, roll or yaw axis.
		/// </summary>
		/// <param name="axis">Axis</param>
		/// <param name="angle">Angle</param>
		private void Rotate(RotationAxis axis, float angle)
		{
			if (axis == RotationAxis.Yaw)
				yawRotation -= angle;
			else
				pitchRotation -= angle;

			/*left handed!
			if (axis == RotationAxis.Yaw)
				yawRotation += angle;
			else
				pitchRotation += angle;

			/*old from RC:
			Rotate(
				axis == RotationAxis.Pitch ? new Vector3(1.0f, 0.0f, 0.0f) :
				// Our world is xz based for some reason ^^ this way it works
				axis == RotationAxis.Roll ? new Vector3(0.0f, 0.0f, 1.0f) :
				new Vector3(0.0f, 1.0f, 0.0f), angle);
				 */
		} // Rotate(axis, angle)

		/*obs
		/// <summary>
		/// Rotate
		/// </summary>
		/// <param name="axis">Axis</param>
		/// <param name="angle">Angle</param>
		private void Rotate(Vector3 axis, float angle)
		{
			quaternion *= Quaternion.CreateFromAxisAngle(axis, angle);
		} // Rotate(axis, angle)
		 */
		#endregion

		#region Translate method
		/// <summary>
		/// Translate into x, y or z axis with a specfic amount.
		/// </summary>
		/// <param name="amount">Amount</param>
		/// <param name="direction">Direction</param>
		private void Translate(float amount, MoveDirections direction)
		{
			Vector3 dir =
				direction == MoveDirections.X ? XAxis :
				direction == MoveDirections.Y ? YAxis : ZAxis;
			pos += dir * amount;
		} // Translate(amount, direction)
		#endregion

		#region Handle free camera
		/// <summary>
		/// Handle free camera
		/// </summary>
		private void HandleFreeCamera()
		{
			float moveFactor =
				BaseGame.MoveFactorPerSecond * 250.0f;//100.0f;
			float slideFactor = moveFactor;// / 10.0f;
			float rotationFactor =
				5 * Player.RotationSpeedPerMouseMovement;//0.0015f;//0.02f;

			#region Mouse/keyboard support
			/*obs
			// Always change camera rotation when mouse is moved,
			// this is how we control our rocket.
			if (Input.MouseRightButtonPressed)
			{
				if (Input.MouseXMovement != 0.0f ||
					Input.MouseYMovement != 0.0f)
				{
					Rotate(RotationAxis.Roll, rotationFactor *
						(Input.MouseXMovement - Input.MouseYMovement));
				} // if (Input.MouseYMovement)
			} // if (Input.MouseRightButtonPressed)
			else
			 */
			if (Input.MouseXMovement != 0.0f ||
				Input.MouseYMovement != 0.0f)
			{
				Rotate(RotationAxis.Yaw,
					-Input.MouseXMovement * rotationFactor);
				Rotate(RotationAxis.Pitch,
					-Input.MouseYMovement * rotationFactor);
			} // if (Mouse.left.Pressed)

			// Use asdw (qwerty keyboard), aoew (dvorak keyboard) or
			// cursor keys (all keyboards?) to move around.
			// Note: If you want to change this keys, you should implement
			// a config file and an options screen.
			if (Input.Keyboard.IsKeyDown(moveLeftKey) ||
				Input.Keyboard.IsKeyDown(Keys.Left) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad4))
				Translate(-slideFactor, MoveDirections.X);
			if (Input.Keyboard.IsKeyDown(moveRightKey) ||
				Input.Keyboard.IsKeyDown(Keys.Right) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad6))
				Translate(+slideFactor, MoveDirections.X);
			if (Input.Keyboard.IsKeyDown(moveForwardKey) ||
				Input.Keyboard.IsKeyDown(Keys.Up) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad8))
				Translate(-moveFactor, MoveDirections.Z);
			if (Input.Keyboard.IsKeyDown(moveBackwardKey) ||
				Input.Keyboard.IsKeyDown(Keys.Down) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad2))
				Translate(+moveFactor, MoveDirections.Z);

			/*obs
			// Use Q and E (or _ and P) to roll
			if (Input.Keyboard.IsKeyDown(rollLeftKey))
				Rotate(RotationAxis.Roll, +BaseGame.MoveFactorPerSecond * 2);
			if (Input.Keyboard.IsKeyDown(rollRightKey))
				Rotate(RotationAxis.Roll, -BaseGame.MoveFactorPerSecond * 2);
			 */
			#endregion

			#region Xbox 360 controller support
			// Change camera rotation when right thumb is used.
			if (Input.GamePad.ThumbSticks.Right.X != 0.0f ||
				Input.GamePad.ThumbSticks.Right.Y != 0.0f)
			{
				// Limit x/y movement to max. 150 units per frame
				float xMovement = Input.GamePad.ThumbSticks.Right.X;
				float yMovement = Input.GamePad.ThumbSticks.Right.Y;
				Rotate(RotationAxis.Yaw,
					-xMovement * rotationFactor * 4);
				Rotate(RotationAxis.Pitch,
					yMovement * rotationFactor * 4);
			} // if (Mouse.left.Pressed)

			// Move and slide with left thumb stick
			if (Input.GamePad.ThumbSticks.Left.X != 0)
				Translate(moveFactor * Input.GamePad.ThumbSticks.Left.X * 2,
					MoveDirections.X);
			if (Input.GamePad.ThumbSticks.Left.Y != 0)
				Translate(-moveFactor * Input.GamePad.ThumbSticks.Left.Y * 4,
					MoveDirections.Z);
			#endregion
		} // HandleFreeCamera()
		#endregion

		#region Handle player input
		/// <summary>
		/// Handle player input for the game. This is where all the input happens.
		/// </summary>
		private void HandlePlayerInput()
		{
			if (Player.lifeTimeMs < Player.LifeTimeZoomAndAccelerateMs)
			{
				float speedPercentage =
					//Player.lifeTimeMs < 3000 ? 0 :
					//(Player.lifeTimeMs - 3000) / 2000.0f;
					Player.lifeTimeMs / (float)Player.LifeTimeZoomAndAccelerateMs;

				// Use quadradric product for better speed up effect
				Player.SetStartingSpeed(speedPercentage * speedPercentage);

				// Always move forward
				Translate(-Player.Speed *
					BaseGame.MoveFactorPerSecond *
					Player.MovementSpeedPerSecond, MoveDirections.Z);

				if (Player.gameTimeMs < 100)//3000)
				{
					//quaternion = Quaternion.Identity;
					yawRotation = MathHelper.Pi;
					pitchRotation = 0;
					pos = Vector3.Zero;
				} // if
			} // if
			
			if (Player.CanControlRocket)
			{
				// Consume some fuel
				Player.fuel -= BaseGame.MoveFactorPerSecond /
					Player.FuelRefillTime;

				float speedScoreFactor = 0.75f * Player.Speed;
				if (Player.speedItemTimeout > 0)
					speedScoreFactor *= 2.0f;

				// Increase score
				if (BaseGame.TotalFrames % 10 == 0)
					Player.score +=
						(int)(speedScoreFactor * BaseGame.ElapsedTimeThisFrameInMs);
			} // if

			float rotationFactor = Player.RotationSpeedPerMouseMovement;
			const float XboxControllerFactor = 2.5f;// 3.25f;
			// Don't allow any movement if still counting down or exploded
			if (Player.CanControlRocket == false)
			{
				if (Player.GameOver)
				{
					// Just rotate around the rocket.
					//quaternion *= Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0),
					//	BaseGame.TotalTimeMs / 3592.0f);
					yawRotation = BaseGame.TotalTimeMs / 3592.0f;
				} // if
				else
				{
					// Allow rotation if dead
					if (Input.MouseXMovement != 0.0f ||
						Input.MouseYMovement != 0.0f)
					{
						// Limit x/y movement to max. 150 units per frame
						float xMovement = Input.MouseXMovement;
						float yMovement = Input.MouseYMovement;
						Rotate(RotationAxis.Yaw,
							-xMovement * mouseSensibility * rotationFactor);
						Rotate(RotationAxis.Pitch,
							-yMovement * mouseSensibility * rotationFactor);
					} // if (GameForm.Mouse.XMovement)

					rotationFactor = XboxControllerFactor * mouseSensibility *
						BaseGame.MoveFactorPerSecond;
					if (Input.GamePad.ThumbSticks.Right.X != 0.0f ||
						Input.GamePad.ThumbSticks.Right.Y != 0.0f)
					{
						// Limit x/y movement to max. 150 units per frame
						float xMovement = Input.GamePad.ThumbSticks.Right.X;
						float yMovement = Input.GamePad.ThumbSticks.Right.Y;
						Rotate(RotationAxis.Yaw,
							-xMovement * rotationFactor);
						Rotate(RotationAxis.Pitch,
							yMovement * rotationFactor);
					} // if (Mouse.left.Pressed)
				} // else

				return;
			} // if

			float maxMoveFactor = BaseGame.MoveFactorPerSecond *
				Player.MovementSpeedPerSecond;
			float maxSlideFactor = maxMoveFactor * Player.SlideFactor;
			rotationFactor = Player.RotationSpeedPerMouseMovement;

			#region Mouse/keyboard support
			/*obs
			// Right mouse button lets us roll around our axis
			if (Input.MouseRightButtonPressed)
			{
				if (Input.MouseXMovement != 0.0f ||
					Input.MouseYMovement != 0.0f)
				{
					Rotate(RotationAxis.Roll, rotationFactor *
						(Input.MouseXMovement - Input.MouseYMovement));
				} // if (Input.MouseYMovement)
			} // if (Input.MouseRightButtonPressed)
			// Always change camera rotation when mouse is moved,
			// this is how we control our rocket.
			else
			 */
			if (Input.MouseXMovement != 0.0f ||
				Input.MouseYMovement != 0.0f)
			{
				// Limit x/y movement to max. 150 units per frame
				float xMovement = Input.MouseXMovement;
				float yMovement = Input.MouseYMovement;
				Rotate(RotationAxis.Yaw,
					-xMovement * rotationFactor);
				Rotate(RotationAxis.Pitch,
					-yMovement * rotationFactor);
			} // if (Mouse.left.Pressed)

			bool consumedAdditionalFuel = false;

			// Use asdw (qwerty keyboard), aoew (dvorak keyboard) or
			// cursor keys (all keyboards?) to move around.
			// Note: If you want to change any keys, use Settings!
			if (Input.Keyboard.IsKeyDown(moveForwardKey) ||
				Input.Keyboard.IsKeyDown(Keys.Up) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad8))
			{
				float oldPlayerSpeed = Player.Speed;
				Player.Speed += 0.75f * BaseGame.MoveFactorPerSecond;

				// Only decrease fuel if change happened
				if (oldPlayerSpeed != Player.Speed)
					consumedAdditionalFuel = true;
			} // if
			if (Input.Keyboard.IsKeyDown(moveBackwardKey) ||
				Input.Keyboard.IsKeyDown(Keys.Down) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad2))
			{
				float oldPlayerSpeed = Player.Speed;
				Player.Speed -= 0.75f * BaseGame.MoveFactorPerSecond;

				// Only decrease fuel if change happened
				if (oldPlayerSpeed != Player.Speed)
					consumedAdditionalFuel = true;
			} // if

			if (Player.speedItemTimeout > 0)
			{
				Player.speedItemTimeout -= BaseGame.ElapsedTimeThisFrameInMs;
				if (Player.speedItemTimeout < 0)
				{
					Player.speedItemTimeout = 0;
					// Reduce to max. possible speed
					if (Player.Speed > Player.MaxSpeedWithoutItem)
						Player.Speed = Player.MaxSpeedWithoutItem;
				} // if
			} // if

			// Adjust current speed by the current player speed.
			float moveFactor = Player.Speed * maxMoveFactor;
			float slideFactor = maxSlideFactor;

			// Always move forward (- because game was designed left handed originally)
			Translate(-moveFactor, MoveDirections.Z);

			// Slide
			if (Input.Keyboard.IsKeyDown(moveLeftKey) ||
				Input.Keyboard.IsKeyDown(Keys.Left) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad4))
			{
				consumedAdditionalFuel = true;
				Translate(-slideFactor, MoveDirections.X);
			} // if
			if (Input.Keyboard.IsKeyDown(moveRightKey) ||
				Input.Keyboard.IsKeyDown(Keys.Right) ||
				Input.Keyboard.IsKeyDown(Keys.NumPad6))
			{
				consumedAdditionalFuel = true;
				Translate(+slideFactor, MoveDirections.X);
			} // if

      // Up/down
      if (Input.Keyboard.IsKeyDown(Keys.F))
      {
          Translate(+slideFactor, MoveDirections.Y);
      } // if
      if (Input.Keyboard.IsKeyDown(Keys.V))
      {
          Translate(-slideFactor, MoveDirections.Y);
      } // if

			/*obs
			// Use Q and E (or _ and P) to roll
			if (Input.Keyboard.IsKeyDown(rollLeftKey))
				Rotate(RotationAxis.Roll,
					+BaseGame.MoveFactorPerSecond * 2.0f);
			if (Input.Keyboard.IsKeyDown(rollRightKey))
				Rotate(RotationAxis.Roll,
					-BaseGame.MoveFactorPerSecond * 2.0f);
			 */
			#endregion

			#region Input support for the XBox360 controller
			// 2006-03-09: Added Input support
			rotationFactor = XboxControllerFactor * mouseSensibility *
				BaseGame.MoveFactorPerSecond;

			/*obs
			// Left and right triggers lets us roll around our axis
			if (Input.GamePad.Triggers.Left != 0 ||
				Input.GamePad.Triggers.Right != 0)
			{
				Rotate(RotationAxis.Roll, rotationFactor *
					(-Input.GamePad.Triggers.Left + Input.GamePad.Triggers.Right));
			} // if (Input.GamePad.Triggers.Left)
			 */
			// Change camera rotation when right thumb is used.
			if (Input.GamePad.ThumbSticks.Right.X != 0.0f ||
				Input.GamePad.ThumbSticks.Right.Y != 0.0f)
			{
				// Limit x/y movement to max. 150 units per frame
				float xMovement = Input.GamePad.ThumbSticks.Right.X;
				float yMovement = Input.GamePad.ThumbSticks.Right.Y;
				Rotate(RotationAxis.Yaw,
					-xMovement * rotationFactor);
				Rotate(RotationAxis.Pitch,
					yMovement * rotationFactor);
			} // if (Mouse.left.Pressed)

			// Use left thumb for moving around
			if (Input.GamePad.ThumbSticks.Left.Y != 0)
			{
				float oldPlayerSpeed = Player.Speed;
				Player.Speed += 0.25f * Input.GamePad.ThumbSticks.Left.Y *
					BaseGame.MoveFactorPerSecond;

				// Only decrease fuel if change happened
				if (oldPlayerSpeed != Player.Speed)
					consumedAdditionalFuel = true;
			} // if

			// Slide
			if (Input.GamePad.ThumbSticks.Left.X != 0)
			{
				consumedAdditionalFuel = true;
				Translate(slideFactor * Input.GamePad.ThumbSticks.Left.X * 2,
					MoveDirections.X);
			} // if
			#endregion

			if (consumedAdditionalFuel)
			{
				// Decrease additional fuel
				Player.fuel -= (BaseGame.MoveFactorPerSecond /
					Player.FuelRefillTime) / 2.0f;

				// Increase score, but only 50% of the amount above.
				// Note: This happens only if we change speed, that is not often.
				if (BaseGame.TotalFrames % 20 == 0)
					Player.score += (int)BaseGame.ElapsedTimeThisFrameInMs / 2;
			} // if
		} // HandlePlayerInput()
		#endregion

		#region Randomly rotate around
		/// <summary>
		/// Randomly rotate around
		/// </summary>
		private void RandomlyRotateAround()
		{
			float moveFactor = BaseGame.MoveFactorPerSecond * 27.5f;
			float rotationFactor = BaseGame.MoveFactorPerSecond * 0.125f;

			// Rotate slightly around
			Rotate(RotationAxis.Yaw,
				(0.4f + 0.25f * (float)Math.Sin(BaseGame.TotalTimeMs / 15040)) *
				rotationFactor);
			Rotate(RotationAxis.Pitch,
				(0.35f + 0.212f * (float)Math.Cos(BaseGame.TotalTimeMs / 38040)) *
				rotationFactor);

			// Just move forward
			Translate(-moveFactor, MoveDirections.Z);

			if (pos.X < -12 * BaseAsteroidManager.SectorWidth)
				pos.X = +10;
			if (pos.X > 12 * BaseAsteroidManager.SectorWidth)
				pos.X = -10;
			if (pos.Y < -12 * BaseAsteroidManager.SectorWidth)
				pos.Y = +10;
			if (pos.Y > 12 * BaseAsteroidManager.SectorWidth)
				pos.Y = -10;
		} // RandomlyRotateAround()
		#endregion

		#region Update view matrix
		/// <summary>
		/// Update view matrix
		/// </summary>
		private void UpdateViewMatrix()
		{
			/*
			// Rotate world by negative of quaternion and translate everything
			// by camera position.
			quaternion.Normalize();
			rotMatrix = Matrix.CreateFromQuaternion(quaternion);
			 */

			// Limit pitchRotation to -90 degrees to +90 degrees.
			// This will stop the player if he flys to straight up or down
			// and make it impossible to fly bottom up (which is very hard).
			if (pitchRotation < -(float)Math.PI / 2.0f)
				pitchRotation = -(float)Math.PI / 2.0f;
			if (pitchRotation > (float)Math.PI / 2.0f)
				pitchRotation = (float)Math.PI / 2.0f;

			rotMatrix =
				Matrix.CreateRotationY(yawRotation) *
				Matrix.CreateRotationX(pitchRotation);

			// Add camera shake if camera wobbel effect is on
			if (Player.cameraWobbelTimeoutMs > 0 &&
				// But only if not zooming in and if in game.
				Player.lifeTimeMs > Player.LifeTimeZoomAndAccelerateMs &&
				InGame)
			{
				float effectStrength = 8.0f *
					Player.cameraWobbelFactor *
					(Player.cameraWobbelTimeoutMs /
					(float)Player.MaxCameraWobbelTimeoutMs);
				rotMatrix *= Matrix.CreateTranslation(
					RandomHelper.GetRandomVector3(
					-effectStrength, +effectStrength));
			} // if

			// Just set view matrix
			BaseGame.ViewMatrix = Matrix.CreateTranslation(-pos) * rotMatrix;
		} // UpdateViewMatrix()
		#endregion

		#region Update
		/// <summary>
		/// Update camera, should be called every frame to handle all the input.
		/// Update: Support for XNA GameComponent class :)
		/// </summary>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (cameraMode == CameraModes.FreeCamera)
				HandleFreeCamera();
			else if (cameraMode == CameraModes.InGame)
				HandlePlayerInput();
			else // for menu
				RandomlyRotateAround();
			
			UpdateViewMatrix();
		} // Update()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test space camera
		/// </summary>
		public static void TestSpaceCamera()
		{
			Model testModel = null;

			TestGame.Start("TestSpaceCamera",
				delegate // Init
				{
					testModel = new Model("asteroid4");
				},
				delegate // Render loop
				{
					// Just render testModel in the middle of the scene.
					testModel.Render(Vector3.Zero);

					TextureFont.WriteText(1, 20,
						"MoveFactorPerSecond=" + BaseGame.MoveFactorPerSecond);
				});
		} // TestSpaceCamera()
#endif
		#endregion
	} // class SpaceCamera
} // namespace RocketCommanderXna.Game
