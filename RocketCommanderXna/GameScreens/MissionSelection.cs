// Project: Rocket Commander, File: MissionSelection.cs
// Namespace: RocketCommanderXna.GameScreens, Class: MissionSelection
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 225, Size of file: 7,66 KB
// Creation date: 11.12.2005 18:08
// Last modified: 12.12.2005 01:34
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Game;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketCommanderXna.Sounds;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Mission selection
	/// </summary>
	class MissionSelection : IGameScreen
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
				return "Mission selection";
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
		/// Create MissionSelection
		/// </summary>
		public MissionSelection()
		{
			xInputLevelSelection = 0;
			SelectLevelForXInput(4);
		} // MissionSelection()
		#endregion

		#region Run
		/// <summary>
		/// Input selection for xbox controller. Initially not set.
		/// Press left/right joystick for changing.
		/// </summary>
		int xInputLevelSelection = -1;
		/// <summary>
		/// Select menu item for Input
		/// </summary>
		void SelectLevelForXInput(int maxLevels)
		{
			Sound.Play(Sound.Sounds.Highlight);
			if (xInputLevelSelection >= 0 &&
				xInputLevelSelection < maxLevels)
				Input.MousePos = new Point(
					(50 + 20) * BaseGame.Width / 1024 +
					xInputLevelSelection * (BaseGame.Width - 175) / 4,
					(220 + 50) * BaseGame.Height / 768);
		} // SelectMenuItemForXInput()

		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(RocketCommanderGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Select mission
			int xPos = 50 * BaseGame.Width / 1024;
			int yPos = 154 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos, Texts.SelectMission);

			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 150)))
			{
				quit = true;
			} // if

			// If pressing XBox controller right/left change selection
			int maxLevels = Math.Min(game.levels.Length, 4);
			if (Input.GamePadRightJustPressed)
			{
				xInputLevelSelection =
					(xInputLevelSelection + 1) % maxLevels;
				SelectLevelForXInput(maxLevels);
			} // if (Input.GamePad)
			else if (Input.GamePadLeftJustPressed)
			{
				if (xInputLevelSelection <= 0)
					xInputLevelSelection = maxLevels;
				xInputLevelSelection--;
				SelectLevelForXInput(maxLevels);
			} // if (Input.GamePad)

			// Show all levels (max. 4 levels are supported right now)
			for (int num = 0; num < game.levels.Length &&
				num < 4; num++)
			{
				Level level = game.levels[num];
				xPos = 50 * BaseGame.Width / 1024 +
					num * (BaseGame.Width - 175) / 4;
				yPos = 192 * BaseGame.Height / 768;

				// Is selected?
				int maxHeight = BaseGame.Height * 5 / 6 - yPos;
				int height = Math.Min(level.Length, maxHeight);
				Rectangle clickArea = new Rectangle(
					xPos, yPos,
					(BaseGame.Width - 180) / 4 - 10, height + 27);
				Color col = Input.MouseInBox(clickArea) && quit == false ?
					Color.Red : Color.White;

				// Write name on top
				TextureFont.WriteText(
					xPos, yPos,// - (num%2==1?40:0),
					level.Name, col);

				// Calc preview rect
				Rectangle previewRect = new Rectangle(
					xPos, yPos + 38, 40, height);

				// Disable linear filtering for level textures
				BaseGame.DisableLinearTextureFiltering();

				// Show level preview.
				// Show 2 parts if bigger than 512
				if (level.Length > 512)
				{
					previewRect = new Rectangle(
						xPos + 70, yPos + 38, 40, height);
					level.Texture.RenderOnScreen(previewRect,
						new Rectangle(0, 0, 40, Math.Min(level.Length, 512)));

					// Draw lines around preview rect to mark highlighting
					Point upperLeft = new Point(previewRect.X, previewRect.Y);
					Point upperRight = new Point(previewRect.Right, previewRect.Y);
					Point lowerLeft = new Point(previewRect.X, previewRect.Bottom);
					Point lowerRight = new Point(previewRect.Right, previewRect.Bottom);
					BaseGame.DrawLine(upperLeft, upperRight, col);
					BaseGame.DrawLine(upperLeft, lowerLeft, col);
					BaseGame.DrawLineWithShadow(upperRight, lowerRight,
						col);
					BaseGame.DrawLineWithShadow(lowerLeft,
						new Point(lowerRight.X + 1, lowerRight.Y), col);

					previewRect = new Rectangle(
						xPos, yPos + 26 + 8, 40, height);
					level.Texture.RenderOnScreen(previewRect,
						new Rectangle(0, 512, 40, Math.Min(level.Length - 512, 512)));

					upperLeft = new Point(previewRect.X, previewRect.Y);
					upperRight = new Point(previewRect.Right, previewRect.Y);
					lowerLeft = new Point(previewRect.X, previewRect.Bottom);
					lowerRight = new Point(previewRect.Right, previewRect.Bottom);
					BaseGame.DrawLine(upperLeft, upperRight, col);
					BaseGame.DrawLine(upperLeft, lowerLeft, col);
					BaseGame.DrawLineWithShadow(upperRight, lowerRight,
						col);
					BaseGame.DrawLineWithShadow(lowerLeft,
						new Point(lowerRight.X + 1, lowerRight.Y), col);

					// Draw connection line (makes more sense to understand structure)
					Point p1 =
						new Point(xPos + 20, previewRect.Y);
					Point p2 =
						new Point(xPos + 20, previewRect.Y - 8);
					Point p3 =
						new Point(xPos + 40 + 15, previewRect.Y - 8);
					Point p4 =
						new Point(xPos + 40 + 15, previewRect.Bottom);
					Point p5 =
						new Point(xPos + 40 + 30 + 20, previewRect.Bottom);
					Point p6 =
						new Point(xPos + 40 + 30 + 20, previewRect.Bottom - 8);
					col = Color.LightGray;
					BaseGame.DrawLine(p1, p2, col);
					BaseGame.DrawLine(p2, p3, col);
					BaseGame.DrawLine(p3, p4, col);
					BaseGame.DrawLine(p4, p5, col);
					BaseGame.DrawLine(p5, p6, col);
				} // if
				else
				{
					// Show just one part
					level.Texture.RenderOnScreen(previewRect,
						new Rectangle(0, 0, 40, Math.Min(level.Length, 512)));

					// Draw lines around preview rect to mark highlighting
					Point upperLeft = new Point(previewRect.X, previewRect.Y);
					Point upperRight = new Point(previewRect.Right, previewRect.Y);
					Point lowerLeft = new Point(previewRect.X, previewRect.Bottom);
					Point lowerRight = new Point(previewRect.Right, previewRect.Bottom);
					BaseGame.DrawLine(upperLeft, upperRight, col);
					BaseGame.DrawLine(upperLeft, lowerLeft, col);
					BaseGame.DrawLineWithShadow(upperRight, lowerRight,
						col);
					BaseGame.DrawLineWithShadow(lowerLeft,
						new Point(lowerRight.X + 1, lowerRight.Y), col);
				} // else

				// Enable linear texture filtering again
				BaseGame.EnableLinearTextureFiltering();

				// Level selected?
				if (Input.MouseInBox(clickArea) &&
					quit == false &&
					(Input.MouseLeftButtonJustPressed ||
					Input.GamePadAJustPressed ||
					Input.GamePadStartPressed))
				{
					// Close mission selection
					game.RemoveCurrentGameScreen();

					// Remember selected level number for the sky rotation.
					Level.currentLevelNumber = num;

					// Start mission with this level
					game.AddGameScreen(new Mission(level, game.asteroidManager));
				} // if
			} // for (num)
		} // Run(game)
		#endregion
	} // class MissionSelection
} // namespace RocketCommanderXna.GameScreens
