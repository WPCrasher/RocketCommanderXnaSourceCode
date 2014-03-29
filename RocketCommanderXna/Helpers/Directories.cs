// Project: RocketCommanderXna, File: Directories.cs
// Namespace: RocketCommanderXna.Helpers, Class: Directories
// Path: C:\code\XnaBook\RocketCommanderXna\Helpers, Author: abi
// Code lines: 95, Size of file: 2,35 KB
// Creation date: 07.12.2006 18:22
// Last modified: 07.12.2006 21:31
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace RocketCommanderXna.Helpers
{
	/// <summary>
	/// Helper class which stores all used directories.
	/// </summary>
	class Directories
	{
		#region Game base directory
		/// <summary>
		/// We can use this to relocate the whole game directory to another
		/// location. Used for testing (everything is stored on a network drive).
		/// </summary>
		public static readonly string GameBaseDirectory =
			// Update to support Xbox360:
			StorageContainer.TitleLocation;
			//"";
		#endregion

		#region Directories
		/// <summary>
		/// Content directory for all our textures, models and shaders.
		/// </summary>
		/// <returns>String</returns>
		public static string ContentDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Content");
			} // get
		} // ContentDirectory

		/// <summary>
		/// Content extension
		/// </summary>
		/// <returns>String</returns>
		public static string ContentExtension
		{
			get
			{
				return "xnb";
			} // get
		} // ContentExtension

		/// <summary>
		/// Sounds directory, for some reason XAct projects don't produce
		/// any content files (bug?). We just load them ourself!
		/// </summary>
		/// <returns>String</returns>
		public static string SoundsDirectory
		{
			get
			{
				return Path.Combine(ContentDirectory, "Sounds");
			} // get
		} // SoundsDirectory

		/// <summary>
		/// Default Screenshots directory.
		/// </summary>
		/// <returns>String</returns>
		public static string ScreenshotsDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Screenshots");
			} // get
		} // ScreenshotsDirectory

		/// <summary>
		/// Levels directory
		/// </summary>
		/// <returns>String</returns>
		public static string LevelsDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Levels");
			} // get
		} // LevelsDirectory
		#endregion
	} // class Directories
} // namespace RocketCommanderXna.Helpers
