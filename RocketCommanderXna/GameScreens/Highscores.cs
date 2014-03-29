// Project: Rocket Commander, File: Highscores.cs
// Namespace: RocketCommanderXna.GameScreens, Class: Highscores
// Path: C:\code\RocketCommanderXna\GameScreens, Author: Abi
// Code lines: 46, Size of file: 863 Bytes
// Creation date: 01.11.2005 23:55
// Last modified: 17.03.2006 03:34
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RocketCommanderXna.GameScreens
{
	/// <summary>
	/// Highscores
	/// </summary>
	class Highscores : IGameScreen
	{
		#region Variables
		/// <summary>
		/// Highscore modes
		/// </summary>
		private enum HighscoreModes
		{
			Local,
			OnlineThisHour,
			OnlineTotal,
		} // enum HighscoreModes

		/// <summary>
		/// Current highscore mode, initially set to local.
		/// </summary>
		private HighscoreModes highscoreMode = HighscoreModes.Local;
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
				return "Highscores";
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

		#region Highscore helper class
		/// <summary>
		/// Highscore helper class
		/// </summary>
		private struct Highscore
		{
			/// <summary>
			/// Player name
			/// </summary>
			public string name;
			/// <summary>
			/// Level name
			/// </summary>
			public string level;
			/// <summary>
			/// Highscore points 
			/// </summary>
			public int points;

			/// <summary>
			/// Create highscore
			/// </summary>
			/// <param name="setName">Set name</param>
			/// <param name="setLevelName">Set level name</param>
			/// <param name="setPoints">Set points</param>
			public Highscore(string setName, string setLevelName, int setPoints)
			{
				name = setName;
				level = setLevelName;
				points = setPoints;
			} // Highscore(setName, setLevelName, setPoints)

			/// <summary>
			/// To string
			/// </summary>
			/// <returns>String</returns>
			public override string ToString()
			{
				return name + ":" + level + ":" + points;
			} // ToString()
		} // struct Highscore

		/// <summary>
		/// Number of highscores displayed in this screen.
		/// </summary>
		private const int NumOfHighscores = 10;
		/// <summary>
		/// List of remembered highscores.
		/// </summary>
		private static Highscore[] highscores = new Highscore[NumOfHighscores];

		/// <summary>
		/// Write highscores to string. Used to save to settings.
		/// </summary>
		private static void WriteHighscoresToSettings()
		{
			GameSettings.Default.Highscores = StringHelper.WriteArrayData(highscores);
		} // WriteHighscoresToSettings()

		/// <summary>
		/// Read highscores from settings
		/// </summary>
		/// <returns>True if reading succeeded, false otherwise.</returns>
		private static bool ReadHighscoresFromSettings()
		{
			if (String.IsNullOrEmpty(GameSettings.Default.Highscores))
				return false;

			try
			{
				string[] allHighscores = GameSettings.Default.Highscores.Split(
					new char[] { ',' });

				for (int num = 0; num < allHighscores.Length &&
					num < highscores.Length; num++)
				{
					string[] oneHighscore =
						allHighscores[num].Split(new char[] { ':' });
					highscores[num] = new Highscore(
						oneHighscore[0], oneHighscore[1],
						Convert.ToInt32(oneHighscore[2]));
				} // for (num)

				return true;
			} // try
			catch (Exception ex)
			{
				Log.Write("Failed to load highscores: " + ex.ToString());
				return false;
			} // catch
		} // ReadHighscoresFromSettings()
		#endregion

		#region Generate hash from file for sending value to online server
		/*obs
		/// <summary>
		/// Generate hash from file
		/// </summary>
		/// <param name="filename">Filename</param>
		private static byte[] GenerateHashFromFile(string filename)
		{
			FileStream file = File.OpenRead(filename);
			byte[] readData = new byte[file.Length];
			file.Read(readData, 0, (int)file.Length);
			file.Close();

			SHA1Managed shaHash = new SHA1Managed();
			return shaHash.ComputeHash(readData);
		} // GenerateHashFromFile(filename)

		/// <summary>
		/// Precalculated game file hash of "Rocket Commander.exe"
		/// </summary>
		static byte[] gameFileHash = null;
		 */
		#endregion
		
		#region Static constructor
		/// <summary>
		/// Create Highscores class, will basically try to load highscore list,
		/// if that fails we generate a standard highscore list!
		/// </summary>
		static Highscores()
		{
			//obs: gameFileHash = GenerateHashFromFile("Rocket Commander.exe");

			if (ReadHighscoresFromSettings() == false)
			{
				// Generate default list
				highscores[9] = new Highscore("Newbie", "Easy Flight", 0);
				highscores[8] = new Highscore("Desert-Fox", "Easy Flight", 5000);
				highscores[7] = new Highscore("Waii", "Easy Flight", 10000);
				highscores[6] = new Highscore("ViperDK", "Easy Flight", 15000);
				highscores[5] = new Highscore("Netfreak", "Easy Flight", 20000);
				highscores[4] = new Highscore("Judge", "Easy Flight", 25000);
				highscores[3] = new Highscore("exDreamBoy", "Easy Flight", 30000);
				highscores[2] = new Highscore("Master_L", "Easy Flight", 35000);
				highscores[1] = new Highscore("Freshman", "Easy Flight", 40000);
				highscores[0] = new Highscore("abi", "Easy Flight", 45000);

				WriteHighscoresToSettings();
			} // if
		} // Highscores()
		#endregion

		#region Constructor
		/// <summary>
		/// Create highscores
		/// </summary>
		public Highscores()
		{
		} // Highscores()
		#endregion

		#region Get rank from current score
		/// <summary>
		/// Get rank from current score.
		/// Used in game to determinate rank while flying around ^^
		/// </summary>
		/// <param name="score">Score</param>
		/// <returns>Int</returns>
		public static int GetRankFromCurrentScore(int score)
		{
			// Just compare with all highscores and return the rank we have reached.
			for (int num = 0; num < highscores.Length; num++)
			{
				if (score >= highscores[num].points)
					return num;
			} // for (num)

			// No Rank found, use rank 11
			return highscores.Length;
		} // GetRankFromCurrentScore(score)
		#endregion

		#region Submit highscore after game
		//obs: static bool onlineUploadHighscoreFailedAlreadyLogged = false;
		/// <summary>
		/// Submit highscore. Done after each game is over (won or lost).
		/// New highscore will be added to the highscore screen and send
		/// to the online server.
		/// </summary>
		/// <param name="score">Score</param>
		/// <param name="levelName">Level name</param>
		public static void SubmitHighscore(int score, string levelName)
		{
			// Search which highscore rank we can replace
			for (int num = 0; num < highscores.Length; num++)
			{
				if (score >= highscores[num].points)
				{
					// Move all highscores up
					for (int moveUpNum = highscores.Length - 1; moveUpNum > num;
						moveUpNum--)
					{
						highscores[moveUpNum] = highscores[moveUpNum - 1];
					} // for (moveUpNum)

					// Add this highscore into the local highscore table
					highscores[num].name = Options.currentPlayerName;
					highscores[num].level = levelName;
					highscores[num].points = score;

					// And save that
					Highscores.WriteHighscoresToSettings();

					break;
				} // if
			} // for (num)

			// Else no highscore was reached, we can't replace any rank.
			/*can't do on the Xbox 360!
			// Upload highscore to server with help of the webservice :)
			// Do this asyncronly, it could take a while and we don't want to wait
			// for it to complete (there is no return value anyway).
			new Thread(new ThreadStart(
				// Anoymous delegates, isn't .NET 2.0 great? ^^
				delegate
				{
					// Note: We could also use UploadHighscoreAsync, this does help,
					// BUT the webservice class still needs to be created and that
					// does take a little while too (0.5 secs sometimes).
					// For this reason we let everything execute asyncronly in
					// this thread. This has also the benefit allowing us to
					// check if something failed (e.g. timeout, not online, etc.)
					try
					{
						//tst: Log.Write("Start uploading score");
						string ret = new www.RocketCommander.com.
							RocketCommanderXnaService().UploadHighscore(
							gameFileHash,
							Options.currentPlayerName,
							score,
							levelName,
							GenerateHashFromFile(Directories.LevelsDirectory +
							"\\" + levelName + ".png"));
						//tst: Log.Write("Finished uploading score: " + ret);
					} // try
					catch (Exception ex)
					{
						// Only log this once
						if (onlineUploadHighscoreFailedAlreadyLogged == false)
						{
							onlineUploadHighscoreFailedAlreadyLogged = true;
							Log.Write("Failed to upload highscore to online server. " +
								"Error message: " + ex.ToString());
						} // if
					} // catch
				})).Start();
			 */
		} // SubmitHighscore(score)
		#endregion

		#region Get online highscores
		Highscore[] onlineHighscores = new Highscore[10];
		//obs: Thread onlineGetHighscoreThread = null;

		/// <summary>
		/// Get online highscores
		/// </summary>
		/// <param name="onlyThisHour">Only this hour</param>
		private void GetOnlineHighscores(bool onlyThisHour)
		{
			// Clear all online highscores and wait for a new update!
			for (int num = 0; num < onlineHighscores.Length; num++)
			{
				onlineHighscores[num].name = "-";
				onlineHighscores[num].level = "";
				onlineHighscores[num].points = 0;
			} // for (num)
			/*obs
			// Stop any old threads
			if (onlineGetHighscoreThread != null)
				onlineGetHighscoreThread.Abort();

			// Ask web service for highscore list! Do this asyncronly,
			// it could take a while and we don't want to wait for it to complete.
			onlineGetHighscoreThread = new Thread(new ThreadStart(
				// Anoymous delegates, isn't .NET 2.0 great? ^^
				delegate
				{
					// See notes above
					try
					{
						string ret = new www.RocketCommander.com.
							RocketCommanderXnaService().GetTop10Highscores(
							onlyThisHour);

						// Now split this up and build the online highscore with it.
						string[] allHighscores = ret.Split(new char[] { ',' });
						for (int num = 0; num < allHighscores.Length &&
							num < onlineHighscores.Length; num++)
						{
							string[] oneHighscore =
								allHighscores[num].Split(new char[] { ':' });
							onlineHighscores[num] = new Highscore(
								oneHighscore[0].Trim(), oneHighscore[2],
								Convert.ToInt32(oneHighscore[1]));
						} // for (num)
					} // try
#if DEBUG
					catch (Exception ex)
					{
						Log.Write("Online highscores failed: " + ex.ToString());
					} // catch
#else
					catch { } // ignore any exceptions!
#endif
				}));

			onlineGetHighscoreThread.Start();
			 */
		} // GetOnlineHighscores(onlyThisHour)
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

			// Show highscores, allow to select between local highscores,
			// online highscores this hour and online highscores best of all time. 
			int xPos = 100 * BaseGame.Width / 1024;
			int yPos = 184 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos,
				Texts.Highscores + ":");

			// Local Highscores
			Rectangle rect = new Rectangle(
				xPos + 210 * BaseGame.Width / 1024,
				yPos + 5 * BaseGame.Height / 768, 130, 28);
			bool highlighted = Input.MouseInBox(rect);
			TextureFont.WriteText(rect.X, rect.Y, Texts.Local,
				highscoreMode == HighscoreModes.Local ? Color.Red :
				highlighted ? Color.LightSalmon : Color.White);
			if (highlighted &&
				Input.MouseLeftButtonJustPressed)
				highscoreMode = HighscoreModes.Local;

			// Online This Hour Highscores
			rect = new Rectangle(
				xPos + 340 * BaseGame.Width / 1024,
				yPos + 5 * BaseGame.Height / 768, 650 - 340, 28);
			highlighted = Input.MouseInBox(rect);
			TextureFont.WriteText(rect.X, rect.Y, Texts.OnlineThisHour,
				highscoreMode == HighscoreModes.OnlineThisHour ? Color.Red :
				highlighted ? Color.LightSalmon : Color.White);
			if (highlighted &&
				Input.MouseLeftButtonJustPressed)
			{
				highscoreMode = HighscoreModes.OnlineThisHour;
				GetOnlineHighscores(true);
			} // if

			// Online Total Highscores
			rect = new Rectangle(
				xPos + 650 * BaseGame.Width / 1024,
				yPos + 5 * BaseGame.Height / 768, 300, 28);
			highlighted = Input.MouseInBox(rect);
			TextureFont.WriteText(rect.X, rect.Y, Texts.OnlineTotal,
				highscoreMode == HighscoreModes.OnlineTotal ? Color.Red :
				highlighted ? Color.LightSalmon : Color.White);
			if (highlighted &&
				Input.MouseLeftButtonJustPressed)
			{
				highscoreMode = HighscoreModes.OnlineTotal;
				GetOnlineHighscores(false);
			} // if

			yPos = 260 * BaseGame.Height / 768;
			Highscore[] selectedHighscores =
				highscoreMode == HighscoreModes.Local ?
				highscores : onlineHighscores;

			for (int num = 0; num < NumOfHighscores; num++)
			{
				Color col = Input.MouseInBox(new Rectangle(
					xPos, yPos + num * 30, 600 + 200, 28)) ?
					Color.White : ColorHelper.FromArgb(200, 200, 200);
				TextureFont.WriteText(xPos, yPos + num * 30,
					(1 + num) + ".", col);
				TextureFont.WriteText(xPos + 50, yPos + num * 30,
					selectedHighscores[num].name, col);
				TextureFont.WriteText(xPos + 380, yPos + num * 30,
					Texts.Score + ": " + selectedHighscores[num].points, col);
				TextureFont.WriteText(xPos + 620, yPos + num * 30,
					Texts.Mission + ": " + selectedHighscores[num].level, col);
			} // for (num)

			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 150)))
			{
				quit = true;
			} // if
			else
			{
				rect = new Rectangle(xPos,
					(768 - 110) * BaseGame.Height / 768, 600, 22 + 14);
				highlighted = Input.MouseInBox(rect);
				TextureFont.WriteText(xPos,
					(768 - 104) * BaseGame.Height / 768,
					"Online Highscores: www.RocketCommander.com",
					highlighted ? Color.Red : Color.White);
				if (highlighted &&
					Input.MouseLeftButtonJustPressed)
				{
#if !XBOX360
					new Thread(new ThreadStart(delegate
						{
							Process.Start("http://www.RocketCommander.com");
						})).Start();
					Thread.Sleep(100);
#endif
				} // if
			} // else
		} // Run(game)
		#endregion
	} // class Highscores
} // namespace RocketCommanderXna.GameScreens
