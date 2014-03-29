// Project: Rocket Commander, File: Options.cs
// Namespace: RocketCommanderXna.GameScreens, Class: Options
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 361, Size of file: 12,36 KB
// Creation date: 24.12.2005 05:01
// Last modified: 24.12.2005 06:50
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Game;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Options
	/// </summary>
	class Options : IGameScreen
	{
		#region Variables
		/// <summary>
		/// Current player name, copied from the settings file.
		/// Can be changed in this screen and will be saved to the settings file.
		/// </summary>
		public static string currentPlayerName;
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
				return "Options";
			} // get
		} // Name

		/// <summary>
		/// Quit
		/// </summary>
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

		#region Static constructor
		/// <summary>
		/// Create options class, will just load the player name.
		/// </summary>
		static Options()
		{
			// Load player name
			currentPlayerName = GameSettings.Default.PlayerName;

			// Get player name from windows computer/user name if not found yet.
			if (String.IsNullOrEmpty(currentPlayerName))
				currentPlayerName = WindowsHelper.GetDefaultPlayerName();
		} // Options()
		#endregion

		#region Constructor
		/// <summary>
		/// Create options
		/// </summary>
		public Options()
		{
		} // Options()
		#endregion

		#region Run
		/// <summary>
		/// Is vsync on
		/// </summary>
		bool isVsyncOn = GameSettings.Default.WaitForVSync;
		/// <summary>
		/// BaseGame width
		/// </summary>
		int resolutionWidth = GameSettings.Default.ResolutionWidth;
    /// <summary>
    /// BaseGame height
    /// </summary>
    int resolutionHeight = GameSettings.Default.ResolutionHeight;

		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(RocketCommanderGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Write player name
			int xPos = 100 * BaseGame.Width / 1024,
				yPos = 168 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos,
				Texts.Options);

			// Edit player name
			yPos += 40 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos,
				Texts.PlayerName + ": " + currentPlayerName +
				((BaseGame.TotalTimeMs / 200) % 2 == 0 ? "|" : ""));

			// Allow editing player name
			string oldPlayerName = currentPlayerName;
			Input.HandleKeyboardInput(ref currentPlayerName);
			if (currentPlayerName != oldPlayerName)
			{
				// , and : are not allowed for the player name!
				StringHelper.RemoveCharacter(ref currentPlayerName, ',');
				StringHelper.RemoveCharacter(ref currentPlayerName, ':');

				GameSettings.Default.PlayerName = currentPlayerName;
			} // if (BaseGame.lastPressedKeys.Length)

			// Show all available languages and allow changing
			yPos += 40 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos, Texts.Language + ": ");

			TextureFont.WriteText(xPos + 225, yPos, Texts.English,
				Texts.Culture == null ||
				Texts.Culture == CultureInfo.InvariantCulture ?
				Color.Red : Color.White);
			if (Input.MouseLeftButtonPressed &&
				Texts.Culture != CultureInfo.InvariantCulture &&
				Input.MouseInBox(new Rectangle(xPos + 225, yPos, 160, 30)))
			{
				Texts.Culture = CultureInfo.InvariantCulture;
				GameSettings.Default.Language = "Invariant";
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 400, yPos, Texts.German,
				Texts.Culture != null && Texts.Culture.Name == "de-DE" ?
				Color.Red : Color.White);
			if (Input.MouseLeftButtonPressed &&
				(Texts.Culture == null || Texts.Culture.Name != "de-DE") &&
				Input.MouseInBox(new Rectangle(xPos + 400, yPos, 200, 30)))
			{
				Texts.Culture = new CultureInfo("de-DE");
				GameSettings.Default.Language = Texts.Culture.Name;
			} // if (Input.MouseLeftButtonPressed)
			//TODO: more languages

			// Allow changing the graphic quality
			yPos += 40 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos, Texts.Performance + ":");

			TextureFont.WriteText(xPos + 40, yPos + 40, Texts.PerformanceMedium,
				BaseGame.GraphicsPerformanceSetting ==
				BaseGame.PerformanceSetting.Medium ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos, yPos + 40, 500, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos, yPos + 40, 500, 30)))
			{
				BaseGame.GraphicsPerformanceSetting =
					BaseGame.PerformanceSetting.Medium;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 40, yPos + 80, Texts.PerformanceQuality,
				BaseGame.GraphicsPerformanceSetting ==
				BaseGame.PerformanceSetting.Quality ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos, yPos + 80, 500, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos, yPos + 80, 500, 30)))
			{
				BaseGame.GraphicsPerformanceSetting =
					BaseGame.PerformanceSetting.Quality;
			} // if (Input.MouseLeftButtonPressed)

			// Allow to select from some of the supported resolutions
			yPos += 120 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos, Texts.ChangeResolution + ":");

			TextureFont.WriteText(xPos + 40, yPos + 40, "Autodetect Best",
				resolutionWidth == 0 &&
				resolutionHeight == 0 ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 40, yPos + 40, 300, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 40, yPos + 40, 300, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 0;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 0;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 370, yPos + 40, "640x480",
				resolutionWidth == 640 &&
				resolutionHeight == 480 ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 350, yPos + 40, 150, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 350, yPos + 40, 150, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 640;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 480;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 550, yPos + 40, "800x600",
				resolutionWidth == 800 &&
				resolutionHeight == 600 ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 510, yPos + 40, 150, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 510, yPos + 40, 150, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 800;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 600;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 40, yPos + 80, "1024x768",
				resolutionWidth == 1024 &&
				resolutionHeight == 768 ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 40, yPos + 80, 150, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 40, yPos + 80, 150, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 1024;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 768;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 370, yPos + 80, "1280x1024",
				resolutionWidth == 1280 &&
				resolutionHeight == 1024 ?
				Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 220, yPos + 80, 150, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 220, yPos + 80, 150, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 1280;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 1024;
			} // if (Input.MouseLeftButtonPressed)

			TextureFont.WriteText(xPos + 550, yPos + 80, "1600x1200",
				resolutionWidth == 1600 &&
				resolutionHeight == 1200 ? Color.Red :
				Input.MouseInBox(new Rectangle(xPos + 400, yPos + 80, 150, 30)) ?
				Color.LightSalmon : Color.White);
			if (Input.MouseLeftButtonJustPressed &&
				Input.MouseInBox(new Rectangle(xPos + 400, yPos + 80, 150, 30)))
			{
				GameSettings.Default.ResolutionWidth = resolutionWidth = 1600;
				GameSettings.Default.ResolutionHeight = resolutionHeight = 1200;
			} // if (Input.MouseLeftButtonPressed)

			// Allow to toggle vsync on/off
			yPos += 80 + 30 + 10;// + 5;
			bool overVsyncOnOff = Input.MouseInBox(new Rectangle(
				xPos, yPos, 200, 20));
			TextureFont.WriteText(
				xPos, yPos,
				"VSync" + ":",
				overVsyncOnOff ? Color.Red : Color.LightGray);
			TextureFont.WriteText(
				xPos + 150, yPos,
				isVsyncOn ? Texts.On : Texts.Off,
				overVsyncOnOff ? Color.Red : Color.LightGray);
			if (overVsyncOnOff &&
				Input.MouseLeftButtonJustPressed)
			{
				// Toggle state and set to both settings and local variable
				isVsyncOn = GameSettings.Default.WaitForVSync = !isVsyncOn;
			} // if (overVsyncOnOff)

			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 150)))
			{
				// Save options (e.g. player name or language) in case they were
				// changed.
				GameSettings.Save();

				quit = true;
			} // if (game.RenderMenuButton)
		} // Run(game)
		#endregion
	} // class Options
} // namespace RocketCommanderXna.GameScreens
