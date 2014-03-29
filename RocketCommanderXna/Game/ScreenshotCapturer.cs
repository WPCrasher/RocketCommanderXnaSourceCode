#if !XBOX360
// Project: EuroVernichter 3D, File: ScreenshotCapturer.cs
// Namespace: XnaEngine.Game, Class: ScreenshotCapturer
// Path: C:\code\EuroVernichter 3D\Engine\Game, Author: abi
// Code lines: 176, Size of file: 4.62 KB
// Creation date: 11/28/2006 10:03 PM
// Last modified: 11/28/2006 10:04 PM
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Graphics;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// This is a game component that implements IUpdateable.
	/// </summary>
	public partial class ScreenshotCapturer : GameComponent
	{
		#region Variables
		/// <summary>
		/// Internal screenshot number (will increase by one each screenshot)
		/// </summary>
		private int screenshotNum = 0;
		/// <summary>
		/// Link to BaseGame class instance. Also holds windows title,
		/// which is used instead of Application.ProgramName.
		/// </summary>
		BaseGame game;
		#endregion

		#region Constructor
		/// <summary>
		/// Create screenshot capturer
		/// </summary>
		/// <param name="setGame">Set game</param>
		public ScreenshotCapturer(BaseGame setGame)
			: base(setGame)
		{
			game = setGame;
			screenshotNum = GetCurrentScreenshotNum();
		} // ScreenshotCapturer(setGame)
		#endregion

		#region Make screenshot
		#region Screenshot name builder
		/// <summary>
		/// Screenshot name builder
		/// </summary>
		/// <param name="num">Num</param>
		/// <returns>String</returns>
		private string ScreenshotNameBuilder(int num)
		{
			return Directories.ScreenshotsDirectory + "\\" +
				game.Window.Title + " Screenshot " +
				num.ToString("0000") + ".jpg";
		} // ScreenshotNameBuilder(num)
		#endregion

		#region Get current screenshot num
		/// <summary>
		/// Get current screenshot num
		/// </summary>
		/// <returns>Int</returns>
		private int GetCurrentScreenshotNum()
		{
			// We must search for last screenshot we can found in list using own
			// fast filesearch
			int i = 0, j = 0, k = 0, l = -1;
			// First check if at least 1 screenshot exist
			if (File.Exists(ScreenshotNameBuilder(0)) == true)
			{
				// First scan for screenshot num/1000
				for (i = 1; i < 10; i++)
				{
					if (File.Exists(ScreenshotNameBuilder(i * 1000)) == false)
						break;
				} // for (i)

				// This i*1000 does not exist, continue scan next level
				// screenshotnr/100
				i--;
				for (j = 1; j < 10; j++)
				{
					if (File.Exists(ScreenshotNameBuilder(i * 1000 + j * 100)) == false)
						break;
				} // for (j)

				// This i*1000+j*100 does not exist, continue scan next level
				// screenshotnr/10
				j--;
				for (k = 1; k < 10; k++)
				{
					if (File.Exists(ScreenshotNameBuilder(
							i * 1000 + j * 100 + k * 10)) == false)
						break;
				} // for (k)

				// This i*1000+j*100+k*10 does not exist, continue scan next level
				// screenshotnr/1
				k--;
				for (l = 1; l < 10; l++)
				{
					if (File.Exists(ScreenshotNameBuilder(
							i * 1000 + j * 100 + k * 10 + l)) == false)
						break;
				} // for (l)

				// This i*1000+j*100+k*10+l does not exist, we have now last
				// screenshot nr!!!
				l--;
			} // if (File.Exists)

			return i * 1000 + j * 100 + k * 10 + l;
		} // GetCurrentScreenshotNum()
		#endregion

		/// <summary>
		/// Make screenshot
		/// </summary>
		private void MakeScreenshot()
		{
			try
			{
				screenshotNum++;
				// Make sure screenshots directory exists
				if (Directory.Exists(Directories.ScreenshotsDirectory) == false)
					Directory.CreateDirectory(Directories.ScreenshotsDirectory);

				using (ResolveTexture2D dstTexture = new ResolveTexture2D(
					BaseGame.Device, BaseGame.Width, BaseGame.Height, 1,
					SurfaceFormat.Color))
				{
					// Get data with help of the resolve method
					BaseGame.Device.ResolveBackBuffer(dstTexture);

					dstTexture.Save(
						ScreenshotNameBuilder(screenshotNum),
						ImageFileFormat.Jpg);
				} // using (dstTexture)
			} // try
			catch (Exception ex)
			{
				Log.Write("Failed to save Screenshot: " + ex.ToString());
			} // catch (ex)
		} // MakeScreenshot()
		#endregion

		#region Update
		/// <summary>
		/// Allows the game component to update itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			if (Input.KeyboardKeyJustPressed(Keys.PrintScreen))
				MakeScreenshot();

			base.Update(gameTime);
		} // Update(gameTime)
		#endregion
	} // class ScreenshotCapturer
} // namespace RocketCommanderXna.Game
#endif