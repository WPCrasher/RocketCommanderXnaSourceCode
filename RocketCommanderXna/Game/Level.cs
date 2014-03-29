// Project: RocketCommanderXna, File: Level.cs
// Namespace: RocketCommanderXna.Game, Class: Level
// Path: C:\code\XnaBook\RocketCommanderXna\Game, Author: abi
// Code lines: 377, Size of file: 11,78 KB
// Creation date: 07.12.2006 18:23
// Last modified: 07.12.2006 21:10
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Texture = RocketCommanderXna.Graphics.Texture;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Level
	/// </summary>
	public class Level
	{
		#region Constants
		/// <summary>
		/// We got 5 item types: Fuel, Health, ExtraLife, Speed, Bomb.
		/// </summary>
		public const int NumOfItemTypes = 5,
			FuelItemType = 0,
			HealthItemType = 1,
			ExtraLifeItemType = 2,
			SpeedItemType = 3,
			BombItemType = 4;

		/// <summary>
		/// Colors in the level file for all item types.
		/// Warning: Don't name them Color.Yellow, the Color compare methods
		/// won't work then (they test the name or something, really bad bug IMO).
		/// </summary>
		public static readonly Color
			FuelItemColor = ColorHelper.FromArgb(255, 255, 0),
			HealthItemColor = ColorHelper.FromArgb(0, 255, 0),
			ExtraLifeItemColor = ColorHelper.FromArgb(255, 0, 255),
			SpeedItemColor = ColorHelper.FromArgb(0, 0, 255),
			BombItemColor = ColorHelper.FromArgb(255, 0, 0);

		/// <summary>
		/// Default level width for all levels (20 sectors, everything farther
		/// away is automatically constructed, less asteroids over distance).
		/// For level bitmaps we use 40 pixels width and mix them together.
		/// </summary>
		private const int DefaultLevelWidth = 20;
		#endregion

		#region Variables
		/// <summary>
		/// Level length in z direction and width (width is used for x and y).
		/// </summary>
		int length = 0,
			width = 0;

		/// <summary>
		/// Level name, set when loading level!
		/// </summary>
		string name = "";

		/// <summary>
		/// Level density in a 2d array.
		/// </summary>
		public float[,] density = null;

		/// <summary>
		/// Sun color changes as we move through the level.
		/// </summary>
		public Color[] sunColor = null;

		/// <summary>
		/// Level texture, just used for the minimap in the game.
		/// </summary>
		Texture texture = null;

		/// <summary>
		/// Items in this level, copied over to AsteroidManager when starting
		/// a mission.
		/// </summary>
		public List<Vector3>[] items = new List<Vector3>[]
			{
				// Fuel
				new List<Vector3>(),
				// Health
				new List<Vector3>(),
				// ExtraLife
				new List<Vector3>(),
				// Speed
				new List<Vector3>(),
				// Bomb
				new List<Vector3>(),
			};

		/// <summary>
		/// Helper variable to store which level number this is.
		/// Only used for PreScreenSkyCubeMapping to rotate the background
		/// differently for each level (adds more diversity to the missions).
		/// </summary>
		public static int currentLevelNumber = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Level width
		/// </summary>
		/// <returns>Int</returns>
		public int Width
		{
			get
			{
				return width;
			} // get
		} // Width

		/// <summary>
		/// Level length
		/// </summary>
		/// <returns>Int</returns>
		public int Length
		{
			get
			{
				return length;
			} // get
		} // Length

		/// <summary>
		/// Get level name (without path or extension, just the name)
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return StringHelper.ExtractFilename(name, true);
			} // get
		} // Name

		/// <summary>
		/// Get level texture, just used for the minimap in the game.
		/// </summary>
		/// <returns>Texture</returns>
		public Texture Texture
		{
			get
			{
				return texture;
			} // get
		} // Texture
		#endregion

		#region Generate item position
		/// <summary>
		/// Generate item position randomly inside of a sector position.
		/// </summary>
		/// <param name="xPos">X coordinate</param>
		/// <param name="zPos">Z coordinate</param>
		/// <returns>Vector 3</returns>
		private Vector3 GenerateItemPosition(int xPos, int zPos)
		{
			// First generate sector position (randomize y a bit)
			return new Vector3((xPos - width / 2) * GameAsteroidManager.SectorWidth,
				RandomHelper.GetRandomFloat(
				-GameAsteroidManager.SectorHeight * 1.8f,
				+GameAsteroidManager.SectorHeight * 1.9f),
				GameAsteroidManager.SectorDepth * zPos) +
				// And then add some randomness to it (sector space)
				RandomHelper.GetRandomVector3(
				-GameAsteroidManager.SectorWidth / 2,
				+GameAsteroidManager.SectorWidth / 2);
		} // GenerateItemPosition(xPos, zPos)
		#endregion

		#region Constructor
		/// <summary>
		/// Load level, only allowed internally (by LoadAllLevels)
		/// </summary>
		/// <param name="fullFilename">Full path to level file</param>
		private Level(string fullFilename)
		{
			// Set level name, also used to upload highscore.
			name = StringHelper.ExtractFilename(fullFilename, true);

			// Load level
			//old: Bitmap levelBmp = new Bitmap(fullFilename);
			FileStream file = File.OpenRead("Content\\" +
				StringHelper.ExtractFilename(fullFilename, true) + ".level");
			BinaryReader reader = new BinaryReader(file);
			width = reader.ReadInt32();
			int height = reader.ReadInt32();
			Color[,] levelColors = new Color[width, height];
			for (int x = 0; x < width; x++)
				for (int y = 0; y < height; y++)
				{
					byte r = reader.ReadByte();
					byte g = reader.ReadByte();
					byte b = reader.ReadByte();
					levelColors[x, y] = new Color(r, g, b);
				} // for for
			file.Close();

			// Load texture
			texture = new Texture(//"..\\Levels\\" +
				StringHelper.ExtractFilename(fullFilename, true));

			// Copy over level height as length, but always use 20 as the levelWidth.
			// We will use all pixels at 0-39 and scale them down to 0-19.
			width = DefaultLevelWidth;
			length = height;// levelBmp.Height;

			// Initialize density array, used to generate sectors
			density = new float[width, length];

			// Also load sun colors
			sunColor = new Color[length];

			// Remember last density value in case we hit a item color,
			// this way we can set both the density and put the item into its list.
			float lastDensityValue = 0.0f;

			// Load everything in as 0-1 density values
			for (int y = 0; y < length; y++)
			{
				// Use inverted y position for the level, we start at the bottom.
				int yPos = length - (y + 1);

				// Load level data
				for (int x = 0; x < width; x++)
				{
					// Note: This is slow by using GetPixel, unsafe pointer handling
					// is much faster, but I didn't want to force you using unsafe.
					// In any other project I would use unsafe pointer code.
					// Profiler: This takes around 1 second for 4 levels to load,
					// with unsafe pointers we could optimize that up to 4 times faster.
					//Note: Invert x because our rendering is different from the
					// original RC code, righthanded messes everything up.
					Color loadedColor = levelColors[(width * 2 -1)-(x * 2), y];
					Color loadedColor2 = levelColors[(width * 2 -1)-(x * 2 + 1), y];

					float densityValue = lastDensityValue;

					// Check if it is any item color
					if (ColorHelper.SameColor(loadedColor, FuelItemColor) ||
						ColorHelper.SameColor(loadedColor2, FuelItemColor))
						items[FuelItemType].Add(GenerateItemPosition(x, yPos));
					else if (ColorHelper.SameColor(loadedColor, HealthItemColor) ||
						ColorHelper.SameColor(loadedColor2, HealthItemColor))
						items[HealthItemType].Add(GenerateItemPosition(x, yPos));
					else if (ColorHelper.SameColor(loadedColor, ExtraLifeItemColor) ||
						ColorHelper.SameColor(loadedColor2, ExtraLifeItemColor))
						items[ExtraLifeItemType].Add(GenerateItemPosition(x, yPos));
					else if (ColorHelper.SameColor(loadedColor, SpeedItemColor) ||
						ColorHelper.SameColor(loadedColor2, SpeedItemColor))
						items[SpeedItemType].Add(GenerateItemPosition(x, yPos));
					else if (ColorHelper.SameColor(loadedColor, BombItemColor) ||
						ColorHelper.SameColor(loadedColor2, BombItemColor))
						items[BombItemType].Add(GenerateItemPosition(x, yPos));
					else
						// Just use the red component, should be a gray value anyway.
						densityValue = (float)loadedColor.R / 255.0f;

					// Apply density
					density[x, yPos] = densityValue;
				} // for (int)

				// Load sun color at this level position (just read pixel at x pos 50)
				sunColor[yPos] = levelColors[50, y];
			} // for (int)

			// Finished!
		} // Level(fullFilename)
		#endregion

		#region Load all levels
		/// <summary>
		/// Load all levels.
		/// This method is a great example why .NET 2.0 generics are so great.
		/// We can simply load everything into a dynamic array and return
		/// a fixed array with just one line of code.
		/// </summary>
		public static Level[] LoadAllLevels()
		{
			List<Level> levelList = new List<Level>();

			string[] defaultMaps = new string[]
			{
				Directories.LevelsDirectory + "\\Easy Flight.png",
				Directories.LevelsDirectory + "\\Lost Civilization.png",
				Directories.LevelsDirectory + "\\Valley of Death.png",
				Directories.LevelsDirectory + "\\The Revenge.png",
			};
			levelList.Add(new Level(defaultMaps[0]));
			levelList.Add(new Level(defaultMaps[1]));
			levelList.Add(new Level(defaultMaps[2]));
			levelList.Add(new Level(defaultMaps[3]));

			/*unsupported in xna version, we can only use imported levels!
			// Default maps available?
			if (File.Exists(defaultMaps[0]) &&
				File.Exists(defaultMaps[1]) &&
				File.Exists(defaultMaps[2]) &&
				File.Exists(defaultMaps[3]))
			{
				// Then just load them
				levelList.Add(new Level(defaultMaps[0]));
				levelList.Add(new Level(defaultMaps[1]));
				levelList.Add(new Level(defaultMaps[2]));
				levelList.Add(new Level(defaultMaps[3]));
			} // if (File.Exists)
			else
			{
				// Dynamic loading, only used if one of the default maps is missing
				// Load all filenames
				string[] levelFiles =
					Directory.GetFiles(Directories.LevelsDirectory, "*.png");

				// Build levels
				foreach (string levelFilename in levelFiles)
					levelList.Add(new Level(levelFilename));
			} // else
			*/
			// And return as an array
			return levelList.ToArray();
		} // LoadAllLevels()
		#endregion

		#region Unit Testing
#if DEBUG
		#region Generate level files
#if !XBOX360
		/// <summary>
		/// Generate landscape height file. Usually we would just load the
		/// LandscapeGridHeights.png file and use the data directly. But sadly
		/// the Xbox360 does not support the Texture.GetData method.
		/// </summary>
		public static void GenerateLevelFiles()
		{
			TestGame.Start("GenerateLevelFiles",
				delegate
				{					
					// Ok, load map grid heights. We can't load this as a Bitmap
					// because we don't have the System.Drawing namespace in XNA.
					// We also can't use BitmapContent or PixelBitmapContent<Color> from
					// the Microsoft.XNA.Framework.Content.Pipeline namespace because
					// our content is not compatible with that (its just a texture).
					// Note: Doesn't work on the Xbox360 (no GetData method available)!
					string[] levelNames = new string[]
						{
							"Easy Flight",
							"Lost Civilization",
							"The Revenge",
							"Valley of Death",
						};
					Texture2D[] texs = new Texture2D[]
						{
							Texture2D.FromFile(BaseGame.Device,
								"Levels\\" + levelNames[0]+".png"),
							Texture2D.FromFile(BaseGame.Device,
								"Levels\\" + levelNames[1]+".png"),
							Texture2D.FromFile(BaseGame.Device,
								"Levels\\" + levelNames[2]+".png"),
							Texture2D.FromFile(BaseGame.Device,
								"Levels\\" + levelNames[3]+".png"),
						};

					int levelNum = 0;
					foreach (Texture2D tex in texs)
					{
						int width = tex.Width;
						int height = tex.Height;
						// With help of GetData we can get to the data.
						Color[] texData = new Color[width * height];
						tex.GetData<Color>(texData, 0, width * height);

						// And finally save it into the content directory
						FileStream file = File.Create("Content\\" +
							levelNames[levelNum++] + ".level");
						BinaryWriter writer = new BinaryWriter(file);
						writer.Write(width);
						writer.Write(height);
						for (int x = 0; x < width; x++)
							for (int y = 0; y < height; y++)
							{
								writer.Write(texData[x + y * width].R);
								writer.Write(texData[x + y * width].G);
								writer.Write(texData[x + y * width].B);
							} // for for
						file.Close();
						// That's it already!
					} // foreach (tex)
				},
				null);
		} // GenerateLandscapeHeightFile()
#endif
		#endregion
#endif
		#endregion
	} // class Level
} // namespace RocketCommanderXna.Game
