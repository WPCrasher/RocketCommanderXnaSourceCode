// Project: Rocket Commander, File: Credits.cs
// Namespace: RocketCommanderXna.GameScreens, Class: Credits
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 46, Size of file: 857 Bytes
// Creation date: 23.11.2005 18:37
// Last modified: 12.12.2005 05:30
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
using System.Threading;
using System.Diagnostics;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Credits
	/// </summary>
	class Credits : IGameScreen
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
				return "Credits";
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
		/// Create credits
		/// </summary>
		public Credits()
		{
		} // Credits()
		#endregion

		#region Write credits
		/// <summary>
		/// Write credits
		/// </summary>
		/// <param name="xPos">X coordinate</param>
		/// <param name="yPos">Y coordinate</param>
		/// <param name="leftText">Left text</param>
		/// <param name="rightText">Right text</param>
		private void WriteCredits(int xPos, int yPos,
			string leftText, string rightText)
		{
			TextureFont.WriteText(xPos, yPos, leftText);
			TextureFont.WriteText(xPos + 440, yPos/* + 8*/, rightText);
		} // WriteCredits(xPos, yPos, leftText)

		/// <summary>
		/// Write credits with link
		/// </summary>
		/// <param name="xPos">X coordinate</param>
		/// <param name="yPos">Y coordinate</param>
		/// <param name="leftText">Left text</param>
		/// <param name="rightText">Right text</param>
		/// <param name="linkText">Link text</param>
		private void WriteCreditsWithLink(int xPos, int yPos, string leftText,
			string rightText, string linkText, RocketCommanderGame game)
		{
			WriteCredits(xPos, yPos, leftText, rightText);

			// Process link (put below rightText)
			bool overLink = Input.MouseInBox(new Rectangle(
				xPos + 440, yPos + 8 + TextureFont.Height, 400, TextureFont.Height));
			TextureFont.WriteText(xPos + 440, yPos /*+ 8*/ + TextureFont.Height, linkText,
				overLink ? Color.Red : Color.White);
			if (overLink &&
				Input.MouseLeftButtonJustPressed)
			{
#if !XBOX360
				new Thread(new ThreadStart(delegate
					{
						Process.Start(linkText);
					})).Start();
				Thread.Sleep(100);
#endif
			} // if
		} // WriteCreditsWithLink(xPos, yPos, leftText)
		#endregion

		#region Run
		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(RocketCommanderGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Credits
			int xPos = 50 * BaseGame.Width / 1024;
			int yPos = 180 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos, Texts.Credits);

			WriteCreditsWithLink(xPos, yPos+72, "Idea, Design, Programming",
				"Benjamin Nitschke (abi)",
				"http://abi.exdream.com", game);
			WriteCredits(xPos, yPos + 167, "Thanks fly out to",
				"Christoph Rienäcker, Leif Griga, Boje Holtz,");
			WriteCredits(xPos, yPos + 217, "",
				"Enrico Cieslik (Judge), ZMan (www.thezbuffer.com),");
			WriteCredits(xPos, yPos + 267, "",
				"Dirk Primbs and Christina Storm of Microsoft and");
			WriteCredits(xPos, yPos + 317, "",
				"and the XNA and .NET Teams at Microsoft :)");

			TextureFont.WriteText(xPos, 647 * BaseGame.Height / 768,
				"Dedicated to the great XNA Framework.");

			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 150)))
			{
				quit = true;
			} // if
		} // Run(game)
		#endregion
	} // class Credits
} // namespace RocketCommanderXna.GameScreens
