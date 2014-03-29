// Project: RocketCommanderXna, File: MainMenu.cs
// Namespace: RocketCommanderXna.GameScreens, Class: MainMenu
// Path: C:\code\XnaBook\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 216, Size of file: 6,13 KB
// Creation date: 04.12.2006 22:43
// Last modified: 07.12.2006 01:15
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Game;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RocketCommanderXna.Sounds;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Main menu
	/// </summary>
	class MainMenu : IGameScreen
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
				return "Main menu";
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

		/// <summary>
		/// Reset camera when starting.
		/// </summary>
		public bool resetCamera = true;
		#endregion

		#region Constructor
		/// <summary>
		/// Create main menu
		/// </summary>
		public MainMenu()
		{
			xInputMenuSelection = 0;
			SelectMenuItemForXInput();
		} // MainMenu()
		#endregion

		#region Run
		/// <summary>
		/// Button locations
		/// </summary>
		Point[] buttonLocations = new Point[]
			{
				new Point(202, 181),
				new Point(161, 266),
				new Point(137, 351),
				new Point(136, 436),
				new Point(150, 521),
				new Point(186, 606),
			};
		/// <summary>
		/// Input selection for xbox controller. Initially not set.
		/// Press up/down joystick for changing.
		/// </summary>
		int xInputMenuSelection = 0;// -1;
		/// <summary>
		/// Select menu item for Input
		/// </summary>
		void SelectMenuItemForXInput()
		{
			Sound.Play(Sound.Sounds.Highlight);
			if (xInputMenuSelection >= 0 &&
				xInputMenuSelection < buttonLocations.Length)
				Input.MousePos = new Point(
					(buttonLocations[xInputMenuSelection].X + 100) *
					BaseGame.Width / 1024,
					(buttonLocations[xInputMenuSelection].Y + 38) *
					BaseGame.Height / 768);
		} // SelectMenuItemForXInput()

		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(RocketCommanderGame game)
		{
			if (xInputMenuSelection < 0)
			{
				xInputMenuSelection = 0;
				SelectMenuItemForXInput();
			} // if

			if (resetCamera)
			{
				resetCamera = false;
				RocketCommanderGame.camera.SetPosition(Vector3.Zero);
				RocketCommanderGame.camera.InGame = false;
				xInputMenuSelection = 0;
			} // if

			// Render background
			game.RenderMenuBackground();

			// Show all buttons
			int buttonNum = 0;
			MenuButton[] menuButtons = new MenuButton[]
				{
					MenuButton.Missions,
					MenuButton.Highscore,
					MenuButton.Credits,
					MenuButton.Help,
					MenuButton.Options,
					MenuButton.Exit,
					MenuButton.Back,
				};
			foreach (MenuButton button in menuButtons)
				//EnumHelper.GetEnumerator(typeof(MenuButton)))
				// Don't render the back button
				if (button != MenuButton.Back)
				{
					if (game.RenderMenuButton(button, buttonLocations[buttonNum]))
					{
						if (button == MenuButton.Missions)
						{
							xInputMenuSelection = -1;
							game.AddGameScreen(new MissionSelection());
						} // if
						else if (button == MenuButton.Highscore)
							game.AddGameScreen(new Highscores());
						else if (button == MenuButton.Credits)
							game.AddGameScreen(new Credits());
						else if (button == MenuButton.Options)
							game.AddGameScreen(new Options());
						else if (button == MenuButton.Help)
							game.AddGameScreen(new Help());
						else if (button == MenuButton.Exit)
							quit = true;
					} // if
					buttonNum++;
					if (buttonNum >= buttonLocations.Length)
						break;
				} // if

			// Hotkeys, M=Mission, H=Highscores, C=Credits, Esc=Quit
			if (Input.KeyboardKeyJustPressed(Keys.M))
			{
				xInputMenuSelection = -1;
				game.AddGameScreen(new MissionSelection());
			} // if
			else if (Input.KeyboardKeyJustPressed(Keys.H))
				game.AddGameScreen(new Highscores());
			else if (Input.KeyboardKeyJustPressed(Keys.C))
				game.AddGameScreen(new Credits());
			else if (Input.KeyboardKeyJustPressed(Keys.O))
				game.AddGameScreen(new Options());
			else if (Input.KeyboardKeyJustPressed(Keys.F1))
				game.AddGameScreen(new Help());
			else if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed)
				quit = true;

			// If pressing XBox controller up/down change selection
			if (Input.GamePadDownJustPressed)
			{
				xInputMenuSelection =
					(xInputMenuSelection + 1) % buttonLocations.Length;
				SelectMenuItemForXInput();
			} // if (Input.GamePad)
			else if (Input.GamePadUpJustPressed)
			{
				if (xInputMenuSelection <= 0)
					xInputMenuSelection = buttonLocations.Length;
				xInputMenuSelection--;
				SelectMenuItemForXInput();
			} // if (Input.GamePad)

			string versionText = "v" +
				//obs: Application.ProductVersion.Substring(0, 3);
				Program.versionHigh + "." + Program.versionLow;
			int xPos = BaseGame.Width -
				versionText.Length * TextureFont.Height / 2 - 2,// + 4,
				yPos = BaseGame.Height - TextureFont.Height - 1;
#if XBOX360
			xPos -= BaseGame.XToRes(42);//32);
			yPos -= BaseGame.YToRes(66);//26);
#endif
			TextureFont.WriteText(xPos, yPos, versionText, Color.Gray);
		} // Run(game)
		#endregion
	} // class MainMenu
} // namespace RocketCommanderXna.GameScreens
