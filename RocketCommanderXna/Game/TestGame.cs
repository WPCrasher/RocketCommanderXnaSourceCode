// Project: RocketCommanderXna, File: TestGame.cs
// Namespace: RocketCommanderXna.Game, Class: TestGame
// Path: C:\code\XnaBook\RocketCommanderXna\Game, Author: Abi
// Code lines: 125, Size of file: 2,82 KB
// Creation date: 26.11.2006 12:22
// Last modified: 27.11.2006 03:50
// Generated with Commenter by abi.exDream.com

// Only used in debug mode
#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Helpers;
using RocketCommanderXna;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Test game
	/// </summary>
	public class TestGame : BaseGame//RocketCommanderGame
	{
		#region Variables
		/// <summary>
		/// Init code
		/// </summary>
		protected RenderDelegate initCode, renderCode;
		#endregion

		#region Constructor
		/// <summary>
		/// Create test game
		/// </summary>
		/// <param name="setWindowsTitle">Set windows title</param>
		/// <param name="windowWidth">Window width</param>
		/// <param name="windowHeight">Window height</param>
		/// <param name="setInitCode">Set init code</param>
		/// <param name="setRenderCode">Set render code</param>
		protected TestGame(string setWindowsTitle,
			RenderDelegate setInitCode,
			RenderDelegate setRenderCode)
		{
			this.Window.Title = setWindowsTitle;

#if !XBOX360
#if DEBUG
			// Force window on top
			WindowsHelper.ForceForegroundWindow(this.Window.Handle.ToInt32());
#endif
#endif
			initCode = setInitCode;
			renderCode = setRenderCode;
		} // TestGame(setWindowsTitle, setRenderCode)
		
		/// <summary>
		/// Initialize
		/// </summary>
		protected override void Initialize()
		{
			// Initialize game
			base.Initialize();

			// Call our custom initCode
			if (initCode != null)
				initCode();
		} // Initialize()
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		protected override void Update(GameTime time)
		{
			base.Update(time);

			// Allow quitting unit tests easily
			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed)
				this.Exit();
		} // Update(time)
		#endregion

		#region Draw
		/// <summary>
		/// Draw
		/// </summary>
		protected override void Draw(GameTime gameTime)
		{
			ClearBackground();

			// Drawing code
			if (renderCode != null)
				renderCode();

			base.Draw(gameTime);
		} // Draw(gameTime)
		#endregion

		#region Start test
		/// <summary>
		/// Game
		/// </summary>
		public static TestGame game;

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="initCode">Init code</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			RenderDelegate initCode, RenderDelegate renderCode)
		{
			using (game = new TestGame(testName, initCode, renderCode))
			{
				game.Run();
			} // using (game)
		} // Start(testName, initCode, renderCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="testName">Test name</param>
		/// <param name="renderCode">Render code</param>
		public static void Start(string testName,
			RenderDelegate renderCode)
		{
			Start(testName, null, renderCode);
		} // Start(testName, renderCode)

		/// <summary>
		/// Start
		/// </summary>
		/// <param name="renderCode">Render code</param>
		public static void Start(RenderDelegate renderCode)
		{
			Start("Unit Test", null, renderCode);
		} // Start(renderCode)
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestEmptyGame
		/// <summary>
		/// Test empty game
		/// </summary>
		public static void TestEmptyGame()
		{
			TestGame.Start(null);
		} // TestEmptyGame()
		#endregion
#endif
		#endregion
	} // class TestGame
} // namespace RocketCommanderXna.Game
