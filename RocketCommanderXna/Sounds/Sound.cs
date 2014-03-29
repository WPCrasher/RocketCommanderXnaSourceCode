// Project: RocketCommanderXna, File: Sound.cs
// Namespace: RocketCommanderXna.Sounds, Class: Sound
// Path: C:\code\XnaBook\RocketCommanderXna\Sounds, Author: abi
// Code lines: 299, Size of file: 7,32 KB
// Creation date: 07.12.2006 18:22
// Last modified: 07.12.2006 22:29
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Game;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
#endregion

namespace RocketCommanderXna.Sounds
{
	/// <summary>
	/// Sound
	/// </summary>
	class Sound
	{
		#region Variables
		/// <summary>
		/// Sound stuff for XAct
		/// </summary>
		static AudioEngine audioEngine;
		/// <summary>
		/// Wave bank
		/// </summary>
		static WaveBank waveBank1;//, waveBank2;
		/// <summary>
		/// Sound bank
		/// </summary>
		static SoundBank soundBank;
		/// <summary>
		/// Motor category to change volume and pitching of the motor sounds.
		/// </summary>
		static AudioCategory motorCategory;
		    
		/// <summary>
		/// Special category for music, we can only play one music at a time
		/// and to stop we use this category. For play just use play.
		/// </summary>
    static AudioCategory musicCategory;
    
		/// <summary>
		/// Rocket motor sound cue, used for several methods here.
		/// </summary>
		static Cue rocketMotorSound = null;
		#endregion

		#region Enums
		/// <summary>
		/// Sounds we use in this game.
		/// </summary>
		/// <returns>Enum</returns>
		public enum Sounds
		{
			Bomb,
			Click,
			Defeat,
			Explosion,
			ExtraLife,
			Fuel,
			Health,
			Highlight,
			GameMusic,
			MenuMusic,
			RocketMotor,
			SideHit,
			Speed,
			Victory,
			Whosh,
		} // enum Sounds
		#endregion

		#region Constructor
		/// <summary>
		/// Create sound
		/// </summary>
		static Sound()
		{
			try
			{
				string dir = Directories.SoundsDirectory;
				audioEngine = new AudioEngine(
					Path.Combine(dir, "RocketCommanderXna.xgs"));
				waveBank1 = new WaveBank(audioEngine,
					Path.Combine(dir, "Wave Bank.xwb"));
				// Use streaming for music to save memory
				//waveBank2 = new WaveBank(audioEngine,
				//	Path.Combine(dir, "Wave Bank 2.xwb"), 0, 16);

				// Dummy wavebank call to get rid of the warning that waveBank is
				// never used (well it is used, but only inside of XNA).
				if (waveBank1 != null)// &&
					//waveBank2 != null)
					soundBank = new SoundBank(audioEngine,
						Path.Combine(dir, "Sound Bank.xsb"));

				// Get the gears category to change volume and pitching of gear sounds
				motorCategory = audioEngine.GetCategory("Motor");
				rocketMotorSound = soundBank.GetCue("RocketMotor");
			} // try
			catch (Exception ex)
			{
				// Audio creation crashes in early xna versions, log it and ignore it!
				Log.Write("Failed to create sound class: " + ex.ToString());
			} // catch
		} // Sound()
		#endregion

		#region Play
		/// <summary>
		/// Play
		/// </summary>
		/// <param name="soundName">Sound name</param>
		public static void Play(string soundName)
		{
			if (soundBank == null)
				return;

			try
			{
				soundBank.PlayCue(soundName);
			} // try
			catch (Exception ex)
			{
				Log.Write("Playing sound " + soundName + " failed: " + ex.ToString());
			} // catch
		} // Play(soundName)

		/// <summary>
		/// Play
		/// </summary>
		/// <param name="sound">Sound</param>
		public static void Play(Sounds sound)
		{
			Play(sound.ToString());
		} // Play(sound)

		/// <summary>
		/// Play item sound
		/// </summary>
		/// <param name="itemType">Item type</param>
		public static void PlayItemSound(int itemType)
		{
			// See GameAsteroidManager.ItemModelFilenames
			Play(itemType == 0 ? Sounds.Fuel :
				itemType == 1 ? Sounds.Health :
				itemType == 2 ? Sounds.ExtraLife :
				itemType == 3 ? Sounds.Speed :
				Sounds.Bomb);
		} // PlayItemSound(itemType)

		/// <summary>
		/// Play rocket motor sound
		/// </summary>
		/// <param name="volume">Volume</param>
		public static void PlayRocketMotorSound(float volume)
		{
			// Get new cue everytime this is called, else we get Xact throwing this:
			// The method or function called cannot be used in the manner requested.
			rocketMotorSound = soundBank.GetCue(Sounds.RocketMotor.ToString());
			rocketMotorSound.Play();//.PlayLooped();
			motorCategory.SetVolume(0.86f);
		} // PlayRocketMotorSound(volume)

		/// <summary>
		/// Change rocket motor pitch effect
		/// </summary>
		/// <param name="pitchFactor">Pitch factor</param>
		public static void ChangeRocketMotorPitchEffect(float pitchFactor)
		{
			//doesn't work at all, need Audio3DListener, etc. to get this to work!
			//rocketMotorSound.SetVariable("OrientationAngle",
				//MathHelper.Pi / 2.0f);
			//rocketMotorSound.SetVariable("Distance", 1000000);
			rocketMotorSound.SetVariable("Pitch", -10 + 55 * pitchFactor);
		} // ChangeRocketMotorPitchEffect(pitchFactor)

		/// <summary>
		/// Stop rocket motor sound
		/// </summary>
		public static void StopRocketMotorSound()
		{
			rocketMotorSound.Stop(AudioStopOptions.Immediate);
		} // StopRocketMotorSound()

		/// <summary>
		/// Play defeat sound
		/// </summary>
		public static void PlayDefeatSound()
		{
			Play(Sounds.Defeat);
		} // PlayDefeatSound()

		/// <summary>
		/// Play victory sound
		/// </summary>
		public static void PlayVictorySound()
		{
			Play(Sounds.Victory);
		} // PlayVictorySound()

		/// <summary>
		/// Play explosion sound
		/// </summary>
		public static void PlayExplosionSound()
		{
			Play(Sounds.Explosion);
		} // PlayExplosionSound()

		public static void PlayWhosh(float volume)
		{
			//*lags a little!
			// Sound bank must be valid
			if (soundBank == null)
				return;

			// Get whosh cue, will either create cue or reuse existing cue
			Cue newWhoshCue = soundBank.GetCue(Sounds.Whosh.ToString());//"Whosh");
			// Set volume
			newWhoshCue.SetVariable("Volume", volume * 100);
			// And play
			newWhoshCue.Play();
			//*/

			//Play(Sounds.Whosh);
		} // PlayWhosh(volume)
		#endregion

		#region Music
		/// <summary>
		/// Start music
		/// </summary>
		public static void StartMusic()
		{
			Play(RocketCommanderGame.InGame ? Sounds.GameMusic : Sounds.MenuMusic);
		} // StartMusic()

		/// <summary>
		/// Stop music
		/// </summary>
		public static void StopMusic()
		{
			musicCategory.Stop(AudioStopOptions.Immediate);
		} // StopMusic()

		static bool currentMusicMode = true;// false;
		/// <summary>
		/// Current music mode
		/// </summary>
		/// <returns>Bool</returns>
		public static bool CurrentMusicMode
		{
			get
			{
				return currentMusicMode;
			} // get
			set
			{
				if (currentMusicMode != value)
				{
					currentMusicMode = value;
					Play(value ? Sounds.GameMusic : Sounds.MenuMusic);
				} // if (currentMusicMode)
			} // set
		} // CurrentMusicMode
		#endregion

		#region Update
		static bool startMusicPlayback = true;
		/// <summary>
		/// Update, just calls audioEngine.Update!
		/// </summary>
		public static void Update()
		{
			if (audioEngine != null)
				audioEngine.Update();

			if (startMusicPlayback)
			{
				startMusicPlayback = false;
				StartMusic();
			} // if
		} // Update()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test play sounds
		/// </summary>
		//[Test]
		public static void TestPlaySounds()
		{
			//int crazyCounter = 0;

			TestGame.Start(
				delegate
				{
					if (Input.MouseLeftButtonJustPressed ||
						Input.GamePadAJustPressed)
						Sound.Play(Sounds.Bomb);
					else if (Input.MouseRightButtonJustPressed ||
						Input.GamePadBJustPressed)
						Sound.Play(Sounds.Click);
					else if (Input.KeyboardKeyJustPressed(Keys.D1))
						Sound.Play(Sounds.GameMusic);
					else if (Input.KeyboardKeyJustPressed(Keys.D2))
						Sound.Play(Sounds.MenuMusic);
					else if (Input.KeyboardKeyJustPressed(Keys.D3))
						Sound.Play(Sounds.Explosion);
					else if (Input.KeyboardKeyJustPressed(Keys.D4))
						Sound.Play(Sounds.Fuel);
					else if (Input.KeyboardKeyJustPressed(Keys.D5))
						Sound.Play(Sounds.Victory);
					else if (Input.KeyboardKeyJustPressed(Keys.D6))
						Sound.Play(Sounds.Defeat);
					else if (Input.KeyboardKeyJustPressed(Keys.D7))
					{
						Sound.PlayRocketMotorSound(0.75f);
						Sound.ChangeRocketMotorPitchEffect(0.5f);
					} // else if
					else if (Input.KeyboardKeyJustPressed(Keys.D8))
						Sound.StopRocketMotorSound();
					else if (Input.KeyboardKeyJustPressed(Keys.D9))
						Sound.Play(Sounds.Whosh);

					TextureFont.WriteText(2, 30,
						"Press 1-8 or A/B or left/right mouse buttons to play back "+
						"sounds!");
				});
		} // TestPlaySounds()
#endif
		#endregion
	} // class Sound
} // RocketCommanderXna.Sounds
