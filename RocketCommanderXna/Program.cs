// Project: RocketCommanderXna, File: Program.cs
// Namespace: RocketCommanderXna, Class: Program
// Path: C:\code\XnaBook\RocketCommanderXna, Author: Abi
// Code lines: 183, Size of file: 5,28 KB
// Creation date: 04.12.2006 22:42
// Last modified: 07.12.2006 01:57
// Generated with Commenter by abi.exDream.com

#region Using directives
using RocketCommanderXna.Graphics;
using System;
using RocketCommanderXna.Sounds;
using RocketCommanderXna.Game;
using Microsoft.Xna.Framework;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.GameScreens;
using RocketCommanderXna.Shaders;
#endregion

namespace RocketCommanderXna
{
	/// <summary>
	/// Program
	/// </summary>
	static class Program
	{
		#region Variables
		/// <summary>
		/// Version number for this program. Used to check for updates
		/// and also displayed in the main menu.
		/// </summary>
		public static int versionHigh = 2,
			versionLow = 0;
		#endregion

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			//* The game:
			using (RocketCommanderGame game = new RocketCommanderGame())
			{
				game.Run();
			} // using (game)
			/*/
			// Unit tests:
			//Level.GenerateLevelFiles();
			//Sound.TestPlaySounds();
			//RocketCommanderXnaGame.StartGame();
			//Texture.TestRenderTexture();
			//TextureFont.TestRenderFont();
			//LineManager2D.TestRenderLines();
			//LineManager3D.TestRenderLines();
			Model.TestSingleModel();
			//Input.TestXboxControllerInput();
			//PostScreenGlow.TestPostScreenGlow();
			//RenderToTexture.TestCreateRenderToTexture();
			//Mission.TestGameHud();
			//LensFlare.TestLensFlare();
			//BaseGame.TestIsVisible();
			//SpaceCamera.TestSpaceCamera();
			//GameAsteroidManager.TestAsteroidManager();
			//RocketCommanderGame.TestRenderSingleAsteroid();
			//new GameAsteroidManager.AsteroidPhysicsUsingAsteroidManagerTests().
			//	TestAsteroidPhysicsSmallScene();
			//*/
		} // Main(args)
		#endregion
	} // class Program
} // namespace RocketCommanderXna
