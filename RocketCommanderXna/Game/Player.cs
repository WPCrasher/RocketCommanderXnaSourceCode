// Project: Rocket Commander, File: Player.cs
// Namespace: RocketCommanderXna.Game, Class: Player
// Path: C:\code\RocketCommanderXna\Game, Author: Abi
// Code lines: 615, Size of file: 17,30 KB
// Creation date: 30.11.2005 03:01
// Last modified: 12.12.2005 08:28
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Sounds;
using RocketCommanderXna.Properties;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.GameScreens;
#endregion

namespace RocketCommanderXna.Game
{
	/// <summary>
	/// Player helper class, holds all the current game properties:
	/// Fuel, Health, Speed, Lifes and Score.
	/// Note: This is a static class and holds always all player entries
	/// for the current game. If we would have more than 1 player (e.g.
	/// in multiplayer mode) this should not be a static class!
	/// </summary>
	static class Player
	{
		#region Global game parameters (game time, game over, explosion, etc.)
		/// <summary>
		/// Current game time in ms. Used for time display in game. Also used to
		/// update the sun position.
		/// </summary>
		public static float gameTimeMs = 0;

		/// <summary>
		/// Current life time in ms. This holds the time how long we currently
		/// live with our rocket. Used to countdown and zooming into the rocket.
		/// </summary>
		public static float lifeTimeMs = 0;

		/// <summary>
		/// If this value is used (greater or at least 0) we have died and are
		/// currently playing the explosion effect and sound. When this timer
		/// reaches 0 we wait until the user clicks for either continue playing
		/// (if lifes are left) or ending the game (if no lifes are left and the
		/// game is over).
		/// </summary>
		public static float explosionTimeoutMs = -1,
			explosionTimeoutMs2 = -1,
			explosionTimeoutMs3 = -1;

		/// <summary>
		/// Time for the explosion effect.
		/// </summary>
		public const int MaxExplosionTimeoutMs = 1500;

		/// <summary>
		/// How long do we zoom in and accelerate at each life.
		/// Zooming in goes on for 3 seconds and
		/// accelerating happens for 2 seconds.
		/// </summary>
		public const int LifeTimeZoomAndAccelerateMs = 5000;

		/// <summary>
		/// Won or lost?
		/// </summary>
		public static bool victory = false;

		/// <summary>
		/// Level name, set when starting game!
		/// </summary>
		public static string levelName = "Easy Flight";

		/// <summary>
		/// Game over?
		/// </summary>
		private static bool gameOver = false;

		/// <summary>
		/// Is game over? True if both all lifes are used and no fuel is left.
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GameOver
		{
			get
			{
				return gameOver;
			} // get
		} // GameOver

		/// <summary>
		/// Remember if we already uploaded our highscore for this game.
		/// Don't do this twice (e.g. when pressing esc).
		/// </summary>
		static bool alreadyUploadedHighscore = false;

		/// <summary>
		/// Set game over and upload highscore
		/// </summary>
		public static void SetGameOverAndUploadHighscore()
		{
			// Set lifes to 0 and set gameOver to true to mark this game as ended.
			lifes = 0;
			gameOver = true;

			// Also set speed to 0, we don't move anymore
			speed = 0;

			// Upload highscore
			if (alreadyUploadedHighscore == false)
			{
				alreadyUploadedHighscore = true;
				Highscores.SubmitHighscore(score, levelName);
			} // if (alreadyUploadedHighscore)
		} // SetGameOverAndUploadHighscore()

		/// <summary>
		/// Helper to determinate if user can control the rocket.
		/// If game or a new life just started we still zoom into the rocket
		/// or if we exploded we can't control the rocket anymore.
		/// </summary>
		/// <returns>Bool</returns>
		public static bool CanControlRocket
		{
			get
			{
				return lifeTimeMs > 3000 &&
					explosionTimeoutMs < 0 &&
					GameOver == false;
			} // get
		} // CanControlRocket
		#endregion

		#region Current player values (fuel, health, speed, etc.)
		/// <summary>
		/// Fuel level, 0 means we are out of fuel, 1 means we are full.
		/// If we are out of fuel we will lose our health until we die or
		/// refill our fuel.
		/// </summary>
		public static float fuel = 1.0f;

		/// <summary>
		/// We have to refill our fuel at least every 40 seconds.
		/// </summary>
		public const float FuelRefillTime = 40.0f;

		/// <summary>
		/// Health, 1 means we have 100% health, everything below means we
		/// are damaged. If we reach 0, we die!
		/// </summary>
		public static float health = 1.0f;

		/// <summary>
		/// It takes 10 seconds without any fuel before we die!
		/// </summary>
		public const float HurtFactorIfFuelIsEmpty = 10.0f;

		/// <summary>
		/// Default number of lifes when the game starts.
		/// </summary>
		private const int DefaultNumberOfLifes = 3;

		/// <summary>
		/// We always start with 3 lifes, if all of them are lost, the game is
		/// over and we return to the main menu.
		/// </summary>
		public static int lifes = DefaultNumberOfLifes;

		/// <summary>
		/// Current score. Used as highscore if game is over.
		/// </summary>
		public static int score = 0;

		/// <summary>
		/// Max. value for camera wobbel timeout.
		/// </summary>
		public const int MaxCameraWobbelTimeoutMs = 700;

		/// <summary>
		/// Camera wobbel timeout.
		/// Used to shake camera after colliding or nearly hitting asteroids.
		/// </summary>
		public static float cameraWobbelTimeoutMs = 0;

		/// <summary>
		/// Camera wobbel factor.
		/// </summary>
		public static float cameraWobbelFactor = 1.0f;

		/// <summary>
		/// Set camera wobbel
		/// </summary>
		/// <param name="factor">Factor</param>
		public static void SetCameraWobbel(float wobbelFactor)
		{
			cameraWobbelTimeoutMs = (int)
				//((0.75f + 0.5f * wobbelFactor) *
				(MaxCameraWobbelTimeoutMs);
			cameraWobbelFactor = wobbelFactor;
		} // SetCameraWobbel(wobbelFactor)

		/// <summary>
		/// Speed values.
		/// </summary>
		public const float DefaultSpeed = 0.5f,
			MinSpeedWithoutItem = 0.5f,//-0.75f,//0.45f,
			MaxSpeedWithoutItem = 0.7f,
			MinSpeedWithItem = 0.7f,
			MaxSpeedWithItem = 1.0f;

		/// <summary>
		/// Current speed of the rocket, can go up to 70% and down to 40%,
		/// the default is 55%. With help of the speed item we can go up to 100%.
		/// </summary>
		private static float speed = DefaultSpeed;

		/// <summary>
		/// Movement speed per second, used in SpaceCamera class.
		/// 500 means we will move up to 5 sectors a second, which is really fast.
		/// </summary>
		public const float MovementSpeedPerSecond = 1111;//1234;//500.0f;// 500.0f;

		/// <summary>
		/// How much sliding movement can we make in percentage of the movement
		/// speed. Sliding works only with left and right, for up/down sliding
		/// we have to rotate the ship.
		/// </summary>
		public const float SlideFactor = 1.0f/5.5f;//1.0f / 6.5f; // / 8.0f;

		/// <summary>
		/// This is how much we can rotate by using mouse by 1 pixel.
		/// This value has to be very low for good and percise rotation control.
		/// Also we don't want the player to freely rotate as much as he wants,
		/// this value limits the ability to move 180 degrees in a short time.
		/// </summary>
		public const float RotationSpeedPerMouseMovement = 0.0015f;

		/// <summary>
		/// Speed
		/// </summary>
		/// <returns>Float</returns>
		public static float Speed
		{
			get
			{
				return speed;
			} // get
			set
			{
				speed = value;

				// If no speed item is active, handle differently
				if (speedItemTimeout == 0)
				{
					// Max. value is 0.75
					if (speed > MaxSpeedWithoutItem)
						speed = MaxSpeedWithoutItem;

					// Min. value is 0.33 (allow lower values if starting new life)
					if (speed < MinSpeedWithoutItem &&
						lifeTimeMs > Player.LifeTimeZoomAndAccelerateMs)
						speed = MinSpeedWithoutItem;
				} // if (speedItemTimeout)
				else
				{
					// Max. value is 1.0
					if (speed > MaxSpeedWithItem)
						speed = MaxSpeedWithItem;

					// Min. value is 0.60
					if (speed < MinSpeedWithItem)
						speed = MinSpeedWithItem;
				} // else
			} // set
		} // Speed

		/// <summary>
		/// Set starting speed
		/// </summary>
		/// <param name="percentage">Percentage</param>
		public static void SetStartingSpeed(float percentage)
		{
			speed = percentage * DefaultSpeed;
		} // SetStartingSpeed(percentage)
		#endregion

		#region Items
		/// <summary>
		/// Speed item timeout. If active this value is greater than 0.
		/// </summary>
		public static float speedItemTimeout = 0;

		/// <summary>
		/// Max speed item stays active for 10 seconds.
		/// </summary>
		public const int MaxSpeedItemTimeout = 10 * 1000;

		/// <summary>
		/// This value holds the number of bomb we are holding.
		/// Will stay active until we collide with any asteroid.
		/// </summary>
		public static int numberOfBombItems = 0;

		/// <summary>
		/// Current item message. Will be displayed after collecting an item.
		/// </summary>
		public static string currentItemMessage = "";
		/// <summary>
		/// Timeout for current item message, fades out if reaching 0.
		/// </summary>
		public static float itemMessageTimeoutMs = 0;
		/// <summary>
		/// Last collected item type for displaying the border color.
		/// </summary>
		public static int lastCollectedItem = 0;

		/// <summary>
		/// Max. time for message timeout
		/// </summary>
		public const int MaxItemMessageTimeoutMs = 6000;

		/// <summary>
		/// Helper to show if health is getting really low!
		/// </summary>
		public static float showHealthWarningTimeoutMs = 0;

		/// <summary>
		/// Set item message
		/// </summary>
		/// <param name="message">Message</param>
		public static void SetItemMessage(string message)
		{
			currentItemMessage = message;
			itemMessageTimeoutMs = MaxItemMessageTimeoutMs;
		} // SetItemMessage(message)

		/// <summary>
		/// Handle item
		/// </summary>
		/// <param name="itemType">Item type</param>
		public static void HandleItem(int itemType)
		{
			lastCollectedItem = itemType;

			// Play item sound
			Sound.PlayItemSound(itemType);

			// Give +2500 score for picking up this item.
			score += 2500;

			// Apply item
			switch (itemType)
			{
				case Level.FuelItemType:
					// Completly refill fuel.
					fuel = 1.0f;
					SetItemMessage(Texts.ItemRefilledFuel);
					break;

				case Level.HealthItemType:
					// Refill 50% health
					health += 0.5f;
					if (health > 1.0f)
						health = 1.0f;
					SetItemMessage(Texts.ItemHealthRefreshed);
					break;

				case Level.SpeedItemType:
					// Speed stays active for 10 seconds
					speedItemTimeout = MaxSpeedItemTimeout;

					// Speed up to the maximum speed
					speed = MaxSpeedWithItem;

					SetItemMessage(Texts.ItemSpeedMode);
					break;

				case Level.BombItemType:
					// Add bomb, we can hold up to 3 bombs
					if (numberOfBombItems < 3)
						numberOfBombItems++;

					SetItemMessage(Texts.ItemBombActive);
					break;

				case Level.ExtraLifeItemType:
					// Increase number of lifes we have
					lifes++;

					SetItemMessage(Texts.ItemExtraLife);
					break;
			} // switch
		} // HandleItem(itemSounds, itemType)
		#endregion

		#region Reset everything for starting a new game
		/// <summary>
		/// Reset all player entries for restarting a game.
		/// </summary>
		/// <param name="setLevelName">Set level name</param>
		public static void Reset(string setLevelName)
		{
			levelName = setLevelName;
			gameOver = false;
			alreadyUploadedHighscore = false;
			gameTimeMs = 0;
			health = 1.0f;
			fuel = 1.0f;
			lifes = DefaultNumberOfLifes;
			score = 0;
			cameraWobbelTimeoutMs = 0;
			explosionTimeoutMs = -1;

			ResetLifeValues();
		} // Reset(setLevelName)
		#endregion

		#region Reset player values if just a life is lost
		/// <summary>
		/// Reset player life values
		/// </summary>
		public static void ResetLifeValues()
		{
			// Reset life time
			lifeTimeMs = 0;

			if (GameOver)
			{
				speed = 0;
			} // if (GameOver)
			else
			{
				// Start with 100% health and fuel
				health = 1.0f;
				fuel = 1.0f;
				speed = DefaultSpeed;
			} // else

			// Deactivate any active items or values
			speedItemTimeout = 0;
			numberOfBombItems = 0;
			itemMessageTimeoutMs = 0;
			showHealthWarningTimeoutMs = 0;
		} // ResetLifeValues()
		#endregion

		#region Handle game logic
		/// <summary>
		/// Handle game logic
		/// </summary>
		/// <param name="asteroidManager">Asteroid manager</param>
		public static void HandleGameLogic(GameAsteroidManager asteroidManager)
		{
			if (gameTimeMs == 0)
			{
				// Start playing rocket motor
				Sound.PlayRocketMotorSound(0.86f);
				//obs: asteroidManager.PlayRocketMotorSound(0.86f);
			} // if (gameTimeMs)

			// Increase explosion effect timeout if used
			if (explosionTimeoutMs >= 0)
			{
				explosionTimeoutMs -= BaseGame.ElapsedTimeThisFrameInMs;
				if (explosionTimeoutMs < 0)
					explosionTimeoutMs = 0;
			} // if (explosionTimeoutMs)
			else if (GameOver == false)
			{
				// Increase game time
				gameTimeMs += BaseGame.ElapsedTimeThisFrameInMs;

				// Same for rocket life time
				lifeTimeMs += BaseGame.ElapsedTimeThisFrameInMs;
			} // else if

			if (explosionTimeoutMs2 >= 0)
			{
				explosionTimeoutMs2 -= BaseGame.ElapsedTimeThisFrameInMs;
				if (explosionTimeoutMs2 < 0)
					explosionTimeoutMs2 = 0;
			} // if (explosionTimeoutMs2)
			if (explosionTimeoutMs3 >= 0)
			{
				explosionTimeoutMs3 -= BaseGame.ElapsedTimeThisFrameInMs;
				if (explosionTimeoutMs3 < 0)
					explosionTimeoutMs3 = 0;
			} // if (explosionTimeoutMs3)

			if (cameraWobbelTimeoutMs > 0)
			{
				cameraWobbelTimeoutMs -= BaseGame.ElapsedTimeThisFrameInMs;
				if (cameraWobbelTimeoutMs < 0)
					cameraWobbelTimeoutMs = 0;
			} // if (cameraWobbelTimeoutMs)

			// Don't handle any more game logic if game is over.
			if (Player.GameOver)
				return;

			float oldHealth = Player.health;

			// Adjust rocket playback frequency to flying speed
			Sound.ChangeRocketMotorPitchEffect(
				-0.24f + speed * 0.6f);
				//0.66f + speed * 0.9f);

			// Check if too near to an asteroid. Check 3x3 sector in middle.
			if (CanControlRocket)
			{
				float playerCollision = asteroidManager.PlayerAsteroidCollision();

				if (playerCollision > 0.0f)
				{
					// Frontal hits might kill us, side hits only hurt
					// (but always at least 10%).
					Player.health -= 0.1f + playerCollision * 4.25f;

					// We shouldn't die on the first hit, even a frontal hit
					// could be survived IF (and only if) we were at 100% health!
					if (oldHealth == 1.0f &&
						Player.health <= 0.0f)
					{
						// Restore to 10% (next hit will kill us!)
						Player.health = 0.1f;
					} // if (oldHealth)
				} // if (playerCollision)
			} // if (CanControlRocket)

			// If we are below 0 fuel, that gonna hurt.
			if (Player.fuel < 0)
			{
				Player.fuel = 0;
				Player.health -= BaseGame.MoveFactorPerSecond /
					Player.HurtFactorIfFuelIsEmpty;
			} // if (Player.fuel)

			// Show health low warning if health is getting very low.
			if (oldHealth >= 0.25f && health < 0.25f ||
				oldHealth >= 0.1f && health < 0.1f)
				showHealthWarningTimeoutMs = 8 * 1000;
			if (showHealthWarningTimeoutMs > 0)
			{
				showHealthWarningTimeoutMs -= BaseGame.ElapsedTimeThisFrameInMs;
				if (showHealthWarningTimeoutMs < 0)
					showHealthWarningTimeoutMs = 0;
			} // if (showHealthWarningTimeoutMs)

			// Die if health is 0 or lower
			if (Player.health <= 0)
			{
				Player.health = 0;

				// Explode!
				Player.explosionTimeoutMs = MaxExplosionTimeoutMs;

				// Reset everything for the player, all items and stuff
				Player.ResetLifeValues();

				// If we have lifes left, reduce them.
				if (Player.lifes > 0)
				{
					Sound.PlayExplosionSound();
					Player.lifes--;
				} // if (Player.lifes)
				else
				{
					victory = false;
					// Play multiple explosions (a little later)
					Player.explosionTimeoutMs2 = (int)(MaxExplosionTimeoutMs * 1.6f);
					Player.explosionTimeoutMs3 = (int)(MaxExplosionTimeoutMs * 2.1f);
					Player.SetGameOverAndUploadHighscore();
					Sound.StopRocketMotorSound();
					Sound.PlayDefeatSound();
				} // else

				// Use minimum possible speed
				Player.Speed = Player.MinSpeedWithoutItem;

				// Kill all asteroids in inner sectors (3x3)
				asteroidManager.KillAllInnerSectorAsteroids();
			} // if (Player.health)

			// Reached target? Then we won!
			if (BaseGame.CameraPos.Z >=
				asteroidManager.CurrentLevel.Length * GameAsteroidManager.SectorDepth)
			{
				victory = true;

				// Add number of lifes left to score (10000 points per life)
				score += lifes * 10000;

				// If player took less than levelLength/2 seconds, add time bonus
				int maxGameTime = (asteroidManager.CurrentLevel.Length / 2) * 1000;
				if (gameTimeMs < maxGameTime)
					// Give 200 points per second, 12000 points per minute
					// E.g. if we took only 4 minutes we get 4 * 12000 extra points =
					// 48000 extra points.
					score += (int)(maxGameTime - gameTimeMs) / 10;

				// And add health and fuel we left to score
				score += (int)(health * 3000);
				score += (int)(fuel * 4000);

				// End game, upload highscore and stuff!
				Player.SetGameOverAndUploadHighscore();
				Sound.StopRocketMotorSound();
				Sound.PlayVictorySound();
			} // if (BaseGame.CameraPos.Z)
		} // HandleGameLogic(asteroidManager)
		#endregion
	} // class Player
} // namespace RocketCommanderXna.Game
