// Project: Rocket Commander, File: ParallaxShader.cs
// Namespace: RocketCommanderXna.Shaders, Class: ParallaxShader
// Creation date: 01.11.2005 17:26
// Last modified: 14.11.2005 06:22
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using RocketCommanderXna.Graphics;
using RocketCommanderXna.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = RocketCommanderXna.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
using RocketCommanderXna.Game;
#endregion

namespace RocketCommanderXna.Shaders
{
	/// <summary>
	/// Parallax shader class for all parallax shaders (most used shader here),
	/// will just derive from BaseShader, not much special stuff in here.
	/// </summary>
	public class ParallaxShader : IGraphicContent
	{
		#region Variables
		/// <summary>
		/// Shader effect filename.
		/// </summary>
		public const string Filename = "ParallaxMapping";//.fx";

		/// <summary>
		/// Technique name
		/// </summary>
		/// <returns>String</returns>
		private string TechniqueName
		{
			get
			{
				return BaseGame.UsePS20 ?
					"Specular20" : "Specular";
			} // get
		} // TechniqueName

		/// <summary>
		/// The shader effect
		/// </summary>
		protected Effect effect = null;

		/// <summary>
		/// Effect handles for ParallaxShader.fx.
		/// If you have more than 1 shader, you should write a BaseShader class
		/// and reuse the existing program logic to simplify all shader classes.
		/// </summary>
		protected EffectParameter viewProj,
			world,
			viewInverse,
			lightDir,
			ambientColor,
			diffuseColor,
			specularColor,
			shininess,
			diffuseTexture,
			normalTexture,
			heightTexture,
			parallaxAmount,
			alphaValue;
		#endregion

		#region Properties
		/// <summary>
		/// Get Xna effect, can be null when effect is not valid.
		/// We have to render with normal methods then
		/// (fallback if shader is not supported!)
		/// </summary>
		public Effect Effect
		{
			get
			{
				return effect;
			} // get
		} // Effect

		/// <summary>
		/// Is shader valid? No error while loading?
		/// When this returns true the shader can be used.
		/// Will also check if we want to use shaders, either
		/// Graphics.CanDoPS11
		/// </summary>
		public bool Valid
		{
			get
			{
				return effect != null &&
					BaseGame.UsePS;
			} // get
		} // Valid

		/// <summary>
		/// Set effect invalid, used when an exception happened and we
		/// can't continue to use this shader anymore.
		/// </summary>
		public void SetEffectInvalid()
		{
			effect = null;
		} // SetEffectInvalid()

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
		/// Set world matrix
		/// </summary>
		public Matrix WorldMatrix
		{
			set
			{
				//SetValue(world, ref lastUsedWorldMatrix, value);
				world.SetValue(value);
			} // set
		} // WorldMatrix

		/// <summary>
		/// Last used inverse view matrix
		/// </summary>
		private Matrix lastUsedInverseViewMatrix = Matrix.Identity;
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

		/// <summary>
		/// Last used view proj matrix
		/// </summary>
		private Matrix lastUsedViewProjMatrix = Matrix.Identity;
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

		/// <summary>
		/// Last used light dir
		/// </summary>
		private Vector3 lastUsedLightDir = Vector3.Zero;
		/// <summary>
		/// Set light direction
		/// </summary>
		protected Vector3 LightDirection
		{
			set
			{
				value.Normalize();
				SetValue(lightDir, ref lastUsedLightDir, value);
			} // set
		} // LightDirection

		/// <summary>
		/// Last used ambient color
		/// </summary>
		private Color lastUsedAmbientColor = ColorHelper.Empty;
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

		/// <summary>
		/// Last used diffuse color
		/// </summary>
		private Color lastUsedDiffuseColor = ColorHelper.Empty;
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

		/// <summary>
		/// Last used specular color
		/// </summary>
		private Color lastUsedSpecularColor = ColorHelper.Empty;
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

		/// <summary>
		/// Last used shininess
		/// </summary>
		private float lastUsedShininess = 0;
		/// <summary>
		/// Shininess for specular color
		/// </summary>
		public float ShininessValue
		{
			set
			{
				SetValue(shininess, ref lastUsedShininess, value);
			} // set
		} // ShininessValue

		/// <summary>
		/// Last used diffuse texture
		/// </summary>
		private XnaTexture lastUsedDiffuseTexture = null;
		/// <summary>
		/// Set diffuse texture
		/// </summary>
		internal Texture DiffuseTexture
		{
			set
			{
				SetValue(diffuseTexture, ref lastUsedDiffuseTexture, value.XnaTexture);
			} // set
		} // DiffuseTexture

		/// <summary>
		/// Last used normal texture
		/// </summary>
		private XnaTexture lastUsedNormalTexture = null;
		/// <summary>
		/// Set normal texture for normal mapping
		/// </summary>
		protected Texture NormalTexture
		{
			set
			{
				SetValue(normalTexture, ref lastUsedNormalTexture, value.XnaTexture);
			} // set
		} // NormalTexture

		/// <summary>
		/// Last used height texture
		/// </summary>
		private XnaTexture lastUsedHeightTexture = null;
		/// <summary>
		/// Set height texture for parallax mapping
		/// </summary>
		protected Texture HeightTexture
		{
			set
			{
				SetValue(heightTexture, ref lastUsedHeightTexture, value.XnaTexture);
			} // set
		} // HeightTexture

		/// <summary>
		/// Last used parallax amount
		/// </summary>
		private float lastUsedParallaxAmount = 0;
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
		
		/// <summary>
		/// Alpha value, used for better optimized shader setting
		/// </summary>
		public float AlphaValue
		{
			set
			{
				//obs: SetValue(alphaValue, ref lastUsedAlphaValue, value);
				alphaValue.SetValue(value);
			} // set
		} // AlphaValue
		#endregion

		#region Constructor
		/// <summary>
		/// Create parallax shader
		/// </summary>
		public ParallaxShader()
		{
			Load();

			BaseGame.RegisterGraphicContentObject(this);
		} // ParallaxShader()
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

		#region Reload effect
		/// <summary>
		/// Reload
		/// </summary>
		public void Load()
		{
			if (effect != null)
				return;

			// Dispose old shader
			//Dispose();

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
					Log.Write("Failed to load shader " + shaderContentName + ". " +
						"Error: " + ex.ToString());
					// Rethrow error, app can't continue!
					throw ex;
				} // catch
			} // catch
#endif

			ResetParameters();
			GetParameters();
		} // Load()
		#endregion

		#region Reset parameters
		/// <summary>
		/// Reset parameters
		/// </summary>
		protected void ResetParameters()
		{
			lastUsedInverseViewMatrix = Matrix.Identity;
			lastUsedViewProjMatrix = Matrix.Identity;
			lastUsedLightDir = Vector3.Zero;
			lastUsedAmbientColor = ColorHelper.Empty;
			lastUsedDiffuseColor = ColorHelper.Empty;
			lastUsedSpecularColor = ColorHelper.Empty;
			lastUsedShininess = 0;
			lastUsedDiffuseTexture = null;
			lastUsedNormalTexture = null;
			lastUsedHeightTexture = null;
			lastUsedParallaxAmount = 0;
		} // ResetParameters()
		#endregion

		#region GetParameters (happens only once when initializing)
		/// <summary>
		/// Get parameters
		/// </summary>
		protected virtual void GetParameters()
		{
			// Can't get parameters if loading failed!
			if (effect == null)
				return;

			world = effect.Parameters["world"];
			viewInverse = effect.Parameters["viewInverse"];
			viewProj = effect.Parameters["viewProj"];
			lightDir = effect.Parameters["lightDir"];
			ambientColor = effect.Parameters["ambientColor"];
			diffuseColor = effect.Parameters["diffuseColor"];
			specularColor = effect.Parameters["specularColor"];
			shininess = effect.Parameters["shininess"];

			diffuseTexture = effect.Parameters["diffuseTexture"];
			normalTexture = effect.Parameters["normalTexture"];
			heightTexture = effect.Parameters["heightTexture"];
			parallaxAmount = effect.Parameters["parallaxAmount"];
			alphaValue = effect.Parameters["alphaValue"];

			// We only got 2 techniques, select the best one we can use.
			effect.CurrentTechnique = effect.Techniques[TechniqueName];

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

		#region Update shader parameters
		/// <summary>
		/// Update matrices and effect parameters
		/// </summary>
		public void UpdateMatricesAndEffectParameters()
		{
			// World matrix is set in UpdateWorldMatrix(.)
			ViewProjMatrix = BaseGame.ViewMatrix *
				BaseGame.ProjectionMatrix;
			InverseViewMatrix = BaseGame.InverseViewMatrix;
			LightDirection = BaseGame.LightDirection;
		} // UpdateMatricesAndEffectParameters()

		/// <summary>
		/// Update parameters, the matrices and light parameters come from
		/// BaseGame. The material (colors and textures) are updated with mat.
		/// </summary>
		/// <param name="setMat">Material</param>
		public void UpdateMaterialParameters(Material setMat)
		{
			// We can't update without valid material
			if (setMat == null)
				return;

			AmbientColor = setMat.ambientColor;
			DiffuseColor = setMat.diffuseColor;
			SpecularColor = setMat.specularColor;
			ShininessValue = setMat.specularPower;
			DiffuseTexture = setMat.diffuseTexture;
			NormalTexture = setMat.normalTexture;
			HeightTexture = setMat.heightTexture;
			ParallaxAmount = setMat.parallaxAmount;
			//obs: DetailTexture = setMat.detailTexture;
		} // UpdateMaterialParameters(mat)

		/// <summary>
		/// Set parameters, this overload sets all material parameters too.
		/// </summary>
		public virtual void SetParametersOptimizedGeneral()
		{
			//if (worldViewProj != null)
			//	worldViewProj.SetValue(BaseGame.WorldViewProjectionMatrix);
			if (viewProj != null)
				viewProj.SetValue(BaseGame.ViewProjectionMatrix);
			if (world != null)
				world.SetValue(BaseGame.WorldMatrix);
			if (viewInverse != null)
				viewInverse.SetValue(BaseGame.InverseViewMatrix);
			if (lightDir != null)
				lightDir.SetValue(BaseGame.LightDirection);

			/*obs
			// Set the reflection cube texture only once
			if (lastUsedReflectionCubeTexture == null &&
				reflectionCubeTexture != null)
			{
				ReflectionCubeTexture = BaseGame.skyCube.SkyCubeMapTexture;
			} // if (lastUsedReflectionCubeTexture)
			 */

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

			// Set parallax amount for all materials
			if (parallaxAmount != null)
				parallaxAmount.SetValue(Material.DefaultParallaxAmount);
			// Use default opacity of 1.0f
			if (alphaValue != null)
				alphaValue.SetValue(1.0f);
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
	} // class ParallaxShader
} // namespace RocketCommanderXna.Shaders
