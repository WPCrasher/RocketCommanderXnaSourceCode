// Project: Rocket Commander, File: Help.cs
// Namespace: RocketCommanderXna.GameScreens, Class: Help
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Administrator
// Code lines: 142, Size of file: 4,02 KB
// Creation date: 24.12.2005 04:59
// Last modified: 24.12.2005 04:59
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Game;
using RocketCommanderXna.Properties;
using Microsoft.Xna.Framework.Graphics;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Help
	/// </summary>
	class Help : IGameScreen
	{
		#region Properties
		/// <summary>
		/// Name of this game screen
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return "Help";
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
		/// Create help
		/// </summary>
		public Help()
		{
		} // Help()
		#endregion

		#region Run
		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co.</param>
		public void Run(RocketCommanderGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Show helper screen texture
			game.helpScreenTexture.RenderOnScreen(
				new Rectangle(0,
				174 * BaseGame.Height / 768,
				BaseGame.Width,
				510 * BaseGame.Height / 768),
				new Rectangle(0, 0, 1024, 510));

			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 140)) ||
				Input.KeyboardEscapeJustPressed ||
				Input.GamePadBJustPressed ||
				Input.GamePadBackJustPressed)
			{
				quit = true;
			} // if
		} // Run(game)
		#endregion
	} // class Help
} // namespace RocketCommanderXna.GameScreens
