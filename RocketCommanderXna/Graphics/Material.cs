// Project: RocketCommanderXna, File: Material.cs
// Namespace: RocketCommanderXna.Graphics, Class: Material
// Path: C:\code\RocketCommanderXna\Graphics, Author: Abi
// Code lines: 360, Size of file: 11,42 KB
// Creation date: 07.09.2006 05:56
// Last modified: 26.10.2006 22:44
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Shaders;
using Microsoft.Xna.Framework.Graphics;
using RocketCommanderXna.Game;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Material class for Xna materials used for Models. Consists of
	/// normal Xna material settings (ambient, diffuse, specular),
	/// the diffuse texture and optionally of normal map, height map and shader
	/// parameters.
	/// </summary>
	public class Material : IGraphicContent
	{
		#region Constants
		/// <summary>
		/// Default color values are:
		/// 0.15f for ambient and 1.0f for diffuse and 1.0f specular.
		/// </summary>
		public static readonly Color
			DefaultAmbientColor = new Color(44, 44, 44),//new Color(40, 40, 40),
			DefaultDiffuseColor = new Color(230, 230, 230),
			DefaultSpecularColor = new Color(255, 255, 255);

		/// <summary>
		/// Default specular power (24)
		/// </summary>
		const float DefaultSpecularPower = 24.0f;

		/// <summary>
		/// Parallax amount for parallax and offset shaders.
		/// </summary>
		public const float DefaultParallaxAmount = 0.024f;//0.04f;//0.07f;
		#endregion

		#region Variables
		/// <summary>
		/// Colors
		/// </summary>
		public Color diffuseColor = DefaultDiffuseColor,
			ambientColor = DefaultAmbientColor,
			specularColor = DefaultSpecularColor;

		/// <summary>
		/// Specular power
		/// </summary>
		public float specularPower = DefaultSpecularPower;

		/// <summary>
		/// Diffuse texture for the material. Can be null for unused.
		/// </summary>
		public Texture diffuseTexture = null;
		/// <summary>
		/// Normal texture in case we use normal mapping. Can be null for unused.
		/// </summary>
		public Texture normalTexture = null;
		/// <summary>
		/// Height texture in case we use parallax mapping. Can be null for unused.
		/// </summary>
		public Texture heightTexture = null;
		/// <summary>
		/// Detail texture, used for landscape rendering. Can be null for unused.
		/// </summary>
		public Texture detailTexture = null;
		/// <summary>
		/// Parallax amount for parallax and offset shaders.
		/// </summary>
		public float parallaxAmount = DefaultParallaxAmount;

		/// <summary>
		/// Filenames for the textures used here for reloading!
		/// </summary>
		public string diffuseTextureFilename = "",
			normalTextureFilename = "",
			heightTextureFilename = "",
			detailTextureFilename = "";
		#endregion

		#region Properties
		/// <summary>
		/// Checks if the diffuse texture has alpha
		/// </summary>
		public bool HasAlpha
		{
			get
			{
				if (diffuseTexture != null)
					return diffuseTexture.HasAlphaPixels;
				else
					return false;
			} // get
		} // HasAlpha
		#endregion

		#region Constructors
		#region Default Constructors
		/// <summary>
		/// Create material, just using default values.
		/// </summary>
		public Material()
		{
			BaseGame.RegisterGraphicContentObject(this);
		} // Material()

		/// <summary>
		/// Create material, just using default color values.
		/// </summary>
		public Material(string setDiffuseTexture)
			: this()
		{
			diffuseTexture = new Texture(setDiffuseTexture);
		} // Material(setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			string setDiffuseTexture)
			: this()
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			diffuseTexture = new Texture(setDiffuseTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			Texture setDiffuseTexture)
			: this()
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			diffuseTexture = setDiffuseTexture;
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(string setDiffuseTexture, string setNormalTexture)
			: this()
		{
			diffuseTexture = new Texture(setDiffuseTexture);
			normalTexture = new Texture(setNormalTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(string setDiffuseTexture, string setNormalTexture,
			string setHeightTexture)
			: this()
		{
			diffuseTexture = new Texture(setDiffuseTexture);
			normalTexture = new Texture(setNormalTexture);
			heightTexture = new Texture(setHeightTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			Color setSpecularColor, string setDiffuseTexture,
			string setNormalTexture, string setHeightTexture,
			string setDetailTexture)
			: this()
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			specularColor = setSpecularColor;
			diffuseTexture = new Texture(setDiffuseTexture);
			if (String.IsNullOrEmpty(setNormalTexture) == false)
				normalTexture = new Texture(setNormalTexture);
			if (String.IsNullOrEmpty(setHeightTexture) == false)
				heightTexture = new Texture(setHeightTexture);
			if (String.IsNullOrEmpty(setDetailTexture) == false)
				detailTexture = new Texture(setDetailTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)
		#endregion

		#region Create material from effect settings
		/// <summary>
		/// Create material
		/// </summary>
		/// <param name="effect">Effect</param>
		public Material(Effect effect)
			: this()
		{
			EffectParameter diffuseTextureParameter =
				effect.Parameters["diffuseTexture"];
			if (diffuseTextureParameter != null)
				diffuseTexture = new Texture(
					diffuseTextureParameter.GetValueTexture2D());
			
			EffectParameter normalTextureParameter =
				effect.Parameters["normalTexture"];
			if (normalTextureParameter != null)
				normalTexture = new Texture(
					normalTextureParameter.GetValueTexture2D());

			EffectParameter heightTextureParameter =
				effect.Parameters["heightTexture"];
			if (heightTextureParameter != null)
				heightTexture = new Texture(
					heightTextureParameter.GetValueTexture2D());

			EffectParameter diffuseColorParameter =
				effect.Parameters["diffuseColor"];
			if (diffuseColorParameter != null)
				diffuseColor = new Color(diffuseColorParameter.GetValueVector4());
			
			EffectParameter ambientColorParameter =
				effect.Parameters["ambientColor"];
			if (ambientColorParameter != null)
				ambientColor = new Color(ambientColorParameter.GetValueVector4());
			// Make sure ambientColor is not darker than DefaultAmbientColor
			if (ambientColor.R < DefaultAmbientColor.R)
				ambientColor = DefaultAmbientColor;
			
			EffectParameter specularColorParameter =
				effect.Parameters["specularColor"];
			if (specularColorParameter != null)
				specularColor = new Color(specularColorParameter.GetValueVector4());

			EffectParameter specularPowerParameter =
				effect.Parameters["specularPower"];
			if (specularPowerParameter != null)
				specularPower = specularPowerParameter.GetValueSingle();

			EffectParameter parallaxAmountParameter =
				effect.Parameters["parallaxAmount"];
			if (parallaxAmountParameter != null)
				parallaxAmount = parallaxAmountParameter.GetValueSingle();
			parallaxAmount = 0.0f;
		} // Material(effect)
		#endregion
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (diffuseTexture != null)
			{
				diffuseTextureFilename = diffuseTexture.Filename;
				diffuseTexture.Dispose();
			} // if
			diffuseTexture = null;

			if (normalTexture != null)
			{
				normalTextureFilename = normalTexture.Filename;
				normalTexture.Dispose();
			} // if
			normalTexture = null;

			if (heightTexture != null)
			{
				heightTextureFilename = heightTexture.Filename;
				heightTexture.Dispose();
			} // if
			heightTexture = null;

			if (detailTexture != null)
			{
				detailTextureFilename = detailTexture.Filename;
				detailTexture.Dispose();
			} // if
			detailTexture = null;
		} // Dispose()
		#endregion

		#region Load
		/// <summary>
		/// Load
		/// </summary>
		public void Load()
		{
			if (diffuseTexture == null &&
				String.IsNullOrEmpty(diffuseTextureFilename) == false)
			{
				diffuseTexture = new Texture(diffuseTextureFilename);
			} // if
			if (normalTexture == null &&
				String.IsNullOrEmpty(normalTextureFilename) == false)
			{
				normalTexture = new Texture(normalTextureFilename);
			} // if
			if (heightTexture == null &&
				String.IsNullOrEmpty(heightTextureFilename) == false)
			{
				heightTexture = new Texture(heightTextureFilename);
			} // if
			if (detailTexture == null &&
				String.IsNullOrEmpty(detailTextureFilename) == false)
			{
				detailTexture = new Texture(detailTextureFilename);
			} // if
		} // Load()
		#endregion
	} // class Material
} // namespace RocketCommanderXna.Graphics
