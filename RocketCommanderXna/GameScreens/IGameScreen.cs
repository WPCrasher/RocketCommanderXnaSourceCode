// Project: Rocket Commander, File: IGameScreen.cs
// Namespace: RocketCommanderXna.GameScreens, Class: IGameScreen
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 41, Size of file: 367 Bytes
// Creation date: 23.11.2005 18:17
// Last modified: 23.11.2005 18:20
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Game screen helper interface for all game screens of our game.
	/// Helps us to put them all into one list and manage them in our BaseGame.
	/// </summary>
	public interface IGameScreen
	{
		/// <summary>
		/// Name of this game screen, e.g. Main menu, Highscores
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns true if we want to quit this screen and return to the
		/// previous screen. If no more screens are left the game is exited.
		/// </summary>
		bool Quit { get; }

		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		void Run(RocketCommanderGame game);
	} // IGameScreen
} // namespace RocketCommanderXna.GameScreens
