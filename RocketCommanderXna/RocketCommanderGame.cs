// Project: RocketCommanderXna, File: RocketCommanderGame.cs
// Namespace: RocketCommanderXna, Class: RocketCommanderGame
// Path: C:\code\XnaBook\RocketCommanderXna, Author: abi
// Code lines: 525, Size of file: 15,16 KB
// Creation date: 07.12.2006 18:22
// Last modified: 07.12.2006 22:27
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Game;
using RocketCommanderXna.GameScreens;
using System;
using System.Collections.Generic;
using Model = RocketCommanderXna.Graphics.Model;
using Texture = RocketCommanderXna.Graphics.Texture;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Sounds;
using RocketCommanderXna.Helpers;
using System.Diagnostics;
using System.Threading;
#endregion

namespace RocketCommanderXna
{
	#region Button types
	/// <summary>
	/// Menu button types
	/// </summary>
	public enum MenuButton
	{
		Missions,
		Highscore,
		Credits,
		// Extra buttons
		Help,
		Options,
		// Exit, put at bottom of all main menu buttons
		Exit,
		// Back button, not used in main menu
		Back,
	} // enum MenuButtons

	/// <summary>
	/// Bottom link buttons
	/// </summary>
	public enum BottomLinkButton
	{
		ExDream,
		RocketCommander,
		Microsoft,
	} // enum BottomLinkButton
	#endregion

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	/// <returns>Base game</returns>
	public class RocketCommanderGame : BaseGame
	{
		#region Variables
		/// <summary>
		/// Game screens stack. We can easily add and remove game screens
		/// and they follow the game logic automatically. Very cool.
		/// </summary>
		private Stack<IGameScreen> gameScreens = new Stack<IGameScreen>();

		/// <summary>
		/// All available levels are preloaded and set to the asteroidManager
		/// when player wants to play them.
		/// </summary>
		public Level[] levels = null;

		/// <summary>
		/// Asteroid manager, always used. Even in the main menu for the
		/// background.
		/// </summary>
		public GameAsteroidManager asteroidManager = null;

		/// <summary>
		/// Rocket model, only used to zoom in at the beginning of each game
		/// and life in the game.
		/// </summary>
		public Model rocketModel = null;

		/// <summary>
		/// Load the main menu and mouse textures at game start, this way
		/// we have access in all game screens and don't ever have to reload
		/// any of these textures.
		/// Update 2005-12-24: Added helper button texture and help screen texture!
		/// </summary>
		public Texture mainMenuTexture = null,
			helperTexture = null,
			helpScreenTexture = null,
			mouseCursorTexture = null;

		/// <summary>
		/// Also preload the in game textures for all in game controls and stuff.
		/// Note: These Hud textures are saved in png format to save disk space.
		/// Light effect texture is used to show items from a far distance.
		/// </summary>
		public Texture hudTexture = null,
			inGameTexture = null,
			lightEffectTexture = null;

		/// <summary>
		/// Explosion texture, displayed when rocket is exploding.
		/// </summary>
		public AnimatedTexture explosionTexture = null;
		#endregion

		#region Properties
		#region Version
		/// <summary>
		/// Version info for the title.
		/// </summary>
		/// <returns>String</returns>
		public static string VersionInfo
		{
			get
			{
				return "v1.0";
				/*old
				string[] splittedVersion =
					Application.ProductVersion.Split(new char[] { '.' });
				return "v" + splittedVersion[0] + "." + splittedVersion[1];
				*/
			} // get
		} // VersionInfo
		#endregion
		#endregion

		#region Constructor
		/// <summary>
		/// Create RocketCommanderXnaGame
		/// </summary>
		public RocketCommanderGame()
		{
			/*obs
			textureBatch = new SpriteBatch(this.graphics.GraphicsDevice);
			//textureBatch.Disposing += new EventHandler(textureBatch_Disposing);
			backgroundTexture = Texture2D.FromFile(
				this.graphics.GraphicsDevice, "Textures\\Goal.png");
			 */

			// Disable mouse, we use our own mouse texture in the menu
			// and don't use any mouse cursor in the game anyway.
			this.IsMouseVisible = false;

			// Don't limit the framerate to the vertical retrace
			graphics.SynchronizeWithVerticalRetrace = false;
			this.IsFixedTimeStep = false;
		} // RocketCommanderXnaGame()

		/// <summary>
		/// Initialize textures and models for the game.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			// Load all available levels
			levels = Level.LoadAllLevels();

			// Initialize asteroidManager and use last avialable level.
			asteroidManager = new GameAsteroidManager(levels[levels.Length - 1]);
			rocketModel = new Model("Rocket");

			// Load menu textures
			mainMenuTexture = new Texture("MainMenu.png");
			helperTexture = new Texture("ExtraButtons.png");
			helpScreenTexture = new Texture("HelpScreen.png");
			mouseCursorTexture = new Texture("MouseCursor.dds");

			hudTexture = new Texture("Hud.png");
			inGameTexture = new Texture("InGame.png");

			lightEffectTexture = new Texture("LightEffect.dds");

			explosionTexture = new AnimatedTexture("Explosion");

			// Create main menu screen
			gameScreens.Push(new MainMenu());

			//tst:
			//gameScreens.Push(new Mission(levels[0], asteroidManager));
			//inGame = gameScreens.Peek().GetType() == typeof(Mission);
			//camera.InGame = inGame;
		} // Initialize()
		#endregion

		#region Toggle music on/off
		/// <summary>
		/// Toggle music on off
		/// </summary>
		public void ToggleMusicOnOff()
		{
			if (GameSettings.Default.MusicOn)
				Sound.StopMusic();
			else
				Sound.StartMusic();
		} // ToggleMusicOnOff()
		#endregion

		#region Add game screen
		static bool inGame = false;
		/// <summary>
		/// In game
		/// </summary>
		/// <returns>Bool</returns>
		public static bool InGame
		{
			get
			{
				return inGame;
			} // get
		} // InGame

		/// <summary>
		/// Add game screen, which will be used until we quit it or add
		/// another game screen on top of it.
		/// </summary>
		/// <param name="newGameScreen">New game screen</param>
		public void AddGameScreen(IGameScreen newGameScreen)
		{
			gameScreens.Push(newGameScreen);

			inGame = newGameScreen.GetType() == typeof(Mission);
			camera.InGame = inGame;
			Sound.CurrentMusicMode = inGame;
		} // AddGameScreen(newGameScreen)
		#endregion

		#region Remove current game screen
		/// <summary>
		/// Remove current game screen
		/// </summary>
		public void RemoveCurrentGameScreen()
		{
			gameScreens.Pop();

			bool inGame = gameScreens.Count > 0 &&
				gameScreens.Peek().GetType() == typeof(Mission);
			camera.InGame = inGame;
			Sound.CurrentMusicMode = inGame;
		} // RemoveCurrentGameScreen()
		#endregion

		#region Render background
		/// <summary>
		/// Render menu background
		/// </summary>
		public void RenderMenuBackground()
		{
			// Make sure alpha blending is enabled.
			BaseGame.EnableAlphaBlending();

			Rectangle upperRect = new Rectangle(
				BaseGame.ResolutionRect.X,
				BaseGame.ResolutionRect.Y,
				BaseGame.ResolutionRect.Width,
				BaseGame.ResolutionRect.Height -
				(int)Math.Round(70 * BaseGame.Height / 768.0f));
			mainMenuTexture.RenderOnScreen(
				upperRect,
				// Skip last 70 pixels, done when rendering bottom link buttons
				new Rectangle(0, 0, 1024, 768 - 70));

			// Show all bottom link buttons (they are part of the background)
			if (RenderBottomLinkButton(BottomLinkButton.ExDream))
			{
#if !XBOX360
				Process.Start("http://www.exDream.com");
				// Give some time to os, won't help much, but anyway ...
				Thread.Sleep(100);
#endif
			} // if
			if (RenderBottomLinkButton(BottomLinkButton.RocketCommander))
			{
#if !XBOX360
				Process.Start("http://www.RocketCommander.com");
				Thread.Sleep(100);
#endif
			} // if
			if (RenderBottomLinkButton(BottomLinkButton.Microsoft))
			{
#if !XBOX360
				Process.Start("http://www.Microsoft.com");
				Thread.Sleep(100);
#endif
			} // if
		} // RenderMenuBackground()
		#endregion

		#region Render button
		/// <summary>
		/// Render button
		/// </summary>
		/// <param name="buttonType">Button type</param>
		/// <param name="rect">Rectangle</param>
		public bool RenderMenuButton(
			MenuButton buttonType,
			//bool highlight,
			Point pos)
		{
			// Calc screen rect for rendering (recalculate relative screen position
			// from 1024x768 to actual screen resolution, just in case ^^).
			Rectangle rect = new Rectangle(
				pos.X * BaseGame.Width / 1024,
				pos.Y * BaseGame.Height / 768,
				200 * BaseGame.Width / 1024,
				77 * BaseGame.Height / 768);

			// Is button highlighted?
			bool highlight = Input.MouseInBox(rect);

			// Was not highlighted last frame?
			if (highlight &&
				Input.MouseWasNotInRectLastFrame(rect))
				Sound.Play(Sound.Sounds.Highlight);

			// See MainMenu.dds for pixel locations
			bool useHelperTexture =
				buttonType == MenuButton.Help ||
				buttonType == MenuButton.Options;
			int buttonNum = (int)buttonType;

			// Correct last 2 button numbers (exit and back)
			if (buttonNum >= (int)MenuButton.Exit)
				buttonNum -= 2;

			Rectangle pixelRect = useHelperTexture ?
				new Rectangle(0 + 204 * ((int)buttonType - (int)MenuButton.Help),
				0 + 80 * (highlight ? 1 : 0), 200, 77) :
				new Rectangle(3 + 204 * buttonNum,
				840 + 80 * (highlight ? 1 : 0), 200, 77);

			// Render
			(useHelperTexture ? helperTexture : mainMenuTexture).RenderOnScreen(
				rect, pixelRect);

			// Play click sound if button was just clicked
			bool ret =
				(Input.MouseLeftButtonJustPressed ||
				Input.GamePadAJustPressed) &&
				this.IsActive &&
				highlight;

			if (buttonType == MenuButton.Back &&
				(Input.GamePadBackJustPressed ||
				Input.GamePadBJustPressed ||
				Input.KeyboardEscapeJustPressed))
				ret = true;
			if (buttonType == MenuButton.Missions &&
				Input.GamePadStartPressed)
				ret = true;

			if (ret == true)
				Sound.Play(Sound.Sounds.Click);

			// Return true if button was pressed, false otherwise
			return ret;
		} // RenderButton(buttonType, rect)
		#endregion

		#region Render bottom link buttons
		/// <summary>
		/// Render bottom link button
		/// </summary>
		/// <param name="buttonType">Button type</param>
		/// <returns>Bool</returns>
		public bool RenderBottomLinkButton(BottomLinkButton buttonType)
		{
			// Calc screen rect for rendering
			Rectangle pixelRect =
				buttonType == BottomLinkButton.ExDream ?
				new Rectangle(0, 698, 195 + 60, 70) :
				buttonType == BottomLinkButton.RocketCommander ?
				new Rectangle(195 + 60, 698, 264 + 392 - (195 + 60), 70) :
				new Rectangle(656, 698, 1024 - 656, 70);
			Rectangle rect = new Rectangle(
				(int)Math.Round(pixelRect.X * BaseGame.Width / 1024.0f),
				(int)Math.Round(pixelRect.Y * BaseGame.Height / 768.0f),
				(int)Math.Round(pixelRect.Width * BaseGame.Width / 1024.0f),
				(int)Math.Round(pixelRect.Height * BaseGame.Height / 768.0f));
			rect.Height = BaseGame.Height - rect.Y + 1;

			// Is button highlighted?
			// Update, don't highlight if out of screen.
			bool highlight = Input.MouseInBox(new Rectangle(
				rect.X + 10, rect.Y + 2, rect.Width - 15, rect.Height - 14));

			// Was not highlighted last frame?
			if (highlight &&
				Input.MouseWasNotInRectLastFrame(rect))
				Sound.Play(Sound.Sounds.Highlight);

			// Render
			if (highlight)
				pixelRect.Y += 70;
			mainMenuTexture.RenderOnScreen(rect, pixelRect);

			// Play click sound if button was just clicked
			bool ret = Input.MouseLeftButtonJustPressed &&
				this.IsActive &&
				highlight;

			if (ret == true)
				Sound.Play(Sound.Sounds.Click);

			// Return true if button was pressed, false otherwise
			return ret;
		} // RenderBottomLinkButton(buttonType)
		#endregion

		#region Render background
		/// <summary>
		/// Render mouse cursor
		/// </summary>
		public void RenderMouseCursor()
		{
#if !XBOX360
			// We got 4 animation steps, rotate them by the current time
			int mouseAnimationStep = (int)(BaseGame.TotalTimeMs / 100) % 4;

			// And render mouse on screen.
			mouseCursorTexture.RenderOnScreen(
				new Rectangle(Input.MousePos.X, Input.MousePos.Y, 60, 60),
				new Rectangle(64 * mouseAnimationStep, 0, 60, 60));

			// Draw all sprites (just the mouse cursor)
			SpriteHelper.DrawSprites(width, height);
#endif
		} // RenderMouseCursor()
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		protected override void Update(GameTime gameTime)
		{
			// If that game screen should be quitted, remove it from stack!
			if (gameScreens.Count > 0 &&
				gameScreens.Peek().Quit)
				RemoveCurrentGameScreen();

			// If no more game screens are left, it is time to quit!
			if (gameScreens.Count == 0)
			{
				/*obs ||
				device == null ||
				device.Disposed)
				quit = true;
				*/
#if DEBUG
				// Don't exit if this is just a unit test
				if (this.GetType() != typeof(TestGame))
#endif
				Exit();
			} // if

			base.Update(gameTime);
		} // Update(gameTime)
		#endregion

		#region Draw
		//Stopwatch perfCounterAsteroidManager = new Stopwatch();
		//long asteroidManagerMs = 0, asteroidManagerTicks = 0;

		//obs: Point lastFrameMousePos = new Point(0, 0);
		//public bool wasMouseButtonPressedLastFrame = false;
		/// <summary>
		/// Render, only method used here, called from Update each frame.
		/// We don't need to implement any more methods in this class.
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			// Kill background (including z buffer, which is important for 3D)
			ClearBackground();

			// Start post screen glow shader, will be shown in BaseGame.Draw
			BaseGame.GlowShader.Start();

			// Render sky cube map as our background.
			BaseGame.skyCube.RenderSky(1.0f, BaseGame.remSkyBackgroundColor);

			// Make sure z buffer is on
			BaseGame.Device.RenderState.DepthBufferEnable = true;

			// Render rocket in front of view in menu mode
			if (camera.InGame == false)
			{
				Vector3 inFrontOfCameraPos =
					new Vector3(0, -1.33f, -2.5f);
				inFrontOfCameraPos = Vector3.Transform(
					inFrontOfCameraPos, InverseViewMatrix);
				rocketModel.Render(
					Matrix.CreateRotationX(-(float)Math.PI / 2.2f) *
					Matrix.CreateRotationZ(BaseGame.TotalTimeMs / 8400.0f) *
					Matrix.Invert(camera.RotationMatrix) *
					Matrix.CreateTranslation(inFrontOfCameraPos));
			} // if
			else if (Player.GameOver == false)
			{
				// Zoom in when starting or a new life was started.
				if (Player.explosionTimeoutMs < 0 &&
				Player.lifeTimeMs < 3000)
				{
					// Slowly zoom into rocket for the first 3 seconds
					Vector3 inFrontOfCameraPos =
						new Vector3(0, -1.5f, -3.0f) * 2.5f;
					inFrontOfCameraPos = Vector3.TransformNormal(
						inFrontOfCameraPos, BaseGame.InverseViewMatrix);
					Matrix startMatrix =
						Matrix.CreateRotationX(-(float)Math.PI / 2.0f) *
						Matrix.CreateRotationZ((float)Math.PI / 4.0f) *
						Matrix.CreateTranslation(new Vector3(0, -0.485f, +0.4f)) *
						Matrix.Invert(camera.RotationMatrix) *
						Matrix.CreateTranslation(inFrontOfCameraPos *
						(1.0f - (Player.lifeTimeMs / 3000.0f))) *
						Matrix.CreateTranslation(BaseGame.CameraPos);
					rocketModel.Render(startMatrix);
				} // if
			} // if

			//*			
			try
			{
				// Always render asteroidManager first!
				asteroidManager.Render(
					// Use the inGameTexture for the hud display.
					camera.InGame ? inGameTexture : null,
					// And the lightEffectTexture for all item glows
					lightEffectTexture);
			} // try
			catch (Exception ex)
			{
				Log.Write("Fatal error, rendering of asteroid field failed: " +
					ex.ToString());
			} // catch

			// Render lens flare on top of 3d stuff
			lensFlare.Render(remLensFlareColor);

			// Add scene glow on top of everything
			//if (showGlow)
				glowShader.Show();

			// Disable z buffer, now only 2d content is rendered.
			BaseGame.Device.RenderState.DepthBufferEnable = false;

			try
			{
				// Execute the game screen on top.
				if (gameScreens.Count > 0)
					gameScreens.Peek().Run(this);
			} // try
			catch (Exception ex)
			{
				Log.Write("Failed to execute " + gameScreens.Peek().Name +
					"\nError: " + ex.ToString());
			} // catch

			// Show distances (if in game)
			if (camera.InGame)
				asteroidManager.ShowAllDistances();//SmallFont);

			base.Draw(gameTime);

			// Show mouse cursor (in all modes except in the game)
			if (camera.InGame == false &&
				gameScreens.Count > 0)
				RenderMouseCursor();
			else
			{
				// In game always center mouse
				Input.CenterMouse();
			} // else
		} // Render()
		#endregion

		#region Unit testing
#if DEBUG
		public static void TestRenderSingleAsteroid()
		{
			//Model testModel1 = null;
			GameAsteroidManager asteroidManager = null;

			TestGame.Start(
				"TestRenderSingleAsteroid",
				delegate
				{
					//testModel1 = new Model("asteroid1");
					// Initialize asteroidManager and use last avialable level.
					asteroidManager = new GameAsteroidManager(
						Level.LoadAllLevels()[0]);
				},
				delegate
				{
					// Render sky cube map as our background.
					BaseGame.skyCube.RenderSky(1.0f, BaseGame.remSkyBackgroundColor);

					asteroidManager.Render(null, null);

					//tst: render asteroid in center
					//asteroidManager.GetAsteroidModel(0).Render(
					//	Matrix.CreateScale(15, 15, 15));
					//testModel1.Render(Matrix.CreateScale(4));

					BaseGame.MeshRenderManager.Render();

					TextureFont.WriteText(2, 30,
						"cam pos=" + BaseGame.CameraPos);
				});
		} // TestRenderSingleAsteroid()
#endif
		#endregion
	} // class RocketCommanderGame
} // namespace RocketCommanderXna
