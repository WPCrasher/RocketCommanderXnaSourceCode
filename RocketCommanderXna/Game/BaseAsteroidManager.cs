// Project: Rocket Commander, File: BaseAsteroidManager.cs
// Namespace: RocketCommanderXna.Game, Class: BaseAsteroidManager
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 1551, Size of file: 55,17 KB
// Creation date: 19.01.2006 18:55
// Last modified: 20.01.2006 04:11
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Shaders;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = RocketCommanderXna.Graphics.Texture;
using Model = RocketCommanderXna.Graphics.Model;
using System.Threading;
using System.Diagnostics;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Base asteroid manager.
	/// Handles all the base variables we need for our asteroid field.
	/// Also generates sectors and handles smaller asteroids.
	/// Provides some basic render methods to show the asteroid field.
	/// This class is nothing we can use for the game, it just provides us
	/// the very basic features for the asteroid field.
	/// <para/>
	/// Derived from this is the PhysicsAsteroidManager, which adds all Physics
	/// (handle player collision, handle sector Physics, etc.).
	/// <para/>
	/// And derived from that we finally get to the GameAsteroidManager,
	/// which calculates sector movement, does visibility checks, etc.
	/// The GameAsteroidManager is the class we use in the game, it provides
	/// a much richer Render method and handles also additional effects like
	/// the Hit effects we see on screen.
	/// </summary>
	public class BaseAsteroidManager
	{
		#region Constants
		/// <summary>
		/// Equally distribute all sectors in 3d space (7x7x7 = 343 sectors)
		/// This are only visible sectors. If we are moving new sectors
		/// are added in that direction and all other ones are shifted then.
		/// </summary>
		protected const int NumberOfSectors = 19,//13,//3,//1,
			MiddleSector = NumberOfSectors / 2,
			MinSector = -MiddleSector,
			MaxSector = +MiddleSector;

		/// <summary>
		/// For smaller asteroids we only need a very small inner sector area.
		/// </summary>
		protected const int NumberOfSmallSectors = 7,//5,//7,//5,
			SmallMiddleSector = NumberOfSmallSectors / 2,
			SmallSectorAdd = MiddleSector - SmallMiddleSector,
			MinSmallSector = -SmallMiddleSector,
			MaxSmallSector = +SmallMiddleSector;

		/// <summary>
		/// Sector size is 100*100*100, each component must be at least
		/// MaxAsteroidSize/2.0f for correct collision checking.
		/// For physics we use SectorWidth+MaxAsteroidSize, and asteroids
		/// could be in more than 1 sector for more accurate physics calculations,
		/// but this is to complicated right now, maybe we add that later.
		/// </summary>
		public const float SectorWidth = 200.0f,//25.0f,
			SectorHeight = 200.0f,//25.0f,
			SectorDepth = 200.0f;//25.0f;

		/// <summary>
		/// Minimum and maximum asteroid size.
		/// If this is increased the sector depth may have to be increased as well.
		/// Smaller asteroid sizes look also nice but they don't serve much of
		/// a game purpose and we would need a lot more of them, which would
		/// slow down rendering.
		/// </summary>
		public const float
			MinAsteroidSize = 32.0f,//26.5f,//32.5f,//12.5f,//50.0f,//15.0f,//3.5f,
			MaxAsteroidSize = 62.0f;//52.5f;//25.0f;//50.0f;//30.0f;//10.0f;

		/// <summary>
		/// Smaller asteroid size
		/// </summary>
		public const float SmallAsteroidSize = 129;//169;//140;//192;//62.5f;

		/// <summary>
		/// Max view depth for our camera!
		/// Everything is faded out at FadeOutDepth.
		/// HalfViewDepth is used to switch to faster lod model type.
		/// SmallAsteroidFadeoutDepth is used to fade out small asteroids,
		/// which use HalfViewDepth for the max view range.
		/// </summary>
		public const float MinViewDepth = 0.33f,//1.0f,
			MaxViewDepth = SectorDepth * MiddleSector - SectorDepth / 2.0f,//3.0f,
			FadeOutDepth = MaxViewDepth * 0.75f,//8f,
			HalfViewDepth = MaxViewDepth * 0.4f,
			SmallAsteroidViewDepth = MaxViewDepth * 0.4f,//0.45f,
			SmallAsteroidFadeOutDepth = SmallAsteroidViewDepth * 0.75f;//66f;//0.75f;

		/// <summary>
		/// Viewable field of view, used to check if a sector is visible or not.
		/// We can't use FieldOfView/2 because most sectors at the screen borders
		/// would pop in and out. FieldOfView/1.5 is very good, but some popping
		/// sometimes occur when rotating. For that reason we use FieldOfView/1.4,
		/// which works very good!
		/// </summary>
		/// <returns>Float</returns>
		public static float ViewableFieldOfView
		{
			get
			{
				// In game / 1.4f is fine, but for GameOver screen we need more fov.
				if (Player.GameOver)
					return BaseGame.FieldOfView;
				else
					return BaseGame.FieldOfView * 0.75f;
			} // get
		} // ViewableFieldOfView
		#endregion

		#region Variables
		/// <summary>
		/// Current level, change it with SetLevel(.)
		/// </summary>
		protected internal Level level = null;

		/// <summary>
		/// Current level
		/// </summary>
		/// <returns>Level</returns>
		public virtual Level CurrentLevel
		{
			get
			{
				return level;
			} // get
			set
			{
				level = value;

				// Update target position
				targetPos = new Vector3(0, 0, (level.Length + 1) * SectorDepth);
			} // set
		} // CurrentLevel

		/// <summary>
		/// Helpers to remember target position. Updated in LoadLevel.
		/// </summary>
		static private Vector3 targetPos = new Vector3(0, 0, 10000);

		/// <summary>
		/// High detail asteroid models. All of them are named
		/// "Asteroid1.x", "Asteroid2.x", etc.
		/// If the model files "Asteroid1Low.x", Asteroid2Low.x" do exists,
		/// they will be used as a lower LOD (level of detail) rendering with
		/// the same textures. These lower polygon models only have 200-300
		/// polygons instead of 1200-1300. Rendering is a little bit faster,
		/// when using instancing up to 200% faster. It will be used for
		/// everything farther away than HalfViewDepth.
		/// </summary>
		protected List<Model> asteroidModels = new List<Model>(),
			asteroidModelsLow = new List<Model>();

		/// <summary>
		/// For the near view distance we also add smaller asteroid parts
		/// to increase realism and the velocity feeling.
		/// We won't collide with any of these objects and each model contains
		/// a group of small particles.
		/// </summary>
		protected List<Model> smallAsteroidModels = new List<Model>();

		/// <summary>
		/// Devide all asteroids into sectors to speed up physics, rendering
		/// and all other kind of checks (visibility, collision).
		/// This improves performance by a couple of thousand percent!
		/// If an asteroid sector leaves a sector, we have to remove it
		/// from the old sector and add it to the new sector. This
		/// would involve a lot of checks per frame and be very slow,
		/// to optimize this we only perform this check for highSpeedAsteroids.
		/// 
		/// Note: This wasn't the first approach. First I used a simple 1d list,
		/// then a queue, then a multidimensional (2d) list of queues and finally
		/// this 3 dimensional list of lists. One list is used for each sector.
		/// Still a bit complicated, but required for sector handling!
		/// </summary>
		protected List<Asteroid>[,] sectorAsteroids =
			new List<Asteroid>[NumberOfSectors, NumberOfSectors];

		/// <summary>
		/// Sector small asteroids, only use inner rect of full sector grid.
		/// </summary>
		protected List<SmallAsteroids>[,] sectorSmallerAsteroids =
			new List<SmallAsteroids>[NumberOfSmallSectors, NumberOfSmallSectors];

		/// <summary>
		/// To check if we are moving to another sector with our camera.
		/// </summary>
		private int centerSectorPositionX = 0,
			centerSectorPositionZ = 0;
		
		#region Sector visibility
		/// <summary>
		/// Is a specific sector in range to be visible? This is a boolean
		/// precalculated list. We only use it for the isVisible calculation.
		/// This alone will reduce the number of sectors we have to render by
		/// almost 50%, which is a great performance boost!
		/// </summary>
		protected bool[,] sectorVisibleInRange =
			new bool[NumberOfSectors, NumberOfSectors];

		/// <summary>
		/// Sector direction, used for calculating if this sector is visible 
		/// or not. To do that we get the angel between the camera looking vector
		/// and the sector vector and check if sector is in field of view.
		/// </summary>
		protected Vector3[,] sectorDirection =
			new Vector3[NumberOfSectors, NumberOfSectors];

		/// <summary>
		/// Is sector visible? This is precalculated and only updated when
		/// we rotate or move into a new sector.
		/// This is the biggest performance gain of this whole game, instead
		/// of rendering over 10000 possible asteroids in our view range we
		/// only have to render 500-1000 asteroids and can skip any visibility
		/// test because we only render sectors that are somehow visible anyway.
		/// </summary>
		protected bool[,] sectorIsVisible =
			new bool[NumberOfSectors, NumberOfSectors];
		#endregion

		/// <summary>
		/// Physics thread. Read on my blog (abi.exdream.com) why and how to use it.
		/// </summary>
		Thread physicsThread = null;
		#endregion

		#region Properties
		/// <summary>
		/// Number of asteroid types
		/// </summary>
		/// <returns>Int</returns>
		public int NumberOfAsteroidTypes
		{
			get
			{
				return asteroidModels.Count;
			} // get
		} // NumOfAsteroidTypes

		/// <summary>
		/// Get one of the asteroid models (always high poly version).
		/// </summary>
		/// <param name="num">Number, will be modulated to number of asteroids
		/// we actually have.</param>
		/// <returns>Asteroid model</returns>
		public Model GetAsteroidModel(int num)
		{
			return asteroidModels[num % asteroidModels.Count];
		} // GetAsteroidModel(num)

		/// <summary>
		/// Total number of asteroids. Not really used, just for debugging.
		/// </summary>
		/// <returns>Int</returns>
		public int TotalNumberOfAsteroids
		{
			get
			{
				int num = 0;
				for (int z = 0; z < NumberOfSectors; z++)
					for (int x = 0; x < NumberOfSectors; x++)
						num += sectorAsteroids[z, x].Count;
				return num;
			} // get
		} // TotalNumberOfAsteroids

		/// <summary>
		/// Visible number of asteroids. Not really used, just for debugging.
		/// </summary>
		/// <returns>Int</returns>
		public int VisibleNumberOfAsteroids
		{
			get
			{
				int num = 0;
				for (int z = 0; z < NumberOfSectors; z++)
					for (int x = 0; x < NumberOfSectors; x++)
						if (sectorIsVisible[z, x])
							num += sectorAsteroids[z, x].Count;
				return num;
			} // get
		} // VisibleNumberOfAsteroids

		/// <summary>
		/// Target position
		/// </summary>
		/// <returns>Vector 3</returns>
		static public Vector3 TargetPosition
		{
			get
			{
				return targetPos;
			} // get
		} // TargetPosition
		#endregion

		#region Constructor
		/// <summary>
		/// Create asteroid manager
		/// </summary>
		public BaseAsteroidManager(Level setLevel)
		{
			// Set level
			CurrentLevel = setLevel;

			// Load all available asteroids
			int asteroidNum = 1;
			while (File.Exists(Directories.ContentDirectory + "\\" +
				"Asteroid" + asteroidNum + "." + Directories.ContentExtension))
			{
				Model asteroidModel = new Model("Asteroid" + asteroidNum);
				asteroidModels.Add(asteroidModel);

				// Try to use low version of asteroid
				if (File.Exists(Directories.ContentDirectory + "\\" +
					"Asteroid" + asteroidNum + "Low." + Model.Extension))
					asteroidModelsLow.Add(new Model("Asteroid" + asteroidNum + "Low"));
				else
					// If none is found, use normal one.
					asteroidModelsLow.Add(asteroidModel);

				asteroidNum++;
			} // while (File.Exists)

			if (asteroidModels.Count == 0)
				throw new Exception("Unable to start game, no asteroid models were " +
					"found, please check the Models directory!");

			// Load all small asteroids
			int smallAsteroidNum = 1;
			while (File.Exists(Directories.ContentDirectory + "\\" +
				"SmallAsteroid" + smallAsteroidNum + "." + Directories.ContentExtension))
			{
				smallAsteroidModels.Add(new Model("SmallAsteroid" + smallAsteroidNum));
				smallAsteroidNum++;
			} // while (File.Exists)

			// Create all asteroids
			for (int z = MinSector; z <= MaxSector; z++)
				for (int x = MinSector; x <= MaxSector; x++)
				{
					int iz = z + MiddleSector,
						ix = x + MiddleSector;
					sectorAsteroids[iz, ix] = new List<Asteroid>();

					GenerateSector(sectorAsteroids[iz, ix], x, z);
				} // for for for (int)

			// Create smaller asteroids
			for (int z = MinSmallSector; z <= MaxSmallSector; z++)
				//for (int y = MinSmallSector; y <= MaxSmallSector; y++)
					for (int x = MinSmallSector; x <= MaxSmallSector; x++)
					{
						int iz = z + SmallMiddleSector,
							//iy = y + SmallMiddleSector,
							ix = x + SmallMiddleSector;
						sectorSmallerAsteroids[iz, ix] = new List<SmallAsteroids>();

						GenerateSmallerAsteroidsSector(
							sectorSmallerAsteroids[iz, ix],
							sectorAsteroids[iz + SmallSectorAdd,// iy + SmallSectorAdd,
							ix + SmallSectorAdd].Count, x, z);
					} // for for for (int)

			// Precalculate visible sector stuff
			for (int z = MinSector; z <= MaxSector; z++)
				for (int x = MinSector; x <= MaxSector; x++)
				{
					int iz = z + MiddleSector,
						ix = x + MiddleSector;

					// Check if distance (that sqrt thingy) is smaller than
					// the max view depth (in int this is MiddleSector) and add
					// a small offset (0.25) to include nearly visible sectors.
					sectorVisibleInRange[iz, ix] =
						(float)Math.Sqrt(x * x + z * z) <
						MiddleSector + 0.25f;

					// Calculate direction (just normalize relative position)
					sectorDirection[iz, ix] = -new Vector3(x, 0, z);
					sectorDirection[iz, ix].Normalize();
				} // for for for (int)

			// Calculate sectors and visibility
			CalculateSectors();

			physicsThread = new Thread(new ThreadStart(PhysicsUpdate));
			physicsThread.Start();
		} // BaseAsteroidManager(setLevel)
		#endregion

		#region Generate sector
		/// <summary>
		/// Generate sector
		/// </summary>
		/// <param name="list">List</param>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="z">Z</param>
		private void GenerateSector(List<Asteroid> list, int x, int z)
		{
			// Make sure there are no asteroids in sector 0, 0, 0
			// and the surrounding sectors
			if (Math.Abs(x) < 2 &&
				Math.Abs(z) < 2)
				return;

			// Check out how much asteroids we got in this sector.
			// If this is not a valid sector, use a medium density.
			float density = 0.1f;//0.3f;
			int levelX = x + level.Width / 2;
			int levelZ = z;//+level.Height / 2;
			if (levelX >= 0 && levelX < level.Width &&
				levelZ >= 0 && levelZ < level.Length)
			{
				density += level.density[levelX, levelZ];
			} // if (levelX)

			try
			{
				int numOfAsteroids = RandomHelper.GetRandomInt(
					//twice as much as RC: (int)(2 + density * 5));
					(int)(2 + density * 10));

				for (int num = 0; num < numOfAsteroids; num++)
				{
					//Better idea: predefinded positions (2x2x2 per sector)
					// and randomized use of 1-4 of them
					// Randomize position, but only on certain positions
					int type = RandomHelper.GetRandomInt(asteroidModels.Count);
					list.Add(new Asteroid(type,
						new Vector3(x * SectorWidth,
						// Always add current y position to make it harder excaping
						// from the asteroid field to top/bottom.
						BaseGame.CameraPos.Y +
						RandomHelper.GetRandomFloat(
						//-SectorWidth * 3.25f, +SectorWidth * 3.25f),
						-SectorWidth * 3.15f, +SectorWidth * 3.15f),
						z * SectorDepth) +
						// Keep slow moving asteroids in the middle of sectors,
						// don't let them move out to quickly.
						//obs: RandomHelper.GetRandomVector3(
						//-SectorWidth * 0.25f, +SectorWidth * 0.25f),
						RandomHelper.GetRandomVector3(
						-SectorWidth * 0.42f, +SectorWidth * 0.42f)));
						//asteroidModels[type % asteroidModels.Count].ObjectDownscaledSize));
				} // for (num)
			} // try
			catch { } // catch
		} // GenerateSector(list, x, y)
		#endregion

		#region Generate smaller asteroids sector
		/// <summary>
		/// Generate smaller asteroids sector
		/// </summary>
		/// <param name="list">List</param>
		/// <param name="numOfAsteroids">Number of asteroids in this sector</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="z">Z coordinate</param>
		private void GenerateSmallerAsteroidsSector(
			List<SmallAsteroids> list, int numOfAsteroids, int x, int z)
		{
			// Always create at least 1 smaller asteroid instance per sector.
			int numOfSmallerAsteroids = 2 + RandomHelper.GetRandomInt(
				//old: (int)(2 + numOfAsteroids));
				// More because of only 1 fixed y sector
				//(int)(5 + 3 * numOfAsteroids));
				(int)(4 + numOfAsteroids));

			for (int num = 0; num < numOfSmallerAsteroids; num++)
			{
				int type = RandomHelper.GetRandomInt(smallAsteroidModels.Count);
				list.Add(new SmallAsteroids(type,
					new Vector3(x * SectorWidth, 0, z * SectorDepth) +
					new Vector3(
					RandomHelper.GetRandomFloat(-SectorWidth / 2, +SectorWidth / 2),
					RandomHelper.GetRandomFloat(-SectorWidth * 2.1f, +SectorWidth * 2.1f),
					RandomHelper.GetRandomFloat(-SectorWidth / 2, +SectorWidth / 2))));
					//new Vector3(x * SectorWidth, y * SectorHeight, z * SectorDepth) +
					//RandomHelper.GetRandomVector3(-SectorWidth / 2, +SectorWidth / 2),
					//smallAsteroidModels[type].ObjectDownscaledSize));
			} // for (num)
		} // GenerateSmallerAsteroidsSector(list, numOfAsteroids, x)
		#endregion

		#region Calculate sectors
		/// <summary>
		/// Calculate sectors, all asteroids we need to update if we are moving
		/// and checks the visibility of all sectors.
		/// This method add sectors if moving happend and calls
		/// CalculateSectorVisibility to update the visibility of each sector.
		/// For more help please check out the unit tests below.
		/// </summary>
		private void CalculateSectors()
		{
			// Step 1: If moving into a new sector, copy all sectors over
			// and add a new sector at the side we moving into.
			CalculateSectorMovement();

			// Step 2: Visiblity test for all sectors in range
			//ThreadPool.QueueUserWorkItem(ExecuteSectorVisibilityTest);
			//done in PhysicsUpdate now! ExecuteSectorVisibilityTest(0);
			//obs: CalculateSectorVisibility();
			//checkSectorVisibility = true;
		} // CalculateSectors()
		#endregion

		#region Calculate sector movement
		/// <summary>
		/// Calculate sector movement, will add new sectors at one end if
		/// we move at least by one sector in any direction.
		/// </summary>
		private void CalculateSectorMovement()
		{
			Vector3 cameraPos = BaseGame.CameraPos;
			int cameraSectorPosX = (int)Math.Round(cameraPos.X / SectorWidth);
			//int cameraSectorPosY = (int)Math.Round(cameraPos.Y / SectorHeight);
			int cameraSectorPosZ = (int)Math.Round(cameraPos.Z / SectorDepth);

			// Changed sector?
			if (centerSectorPositionX != cameraSectorPosX ||
				//centerSectorPositionY != cameraSectorPosY ||
				centerSectorPositionZ != cameraSectorPosZ)
			{
				// Move sectors over, check out how much we are moving
				int movedXSectors = cameraSectorPosX - centerSectorPositionX;
				//int movedYSectors = cameraSectorPosY - centerSectorPositionY;
				int movedZSectors = cameraSectorPosZ - centerSectorPositionZ;

				// Helpers to modulate sector position to presever helper lists.
				int modulatedXSectors = movedXSectors;
				//int modulatedYSectors = movedYSectors;
				int modulatedZSectors = movedZSectors;
				if (modulatedXSectors < 0)
					modulatedXSectors += NumberOfSectors +
						(-modulatedXSectors / NumberOfSectors) * NumberOfSectors;
				//if (modulatedYSectors < 0)
				//	modulatedYSectors += NumberOfSectors +
				//		(-modulatedYSectors / NumberOfSectors) * NumberOfSectors;
				if (modulatedZSectors < 0)
					modulatedZSectors += NumberOfSectors +
						(-modulatedZSectors / NumberOfSectors) * NumberOfSectors;

				#region Normal asteroids
				// Helper list for CalculateSectors, which allows us to copy over
				// the whole sectorAsteroids list and add new elements as we need it
				// on any border.
				List<Asteroid>[,] helperCopyAsteroids =
					new List<Asteroid>[NumberOfSectors, NumberOfSectors];

				for (int z = 0; z < NumberOfSectors; z++)
					//for (int y = 0; y < NumberOfSectors; y++)
						for (int x = 0; x < NumberOfSectors; x++)
							helperCopyAsteroids[z, x] = new List<Asteroid>();

				// It is easier to just create a new list and copy everything over
				// that is the same, everything else gets created as we need it.
				// Note: This is complicated ^^ Better paint it on paper to understand
				// this optimized logic. Basically we are just copying all parts
				// that are still used over.
				for (int z = 0; z < NumberOfSectors; z++)
					//for (int y = 0; y < NumberOfSectors; y++)
						for (int x = 0; x < NumberOfSectors; x++)
						{
							// Note: This all might look confusing with this temp variable.
							// The only reason we need it is to preserve the old list from
							// our helperCopyAsteroids list. If we would not do that we would
							// have to create a new array of over 1000 elements every time
							// we call this method, which would be too slow!
							//unused: List<Asteroid> temp = helperCopyAsteroids[z, y, x];

							// Can we copy the sector over from sectorAsteroids
							if (x >= -movedXSectors &&
								//y >= -movedYSectors &&
								z >= -movedZSectors &&
								x < NumberOfSectors - movedXSectors &&
								//y < NumberOfSectors - movedYSectors &&
								z < NumberOfSectors - movedZSectors)
							{
								// Swap lists, we have to do that do not lose the old list
								helperCopyAsteroids[z, x] =
									sectorAsteroids[z + movedZSectors,
										//y + movedYSectors,
										x + movedXSectors];
							} // if (x)
							else
							{
								// Else we have to generate this sector!
								GenerateSector(helperCopyAsteroids[z, x],
									x - MiddleSector + cameraSectorPosX,
									//y - MiddleSector + cameraSectorPosY,
									z - MiddleSector + cameraSectorPosZ);
							} // else
						} // for for for (int)

				// Swap helperCopyAsteroids and sectorAsteroids.
				// We can't just copy helperCopyAsteroids to sectorAsteroids because
				// next time we call MoveSectors helperCopyAsteroids and
				// sectorAsteroids would be the same array and we can't copy over then.
				sectorAsteroids = helperCopyAsteroids;
				#endregion

				#region Smaller asteroids
				// Same stuff for smaller asteroids
				// Helper list for CalculateSectors, which allows us to copy over
				// the whole sectorAsteroids list and add new elements as we need it
				// on any border.
				List<SmallAsteroids>[,] helperCopySmallerAsteroids =
					new List<SmallAsteroids>[NumberOfSmallSectors, NumberOfSmallSectors];

				for (int z = 0; z < NumberOfSmallSectors; z++)
					//for (int y = 0; y < NumberOfSmallSectors; y++)
						for (int x = 0; x < NumberOfSmallSectors; x++)
							helperCopySmallerAsteroids[z, x] = new List<SmallAsteroids>();

				// It is easier to just create a new list and copy everything over
				// that is the same, everything else gets created as we need it.
				for (int z = 0; z < NumberOfSmallSectors; z++)
					//for (int y = 0; y < NumberOfSmallSectors; y++)
						for (int x = 0; x < NumberOfSmallSectors; x++)
						{
							// Can we copy the sector over from sectorAsteroids
							if (x >= -movedXSectors &&
								//y >= -movedYSectors &&
								z >= -movedZSectors &&
								x < NumberOfSmallSectors - movedXSectors &&
								//y < NumberOfSmallSectors - movedYSectors &&
								z < NumberOfSmallSectors - movedZSectors)
							{
								// Swap lists, we have to do that do not lose the old list
								helperCopySmallerAsteroids[z, x] =
									sectorSmallerAsteroids[z + movedZSectors,
										//y + movedYSectors,
										x + movedXSectors];
							} // if (x)
							else
							{
								// Else we have to generate this sector!
								GenerateSmallerAsteroidsSector(
									helperCopySmallerAsteroids[z, x],
									sectorAsteroids[z + SmallSectorAdd, x + SmallSectorAdd].Count,
									x - SmallMiddleSector + cameraSectorPosX,
									//y - SmallMiddleSector + cameraSectorPosY,
									z - SmallMiddleSector + cameraSectorPosZ);
							} // else
						} // for for for (int)

				// Copy helperCopySmallerAsteroids to sectorSmallerAsteroids.
				sectorSmallerAsteroids = helperCopySmallerAsteroids;
				#endregion

				// Update current sector position
				centerSectorPositionX = cameraSectorPosX;
				//centerSectorPositionY = cameraSectorPosY;
				centerSectorPositionZ = cameraSectorPosZ;
			} // if (centerSectorPositionX)
		} // CalculateSectorMovement()
		#endregion

		#region Calculate sector visibility
		/// <summary>
		/// Return angle between two vectors. Used for visbility testing.
		/// </summary>
		/// <param name="vec1">Vector 1</param>
		/// <param name="vec2">Vector 2</param>
		/// <returns>Float</returns>
		public static float GetAngleBetweenVectors(Vector3 vec1, Vector3 vec2)
		{
			// See http://en.wikipedia.org/wiki/Vector_(spatial)
			// for help and check out the Dot Product section ^^
			// Both vectors are normalized so we can save deviding through the
			// lengths.
			return (float)Math.Acos(Vector3.Dot(vec1, vec2));
		} // GetAngleBetweenVectors(vec1, vec2)
		#endregion

		#region Abstract methods, not implemented here, but in derived classes
		/// <summary>
		/// Handle sector physics. Not done here, but in derived class!
		/// </summary>
		protected virtual void HandleSectorPhysics(
			int checkX, //int checkY,
			int checkZ,
			int cameraSectorPosX, //int cameraSectorPosY,
			int cameraSectorPosZ)
		{
			// Implement in derived class!
		} // HandleSectorPhysics(checkX, checkY, checkZ)

		/// <summary>
		/// Show all items
		/// </summary>
		/// <param name="shader">Shader</param>
		/// <param name="objectColor">Object color</param>
		/// <param name="lastUpdatedAlpha">Last updated alpha</param>
		/// <param name="cameraPos">Camera position</param>
		protected virtual void ShowAllItems(//ParallaxShader shader,
			ref Color objectColor, ref float lastUpdatedAlpha,
			ref Vector3 cameraPos)
		{
			// Implement in derived class!
		} // ShowAllItems(shader, objectColor, lastUpdatedAlpha)
		#endregion

		#region Render
		#region RenderUpdate
		//tst: Stopwatch perfCounterUpdate = new Stopwatch();
		/// <summary>
		/// Execute update thread
		/// </summary>
		private void ExecuteUpdateThreadLoop(Object threadContext)
		{
			/*tst
			if (BaseGame.TotalFrames % 100 == 0)
			{
				perfCounterUpdate.Reset();
				perfCounterUpdate.Start();
			} // if
			*/

			#region Physics (works in another thread)
			//ThreadPool.QueueUserWorkItem(PhysicsUpdate);
			//PhysicsUpdate(0);
			letPhysicsThreadUpdate = true;
			#endregion

			#region Initialize and calculate sectors
				// Get current sector we are in.
				Vector3 cameraPos = BaseGame.CameraPos;
				int cameraSectorPosX = (int)Math.Round(cameraPos.X / SectorWidth);
				//int cameraSectorPosY = (int)Math.Round(cameraPos.Y / SectorHeight);
				int cameraSectorPosZ = (int)Math.Round(cameraPos.Z / SectorDepth);

				// Note: This method only eats up 2% of the performance in this method,
				// optimizing would still be good, but I've got better things to do
				// right now. Maybe later. Visibility test could also be improved
				// and give us more speed if we skip more sectors!
				CalculateSectors();

				/*obs
				// Clear all visibleAsteroids
				for (int num = 0; num < NumberOfAsteroidTypes; num++)
					visibleAsteroids[num].Clear();
				 */
				#endregion

			#region Prepare level position and colors
			// Get level position for sun color
			int levelPos = cameraSectorPosZ;
			if (levelPos < 0)
				levelPos = 0;
			if (levelPos >= level.Length)
				levelPos = level.Length - 1;

			// Only use brown color in the beginning.
			Color brownAsteroidColor = Color.White;
			// Update sun color, also for lighting and lens flare!
			Color sunColor = ColorHelper.InterpolateColor(
				//ColorHelper.FromArgb(255, 255, 255),
				brownAsteroidColor,
				level.sunColor[levelPos],
				0.3f);//0.2f);
			BaseGame.SkyBackgroundColor = ColorHelper.InterpolateColor(
				new Color(255, 255, 255),
				level.sunColor[levelPos],
				0.45f);//0.33f);
			BaseGame.LensFlareColor = ColorHelper.InterpolateColor(
				new Color(255, 255, 255),
				level.sunColor[levelPos],
				0.75f);//0.6f);

			// Set color to lighting.
			BaseGame.LightColor = sunColor;

			// Also update sun position!
			Vector3 sunPos = LensFlare.RotateSun(
				(float)Math.PI / 4.0f +
				Player.gameTimeMs / 50000.0f);
				//tst:
				//BaseGame.TotalTimeMs / 5000.0f);
			LensFlare.Origin3D = sunPos;
			BaseGame.LightDirection = -sunPos;

			// Apply sun color for object colors (to 25%)
			Color objectColor = ColorHelper.InterpolateColor(
				brownAsteroidColor,
				sunColor,
				0.33f);//0.2f);
			Color ambientObjectColor =
				ColorHelper.InterpolateColor(
				Material.DefaultAmbientColor,
				level.sunColor[levelPos],
				0.15f);//0.1f);

			float lastUpdatedAlpha = 1.0f;
			#endregion

			#region Render asteroids
			// Note: We don't have to handle the shader and matrices anymore in Xna,
			// the render classes (Model and MeshRenderManager) will handle this
			// for us automatically.

			/*obs
			// Just render everything with the selected shader
			for (int asteroidType = 0; asteroidType < visibleAsteroids.Length;
				asteroidType++)
			{
				//foreach (Asteroid asteroid in visibleAsteroids[asteroidType])
				for (int asteroidNum = 0;
					asteroidNum < visibleAsteroids[asteroidType].Count;  asteroidNum++)
				{
					Asteroid asteroid = visibleAsteroids[asteroidType][asteroidNum];
			 */
			for (int z = 0; z < NumberOfSectors; z++)
				for (int x = 0; x < NumberOfSectors; x++)
					if (sectorIsVisible[z, x])
					{
						// Update rotation and movement for all asteroids
						for (int asteroidNum = 0;
							asteroidNum < sectorAsteroids[z, x].Count; asteroidNum++)
						{
							Asteroid asteroid = sectorAsteroids[z, x][asteroidNum];
							int asteroidType = asteroid.Type;

							// Update rotation and position.
							asteroid.UpdateMovement(BaseGame.MoveFactorPerSecond);

							// Render code now (was seperate in old Rocket Commander):

							// Get distance to viewer
							float distance = (asteroid.position - cameraPos).Length();

							// Skip if out of visible range
							if (distance > MaxViewDepth)
							{
								continue;
							} // if (distance)

							// Choose second lod level at 50% distance!
							if (distance > HalfViewDepth)
							{
								// Alpha by distance! Fade out at FadeOutDepth
								float alpha = 1.0f;
								if (distance > FadeOutDepth)
								{
									alpha = 1.0f -
										((distance - FadeOutDepth) /
										(MaxViewDepth - FadeOutDepth));
								} // if (distance)

								asteroidModelsLow[asteroidType].Render(
									asteroid.RenderMatrix, alpha);
							} // if
							else
								asteroidModels[asteroidType].Render(
									asteroid.RenderMatrix);
						} // foreach (asteroid)
					} // for (asteroidType)

			// Show all items with shader
			ShowAllItems(ref objectColor, ref lastUpdatedAlpha, ref cameraPos);
			#endregion

			#region Small asteroids
			// Go through all visible smaller asteroids
			for (int z = SmallSectorAdd;
				z < SmallSectorAdd + NumberOfSmallSectors; z++)
				//for (int y = SmallSectorAdd;
				//	y < SmallSectorAdd + NumberOfSmallSectors; y++)
				for (int x = SmallSectorAdd;
					x < SmallSectorAdd + NumberOfSmallSectors; x++)
					if (sectorIsVisible[z, x])
					{
						List<SmallAsteroids> list = sectorSmallerAsteroids[
							z - SmallSectorAdd, x - SmallSectorAdd];
						for (int num = 0; num < list.Count; num++)
						{
							SmallAsteroids smallAsteroids = list[num];

							// Get distance to viewer
							float distance =
								(smallAsteroids.Position - cameraPos).Length();

							// Skip if out of visible range
							if (distance > SmallAsteroidViewDepth)
								continue;

							// Alpha by distance! Fade out at SmallAsteroidFadeOutDepth
							float alpha = 1.0f;

							if (distance > SmallAsteroidFadeOutDepth)
							{
								alpha = 1.0f -
									((distance - SmallAsteroidFadeOutDepth) /
									(SmallAsteroidViewDepth - SmallAsteroidFadeOutDepth));
							} // if (distance)

							smallAsteroidModels[smallAsteroids.Type].Render(
								smallAsteroids.RenderMatrix, alpha);
						} // for (smallAsteroids)
					} // for for for if (sectorIsVisible[z,)
			#endregion

			/*tst
			if (BaseGame.TotalFrames % 100 == 0)
			{
				perfCounterUpdate.Stop();
				updateThreadMs =
					perfCounterUpdate.ElapsedMilliseconds;
				updateThreadTicks =
					perfCounterUpdate.ElapsedTicks;
			} // if
			 */
		} // ExecuteUpdateThreadLoop()

		/*perf test code
		public static long updateThreadMs = 0;
		public static long updateThreadTicks = 0;
		 */
		#endregion

		#region PhysicsUpdate
		//Stopwatch perfCounterPhysics = new Stopwatch();
		bool letPhysicsThreadUpdate = false;
		/// <summary>
		/// PhysicsUpdate
		/// </summary>
		private void PhysicsUpdate()//Object threadContext)
		{
			/*
			if (BaseGame.TotalFrames % 100 == 0)
			{
				perfCounterPhysics.Reset();
				perfCounterPhysics.Start();
			} // if
			 */

			do
			{
				// Wait if we are not supposed to update physics now.
				while (letPhysicsThreadUpdate == false &&
					BaseGame.Quit == false)
					Thread.Sleep(10);
				letPhysicsThreadUpdate = false;

				#region Sector visibility update
				Vector3 cameraRotation = BaseGame.CameraRotation;

				for (int z = MinSector; z <= MaxSector; z++)
					for (int x = MinSector; x <= MaxSector; x++)
					{
						int iz = z + MiddleSector,
							ix = x + MiddleSector;

						// Pre check by distance, sector must be in range.
						bool isVisible = sectorVisibleInRange[iz, ix];

						// Only continue checking if sector is visible at all.
						if (isVisible &&
							// Allow all if looking up or down now!
							Math.Abs(cameraRotation.Y) < 0.75f)//0.85f)
						{
							// Half field of view should be fov / 2, but because of
							// the aspect ratio (1.33) and an additional offset we need
							// to include to see asteroids at the borders.
							isVisible = Vector3Helper.GetAngleBetweenVectors(
								cameraRotation, sectorDirection[iz, ix]) <
								ViewableFieldOfView;
						} // if (isVisible)

						// Always show inner 4x4 (3x3 center+4 edges) sectors
						if (Math.Abs(x) + Math.Abs(z) <= 2)//3)//2)
							isVisible = true;

						// Assign our result
						sectorIsVisible[iz, ix] = isVisible;
					} // for for for (int)
				#endregion

				try
				{
				#region Physics
				// Get current sector we are in.
				Vector3 cameraPos = BaseGame.CameraPos;
				int cameraSectorPosX = (int)Math.Round(cameraPos.X / SectorWidth);
				int cameraSectorPosZ = (int)Math.Round(cameraPos.Z / SectorDepth);

				for (int z = 0; z < NumberOfSectors; z++)
					for (int x = 0; x < NumberOfSectors; x++)
						//obs, slower now by checking all sectors, but thats ok:
						if (sectorIsVisible[z, x])
						{
							// Warning: Physics are REALLY slow because we check everything
							// with everything and it involves a lot of math.
							// Only perform physics when we really need it
							// and only apply physics inside of a sector (which might change
							// later in this loop when asteroids are moving outside).
							// See HandleSectorPhysics for more details.

							// Handle sector physics (only calculate 50% per frame).
							//obs, also slower now, checking 100% instead of 50%:
							//if ((z + x) % 2 == BaseGame.TotalFrames % 2)
							// Just update all, next physics thread can start later
							HandleSectorPhysics(x, z,
								cameraSectorPosX,
								cameraSectorPosZ);
						} // for for for if (sectorIsVisible[z,x])
				#endregion
				} // try
#if DEBUG
				catch (Exception ex)
				{
					Log.Write("PhysicsUpdate failed: " +
						ex.ToString());
				} // catch
#else
				catch
				{
					// Just ignore, can happen if sector stuff is changed while in physics
					// update.
				} // catch
#endif
			} while (BaseGame.Quit == false);

			/*
			if (BaseGame.TotalFrames % 100 == 0)
			{
				perfCounterPhysics.Stop();
				physicsThreadMs =
					perfCounterPhysics.ElapsedMilliseconds;
				physicsThreadTicks =
					perfCounterPhysics.ElapsedTicks;
			} // if
			 */
		} // PhysicsUpdate()

		/*tst
		public static long physicsThreadMs = 0;
		public static long physicsThreadTicks = 0;
		 */
		#endregion

		#region Render
		/// <summary>
		/// Render, most performance critical method of the game.
		/// Renders just all asteroids, everything else (physics, items, etc.)
		/// is done in the derived classes.
		/// We use (shader) instancing to speed up asteroid rendering!
		/// </summary>
		public void RenderAsteroids()
		{
			// Make sure we start the mesh render manager for this frame.
			// It will copy all models from last frame over to this frame.
			// Render all models from last frame, this way we can add
			// new models this frame using multithreading!
			BaseGame.MeshRenderManager.Render();

			ExecuteUpdateThreadLoop(0);
		} // Render()
		#endregion
		#endregion

		#region Unit Testing
#if DEBUG
		#region Test rendering asteroids
		/// <summary>
		/// Test rendering asteroids for Tutorial 2.
		/// We want just to display a bunch of asteroids in 3d space.
		/// </summary>
		public static void TestRenderingAsteroids()
		{
			List<Asteroid> heinoAsteroids = new List<Asteroid>();
			heinoAsteroids.Add(new Asteroid(
				4, Vector3.Zero));//, 1.0f));
			heinoAsteroids.Add(new Asteroid(
				4, new Vector3(100, 0, 0)));//, 3.0f));
			heinoAsteroids.Add(new Asteroid(
				4, new Vector3(0, 100, 0)));//, 0.5f));
			Model asteroidModel = null;

			TestGame.Start("Test rendering asteroids",
				delegate
				{
					asteroidModel = new Model("Asteroid5.x");
				},
				delegate
				{
					foreach (Asteroid asteroid in heinoAsteroids)
						asteroidModel.Render(asteroid.RenderMatrix);
				});
		} // TestRenderingAsteroids()
		#endregion

		#region Test asteroid manager
		/// <summary>
		/// Basically test the BaseAsteroidManager class.
		/// </summary>
		public static void TestAsteroidManager()
		{
			//already done in TestGame:
			BaseAsteroidManager asteroidManager = null;
			Model testModel = null;

			TestGame.Start("Test asteroid manager",
				delegate
				{
					asteroidManager = new BaseAsteroidManager(
						Level.LoadAllLevels()[0]);
					testModel = new Model("asteroid5");
				},
				delegate
				{
					BaseGame.GlowShader.Start();

					// Render sky cube map as our background.
					BaseGame.skyCube.RenderSky(1.0f, BaseGame.SkyBackgroundColor);

					//testModel.Render(Matrix.CreateScale(50),
					//	1.0f - BaseGame.CameraPos.Length() / 500);
					asteroidManager.RenderAsteroids();

					/*tst
					BaseGame.DrawLine(Vector3.Zero, new Vector3(1000, 0, 0), Color.Red);
					BaseGame.DrawLine(Vector3.Zero, new Vector3(0, 0, 1000), Color.Green);

					TextureFont.WriteText(2, 30, "Camera pos=" + BaseGame.CameraPos);
					TextureFont.WriteText(2, 60, "FarPlane=" + BaseGame.FarPlane);
					TextureFont.WriteText(2, 90, "Distance=" + BaseGame.CameraPos.Length());
					TextureFont.WriteText(2, 120, "HalfViewDepth=" + HalfViewDepth);
					TextureFont.WriteText(2, 150, "NumberOfSectors=" +
						BaseAsteroidManager.NumberOfSectors);

					for (int x=0; x<19; x++)
						for (int z=0; z<19; z++)
							TextureFont.WriteText(300+x*15, 100+z*20,
								asteroidManager.sectorIsVisible[x, z] ? "*":".");
					//*/
				});
		} // TestAsteroidManager()
		#endregion
#endif
		#endregion
	} // class BaseAsteroidManager
} // namespace RocketCommanderXna.Game
