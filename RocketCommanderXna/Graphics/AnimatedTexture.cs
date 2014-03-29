// Project: RocketCommanderXna, File: AnimatedTexture.cs
// Namespace: RocketCommanderXna.Graphics, Class: AnimatedTexture
// Path: C:\code\XnaBook\RocketCommanderXna\Graphics, Author: abi
// Code lines: 46, Size of file: 1,03 KB
// Creation date: 07.12.2006 18:22
// Last modified: 07.12.2006 21:41
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Game;
using Microsoft.Xna.Framework.Graphics;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Animated texture
	/// </summary>
	public class AnimatedTexture : Texture
	{
		#region Variables
		/// <summary>
		/// Textures for the animation.
		/// You can still use d3dTexture, it will just hold current animation
		/// texture! See Select(.).
		/// </summary>
		private XnaTexture[] xnaTextures = null;

		/// <summary>
		/// Get animation length (how many textures are used inside of this
		/// animated texture). Can be used for the Select(.) method.
		/// </summary>
		/// <returns>Int</returns>
		public int AnimationLength
		{
			get
			{
				return xnaTextures != null ?
					xnaTextures.Length : 1;
			} // get
		} // AnimationLength
		#endregion

		#region Constructor
		/// <summary>
		/// Create animated texture, don't allow empty constructor from outside.
		/// </summary>
		protected AnimatedTexture()
		{
		} // AnimatedTexture()

		/// <summary>
		/// Create animated texture from given filename.
		/// Will go through all filenames with textureFilename + "0001", "0002",
		/// etc. until the filename is not longer found.
		/// </summary>
		/// <param name="setFilename">Set filename, must be relative and
		/// without the 0001.dds, 0002.dds, etc. extension.</param>
		public AnimatedTexture(string setFilename)
		{
			if (String.IsNullOrEmpty(setFilename))
				throw new ArgumentNullException("setFilename",
					"Unable to create texture without valid filename.");

			// Set filename
			texFilename = StringHelper.ExtractFilename(setFilename, true);

			// Try to load texture
			try
			{
				string filenameFirstPart =
					Path.Combine(Directories.ContentDirectory, texFilename);

				// Build full filename
				string fullFilename = filenameFirstPart + "0001";

				// Check if file exists, else we can't continue loading!
				if (File.Exists(fullFilename+".xnb") == false)
					throw new FileNotFoundException(fullFilename + ".xnb");

				// Try loading as 2d texture
				internalXnaTexture = BaseGame.Content.Load<Texture2D>(fullFilename);

				// Get info from the texture directly.
				texWidth = internalXnaTexture.Width;
				texHeight = internalXnaTexture.Height;

				// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
				// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
				hasAlpha =
					internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
					internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
					internalXnaTexture.Format.ToString().StartsWith("A");

				loaded = true;

				CalcHalfPixelSize();

				// Ok, now load all other animated textures
				List<XnaTexture> animatedTextures =
					new List<XnaTexture>();
				animatedTextures.Add(internalXnaTexture);
				int texNumber = 2;
				while (File.Exists(filenameFirstPart +
					texNumber.ToString("0000")+".xnb"))
				{
					animatedTextures.Add(BaseGame.Content.Load<Texture2D>(
						filenameFirstPart + texNumber.ToString("0000")));
					texNumber++;
				} // while (File.Exists)
				xnaTextures = animatedTextures.ToArray();
			} // try
			catch (Exception ex)
			{
				// Failed to load
				loaded = false;
				Log.Write("Failed to load animated texture " + texFilename +
					", will use empty texture! Error: " + ex.ToString());
			} // catch (ex)
		} // AnimatedTexture(name)
		#endregion

		#region Select
		/// <summary>
		/// Select this texture as texture stage 0
		/// </summary>
		/// <param name="animationNumber">Number</param>
		public void Select(int animationNumber)
		{
			if (xnaTextures != null &&
				xnaTextures.Length > 0)
			{
				// Select new animation number
				internalXnaTexture = xnaTextures[animationNumber % xnaTextures.Length];
			} // if
		} // Select(num)
		#endregion

		#region Unit Testing
#if DEBUG
		/*TODO
		/// <summary>
		/// Test textures
		/// </summary>
		public static void TestAnimatedTexture()
		{
			AnimatedTexture explosionTexture = null;
			Sample explosionSample = null;
			GameForm.StartTest("TestAnimatedTexture",
				delegate
				{
					explosionTexture = new AnimatedTexture(
						Directories.TextureEffectsSubDirectory + "\\" +
						"Explosion");
					explosionSample = new Sample("Explosion.wav");
				},
				delegate
				{
					GameForm.DirectXDevice.RenderState.AlphaBlendEnable = true;
					GameForm.DirectXDevice.RenderState.SourceBlend = Blend.SourceAlpha;
					GameForm.DirectXDevice.RenderState.DestinationBlend =
						Blend.InvSourceAlpha;
					explosionTexture.Select(ElapsedTime.MsTotal / 50);
					Size res = GameForm.Resolution;
					int screenSize = Math.Max(res.Width * 2 / 3, res.Height * 2 / 3);
					explosionTexture.RenderOnScreen(new Rectangle(
						res.Width / 2 - screenSize / 2,
						res.Height / 2 - screenSize / 2,
						screenSize, screenSize));

					if (GameForm.Mouse.LeftButtonPressed)
						explosionSample.Play();
				});
		} // TestAnimatedTexture()
		 */
#endif
		#endregion
	} // class AnimatedTexture
} // namespace RocketCommanderXna.Graphics
