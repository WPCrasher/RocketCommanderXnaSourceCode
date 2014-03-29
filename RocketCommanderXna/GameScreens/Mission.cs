// Project: Rocket Commander, File: Mission.cs
// Namespace: RocketCommanderXna.GameScreens, Class: Mission
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 46, Size of file: 857 Bytes
// Creation date: 01.11.2005 23:55
// Last modified: 08.12.2005 16:14
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = RocketCommanderXna.Graphics.Texture;
using Model = RocketCommanderXna.Graphics.Model;
using RocketCommanderXna.Sounds;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Mission, just manages the on screen display for the game.
	/// Controlling is done in SpaceCamera class.
	/// Most graphical stuff is done in AsteroidManager.
	/// </summary>
	class Mission : IGameScreen
	{
		#region Variables
		/// <summary>
		/// Link to game.hudTexture and inGameTexture, the texture itself is not
		/// loaded here, but in the main game. This are just references.
		/// </summary>
		private Texture hudTexture, inGameTexture, levelTexture;

		/// <summary>
		/// Rectangles for InGame.png.
		/// Used to display on screen hud.
		/// </summary>
		private static readonly Rectangle
			HudRect = new Rectangle(0, 0, 256, 157),
			ArrowRightRect = new Rectangle(0, 0, 53, 57),
			ArrowLeftRect = new Rectangle(53, 0, 53, 57),
			ArrowUpRect = new Rectangle(0, 57, 53, 57),
			ArrowDownRect = new Rectangle(53, 57, 53, 57),
			TargetIconRect = new Rectangle(106, 49, 61, 61),
			RocketLifeRect = new Rectangle(106, 0, 37, 49),
			MiniMapRocketRect = new Rectangle(143, 21, 24, 24),
			MiniMapBackgroundRect = new Rectangle(168, 0, 88, 115),
			FuelBarRect = new Rectangle(0, 115, 256, 81),
			HealthBarRect = new Rectangle(0, 196, 256, 29),
			SpeedBarRect = new Rectangle(0, 225, 256, 31),
			// Those 2 are not on inGameTexture, but on hudTexture:
			MiniMapFieldOfViewRect = new Rectangle(1, 256 - 80, 80, 80),
			ScreenBorderEffectRect = new Rectangle(164, 164, 90, 90);

		/// <summary>
		/// Game text and hud colors
		/// </summary>
		private static readonly Color
			GameTextColor = Color.Green,//.DarkGreen,
			HudColor = Color.LightGreen,
			HudRedColor = Color.Red;

		/// <summary>
		/// Usable width for fuel, health and speed bars!
		/// </summary>
		private const int FuelHealthAndSpeedWidth = 208;

		/// <summary>
		/// Distance for hud to screen borders.
		/// </summary>
		private const int OriginalHudDistanceFromBorder = 8;

		/// <summary>
		/// Hud background scale factor, makes hud a little bit bigger.
		/// </summary>
		private const float HudBackgroundScaleFactor = 1.33f,//1.125f;
			HudElementScaleFactor = 1.33f / 1.075f;
		#endregion
		
		#region Properties
		/// <summary>
		/// Name of this game screen
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return "Mission";
			} // get
		} // Name

		private bool quit = false;
		/// <summary>
		/// Returns true if we want to quit this screen and return to the
		/// previous screen. If no more screens are left the game is exited.
		/// </summary>
		/// <returns>Bool</returns>
		public bool Quit
		{
			get
			{
				return quit;
			} // get
		} // Quit
		#endregion

		#region Constructor
		/// <summary>
		/// Create mission
		/// </summary>
		public Mission(Level level, GameAsteroidManager asteroidManager)
		{
			// Set level for asteroid manager.
			asteroidManager.CurrentLevel = level;

			// Start new game.
			Player.Reset(level.Name);
		} // Mission()
		#endregion

		#region Zoom into rocket
		float rocketEndGameScale = 0.0f;
		/// <summary>
		/// Zoom into rocket
		/// </summary>
		/// <param name="camera">Camera</param>
		private void ZoomIntoRocket(SpaceCamera camera, Model rocketModel)
		{
			if (Player.GameOver)
			{
				// Only start showing rocket after all explosions are nearly over!
				// Only 400ms left.
				if (Player.explosionTimeoutMs < 400 &&
					Player.explosionTimeoutMs2 < 400 &&
					Player.explosionTimeoutMs3 < 400)
				{
					// Make sure z buffer is on
					BaseGame.Device.RenderState.DepthBufferEnable = true;

					// Scale in rocket (cool zoom effect)
					rocketEndGameScale += BaseGame.MoveFactorPerSecond * 5.0f;
					if (rocketEndGameScale > 10.0f)
						rocketEndGameScale = 10.0f;
					float scale = rocketEndGameScale;

					// Show rocket in middle of screen.
					Vector3 inFrontOfCameraPos =
						new Vector3(0, -0.3f, -1.75f) * 10.0f;
					inFrontOfCameraPos = Vector3.TransformNormal( 
						inFrontOfCameraPos, BaseGame.InverseViewMatrix);
					Matrix startMatrix =
						Matrix.CreateScale(scale, scale, scale) *
						Matrix.CreateRotationX((float)Math.PI / 2.0f) *
						Matrix.CreateRotationY(BaseGame.TotalTimeMs / 2293.0f) *
						Matrix.CreateRotationX(-(float)Math.PI) *
						Matrix.CreateRotationZ((float)Math.PI / 4.0f) *
						Matrix.CreateTranslation(inFrontOfCameraPos + BaseGame.CameraPos);
					rocketModel.Render(startMatrix);

					// Disable z buffer, now only 2d content is rendered.
					BaseGame.Device.RenderState.DepthBufferEnable = false;
				} // if
				else
					rocketEndGameScale = 0.0f;
			} // if
		} // ZoomIntoRocket(camera)
		#endregion

		#region Show target
		/// <summary>
		/// Return angle between two vectors. Used for visbility testing.
		/// </summary>
		/// <param name="vec1">Vector 1</param>
		/// <param name="vec2">Vector 2</param>
		/// <returns>Float</returns>
		public float GetAngleBetweenVectors(Vector3 vec1, Vector3 vec2)
		{
			// See http://en.wikipedia.org/wiki/Vector_(spatial)
			// for help and check out the Dot Product section ^^
			// Both vectors are normalized so we can save deviding through the
			// lengths.
			return (float)Math.Acos(Vector3.Dot(vec1, vec2));
		} // Angle(vec1, vec2)

		/// <summary>
		/// Show arrow to target position on screen.
		/// Will show the target itself if it is on screen, else a arrow
		/// pointing to the nearest direction is shown at the screen border.
		/// Will also display the distance to the target below the arrow or target.
		/// </summary>
		/// <param name="levelLength">Level length for target position</param>
		private void ShowTarget(int levelLength)
		{
			// First find out where the target is.
			Vector3 targetPosition = GameAsteroidManager.TargetPosition;
			float targetDistance = (targetPosition - BaseGame.CameraPos).Length();
			Point screenPos =
				BaseGame.Convert3DPointTo2D(targetPosition);
			bool isVisible =
				BaseGame.IsInFrontOfCamera(targetPosition);

			if (isVisible &&
				screenPos.X >= 0 && screenPos.X < BaseGame.Width &&
				screenPos.Y >= 0 && screenPos.Y < BaseGame.Height)
			{
				// Don't show if in middle (background is already shown
				// and we introduce the problem of z buffer sorting with the
				// alpha of the asteroids). Only show if getting close to the borders.
				Vector2 distanceVector = new Vector2(
					screenPos.X - BaseGame.Width / 2,
					screenPos.Y - BaseGame.Height / 2);
				float borderFactor = distanceVector.Length() /
					(float)(BaseGame.Height / 2);
				if (borderFactor > 1.0f)
					borderFactor = 1.0f;
				inGameTexture.RenderOnScreen(new Rectangle(
					screenPos.X - TargetIconRect.Width / 2,
					screenPos.Y - TargetIconRect.Height / 2,
					TargetIconRect.Width, TargetIconRect.Height),
					TargetIconRect,
					Color.White, borderFactor);

				GameAsteroidManager.AddDistanceToBeDisplayed(screenPos, targetDistance,
					0.25f + 0.75f * (1.0f -
					(targetDistance / (levelLength * GameAsteroidManager.SectorDepth))));//,
					//GameAsteroidManager.ProductLogo.None);
			} // if (isVisible)
			else
			{
				// We have to normalize the target position for angle calculations.
				targetPosition.Normalize();

				// Find out direction of the target by calculating 4 angels
				// for all 4 directions and then choosing the closest one!
				Vector3 leftRotation = new Vector3(-1, 0, 0);
				leftRotation = Vector3.TransformNormal( leftRotation,
					BaseGame.InverseViewMatrix);
				float leftAngle =
					GetAngleBetweenVectors(leftRotation, targetPosition);

				Vector3 rightRotation = new Vector3(+1, 0, 0);
				rightRotation = Vector3.TransformNormal( rightRotation,
					BaseGame.InverseViewMatrix);
				float rightAngle =
					GetAngleBetweenVectors(rightRotation, targetPosition);

				Vector3 upRotation = new Vector3(0, +1, 0);
				upRotation = Vector3.TransformNormal( upRotation,
					BaseGame.InverseViewMatrix);
				float upAngle =
					GetAngleBetweenVectors(upRotation, targetPosition);

				Vector3 downRotation = new Vector3(0, -1, 0);
				downRotation = Vector3.TransformNormal( downRotation,
					BaseGame.InverseViewMatrix);
				float downAngle =
					GetAngleBetweenVectors(downRotation, targetPosition);

				// Find out direction and screen coordinates on the screen borders.
				float xPosPercent = 0.5f + (leftAngle - (float)Math.PI / 2.0f) * 0.6f;
				float yPosPercent = 0.5f + (upAngle - (float)Math.PI / 2.0f) * 0.85f;
				int xPos = (int)(xPosPercent * BaseGame.Width) -
					ArrowLeftRect.Width / 2;
				int yPos = (int)(yPosPercent * BaseGame.Height) -
					ArrowLeftRect.Height / 2;

				// Make sure arrows are fully visible!
				if (xPos < 0)
					xPos = 0;
				if (xPos >= BaseGame.Width - ArrowLeftRect.Width)
					xPos = BaseGame.Width - ArrowLeftRect.Width - 1;
				if (yPos < 0)
					yPos = 0;
				if (yPos >= BaseGame.Height - ArrowLeftRect.Height)
					yPos = BaseGame.Height - ArrowLeftRect.Height - 1;

				// First try, just get the closest angel
				if (leftAngle < rightAngle &&
					leftAngle < upAngle &&
					leftAngle < downAngle)
				{
					// Left border
					inGameTexture.RenderOnScreen(new Rectangle(
						0, yPos, ArrowLeftRect.Width, ArrowLeftRect.Height),
						ArrowLeftRect);
					GameAsteroidManager.AddDistanceToBeDisplayed(new Point(
						ArrowLeftRect.Width * 2 - 7 + BaseGame.XToRes(8),
						yPos + ArrowLeftRect.Height / 2),
						targetDistance, 0.85f);//,
						//GameAsteroidManager.ProductLogo.None);
				} // if (leftAngle)
				else if (rightAngle < upAngle &&
					rightAngle < downAngle)
				{
					// Right border
					inGameTexture.RenderOnScreen(new Rectangle(
						BaseGame.Width - ArrowRightRect.Width, yPos,
						ArrowRightRect.Width, ArrowRightRect.Height),
						ArrowRightRect);
					GameAsteroidManager.AddDistanceToBeDisplayed(new Point(
						BaseGame.Width - ArrowRightRect.Width * 2 - BaseGame.XToRes(10),
						yPos + ArrowLeftRect.Height / 2),
						targetDistance, 0.85f);//,
						//GameAsteroidManager.ProductLogo.None);
				} // else if
				else if (upAngle < downAngle)
				{
					// Top border
					inGameTexture.RenderOnScreen(new Rectangle(
						xPos, 0, ArrowUpRect.Width, ArrowUpRect.Height),
						ArrowUpRect);
					GameAsteroidManager.AddDistanceToBeDisplayed(new Point(
						xPos + ArrowRightRect.Width / 2, ArrowRightRect.Height / 2 - 4),
						targetDistance, 0.85f);//,
						//GameAsteroidManager.ProductLogo.None);
				} // else if
				else
				{
					// Bottom border
					inGameTexture.RenderOnScreen(new Rectangle(
						xPos, BaseGame.Height - ArrowDownRect.Height,
						ArrowDownRect.Width, ArrowDownRect.Height),
						ArrowDownRect);

					// We can't add the text below the arrow because there is
					// no space left. We have to display it on top!
					GameAsteroidManager.AddDistanceToBeDisplayed(new Point(
						xPos + ArrowRightRect.Width / 2,
						BaseGame.Height - ArrowDownRect.Height * 2 + 5),
						targetDistance, 0.85f);//,
						//GameAsteroidManager.ProductLogo.None);
				} // else
			} // else
		} // ShowTarget(levelLength)
		#endregion

		#region Show on screen effects
		/// <summary>
		/// Show on screen effects
		/// </summary>
		private void ShowOnScreenEffects(AnimatedTexture explosionTexture)
		{
			// Show explosion if we just died
			if (Player.explosionTimeoutMs > 0)
			{
				int screenSize =
					Math.Max(BaseGame.Width * 2 / 3, BaseGame.Height * 2 / 3);
				float explosionTimePercent = 1.0f -
					(Player.explosionTimeoutMs / (float)Player.MaxExplosionTimeoutMs);
				explosionTexture.Select(
					(int)(explosionTexture.AnimationLength * explosionTimePercent));
				explosionTexture.RenderOnScreen(new Rectangle(
					BaseGame.Width / 2 - screenSize / 2,
					BaseGame.Height / 2 - screenSize / 2,
					screenSize, screenSize));

				// Just rumble very heavy
				Input.GamePadRumble(0.8f, 0.9f);
			} // if

			if (Player.explosionTimeoutMs2 > 0 &&
				Player.explosionTimeoutMs2 < Player.MaxExplosionTimeoutMs)
			{
				int screenSize = Math.Max(BaseGame.Width * 2 / 3, BaseGame.Height * 2 / 3);
				float explosionTimePercent = 1.0f -
					(Player.explosionTimeoutMs2 / (float)Player.MaxExplosionTimeoutMs);
				explosionTexture.Select(
					(int)(explosionTexture.AnimationLength * explosionTimePercent));
				explosionTexture.RenderOnScreen(new Rectangle(
					BaseGame.Width / 2 - screenSize / 2 + BaseGame.Width / 7,
					BaseGame.Height / 2 - screenSize / 2 + BaseGame.Height / 12,
					screenSize, screenSize));
			} // if

			if (Player.explosionTimeoutMs3 > 0 &&
				Player.explosionTimeoutMs3 < Player.MaxExplosionTimeoutMs)
			{
				int screenSize = Math.Max(BaseGame.Width * 2 / 3, BaseGame.Height * 2 / 3);
				float explosionTimePercent = 1.0f -
					(Player.explosionTimeoutMs3 / (float)Player.MaxExplosionTimeoutMs);
				explosionTexture.Select(
					(int)(explosionTexture.AnimationLength * explosionTimePercent));
				explosionTexture.RenderOnScreen(new Rectangle(
					BaseGame.Width / 2 - screenSize / 2 + BaseGame.Width / 9,
					BaseGame.Height / 2 - screenSize / 2 - BaseGame.Height / 7,
					screenSize, screenSize));
			} // if

			const float BorderColorStrength = 0.6f;

			// Show blue glow border in middle if speed item is active
			if (Player.speedItemTimeout > 0)
			{
				float alpha = BorderColorStrength;
				if (Player.speedItemTimeout < 1000)
					alpha *= Player.speedItemTimeout / 1000.0f;

				hudTexture.RenderOnScreen(
					BaseGame.ResolutionRect,
					ScreenBorderEffectRect,
					Level.SpeedItemColor, alpha);
			} // if
			else if (Player.itemMessageTimeoutMs >
				Player.MaxItemMessageTimeoutMs - 1000 &&
				Player.lastCollectedItem != Level.SpeedItemType &&
				Player.lastCollectedItem != Level.BombItemType)
			{
				float alpha = BorderColorStrength;
				float time = Player.itemMessageTimeoutMs -
					(Player.MaxItemMessageTimeoutMs - 1000);
				alpha *= time / 1000.0f;

				hudTexture.RenderOnScreen(
					BaseGame.ResolutionRect,
					ScreenBorderEffectRect,
					Player.lastCollectedItem == Level.FuelItemType ?
					Level.FuelItemColor :
					Player.lastCollectedItem == Level.HealthItemType ?
					Level.HealthItemColor :
					Level.ExtraLifeItemColor, alpha);
			} // if
			// Show red glow border in middle if bomb item is active
			else if (Player.numberOfBombItems > 0)
			{
				hudTexture.RenderOnScreen(
					BaseGame.ResolutionRect,
					ScreenBorderEffectRect,
					Level.BombItemColor,
					// Increase border strength the more bombs we have collected.
					Math.Min(1.0f, Player.numberOfBombItems * 0.3f));
			} // if
		} // ShowOnScreenEffects()
		#endregion

		#region Show minimap
		#region Rotation helpers
		/// <summary>
		/// Get camera rotation y coordinate
		/// </summary>
		/// <returns>Float</returns>
		private float GetCameraRotationY()
		{
			Vector3 cameraRotation = new Vector3(0, 1, 0);
			cameraRotation = Vector3.TransformNormal(cameraRotation,
				BaseGame.InverseViewMatrix);
			return (float)Math.PI / 2.0f +
				(float)Math.Atan2(cameraRotation.Y, cameraRotation.X);
		} // GetCameraRotationY()

		/// <summary>
		/// Get camera rotation z
		/// </summary>
		/// <returns>Float</returns>
		private float GetCameraRotationZ()
		{
			Vector3 cameraRotation = new Vector3(0, 0, 1);
			cameraRotation = Vector3.TransformNormal(cameraRotation,
				BaseGame.InverseViewMatrix);
			return (float)Math.PI -
				(float)Math.Atan2(cameraRotation.Z, cameraRotation.X);
		} // GetCameraRotationZ()

		/// <summary>
		/// Get rotated point. Position is rotated around center by rotation.
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <returns>Point</returns>
		private Point GetRotatedPoint(Point offset, Point pos, float rotation)
		{
			// See Texture.RenderOnScreenWithRotation for more help.
			float sinRotation = (float)Math.Sin(rotation);
			float cosRotation = (float)Math.Cos(rotation);
			return new Point(
				offset.X + (int)(-cosRotation * pos.X - sinRotation * pos.Y),
				offset.Y + (int)(-sinRotation * pos.X + cosRotation * pos.Y));
		} // GetRotatedPoint(pos, rotation)
		#endregion

		/// <summary>
		/// Show minimap
		/// </summary>
		private void ShowMinimap(Rectangle rect)
		{
			// Init helpers
			Vector3 camPos = BaseGame.CameraPos;

			// Use a little bigger height than given in the texture
			//int height = MiniMapBackgroundRect.Height * 6 / 5;
			Point pos = new Point(rect.X, rect.Y);
			int height = rect.Height;
			int width = BaseGame.XToRes(
				(int)(HudElementScaleFactor * MiniMapBackgroundRect.Width));
			Rectangle bgRect = new Rectangle(pos.X-1, pos.Y-1,
				width, height);

			// Render background
			inGameTexture.RenderOnScreen(bgRect, MiniMapBackgroundRect);

			// Show minimap on top of that
			Rectangle minimapRect = new Rectangle(
				bgRect.X + 3, bgRect.Y + 3,
				bgRect.Width - 8, height - 10);
			int minimapDisplayHeight = 60;// minimapRect.Height / 2;
			int minimapPos = levelTexture.Height -
				(int)(camPos.Z / GameAsteroidManager.SectorDepth) -
				minimapDisplayHeight * 3 / 4;
			if (minimapPos < 0)
				minimapPos = 0;
			if (minimapPos > levelTexture.Height -
				minimapDisplayHeight - 1)
				minimapPos = levelTexture.Height -
					minimapDisplayHeight - 1;

			// And render
			levelTexture.RenderOnScreen(minimapRect,
				new Rectangle(0, minimapPos, 40, minimapDisplayHeight));

			// Show rocket on top of map
			Point rocketPos = new Point(
				minimapRect.X + minimapRect.Width / 2 -
				(int)(4 * camPos.X / GameAsteroidManager.SectorWidth),
				minimapRect.Bottom -
				(int)(camPos.Z / GameAsteroidManager.SectorDepth) +
				((levelTexture.Height - minimapDisplayHeight) -
				minimapPos - MiniMapRocketRect.Height * 2 / 3));

			// Limit rocket pos to somewhere on the minimap
			if (rocketPos.X < minimapRect.X)
				rocketPos.X = minimapRect.X;
			if (rocketPos.Y < minimapRect.Y)
				rocketPos.Y = minimapRect.Y;
			if (rocketPos.X > minimapRect.Right)
				rocketPos.X = minimapRect.Right;
			if (rocketPos.Y > minimapRect.Bottom)
				rocketPos.Y = minimapRect.Bottom;

			// Get rotation for our rocket (inverted because every freaking
			// thing is inverted here from left handed to right handed).
			float rocketRotation =
				-GetCameraRotationZ() - MathHelper.PiOver2;

			// Render field of view
			hudTexture.RenderOnScreenWithRotation(
				rocketPos, BaseGame.XToRes(MiniMapFieldOfViewRect.Width / 2),
				rocketRotation,
				MiniMapFieldOfViewRect,
				new Vector2(MiniMapFieldOfViewRect.Width / 2,
				MiniMapFieldOfViewRect.Width / 2.0f));

			// And finally render
			inGameTexture.RenderOnScreenWithRotation(
				rocketPos, BaseGame.XToRes(MiniMapRocketRect.Width / 3),
				rocketRotation,
				MiniMapRocketRect,
				new Vector2(MiniMapRocketRect.Width / 2,
				MiniMapRocketRect.Width / 2.0f));
		} // ShowMinimap()
		#endregion

		#region Show hud controls
		float visionOccluded = 0.0f;

		/// <summary>
		/// Show hud controls
		/// </summary>
		private void ShowHudControls()
		{
			// Calculate hud areas
			int hudWidth = BaseGame.XToRes(
				(int)(HudRect.Width * HudBackgroundScaleFactor));
			int hudHeight = BaseGame.YToRes(
				(int)(HudRect.Height * HudBackgroundScaleFactor));
			int hudDistanceFromBorderX =
				OriginalHudDistanceFromBorder;
			int hudDistanceFromBorderY =
				OriginalHudDistanceFromBorder;
#if XBOX360
			hudDistanceFromBorderX += BaseGame.XToRes(32);
			hudDistanceFromBorderY += BaseGame.YToRes(18);
#endif
			Rectangle leftHud = new Rectangle(
				hudDistanceFromBorderX,
				BaseGame.Height - (hudHeight + hudDistanceFromBorderY),
				hudWidth, hudHeight);
			Rectangle rightHud = new Rectangle(
				BaseGame.Width - (hudWidth + hudDistanceFromBorderX),
				leftHud.Y,
				hudWidth, hudHeight);

			// First show hud background
			hudTexture.RenderOnScreen(leftHud, HudRect);

			// And mirror hud on the right side!
			hudTexture.RenderOnScreen(rightHud, new Rectangle(
				HudRect.Right, HudRect.Y, -HudRect.Width, HudRect.Height));

			// Fuel, health and speed
			int fuelPixels =
				(int)(FuelHealthAndSpeedWidth * (1.0f - Player.fuel));
			int fuelWidth =
				(int)(HudElementScaleFactor * (FuelBarRect.Width - fuelPixels));
			int fuelHeight = (int)(HudElementScaleFactor * FuelBarRect.Height);
			inGameTexture.RenderOnScreen(new Rectangle(
				leftHud.X + BaseGame.XToRes(14 + (int)(HudElementScaleFactor * fuelPixels)),
				leftHud.Y + BaseGame.YToRes(22),
				BaseGame.XToRes(fuelWidth),
				BaseGame.YToRes(fuelHeight)),
				new Rectangle(FuelBarRect.X + fuelPixels, FuelBarRect.Y,
				FuelBarRect.Width - fuelPixels, FuelBarRect.Height));

			int healthPixels =
				(int)(FuelHealthAndSpeedWidth * (1.0f - Player.health));
			int healthWidth =
				(int)(HudElementScaleFactor * (HealthBarRect.Width - healthPixels));
			int healthHeight = (int)(HudElementScaleFactor * HealthBarRect.Height);
			inGameTexture.RenderOnScreen(new Rectangle(
				leftHud.X + BaseGame.XToRes(14 + (int)(HudElementScaleFactor * healthPixels)),
				leftHud.Y + BaseGame.YToRes(22 + fuelHeight - 1),
				BaseGame.XToRes(healthWidth),
				BaseGame.YToRes(healthHeight)),
				new Rectangle(HealthBarRect.X + healthPixels, HealthBarRect.Y,
				HealthBarRect.Width - healthPixels, HealthBarRect.Height));

			int speedPixels =
				(int)(FuelHealthAndSpeedWidth * (1.0f - Player.Speed));
			int speedWidth =
				(int)(HudElementScaleFactor * (SpeedBarRect.Width - speedPixels));
			int speedHeight = (int)(HudElementScaleFactor * SpeedBarRect.Height);
			inGameTexture.RenderOnScreen(new Rectangle(
				leftHud.X + BaseGame.XToRes(14 + (int)(HudElementScaleFactor * speedPixels)),
				leftHud.Y + BaseGame.YToRes(22 + fuelHeight + healthHeight - 2),
				BaseGame.XToRes(speedWidth),
				BaseGame.YToRes(speedHeight)),
				new Rectangle(SpeedBarRect.X + speedPixels, SpeedBarRect.Y,
				SpeedBarRect.Width - speedPixels, SpeedBarRect.Height));

			// Show minimap on right hud
			Rectangle miniMapRect = new Rectangle(
				rightHud.X + BaseGame.XToRes(13),
				rightHud.Y + BaseGame.YToRes(22),
				rightHud.Width + BaseGame.XToRes(16),
				rightHud.Height - BaseGame.YToRes(28));
			ShowMinimap(miniMapRect);

			// Show number of lifes we have left
			for (int i = 0; i < Player.lifes; i++)
			{
				inGameTexture.RenderOnScreen(new Rectangle(
#if XBOX360
					(BaseGame.Width - (6 + BaseGame.XToRes(36))) -
					(2 + (RocketLifeRect.Width - 2) * (i + 1)),
					BaseGame.YToRes(28),
					BaseGame.XToRes(RocketLifeRect.Width),
					BaseGame.YToRes(RocketLifeRect.Height)),
#else
					(BaseGame.Width - 6) -
					(2 + (RocketLifeRect.Width - 2) * (i + 1)),
					2, RocketLifeRect.Width, RocketLifeRect.Height),
#endif
					RocketLifeRect);
			} // for

			// Show hud only if in game and alive.
			if (Player.GameOver == false)
			{
				// Show on screen altitude meter and balance
				float hudRotation = GetCameraRotationY();
				Point middle = new Point(
					BaseGame.Width / 2,
					BaseGame.Height / 2);
				int hudSize = BaseGame.Width / 8;
				int hudLineDistance = BaseGame.Width / 40;
				float hudAltitude =
					BaseGame.CameraPos.Y / GameAsteroidManager.SectorDepth;
				BaseGame.Device.RenderState.DepthBufferEnable = false;

				// Limit hud altitude to -20 up to +20
				if (hudAltitude > 20)
					hudAltitude = 20;
				if (hudAltitude < -20)
					hudAltitude = -20;

				// Render all hud lines
				for (int num = -25; num <= +25; num++)
				{
					int yPos = (int)((num + hudAltitude) * hudLineDistance);

					// Only show inner -5 to +5 hud lines
					if (yPos >= -5 * hudLineDistance &&
						yPos <= +5 * hudLineDistance)
					{
						// Use big line for center, medium for -5 and +5
						int innerXPos =
							num == 0 ? hudSize * 5 / 9 ://hudSize * 16 / 32 :
							num == 5 || num == -5 ? hudSize * 25 / 32 :
							num == 10 || num == -10 ? hudSize * 27 / 32 :
							num % 5 == 0 ? hudSize * 29 / 32 :
							hudSize * 30 / 32;

						// Change hud color to red if altitude is more than 5 units
						// away from center line.
						float awayFactor = 0.0f;
						if (Math.Abs(num) > 5)
						{
							awayFactor = (float)(Math.Abs(num) - 5) / 10.0f;
							if (awayFactor > 1.0f)
								awayFactor = 1.0f;
						} // if (Math.Abs)
						Color col = ColorHelper.InterpolateColor(
							HudColor, HudRedColor, awayFactor);

						// Left part
						BaseGame.DrawLineWithShadow(
							new Point(middle.X - hudSize, middle.Y + yPos),
							new Point(middle.X - innerXPos, middle.Y + yPos),
							col);
						// Right part
						BaseGame.DrawLineWithShadow(
							new Point(middle.X + hudSize, middle.Y + yPos),
							new Point(middle.X + innerXPos, middle.Y + yPos),
							col);

						// Show height at markers
						if (num % 5 == 0)
						{
							// Left text
							TextureFont.WriteSmallTextCentered(
								middle.X - hudSize - 15,
								middle.Y + yPos - TextureFont.Height / 3 - 2,
								(-num).ToString(),
								col, 0.75f);
							// Right text
							TextureFont.WriteSmallTextCentered(
								middle.X + hudSize + 10,
								middle.Y + yPos - TextureFont.Height / 3 - 2,
								(-num).ToString(),
								col, 0.75f);
						} // if (num)
					} // if (yPos)
				} // for (num)

				// Do collision test with occulusion culling like in lens flare class.
				float thisVisionOccluded =
					BaseGame.OcclusionIntensity(hudTexture, middle, 12);

				// Interpolate for a nicer transission effect.
				visionOccluded = thisVisionOccluded * 0.2f + visionOccluded * 0.8f;
				Color crosshairColor = ColorHelper.InterpolateColor(
					HudRedColor, HudColor, visionOccluded);

				// Render crosshair, rotate by camera rotation y
				BaseGame.DrawLineWithShadow(
					GetRotatedPoint(middle, new Point(-hudSize * 4 / 9, 0), hudRotation),
					GetRotatedPoint(middle, new Point(-hudSize / 12, 0), hudRotation),
					crosshairColor);
				BaseGame.DrawLineWithShadow(
					GetRotatedPoint(middle, new Point(+hudSize * 4 / 9, 0), hudRotation),
					GetRotatedPoint(middle, new Point(+hudSize / 12, 0), hudRotation),
					crosshairColor);
				BaseGame.DrawLineWithShadow(
					GetRotatedPoint(middle, new Point(0, -hudSize / 5), hudRotation),
					GetRotatedPoint(middle, new Point(0, -hudSize / 12), hudRotation),
					crosshairColor);
				BaseGame.DrawLineWithShadow(
					GetRotatedPoint(middle, new Point(0, +hudSize / 7), hudRotation),
					GetRotatedPoint(middle, new Point(0, +hudSize / 12), hudRotation),
					crosshairColor);
			} // if

			// Show level position, altitude, score+rank and game time.
			int mapWidth = BaseGame.XToRes(12+
				(int)(HudElementScaleFactor * MiniMapBackgroundRect.Width))+
				// Increase width for higher resolutions
				BaseGame.XToRes(18) - 18;
			int yMiddle =
				miniMapRect.Y + miniMapRect.Height / 2 + TextureFont.Height / 2;
			TextureFont.WriteText(
				rightHud.X + mapWidth, yMiddle - BaseGame.YToRes(35),
				// Min. 0, max. 100, get level position.
				"Pos: " +
				Math.Min(100, Math.Max(0, (int)(101 * BaseGame.CameraPos.Z /
				(levelTexture.Height * GameAsteroidManager.SectorDepth)))) +
				"%", GameTextColor);

			TextureFont.WriteText(
				rightHud.X + mapWidth, yMiddle,
				"Rank: "+
				//Score is now displayed on top center! Player.score.ToString(),// + " " +
				"#" + (Highscores.GetRankFromCurrentScore(Player.score) + 1),
				GameTextColor);

			TextureFont.WriteTextCentered(BaseGame.Width / 2,
#if XBOX360
				BaseGame.YToRes(34),
#else
				BaseGame.YToRes(4),
#endif
				"Score: "+Player.score.ToString());

			TextureFont.WriteText(
				rightHud.X + mapWidth, yMiddle + BaseGame.YToRes(35),
				"Time: "+
				((Player.gameTimeMs / 1000) / 60).ToString("0") + ":" +
				((Player.gameTimeMs / 1000) % 60).ToString("00"), GameTextColor);
		} // ShowHudControls()
		#endregion

		#region Show screen messages
		/// <summary>
		/// Show screen messages
		/// </summary>
		private void ShowScreenMessages(GameAsteroidManager asteroidManager)
		{
			Vector3 pos = BaseGame.CameraPos;
			Point messagePos = new Point(BaseGame.Width / 2, BaseGame.Height / 5);

			// If game is over, show end screen and wait until user clicks
			// or presses space.
			if (Player.GameOver)
			{
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Player.victory ? Texts.Victory : Texts.GameOver);
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y +
					TextureFont.Height * 4 / 3,
					Texts.YourHighscore + " " + Player.score + " " +
					"(#" + (Highscores.GetRankFromCurrentScore(Player.score) + 1) + ")");
			} // if

			// If we just lost a life, display message and let user continue
			// by pressing the left mouse button.
			else if (Player.explosionTimeoutMs >= 0)
			{
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Texts.YouLostALife);
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y +
					TextureFont.Height * 4 / 3,
					Texts.ClickToContinue);

				if (Input.MouseLeftButtonPressed ||
					Input.GamePadAPressed ||
					Input.GamePadBPressed ||
					Input.GamePadXPressed ||
					Input.GamePadYPressed ||
					Input.GamePadStartPressed)
					Player.explosionTimeoutMs = -1;
			} // if

			// Show countdown if a new life started
			else if (Player.lifeTimeMs < Player.LifeTimeZoomAndAccelerateMs)
			{
				float alpha = 1.0f - ((Player.lifeTimeMs % 1000) / 1000.0f);
				alpha *= 2.0f;
				if (alpha > 1.0f)
					alpha = 1.0f;

				// Show "Get ready" for 3 seconds with countdown,
				// then display "Go!".
				if (Player.lifeTimeMs < 3000)
				{
					TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
						Texts.GetReady);
					TextureFont.WriteTextCentered(messagePos.X, messagePos.Y +
						TextureFont.Height * 4 / 3,
						(3 - (int)(Player.lifeTimeMs / 1000)).ToString(),
						Color.White, alpha);
				} // if
				else
					TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
						Texts.Go, Color.White, alpha);
			} // if

			// Show item helper messages
			else if (Player.itemMessageTimeoutMs > 0 &&
				String.IsNullOrEmpty(Player.currentItemMessage) == false)
			{
				Player.itemMessageTimeoutMs -= BaseGame.ElapsedTimeThisFrameInMs;
				if (Player.itemMessageTimeoutMs < 0)
					Player.itemMessageTimeoutMs = 0;

				float alpha = 1.0f;
				if (Player.itemMessageTimeoutMs < 1000)
					alpha = Player.itemMessageTimeoutMs / 1000.0f;

				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Player.currentItemMessage, Color.LightGray, alpha);
			} // if

			// Warning if nearly dead (only if that just happened)
			else if (Player.showHealthWarningTimeoutMs > 0)
			{
				float alpha = 1.0f;
				if (Player.showHealthWarningTimeoutMs < 1000)
					alpha = Player.showHealthWarningTimeoutMs / 1000.0f;

				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Texts.WarningLowHealth, Color.Red, 1.0f);
			} // if

			// Warning if going out of fuel (less than 10%)
			else if (Player.fuel < 0.1f)
			{
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Texts.WarningOutOfFuel, Color.Red, 1.0f);
			} // if

			// Warning if out of playable area!
			else if (pos.X / GameAsteroidManager.SectorWidth <
				-asteroidManager.CurrentLevel.Width / 2 ||
				pos.X / GameAsteroidManager.SectorWidth >
				+asteroidManager.CurrentLevel.Width / 2 ||
				pos.Y / GameAsteroidManager.SectorHeight <
				-asteroidManager.CurrentLevel.Width / 2 ||
				pos.Y / GameAsteroidManager.SectorHeight >
				+asteroidManager.CurrentLevel.Width / 2 ||
				pos.Z < 0)
			{
				TextureFont.WriteTextCentered(messagePos.X, messagePos.Y,
					Texts.WarningOutOfLevel, Color.Red, 1.0f);
			} // if
		} // ShowScreenMessages()
		#endregion

		#region Run
		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(RocketCommanderGame game)
		{
			// Make sure the textures are linked correctly
			hudTexture = game.hudTexture;
			inGameTexture = game.inGameTexture;
			levelTexture = game.asteroidManager.CurrentLevel.Texture;

			// Enable alpha blending for all controls
			BaseGame.EnableAlphaBlending();

			// Zoom into rocket for countdown time
			ZoomIntoRocket(RocketCommanderGame.camera, game.rocketModel);

			// Show target in z direction, it is very far away.
			ShowTarget(game.asteroidManager.CurrentLevel.Length);

			// Show on screen effects like explosion or item screen border colors.
			ShowOnScreenEffects(game.explosionTexture);

			// Always show hit direction effect if it is active (similar to screen
			// effects always on top).
			game.asteroidManager.ShowHitDirectionEffect();

			// Show all hud controls
			// Don't display them in low resolution, we can't display any useful
			// information anyway.
			//obs, always display, is streched now: if (BaseGame.Width >= 640)
				ShowHudControls();

			// Handle game stuff
			Player.HandleGameLogic(game.asteroidManager);

			// Show on screen helper messages
			ShowScreenMessages(game.asteroidManager);

			// End game if escape was pressed or game is over and space or mouse
			// button was pressed.
			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed ||
				(Player.GameOver &&
				(Input.Keyboard.IsKeyDown(Keys.Space) ||
				Input.GamePadAPressed ||
				Input.GamePadBPressed ||
				Input.GamePadXPressed ||
				Input.GamePadXPressed ||
				Input.MouseLeftButtonPressed)))
			{
				// Upload new highscore (as we currently are in game,
				// no bonus or anything will be added, this score is low!)
				Player.SetGameOverAndUploadHighscore();

				// Stop rocket sound if it still playing
				Sound.StopRocketMotorSound();

				// Reset camera to origin and notify we are not longer in game mode
				RocketCommanderGame.camera.SetPosition(Vector3.Zero);
				RocketCommanderGame.camera.InGame = false;

				// Quit to the main menu
				quit = true;
			} // if
		} // Run(game)
		#endregion

		#region Unit testing
		public static void TestGameHud()
		{
			Mission missionDummy = null;
			GameAsteroidManager asteroidManager = null;

			TestGame.Start(
				"TestGameHud",
				delegate
				{
					Level firstLevel = Level.LoadAllLevels()[0];
					asteroidManager = new GameAsteroidManager(firstLevel);
					missionDummy = new Mission(firstLevel, asteroidManager);
					
					// Make sure the textures are linked correctly
					missionDummy.hudTexture = new Texture("Hud");
					missionDummy.inGameTexture = new Texture("InGame");
					missionDummy.levelTexture = asteroidManager.CurrentLevel.Texture;
				},
				delegate
				{
					// Render sky cube map as our background.
					BaseGame.skyCube.RenderSky(1.0f, BaseGame.SkyBackgroundColor);

					// We just want to display the hud
					missionDummy.ShowHudControls();

					// Show on screen helper messages
					missionDummy.ShowScreenMessages(asteroidManager);

					/*tst
					//missionDummy.hudTexture.RenderOnScreen(new Point(100, 100));
					//missionDummy.inGameTexture.RenderOnScreen(new Point(400, 100));
					Point rocketPos = new Point(100, 100);
					float rocketRotation = Input.MousePos.X / 500.0f;
					missionDummy.hudTexture.RenderOnScreenWithRotation(
						rocketPos,
						MiniMapFieldOfViewRect.Width / 2, rocketRotation,
						MiniMapFieldOfViewRect,
						new Vector2(MiniMapFieldOfViewRect.Width / 2,
						MiniMapFieldOfViewRect.Width / 2.0f));
					missionDummy.inGameTexture.RenderOnScreenWithRotation(
						rocketPos, MiniMapRocketRect.Width / 3,
						rocketRotation,
						MiniMapRocketRect,
						new Vector2(MiniMapRocketRect.Width / 2,
						MiniMapRocketRect.Width / 2.0f));
					//missionDummy.inGameTexture.RenderOnScreen(
					//	new Rectangle(100, 100, MiniMapRocketRect.Width, MiniMapRocketRect.Height),
					//	MiniMapRocketRect);
					//*/
				});
		} // TestGameHud()
		#endregion
	} // class Mission
} // namespace RocketCommanderXna.GameScreens
