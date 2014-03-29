// Project: Rocket Commander, File: GameAsteroidManager.cs
// Namespace: RocketCommanderXna.Game, Class: GameAsteroidManager
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 4019, Size of file: 148423 Bytes
// Creation date: 07.11.2005 19:38
// Last modified: 27.02.2006 19:59
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Shaders;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = RocketCommanderXna.Graphics.Texture;
using Model = RocketCommanderXna.Graphics.Model;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Game asteroid manager, derived from BaseAsteroidManager over the
	/// PhysicsAsteroidManager.
	/// 
	/// The BaseAsteroidManager handles all the base variables we need for our
	/// asteroid field. Also generates sectors and handles smaller asteroids.
	/// Provides some basic render methods to show the asteroid field.
	/// This class is nothing we can use for the game, it just provides us
	/// the very basic features for the asteroid field.
	/// <para/>
	/// Derived from this is the PhysicsAsteroidManager, which adds all Physics
	/// (handle player collision, handle sector Physics, etc.).
	/// <para/>
	/// And derived from that we finally get to the GameAsteroidManager,
	/// which calculates sector movement, does visibility checks, etc.
	/// The GameAsteroidManager is the class we use in the game, it provides
	/// a much richer Render method and handles also additional effects like
	/// the Hit effects we see on screen.
	/// </summary>
	public class GameAsteroidManager : PhysicsAsteroidManager, IDisposable
	{
		#region Constants
		/// <summary>
		/// All item colors in an array, used for glow effects in Render(..)
		/// </summary>
		static readonly Color[] ItemColors =
			new Color[Level.NumOfItemTypes]
			{
				Level.FuelItemColor,
				Level.HealthItemColor,
				Level.ExtraLifeItemColor,
				Level.SpeedItemColor,
				Level.BombItemColor,
			};

		/// <summary>
		/// Filenames for the item models.
		/// </summary>
		public static readonly string[] ItemModelFilenames =
			new string[Level.NumOfItemTypes]
			{
				"Fuel",
				"Health",
				"ExtraLife",
				"Speed",
				"Bomb",
			};

		/// <summary>
		/// Size for items.
		/// </summary>
		public const float ItemSize = 75.0f;//33.33f;
		#endregion

		#region Variables
		/// <summary>
		/// Current items in our current level,
		/// rendered at the end of the Render method.
		/// Will be copied over from level.items when calling SetLevel!
		/// </summary>
		List<Vector3>[] items = new List<Vector3>[]
			{
				// Fuel
				new List<Vector3>(),
				// Health
				new List<Vector3>(),
				// ExtraLife
				new List<Vector3>(),
				// Speed
				new List<Vector3>(),
				// Bomb
				new List<Vector3>(),
			};

		/// <summary>
		/// Item models for all 5 item types: Fuel, Health, ExtraLife, Speed, Bomb
		/// </summary>
		private AnimatedModel[] itemModels =
			new AnimatedModel[Level.NumOfItemTypes];

		/// <summary>
		/// Helper texture to show direction where we hit some asteroid.
		/// This greatly helps the player to understand why he loses hitpoints
		/// and that he loses hitpoints at all (together with the sound effect).
		/// </summary>
		private Texture hitDirectionTexture = null;

		/// <summary>
		/// Goal model, added because we can't just generate and draw a mesh.
		/// </summary>
		private Model goalModel = null;
		#endregion

		#region Constructor
		/// <summary>
		/// Create asteroid manager
		/// </summary>
		public GameAsteroidManager(Level setLevel)
			: base(setLevel)
		{
			// Load all items
			for (int num = 0; num < Level.NumOfItemTypes; num++)
			{
				// All items are animated, load with help of AnimatedModel.
				itemModels[num] = new AnimatedModel(ItemModelFilenames[num]);
			} // for (num)

			// Load hit direction texture
			hitDirectionTexture = new Texture("HitDirection.dds");

			// Load goal model
			goalModel = new Model("Goal");
		} // AsteroidManager()

		/// <summary>
		/// Current level
		/// </summary>
		/// <returns>Level</returns>
		public override Level CurrentLevel
		{
			get
			{
				return level;
			} // get
			set
			{
				base.CurrentLevel = value;

				// Copy over all items
				for (int num = 0; num < Level.NumOfItemTypes; num++)
				{
					items[num] = new List<Vector3>(level.items[num]);
				} // for (num)
			} // set
		} // CurrentLevel
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (hitDirectionTexture != null)
				hitDirectionTexture.Dispose();
		} // Dispose()
		#endregion

		#region Add distance to be displayed
		/// <summary>
		/// Helper list to hold all screen positions and distances we want
		/// to display later on (after all the 3d rendering).
		/// </summary>
		static List<PosAndDistance> remToDisplayDistance =
			new List<PosAndDistance>();

		/// <summary>
		/// Position and distance
		/// </summary>
		class PosAndDistance
		{
			/// <summary>
			/// Screen position
			/// </summary>
			public Point pos;
			/// <summary>
			/// Distance
			/// </summary>
			public float distance;
			/// <summary>
			/// Alpha
			/// </summary>
			public float alpha;

			/// <summary>
			/// Create position and distance
			/// </summary>
			/// <param name="setPos">Set position</param>
			/// <param name="setDistance">Set distance</param>
			/// <param name="setAlpha">Set alpha</param>
			public PosAndDistance(
				Point setPos, float setDistance, float setAlpha)
			{
				pos = setPos;
				distance = setDistance;
				alpha = setAlpha;
			} // PosAndDistance(setPos, setDistance, setAlpha)
		} // class PosAndDistance

		/// <summary>
		/// Add distance to be displayed
		/// </summary>
		/// <param name="screenPos">Screen position</param>
		/// <param name="distance">Distance</param>
		public static void AddDistanceToBeDisplayed(
			Point screenPos, float distance, float alpha)//,
			//ProductLogo productLogo)
		{
			// Max. display 5 distances, else the screen will get very confusing!
			if (remToDisplayDistance.Count < 5)//10)
				remToDisplayDistance.Add(new PosAndDistance(
					screenPos, distance, alpha));
			else
			{
				// If we have more distances, check if we can replace the farest
				float farestDistance = float.MinValue;
				PosAndDistance remFarestItem = null;
				foreach (PosAndDistance pad in remToDisplayDistance)
					if (pad.distance > farestDistance)
					{
						farestDistance = pad.distance;
						remFarestItem = pad;
					} // if

				// Is this better than farest distance?
				if (distance < farestDistance)
				{
					// Then replace
					remFarestItem.pos = screenPos;
					remFarestItem.distance = distance;
					remFarestItem.alpha = alpha;
					//remFarestItem.productLogo = productLogo;
				} // if
			} // else
		} // AddDistanceToBeDisplayed(screenPos, distance, alpha)

		/// <summary>
		/// Show all distances
		/// </summary>
		public void ShowAllDistances()//TextureFont TextureFont)
		{
			foreach (PosAndDistance posDistance in remToDisplayDistance)
			{
				TextureFont.WriteSmallTextCentered(
					posDistance.pos.X, posDistance.pos.Y + 40,
					// Only display full numbers (round them up)
					Texts.Distance + ": " +
					(int)Math.Round(posDistance.distance / SectorDepth - 0.26f),
					// Only display with half the alpha!
					posDistance.alpha * 0.5f);
			} // foreach  (posDistance)

			// Clear
			remToDisplayDistance.Clear();
		} // ShowAllDistances()
		#endregion

		#region Show hit direction effect
		const int MaxHitDirectionTimeoutMs = 2000;
		int hitDirectionEffectTimeoutMs = 0;
		float hitDirection = 0.0f;
		/// <summary>
		/// Set hit direction effect
		/// </summary>
		/// <param name="setDirection">Direction</param>
		protected override void SetHitDirectionEffect(float setDirection)
		{
			hitDirectionEffectTimeoutMs = MaxHitDirectionTimeoutMs;
			hitDirection = setDirection;
		} // SetHitDirectionEffect(direction)

		/// <summary>
		/// Show hit direction effect
		/// </summary>
		public void ShowHitDirectionEffect()
		{
			if (hitDirectionEffectTimeoutMs > 0)
			{
				hitDirectionEffectTimeoutMs -= (int)BaseGame.ElapsedTimeThisFrameInMs;

				hitDirectionTexture.RenderOnScreenWithRotation(
					new Point(BaseGame.Width / 2,
					BaseGame.Height / 2),
					BaseGame.Height / 2,
					(float)Math.PI + hitDirection,
					hitDirectionTexture.GfxRectangle,//0, 0, 1, 1,
					ColorHelper.ApplyAlphaToColor(
					Color.White,
					// Use 1.0f for first second, then fade out.
					Math.Min(1.0f,
					(hitDirectionEffectTimeoutMs /
					((float)MaxHitDirectionTimeoutMs / 2.0f)))));
			} // if
		} // ShowHitDirectionEffect()
		#endregion

		#region Show all items helper method
		/// <summary>
		/// Show all items
		/// </summary>
		/// <param name="useShader">Use shader</param>
		protected override void ShowAllItems(//ParallaxShader shader,
			ref Color objectColor, ref float lastUpdatedAlpha,
			ref Vector3 cameraPos)
		{
			// Show all items
			for (int num = 0; num < Level.NumOfItemTypes; num++)
			{
				// Animate item type (Note: Not really supported on XNA yet)
				itemModels[num].Animate();

				// Go through all items of this type
				for (int itemNum = 0; itemNum < items[num].Count; itemNum++)
				{
					Vector3 objPos = items[num][itemNum];

					// Get distance to viewer
					float distance = (objPos - cameraPos).Length();

					// Skip if out of visible range
					// Use trick to display even items 6 times out of visible area!
					if (distance > MaxViewDepth * 6)
						continue;

					// If very close to item, collect it and play sound
					// Use bigger collect distance on xbox360 than on pc!
					if (distance <= ItemSize * 2.15f)// 1.45f)
					{
						Player.HandleItem(num);
						// Remove item!
						items[num].RemoveAt(itemNum--);
						continue;
					} // if (distance)

					// Update world matrix
					float size = ItemSize;

					// Scale down if out of visible area!
					if (distance > MaxViewDepth)
					{
						size = size / (distance / MaxViewDepth);
						// And reposition to visible distance!
						Vector3 distVector = (objPos - cameraPos);
						distVector.Normalize();
						objPos = cameraPos + distVector * (MaxViewDepth - 25.0f);
					} // if (distance)

					// Apply to matrix
					Matrix matrix = Matrix.CreateScale(size, size, size);

					// And set translation (faster than using Matrix.Translate!)
					matrix.M41 = objPos.X;
					matrix.M42 = objPos.Y;
					matrix.M43 = objPos.Z;

					// Note: Alpha fadeout at great distances is not required because
					// we can see much farther now and we don't really see the items!

					// And finally render.
					itemModels[num].Render(matrix);//.RenderWithShader(matrix, shader);
				} // for (itemNum)
			} // for (num)
		} // ShowAllItems(useShader)
		#endregion

		#region Render
		/// <summary>
		/// Render, most performance critical method of the game.
		/// Renders all asteroids and handles all physics, updating, etc.
		/// We use instancing to speed up asteroid rendering!
		/// </summary>
		public void Render(Texture inGameTexture, Texture lightEffectTexture)
		{
			#region Initialize
			// Use alpha blending for blending out asteroids
			BaseGame.EnableAlphaBlending();

			// Get camera position, no need to call the property 1 mio times.
			Vector3 cameraPos = BaseGame.CameraPos;

			// Empty display distance list
			remToDisplayDistance.Clear();
			#endregion

			#region Show target behind asteroids
			// Show target icon behind all asteroids and items (only in game)
			if (inGameTexture != null)
			{
				// First find out where the target is.
				Point screenPos = BaseGame.Convert3DPointTo2D(TargetPosition);
				bool isVisible = BaseGame.IsInFrontOfCamera(TargetPosition);

				if (isVisible &&
					screenPos.X >= 0 && screenPos.X < BaseGame.Width &&
					screenPos.Y >= 0 && screenPos.Y < BaseGame.Height)
				{
					// From Mission.cs:70.
					Rectangle TargetIconRect = new Rectangle(106, 49, 61, 61);

					// Render target icon centered at screenPos
					// Note: This will be blurred because we render it before
					// the glow/bloom/motion blur shader is applied, but if we
					// render after that we can't render behind the asteroids.
					// See Mission.RenderTarget!
					inGameTexture.RenderOnScreen(new Rectangle(
						screenPos.X - TargetIconRect.Width / 2,
						screenPos.Y - TargetIconRect.Height / 2,
						TargetIconRect.Width, TargetIconRect.Height),
						TargetIconRect, Color.White, SpriteBlendMode.Additive);
				} // if (isVisible)
			} // if (inGameTexture)
			#endregion

			#region Show glow behind items
			// Show glow behind all items
			for (int num = 0; num < Level.NumOfItemTypes; num++)
			{
				// Go through all items of this type
				foreach (Vector3 pos in items[num])
				{
					// Get distance to viewer
					float distance = (pos - cameraPos).Length();

					// Skip if out of visible range * 6
					if (distance > MaxViewDepth * 6 ||
						BaseGame.IsInFrontOfCamera(pos) == false)
						continue;

					// Convert to screen coordinates
					Point screenPos = BaseGame.Convert3DPointTo2D(pos);
					int glowSize = 36 * BaseGame.Width / 1024;

					// If not visible, skip!
					if (screenPos.X < -glowSize ||
						screenPos.Y < -glowSize ||
						screenPos.X > BaseGame.Width + glowSize ||
						screenPos.Y > BaseGame.Height + glowSize)
						continue;

					// Calculate alpha
					float alpha = 1.0f;
					if (distance > MaxViewDepth * 4)
						alpha = 1.0f -
							((distance - MaxViewDepth * 4) / (MaxViewDepth * (6 - 4)));

					// Show glow with help of light effect
					if (lightEffectTexture != null)
						lightEffectTexture.RenderOnScreen(
							new Rectangle(screenPos.X - glowSize, screenPos.Y - glowSize,
							glowSize * 2, glowSize * 2),
							lightEffectTexture.GfxRectangle,
							ColorHelper.ApplyAlphaToColor(ItemColors[num], alpha * 0.5f),
							SpriteBlendMode.Additive);

					// And display distance to item below it (not here, see below).
					float textAlpha = alpha * 1.5f *
						(1.0f - (distance / (MaxViewDepth * 6)));
					AddDistanceToBeDisplayed(screenPos, distance,
						textAlpha < 1.0f ? textAlpha : 1.0f);
				} // foreach (pos)
			} // for (num)

			// Flush all glow sprites on the screen (before rendering asteroids!)
			SpriteHelper.DrawSprites();
			#endregion

			#region Render 3d models, especially the asteroids
			// Draw goal at target position
			goalModel.Render(Matrix.CreateScale(100) *
				Matrix.CreateRotationX(-(float)Math.PI / 2.0f) *
				Matrix.CreateTranslation(TargetPosition));

			// Call base render method, target and item glow is behind of the
			// asteroids.
			base.RenderAsteroids();
			#endregion
		} // Render()
		#endregion
	} // class GameAsteroidManager
} // namespace RocketCommanderXna.Game
