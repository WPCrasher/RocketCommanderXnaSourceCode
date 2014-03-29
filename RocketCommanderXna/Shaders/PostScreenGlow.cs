// Project: Rocket Commander, File: PostScreenGlow.cs
// Namespace: RocketCommanderXna.Shaders, Class: PostScreenGlow
// Path: C:\code\RocketCommanderXna\Shaders, Author: Abi
// Code lines: 618, Size of file: 19,89 KB
// Creation date: 10.11.2005 07:46
// Last modified: 24.12.2005 01:40
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Game;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Texture = RocketCommanderXna.Graphics.Texture;
using Model = RocketCommanderXna.Graphics.Model;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RocketCommanderXna.Shaders
{
	/// <summary>
	/// Post screen glow shader based on PostScreenGlow.fx
	/// </summary>
	public class PostScreenGlow : IGraphicContent
	{
		#region Variables
		/// <summary>
		/// The shader effect filename for this shader.
		/// </summary>
		private const string Filename = "PostScreenGlow";//.fx";

		/*obs
		/// <summary>
		/// Helper texture for the ScreenGlow20 shader.
		/// </summary>
		private const string GlowShaderLookupTexture = "GlowShaderLookup.dds";
		 */

		/// <summary>
		/// Effect
		/// </summary>
		private Effect effect = null;
		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		private EffectParameter windowSize,
			sceneMap,
			downsampleMap,
			blurMap1,
			blurMap2,
			radialSceneMap,
			radialBlurScaleFactor,
			screenBorderFadeoutMap;

		/// <summary>
		/// Links to the passTextures, easier to write code this way.
		/// This are just reference copies.
		/// </summary>
		private RenderToTexture sceneMapTexture,
			downsampleMapTexture,
			blurMap1Texture,
			blurMap2Texture,
			radialSceneMapTexture;

		/// <summary>
		/// Helper texture for the screen border (darken the borders).
		/// </summary>
		private Texture screenBorderFadeoutMapTexture = null;

		/// <summary>
		/// Is this post screen shader started?
		/// Else don't execute Show if it is called.
		/// </summary>
		protected static bool started = false;
		#endregion

		#region Properties
		/// <summary>
		/// Started
		/// </summary>
		/// <returns>Bool</returns>
		public bool Started
		{
			get
			{
				return started;
			} // get
		} // Started

		/// <summary>
		/// Valid
		/// </summary>
		/// <returns>Bool</returns>
		public bool Valid
		{
			get
			{
				return effect != null &&
					BaseGame.UsePS;
			} // get
		} // Valid

		/// <summary>
		/// Set value helper to set an effect parameter.
		/// </summary>
		/// <param name="param">Param</param>
		/// <param name="setMatrix">Set matrix</param>
		private void SetValue(EffectParameter param,
			ref Matrix lastUsedMatrix, Matrix newMatrix)
		{
			/*obs, always update, matrices change every frame anyway!
			 * matrix compare takes too long, it eats up almost 50% of this method.
			if (param != null &&
				lastUsedMatrix != newMatrix)
			 */
			{
				lastUsedMatrix = newMatrix;
				param.SetValue(newMatrix);
			} // if (param)
		} // SetValue(param, setMatrix)

		/// <summary>
		/// Set value helper to set an effect parameter.
		/// </summary>
		/// <param name="param">Param</param>
		/// <param name="lastUsedVector">Last used vector</param>
		/// <param name="newVector">New vector</param>
		private void SetValue(EffectParameter param,
			ref Vector3 lastUsedVector, Vector3 newVector)
		{
			if (param != null &&
				lastUsedVector != newVector)
			{
				lastUsedVector = newVector;
				param.SetValue(newVector);
			} // if (param)
		} // SetValue(param, lastUsedVector, newVector)

		/// <summary>
		/// Set value helper to set an effect parameter.
		/// </summary>
		/// <param name="param">Param</param>
		/// <param name="lastUsedColor">Last used color</param>
		/// <param name="newColor">New color</param>
		private void SetValue(EffectParameter param,
			ref Color lastUsedColor, Color newColor)
		{
			// Note: This check eats few % of the performance, but the color
			// often stays the change (around 50%).
			if (param != null &&
				//slower: lastUsedColor != newColor)
				lastUsedColor.PackedValue != newColor.PackedValue)
			{
				lastUsedColor = newColor;
				//obs: param.SetValue(ColorHelper.ConvertColorToVector4(newColor));
				param.SetValue(newColor.ToVector4());
			} // if (param)
		} // SetValue(param, lastUsedColor, newColor)

		/// <summary>
		/// Set value helper to set an effect parameter.
		/// </summary>
		/// <param name="param">Param</param>
		/// <param name="lastUsedValue">Last used value</param>
		/// <param name="newValue">New value</param>
		private void SetValue(EffectParameter param,
			ref float lastUsedValue, float newValue)
		{
			if (param != null &&
				lastUsedValue != newValue)
			{
				lastUsedValue = newValue;
				param.SetValue(newValue);
			} // if (param)
		} // SetValue(param, lastUsedValue, newValue)

		/// <summary>
		/// Set value helper to set an effect parameter.
		/// </summary>
		/// <param name="param">Param</param>
		/// <param name="lastUsedValue">Last used value</param>
		/// <param name="newValue">New value</param>
		private void SetValue(EffectParameter param,
			ref XnaTexture lastUsedValue, XnaTexture newValue)
		{
			if (param != null &&
				lastUsedValue != newValue)
			{
				lastUsedValue = newValue;
				param.SetValue(newValue);
			} // if (param)
		} // SetValue(param, lastUsedValue, newValue)

		/// <summary>
		/// Last used radial blur scale factor
		/// </summary>
		private float lastUsedRadialBlurScaleFactor = 0;
		/// <summary>
		/// Radial blur scale factor
		/// </summary>
		public float RadialBlurScaleFactor
		{
			set
			{
				SetValue(radialBlurScaleFactor,
					ref lastUsedRadialBlurScaleFactor, value);
			} // set
		} // RadialBlurScaleFactor
		#endregion

		#region Constructor
		/// <summary>
		/// Create post screen glow
		/// </summary>
		public PostScreenGlow()
		{
			Load();

			BaseGame.RegisterGraphicContentObject(this);
		} // PostScreenGlow()
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			// Dispose old shader
			if (effect != null)
			{
				try
				{
					effect.Dispose();
				} // try
				catch (Exception ex)
				{
					Log.Write("Failed to dispose shader " + Filename + ": " +
						ex.ToString());
				} // catch
			} // if (effect)
			effect = null;
		} // Dispose()
		#endregion

		#region Reload
		/// <summary>
		/// Reload
		/// </summary>
		public void Load()
		{
			if (effect != null)
				return;

			// Dispose old shader
			//Dispose();

			// Scene map texture
			sceneMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.FullScreen);
			// Downsample map texture (to 1/4 of the screen)
			downsampleMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.QuarterScreen);

			// Blur map texture
			blurMap1Texture = new RenderToTexture(
				RenderToTexture.SizeType.QuarterScreen);
			// Blur map texture
			blurMap2Texture = new RenderToTexture(
				RenderToTexture.SizeType.QuarterScreen);

			// Final map for glow, used to perform radial blur next step
			radialSceneMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.FullScreen);

			string shaderContentName = Filename;
			// Load shader
			try
			{
				// We have to try, there is no "Exists" method.
				// We could try to check the xnb filename, but why bother? ^^
				effect = BaseGame.Content.Load<Effect>(
					Path.Combine(Directories.ContentDirectory, shaderContentName));
			} // try
#if XBOX360
			catch (Exception ex)
			{
				Log.Write("Failed to load shader "+shaderContentName+". " +
					"Error: " + ex.ToString());
				// Rethrow error, app can't continue!
				throw ex;
			}
#else
			catch
			{
				// Try again by loading by filename (only allowed for windows!)
				// Content file was most likely removed for easier testing :)
				try
				{
					CompiledEffect compiledEffect = Effect.CompileEffectFromFile(
						Path.Combine("Shaders", shaderContentName + ".fx"),
						null, null, CompilerOptions.None,
						TargetPlatform.Windows);

					effect = new Effect(BaseGame.Device,
						compiledEffect.GetEffectCode(), CompilerOptions.None, null);
				} // try
				catch (Exception ex)
				{
					/*obs
					Log.Write("Failed to compile shader, this happend most likely " +
						"because the shader has some syntax error, please check it. " +
						"Error: " + ex.ToString());
					 */
					Log.Write("Failed to load shader " + shaderContentName + ". " +
						"Error: " + ex.ToString());
					// Rethrow error, app can't continue!
					throw ex;
				} // catch
			} // catch
#endif

			// Reset and get all avialable parameters.
			// This is especially important for derived classes.
			ResetParameters();
			GetParameters();
		} // Load()
		#endregion

		#region Reset parameters
		/// <summary>
		/// Reset parameters
		/// </summary>
		protected virtual void ResetParameters()
		{
		} // ResetParameters()
		#endregion

		#region Get parameters
		/// <summary>
		/// Reload
		/// </summary>
		protected void GetParameters()
		{
			// Can't get parameters if loading failed!
			if (effect == null)
				return;

			windowSize = effect.Parameters["windowSize"];
			sceneMap = effect.Parameters["sceneMap"];

			// We need both windowSize and sceneMap.
			if (windowSize == null ||
				sceneMap == null)
				throw new NotSupportedException("windowSize and sceneMap must be " +
					"valid in PostScreenShader=" + Filename);

			// Init additional stuff
			downsampleMap = effect.Parameters["downsampleMap"];
			blurMap1 = effect.Parameters["blurMap1"];
			blurMap2 = effect.Parameters["blurMap2"];
			radialSceneMap = effect.Parameters["radialSceneMap"];

			// Load screen border texture
			screenBorderFadeoutMap = effect.Parameters["screenBorderFadeoutMap"];
			screenBorderFadeoutMapTexture =
				new Texture("ScreenBorderFadeout.dds");
			// Set texture
			screenBorderFadeoutMap.SetValue(
				screenBorderFadeoutMapTexture.XnaTexture);

			// Glow shader lookup texture must exists (used for ps2.0)
			if (File.Exists(Directories.ContentDirectory + "\\GlowShaderLookup.xnb"))
			{
				radialBlurScaleFactor =
					effect.Parameters["radialBlurScaleFactor"];

				// Load GlowShader helper texture, only used for ps_2_0
				EffectParameter glowShaderLookup =
					effect.Parameters["GlowShaderLookup"];
				if (glowShaderLookup != null)
				{
					glowShaderLookup.SetValue(
						new Texture("GlowShaderLookup.dds").XnaTexture);
				} // if (normalizeCubeTexture)
			} // if (Glow shader lookup texture must exists)
		} // GetParameters()
		#endregion

		#region Start
		/// <summary>
		/// Start this post screen shader, will just call SetRenderTarget.
		/// All render calls will now be drawn on the sceneMapTexture.
		/// Make sure you don't reset the RenderTarget until you call Show()!
		/// </summary>
		public void Start()
		{
			// Only apply post screen shader if texture is valid and effect is valid 
			if (sceneMapTexture == null ||
				effect == null ||
				started == true ||
				// Also skip if we don't use post screen shaders at all!
				BaseGame.UsePostScreenShaders == false)
				return;

			RenderToTexture.SetRenderTarget(sceneMapTexture.RenderTarget, true);
			started = true;
		} // Start()
		#endregion

		#region Show
		/// <summary>
		/// Execute shaders and show result on screen, Start(..) must have been
		/// called before and the scene should be rendered to sceneMapTexture.
		/// </summary>
		public void Show()
		{
			// Only apply post screen glow if texture is valid and effect is valid 
			if (sceneMapTexture == null ||
				Valid == false ||
				started == false)
				return;

			started = false;

			// Resolve sceneMapTexture render target for Xbox360 support
			sceneMapTexture.Resolve(true);

			try
			{
				// Don't use or write to the z buffer
				BaseGame.Device.RenderState.DepthBufferEnable = false;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = false;
				// Also don't use any kind of blending.
				BaseGame.Device.RenderState.AlphaBlendEnable = false;
				//unused: BaseGame.Device.RenderState.Lighting = false;

				if (windowSize != null)
					windowSize.SetValue(new float[]
						{ sceneMapTexture.Width, sceneMapTexture.Height });
				if (sceneMap != null)
					sceneMap.SetValue(sceneMapTexture.XnaTexture);
				if (downsampleMap != null)
					downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
				if (blurMap1 != null)
					blurMap1.SetValue(blurMap1Texture.XnaTexture);
				if (blurMap2 != null)
					blurMap2.SetValue(blurMap2Texture.XnaTexture);
				if (radialSceneMap != null)
					radialSceneMap.SetValue(radialSceneMapTexture.XnaTexture);

				RadialBlurScaleFactor = //-0.0125f;
					// Warning: To big values will make the motion blur look to
					// stepy (we see each step and thats not good). -0.02 should be max.
					-(0.005f + Player.Speed * 0.015f);

				if (BaseGame.UsePS20)
					effect.CurrentTechnique = effect.Techniques["ScreenGlow20"];
				else
					effect.CurrentTechnique = effect.Techniques["ScreenGlow"];
				
				// We must have exactly 5 passes!
				if (effect.CurrentTechnique.Passes.Count != 5)
					throw new Exception("This shader should have exactly 5 passes!");

				effect.Begin();//SaveStateMode.None);
				for (int pass = 0; pass < effect.CurrentTechnique.Passes.Count;
					pass++)
				{
					if (pass == 0)
						radialSceneMapTexture.SetRenderTarget();
					else if (pass == 1)
						downsampleMapTexture.SetRenderTarget();
					else if (pass == 2)
						blurMap1Texture.SetRenderTarget();
					else if (pass == 3)
						blurMap2Texture.SetRenderTarget();
					else
						// Do a full reset back to the back buffer
						RenderToTexture.ResetRenderTarget(true);

					EffectPass effectPass = effect.CurrentTechnique.Passes[pass];
					effectPass.Begin();
					// For first effect we use radial blur, draw it with a grid
					// to get cooler results (more blur at borders than in middle).
					if (pass == 0)
						VBScreenHelper.Render10x10Grid();
					else
						VBScreenHelper.Render();
					effectPass.End();
					
					if (pass == 0)
					{
						radialSceneMapTexture.Resolve(false);
						if (radialSceneMap != null)
							radialSceneMap.SetValue(radialSceneMapTexture.XnaTexture);
						effect.CommitChanges();
					} // if
					else if (pass == 1)
					{
						downsampleMapTexture.Resolve(false);
						if (downsampleMap != null)
							downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
						effect.CommitChanges();
					} // if
					else if (pass == 2)
					{
						blurMap1Texture.Resolve(false);
						if (blurMap1 != null)
							blurMap1.SetValue(blurMap1Texture.XnaTexture);
						effect.CommitChanges();
					} // else if
					else if (pass == 3)
					{
						blurMap2Texture.Resolve(false);
						if (blurMap2 != null)
							blurMap2.SetValue(blurMap2Texture.XnaTexture);
						effect.CommitChanges();
					} // else if
				} // for (pass, <, ++)
			} // try
			catch (Exception ex)
			{
				// Make effect invalid, continue rendering without this
				// post screen shader.
				effect = null;
				RenderToTexture.ResetRenderTarget(true);
#if DEBUG
				throw ex;
#else
				Log.Write("Failed to render post screen shader "+Filename+": "+
					ex.ToString());
#endif
			} // catch
			finally
			{
				if (effect != null)
					effect.End();

				// Restore z buffer state
				BaseGame.Device.RenderState.DepthBufferEnable = true;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			} // finally
		} // Show()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test post screen glow
		/// </summary>
		public static void TestPostScreenGlow()
		{
			Model testModel = null;
			PostScreenGlow glowShader = null;

			TestGame.Start("TestPostScreenGlow",
				delegate
				{
					testModel = new Model("Asteroid2");
					glowShader = new PostScreenGlow();
				},
				delegate
				{
					//Thread.Sleep(10);

					glowShader.Start();

					BaseGame.skyCube.RenderSky();

					testModel.Render(Vector3.Zero);
					BaseGame.MeshRenderManager.Render();

					if (Input.Keyboard.IsKeyDown(Keys.LeftAlt) == false &&
						Input.GamePadAPressed == false)
						glowShader.Show();
					else
					{
						// Resolve first
						glowShader.sceneMapTexture.Resolve(false);
						started = false;

						// Reset background buffer
						RenderToTexture.ResetRenderTarget(true);
						// Just show scene map
						glowShader.sceneMapTexture.RenderOnScreen(BaseGame.ResolutionRect);
					} // else

					TextureFont.WriteText(2, 30,
						"Press left alt or A to just show the unchanged screen.");
					TextureFont.WriteText(2, 60,
						"Press space or B to see all menu post screen render passes.");

					//*TODO
					if (Input.Keyboard.IsKeyDown(Keys.Space) ||// == false)
						Input.GamePadBPressed)
					{
						glowShader.sceneMapTexture.RenderOnScreen(
							new Rectangle(10, 10, 256, 256));
						glowShader.downsampleMapTexture.RenderOnScreen(
							new Rectangle(10 + 256 + 10, 10, 256, 256));
						glowShader.blurMap1Texture.RenderOnScreen(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10, 256, 256));
						glowShader.blurMap2Texture.RenderOnScreen(
							new Rectangle(10, 10 + 256 + 10, 256, 256));
					} // if (Input.Keyboard.IsKeyDown)
				});
		} // TestPostScreenGlow()
#endif
		#endregion
	} // class PostScreenGlow
} // namespace RocketCommanderXna.Shaders
