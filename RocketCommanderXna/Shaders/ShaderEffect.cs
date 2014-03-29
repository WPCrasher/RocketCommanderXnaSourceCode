// Project: RocketCommanderXna, File: ShaderEffect.cs
// Namespace: RocketCommanderXna.Shaders, Class: ShaderEffect
// Path: C:\code\RocketCommanderXna\Shaders, Author: Abi
// Code lines: 904, Size of file: 24,94 KB
// Creation date: 07.09.2006 05:56
// Last modified: 05.11.2006 00:32
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using Texture = RocketCommanderXna.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture;
using RocketCommanderXna.Game;
#endregion

namespace RocketCommanderXna.Shaders
{
	/// <summary>
	/// Shader effect class. You can either directly use this class by
	/// providing a fx filename in the constructor or derive from this class
	/// for special shader functionality (see post screen shaders for a more
	/// complex example).
	/// </summary>
	public class ShaderEffect : IGraphicContent
	{
		#region Some shaders
		/// <summary>
		/// Line rendering shader
		/// </summary>
		public static ShaderEffect lineRendering =
			new ShaderEffect("LineRendering.fx");
		#endregion

		#region Variables
		/// <summary>
		/// Content name for this shader
		/// </summary>
		private string shaderContentName = "";

		/// <summary>
		/// Effect
		/// </summary>
		protected Effect effect = null;
		/// <summary>
		/// Effect handles for shaders.
		/// </summary>
		protected EffectParameter worldViewProj,
			viewProj,
			world,
			viewInverse,
			projection,
			lightDir,
			ambientColor,
			diffuseColor,
			specularColor,
			specularPower,
			alphaFactor,
			scale,
			diffuseTexture,
			normalTexture,
			heightTexture,
			reflectionCubeTexture,
			detailTexture,
			parallaxAmount;
		#endregion

		#region Properties
		/// <summary>
		/// Is this shader valid to render? If not we can't perform any rendering.
		/// </summary>
		/// <returns>Bool</returns>
		public bool Valid
		{
			get
			{
				return effect != null;
			} // get
		} // Valid

		/// <summary>
		/// Effect
		/// </summary>
		/// <returns>Effect</returns>
		public Effect Effect
		{
			get
			{
				return effect;
			} // get
		} // Effect

		/// <summary>
		/// Number of techniques
		/// </summary>
		/// <returns>Int</returns>
		public int NumberOfTechniques
		{
			get
			{
				return effect.Techniques.Count;
			} // get
		} // NumberOfTechniques

		/// <summary>
		/// Get technique
		/// </summary>
		/// <param name="techniqueName">Technique name</param>
		/// <returns>Effect technique</returns>
		public EffectTechnique GetTechnique(string techniqueName)
		{
			return effect.Techniques[techniqueName];
		} // GetTechnique(techniqueName)

		/// <summary>
		/// World parameter
		/// </summary>
		/// <returns>Effect parameter</returns>
		public EffectParameter WorldParameter
		{
			get
			{
				return world;
			} // get
		} // WorldParameter
		
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

		protected Matrix lastUsedWorldViewProjMatrix = Matrix.Identity;
		/// <summary>
		/// Set world view proj matrix
		/// </summary>
		protected Matrix WorldViewProjMatrix
		{
			set
			{
				SetValue(worldViewProj, ref lastUsedWorldViewProjMatrix, value);
			} // set
		} // WorldViewProjMatrix

		protected Matrix lastUsedViewProjMatrix = Matrix.Identity;
		/// <summary>
		/// Set view proj matrix
		/// </summary>
		protected Matrix ViewProjMatrix
		{
			set
			{
				SetValue(viewProj, ref lastUsedViewProjMatrix, value);
			} // set
		} // ViewProjMatrix

		//obs: protected Matrix lastUsedWorldMatrix = Matrix.Identity;
		/// <summary>
		/// Set world matrix
		/// </summary>
		public Matrix WorldMatrix
		{
			set
			{
				// This is the most used property here.
				//obs: SetValue(world, ref lastUsedWorldMatrix, value);
				/*obs, world matrix ALWAYS changes! and it is always used!
				if (world != null &&
					lastUsedWorldMatrix != value)
				{
					lastUsedWorldMatrix = value;
					world.SetValue(lastUsedWorldMatrix);
				} // if (world)
				 */

				// Faster, we checked world matrix in constructor.
				world.SetValue(value);
			} // set
		} // WorldMatrix

		protected Matrix lastUsedInverseViewMatrix = Matrix.Identity;
		/// <summary>
		/// Set view inverse matrix
		/// </summary>
		protected Matrix InverseViewMatrix
		{
			set
			{
				SetValue(viewInverse, ref lastUsedInverseViewMatrix, value);
			} // set
		} // InverseViewMatrix

		protected Matrix lastUsedProjectionMatrix = Matrix.Identity;
		/// <summary>
		/// Set projection matrix
		/// </summary>
		protected Matrix ProjectionMatrix
		{
			set
			{
				SetValue(projection, ref lastUsedProjectionMatrix, value);
			} // set
		} // ProjectionMatrix

		protected Vector3 lastUsedLightDir = Vector3.Zero;
		/// <summary>
		/// Set light direction
		/// </summary>
		protected Vector3 LightDir
		{
			set
			{
				// Make sure lightDir is normalized (fx files are optimized
				// to work with a normalized lightDir vector)
				value.Normalize();
				// Set negative value, shader is optimized not to negate dir!
				SetValue(lightDir, ref lastUsedLightDir, -value);
			} // set
		} // LightDir

		protected Color lastUsedAmbientColor = ColorHelper.Empty;
		/// <summary>
		/// Ambient color
		/// </summary>
		public Color AmbientColor
		{
			set
			{
				SetValue(ambientColor, ref lastUsedAmbientColor, value);
			} // set
		} // AmbientColor

		protected Color lastUsedDiffuseColor = ColorHelper.Empty;
		/// <summary>
		/// Diffuse color
		/// </summary>
		public Color DiffuseColor
		{
			set
			{
				SetValue(diffuseColor, ref lastUsedDiffuseColor, value);
			} // set
		} // DiffuseColor

		protected Color lastUsedSpecularColor = ColorHelper.Empty;
		/// <summary>
		/// Specular color
		/// </summary>
		public Color SpecularColor
		{
			set
			{
				SetValue(specularColor, ref lastUsedSpecularColor, value);
			} // set
		} // SpecularColor

		private float lastUsedSpecularPower = 0;
		/// <summary>
		/// SpecularPower for specular color
		/// </summary>
		public float SpecularPower
		{
			set
			{
				SetValue(specularPower, ref lastUsedSpecularPower, value);
			} // set
		} // SpecularPower

		private float lastUsedAlphaFactor = 0;
		/// <summary>
		/// Alpha factor
		/// </summary>
		/// <returns>Float</returns>
		public float AlphaFactor
		{
			set
			{
				SetValue(alphaFactor, ref lastUsedAlphaFactor, value);
			} // set
		} // AlphaFactor

		protected float lastUsedScale = 1.0f;
		/// <summary>
		/// Subsurface color for subsurface shaders.
		/// </summary>
		public float Scale
		{
			set
			{
				SetValue(scale, ref lastUsedScale, value);
			} // set
		} // Scale

		protected XnaTexture lastUsedDiffuseTexture = null;
		/// <summary>
		/// Set diffuse texture
		/// </summary>
		public Texture DiffuseTexture
		{
			set
			{
				SetValue(diffuseTexture, ref lastUsedDiffuseTexture,
					value != null ? value.XnaTexture : null);
			} // set
		} // DiffuseTexture

		protected XnaTexture lastUsedNormalTexture = null;
		/// <summary>
		/// Set normal texture for normal mapping
		/// </summary>
		public Texture NormalTexture
		{
			set
			{
				SetValue(normalTexture, ref lastUsedNormalTexture,
					value != null ? value.XnaTexture : null);
			} // set
		} // NormalTexture

		protected XnaTexture lastUsedHeightTexture = null;
		/// <summary>
		/// Set height texture for parallax mapping
		/// </summary>
		public Texture HeightTexture
		{
			set
			{
				SetValue(heightTexture, ref lastUsedHeightTexture,
					value != null ? value.XnaTexture : null);
			} // set
		} // HeightTexture

		protected TextureCube lastUsedReflectionCubeTexture = null;
		/// <summary>
		/// Set reflection cube map texture for reflection stuff.
		/// </summary>
		public TextureCube ReflectionCubeTexture
		{
			get
			{
				return lastUsedReflectionCubeTexture;
			} // get
			set
			{
				if (reflectionCubeTexture != null &&
					lastUsedReflectionCubeTexture != value)
				{
					lastUsedReflectionCubeTexture = value;
					reflectionCubeTexture.SetValue(value);
				} // if (effect.D3DEffect)
			} // set
		} // ReflectionCubeTexture

		protected XnaTexture lastUsedDetailTexture = null;
		/// <summary>
		/// Set height texture for parallax mapping
		/// </summary>
		public Texture DetailTexture
		{
			set
			{
				SetValue(detailTexture, ref lastUsedDetailTexture,
					value != null ? value.XnaTexture : null);
			} // set
		} // DetailTexture

		protected float lastUsedParallaxAmount = -1.0f;
		/// <summary>
		/// Parallax amount for parallax and offset shaders.
		/// </summary>
		public float ParallaxAmount
		{
			set
			{
				SetValue(parallaxAmount, ref lastUsedParallaxAmount, value);
			} // set
		} // ParallaxAmount
		#endregion

		#region Constructor
		public ShaderEffect(string shaderName)
		{
			if (BaseGame.Device == null)
				throw new NullReferenceException(
					"XNA device is not initialized, can't create ShaderEffect.");

			shaderContentName = StringHelper.ExtractFilename(shaderName, true);

			Load();

			BaseGame.RegisterGraphicContentObject(this);
		} // SimpleShader()
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public virtual void Dispose()
		{
			// Dispose shader effect
			if (effect != null)
				effect.Dispose();
			effect = null;
		} // Dispose()
		#endregion

		#region Reload effect
		/// <summary>
		/// Reload effect (can be useful if we change the fx file dynamically).
		/// </summary>
		public void Load()
		{
			if (effect != null)
				return;

			/*obs
			// Dispose old shader
			if (effect != null)
				Dispose();
			 */

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
					Log.Write("Failed to load shader "+shaderContentName+". " +
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
		} // Reload()
		#endregion

		#region Reset parameters
		/// <summary>
		/// Reset parameters
		/// </summary>
		protected virtual void ResetParameters()
		{
			lastUsedInverseViewMatrix = Matrix.Identity;
			lastUsedScale = 0.0f;
			lastUsedAmbientColor = ColorHelper.Empty;
			lastUsedDiffuseTexture = null;
		} // ResetParameters()
		#endregion

		#region Get parameters
		/// <summary>
		/// Get parameters, override to support more
		/// </summary>
		protected virtual void GetParameters()
		{
			worldViewProj = effect.Parameters["worldViewProj"];
			viewProj = effect.Parameters["viewProj"];
			world = effect.Parameters["world"];
			viewInverse = effect.Parameters["viewInverse"];
			projection = effect.Parameters["projection"];
			lightDir = effect.Parameters["lightDir"];
			ambientColor = effect.Parameters["ambientColor"];
			diffuseColor = effect.Parameters["diffuseColor"];
			specularColor = effect.Parameters["specularColor"];
			specularPower = effect.Parameters["specularPower"];
			alphaFactor = effect.Parameters["alphaFactor"];
			// Default alpha factor to 1.0f for hotels and stuff
			AlphaFactor = 1.0f;
			scale = effect.Parameters["scale"];
			diffuseTexture = effect.Parameters["diffuseTexture"];
			normalTexture = effect.Parameters["normalTexture"];
			heightTexture = effect.Parameters["heightTexture"];
			reflectionCubeTexture = effect.Parameters["reflectionCubeTexture"];
			detailTexture = effect.Parameters["detailTexture"];
			//obs: parallaxAmount = effect.Parameters["parallaxAmount"];

			// Have to use the ps11 versions?
			if (BaseGame.CanUsePS20 == false)
			{
				// Then load and set the NormalizeCubeTexture helper texture.
				EffectParameter normalizeCubeTexture =
					effect.Parameters["NormalizeCubeTexture"];
				// Only set if this parameter exists
				if (normalizeCubeTexture != null)
				{
					normalizeCubeTexture.SetValue(
						BaseGame.Content.Load<TextureCube>(
						// Use content name we already have through the models
						Path.Combine(Directories.ContentDirectory, "NormalizeCubeMap~0")));
				} // if (normalizeCubeTexture)
			} // if (BaseGame.CanUsePS20)
		} // GetParameters()
		#endregion

		#region SetParameters
		/// <summary>
		/// Set parameters, this overload sets all material parameters too.
		/// </summary>
		public virtual void SetParameters(Material setMat)
		{
			if (worldViewProj != null)
				worldViewProj.SetValue(BaseGame.WorldViewProjectionMatrix);
			if (viewProj != null)
				viewProj.SetValue(BaseGame.ViewProjectionMatrix);
			if (world != null)
				world.SetValue(BaseGame.WorldMatrix);
			if (viewInverse != null)
				viewInverse.SetValue(BaseGame.InverseViewMatrix);
			if (lightDir != null)
				lightDir.SetValue(BaseGame.LightDirection);

			/*TODO?
			// Set the reflection cube texture only once
			if (lastUsedReflectionCubeTexture == null &&
				reflectionCubeTexture != null)
			{
				ReflectionCubeTexture = TextureFont.SkyCubeMapTexture;
			} // if (lastUsedReflectionCubeTexture)
			 */

			// Set all material properties
			if (setMat != null)
			{
				AmbientColor = setMat.ambientColor;
				DiffuseColor = setMat.diffuseColor;
				SpecularColor = setMat.specularColor;
				SpecularPower = setMat.specularPower;
				DiffuseTexture = setMat.diffuseTexture;
				NormalTexture = setMat.normalTexture;
				HeightTexture = setMat.heightTexture;
				//unusupported: ParallaxAmount = setMat.parallaxAmount;
				DetailTexture = setMat.detailTexture;
			} // if (setMat)
		} // SetParameters()

		/// <summary>
		/// Set parameters, override to set more
		/// </summary>
		public virtual void SetParameters()
		{
			SetParameters(null);
		} // SetParameters()
		
		/// <summary>
		/// Set parameters, this overload sets all material parameters too.
		/// </summary>
		public virtual void SetParametersOptimizedGeneral()
		{
			if (worldViewProj != null)
				worldViewProj.SetValue(BaseGame.WorldViewProjectionMatrix);
			if (viewProj != null)
				viewProj.SetValue(BaseGame.ViewProjectionMatrix);
			if (world != null)
				world.SetValue(BaseGame.WorldMatrix);
			if (viewInverse != null)
				viewInverse.SetValue(BaseGame.InverseViewMatrix);
			if (lightDir != null)
				lightDir.SetValue(BaseGame.LightDirection);

			// Set the reflection cube texture only once
			if (lastUsedReflectionCubeTexture == null &&
				reflectionCubeTexture != null)
			{
				ReflectionCubeTexture = BaseGame.skyCube.SkyCubeMapTexture;
			} // if (lastUsedReflectionCubeTexture)

			// This shader is used for MeshRenderManager and we want all
			// materials to be opacque, else hotels will look wrong.
			//obs: AlphaFactor = 1.0f;

			// lastUsed parameters for colors and textures are not used,
			// but we overwrite the values in SetParametersOptimized.
			// We fix this by clearing all lastUsed values we will use later.
			lastUsedAmbientColor = ColorHelper.Empty;
			lastUsedDiffuseColor = ColorHelper.Empty;
			lastUsedSpecularColor = ColorHelper.Empty;
			lastUsedDiffuseTexture = null;
			lastUsedNormalTexture = null;
		} // SetParametersOptimizedGeneral()

		/// <summary>
		/// Set parameters, this overload sets all material parameters too.
		/// </summary>
		public virtual void SetParametersOptimized(Material setMat)
		{
			// No need to set world matrix, will be done later in mesh rendering
			// in the MeshRenderManager. All the rest is set with help of the
			// SetParametersOptimizedGeneral above.

			// Only update ambient, diffuse, specular and the textures, the rest
			// will not change for a material change in MeshRenderManager.
			ambientColor.SetValue(setMat.ambientColor.ToVector4());
			diffuseColor.SetValue(setMat.diffuseColor.ToVector4());
			specularColor.SetValue(setMat.specularColor.ToVector4());
			if (setMat.diffuseTexture != null)
				diffuseTexture.SetValue(setMat.diffuseTexture.XnaTexture);
			if (setMat.normalTexture != null)
				normalTexture.SetValue(setMat.normalTexture.XnaTexture);
		} // SetParametersOptimized(setMat)
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		public void Update()
		{
			effect.CommitChanges();
		} // Update()
		#endregion

		#region Render
		/*unused, always specify a technique name
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderDelegate">Render delegate</param>
		public void Render(BaseGame.RenderDelegate renderDelegate)
		{
			SetParameters();

			//not supported or bug, no multiple techniques possible yet:
			//effect.Technique = "SpecularPerPixel";

			// Start shader
			effect.Begin(EffectStateOptions.Default);
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				// Render each pass
				pass.Begin();
				renderDelegate();
				pass.End();
			} // foreach (pass)

			// End shader
			effect.End();
		} // Render(renderDelegate)
		 */

		/*unused, always specify a technique name
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="setMat">Set matrix</param>
		/// <param name="renderDelegate">Render delegate</param>
		public void Render(Material setMat,
			BaseGame.RenderDelegate renderDelegate)
		{
			SetParameters(setMat);

			//not supported or bug, no multiple techniques possible yet:
			//effect.Technique = "SpecularPerPixel";

			// Start shader
			effect.Begin(EffectStateOptions.Default);
			foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			{
				// Render each pass
				pass.Begin();
				renderDelegate();
				pass.End();
			} // foreach (pass)

			// End shader
			effect.End();
		} // Render(setMat, renderDelegate)
		 */

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="setMat">Set matrix</param>
		/// <param name="passName">Pass name</param>
		/// <param name="renderDelegate">Render delegate</param>
		public void Render(Material setMat,
			string techniqueName,
			BaseGame.RenderDelegate renderDelegate)
		{
			SetParameters(setMat);

			// Can we do the requested technique?
			// For graphic cards not supporting ps2.0, fall back to ps1.1
			if (BaseGame.CanUsePS20 == false &&
				techniqueName.EndsWith("20"))
				// Use same technique without the 20 ending!
				techniqueName = techniqueName.Substring(0, techniqueName.Length - 2);

			// Start shader
			effect.CurrentTechnique = effect.Techniques[techniqueName];
			effect.Begin(SaveStateMode.None);

			// Render all passes (usually just one)
			//foreach (EffectPass pass in effect.CurrentTechnique.Passes)
			for (int num = 0; num < effect.CurrentTechnique.Passes.Count; num++)
			{
				EffectPass pass = effect.CurrentTechnique.Passes[num];

				pass.Begin();
				renderDelegate();
				pass.End();
			} // foreach (pass)

			// End shader
			effect.End();
		} // Render(setMat, passName, renderDelegate)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="techniqueName">Technique name</param>
		/// <param name="renderDelegate">Render delegate</param>
		public void Render(string techniqueName,
			BaseGame.RenderDelegate renderDelegate)
		{
			Render(null, techniqueName, renderDelegate);
		} // Render(passName, renderDelegate)
		#endregion

		#region Render single pass shader
		/// <summary>
		/// Render single pass shader
		/// </summary>
		/// <param name="renderDelegate">Render delegate</param>
		public void RenderSinglePassShader(
			BaseGame.RenderDelegate renderDelegate)
		{
			// Start effect (current technique should be set)
			effect.Begin(SaveStateMode.None);
			// Start first pass
			effect.CurrentTechnique.Passes[0].Begin();

			// Render
			renderDelegate();

			// End pass and shader
			effect.CurrentTechnique.Passes[0].End();
			effect.End();
		} // RenderSinglePassShader(renderDelegate)
		#endregion
	} // class ShaderEffect
} // namespace RocketCommanderXna.Shaders
