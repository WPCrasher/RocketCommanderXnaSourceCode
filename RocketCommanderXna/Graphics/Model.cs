// Author: abi
// Project: RocketCommanderXna
// Path: C:\code\Xna\RocketCommanderXna\Graphics
// Creation date: 30.01.2008 00:44
// Last modified: 30.01.2008 00:55

// Project: RocketCommanderXna, File: Model.cs
// Namespace: RocketCommanderXna.Graphics, Class: Model
// Path: C:\code\XnaBook\RocketCommanderXna\Graphics, Author: Abi
// Code lines: 36, Size of file: 900 Bytes
// Creation date: 27.11.2006 03:27
// Last modified: 27.11.2006 03:50
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Game;
using RocketCommanderXna.Helpers;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Model
	/// </summary>
	public class Model : IGraphicContent
	{
		#region Variables
		/// <summary>
		/// Name of this model, also used to load it from the content system.
		/// </summary>
		string name = "";

		/// <summary>
		/// All x files
		/// </summary>
		public const string Extension = "x";

		/// <summary>
		/// Underlying xna model object. Loaded with the content system.
		/// </summary>
		XnaModel xnaModel = null;

		/// <summary>
		/// Default object matrix to fix models from 3ds max to our engine!
		/// </summary>
		Matrix objectMatrix =
			// left handed models (else everything is mirrored with x files)
			Matrix.CreateRotationX(MathHelper.Pi / 2.0f);

		/// <summary>
		/// Scaling for this object, used for distance comparisons.
		/// </summary>
		float downScaling = 1.0f;

		/// <summary>
		/// Since we only support one mesh and only one mesh part in there,
		/// store it into this variable!
		/// </summary>
		ModelMeshPart meshPart = null;

		/// <summary>
		/// Store the only renderable mesh helper we need here.
		/// Rendering happens in the MeshRenderManager class.
		/// </summary>
		MeshRenderManager.RenderableMesh renderableMesh = null;
		#endregion

		#region Properties
		/// <summary>
		/// Name for this model, this is the content name.
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return name;
			} // get
		} // Name

		/// <summary>
		/// Object downscaled size to be standard size of 1.0
		/// </summary>
		/// <returns>Float</returns>
		public float ObjectDownscaledSize
		{
			get
			{
				return downScaling;
			} // get
		} // ObjectDownscaledSize

		/// <summary>
		/// Xna model
		/// </summary>
		/// <returns>Xna model</returns>
		public XnaModel XnaModel
		{
			get
			{
				return xnaModel;
			} // get
		} // XnaModel

		/// <summary>
		/// Number of mesh parts
		/// </summary>
		/// <returns>Int</returns>
		public int NumOfMeshParts
		{
			get
			{
				int ret = 0;
				//obs: foreach (ModelMesh mesh in xnaModel.Meshes)
				for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
					ret += xnaModel.Meshes[meshNum].MeshParts.Count;
				return ret;
			} // get
		} // NumOfMeshParts
		#endregion

		#region Constructor
		/// <summary>
		/// Create model
		/// </summary>
		/// <param name="setModelName">Set model name</param>
		public Model(string setModelName)
		{
			name = setModelName;

			Load();

			BaseGame.RegisterGraphicContentObject(this);

			// Get matrix transformations of the model
			// Has to be done only once because we don't use animations in this game.
			Matrix[] transforms = new Matrix[xnaModel.Bones.Count];
			xnaModel.CopyAbsoluteBoneTransformsTo(transforms);

			// Calculate scaling for this object, used for distance comparisons.
			if (xnaModel.Meshes.Count > 0)
				downScaling =// scaling =
					xnaModel.Meshes[0].BoundingSphere.Radius *
					transforms[0].Right.Length();
			if (downScaling > 0)
				downScaling = 1.0f / downScaling;
			objectMatrix =
				// Use transformation of our first mesh (we support only one)
				transforms[xnaModel.Meshes[0].ParentBone.Index] *
				Matrix.CreateRotationX(MathHelper.Pi / 2.0f) *
				Matrix.CreateScale(downScaling);
		} // Model(setModelName)
		#endregion

		#region Load
		public void Load()
		{
			if (xnaModel == null)
			{
				xnaModel = BaseGame.Content.Load<XnaModel>(
					@"Content\" + name);

				// Go through all meshes in the model
				//obs: foreach (ModelMesh mesh in xnaModel.Meshes)
				//obs: for (int meshNum = 0; meshNum < xnaModel.Meshes.Count; meshNum++)
				//{
				ModelMesh mesh = xnaModel.Meshes[0];
				meshPart = mesh.MeshParts[0];
				string meshName = mesh.Name;
				// Only support one effect
				Effect effect = mesh.Effects[0];

				// And for each effect this mesh uses (usually just 1, multimaterials
				// are nice in 3ds max, but not efficiently for rendering stuff).
				//obs: foreach (Effect effect in mesh.Effects)
				//obs: for (int effectNum = 0; effectNum < mesh.Effects.Count; effectNum++)
				//{
				//Effect effect = mesh.Effects[effectNum];
				// Get technique from meshName
				int techniqueIndex = -1;
				if (meshName.Length > 0)
				{
					try
					{
						//Log.Write("meshName="+meshName);
						string techniqueNumberString = meshName.Substring(
							meshName.Length - 1, 1);
#if !XBOX360
						// Faster and does not throw an exception!
						int.TryParse(techniqueNumberString, out techniqueIndex);
#else
					techniqueIndex = Convert.ToInt32(techniqueNumberString);
#endif
						//Log.Write("techniqueIndex="+techniqueIndex);
					} // try
					catch { } // ignore if that failed
				} // if (meshName.Length)

				// No technique found or invalid?
				if (techniqueIndex < 0 ||
					techniqueIndex >= effect.Techniques.Count ||
					// Or if this is an asteroid (use faster diffuse technique!)
					StringHelper.BeginsWith(name, "asteroid"))
				{
					// Try to use last technique
					techniqueIndex = effect.Techniques.Count - 1;
				} // if (techniqueIndex)

				// If the technique ends with 20, but we can't do ps20,
				// use the technique before that (which doesn't use ps20)
				if (BaseGame.CanUsePS20 == false &&
					effect.Techniques[techniqueIndex].Name.EndsWith("20"))
					techniqueIndex--;

				// Set current technique for rendering below
				effect.CurrentTechnique = effect.Techniques[techniqueIndex];
				//} // foreach (effect)

				// Add all mesh parts!
				//obs: for (int partNum = 0; partNum < mesh.MeshParts.Count; partNum++)
				//{
				//obs: ModelMeshPart part = mesh.MeshParts[partNum];

				// The model mesh part is not really used, we just extract the
				// index and vertex buffers and all the render data.
				// Material settings are build from the effect settings.
				// Also add this to our own dictionary for rendering.
				renderableMesh = BaseGame.MeshRenderManager.Add(
					mesh.VertexBuffer, mesh.IndexBuffer, meshPart, effect);
				//} // for (partNum)
				//} // foreach (mesh)
			} // if
		} // Load()
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			xnaModel = null;
		} // Dispose()
		#endregion

		#region Render
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void Render(Matrix renderMatrix, float alpha)
		{
			// Make sure alpha is valid
			if (alpha <= 0)
				return; // skip, no reason to render this!
			if (alpha > 1)
				alpha = 1;

			float distanceSquared = Vector3.DistanceSquared(
				BaseGame.CameraPos, renderMatrix.Translation);
			// Check out if object is behind us or not visible, then we can skip
			// rendering. This is the GREATEST performance gain in the whole game!
			// Object must be away at least 20 units!
			if (distanceSquared > 20 * 20)
			{
				Vector3 objectDirection =
					Vector3.Normalize(BaseGame.CameraPos - renderMatrix.Translation);

				// Half field of view should be fov / 2, but because of
				// the aspect ratio (1.33) and an additional offset we need
				// to include to see objects at the borders.
				float objAngle = Vector3Helper.GetAngleBetweenVectors(
					BaseGame.CameraRotation, objectDirection);
				if (objAngle > BaseGame.FieldOfView)//.ViewableFieldOfView)
					// Skip.
					return;
			} // if (distanceSquared)

			// Just render the only renderable mesh we got. The mesh part adds
			// the render matrix to be picked up in the mesh rendering later.
			renderableMesh.thisFrameRenderMatricesAndAlpha.Add(
				new MeshRenderManager.MatrixAndAlpha(objectMatrix * renderMatrix, alpha));
		} // Render(renderMatrix, alpha)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void Render(Matrix renderMatrix)
		{
			Render(renderMatrix, 1.0f);
		} // Render(renderMatrix)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderPos">Render position</param>
		public void Render(Vector3 renderPos)
		{
			Render(Matrix.CreateTranslation(renderPos));
		} // Render(renderPos)
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestSingleModel
		/// <summary>
		/// Test single model
		/// </summary>
		static public void TestSingleModel()
		{
			Model testModel1 = null;
			Model testModel2 = null;
			TestGame.Start("TestSingleModel",
				delegate
				{
					testModel1 = new Model("goal");//"asteroid1");
					testModel2 = new Model("asteroid1Low");
				},
				delegate
				{
					for (int num = 0; num < 200; num++)
					{
						BaseGame.DrawLine(
							new Vector3(-12.0f + num / 4.0f, 13.0f, 0),
							new Vector3(-17.0f + num / 4.0f, -13.0f, 0),
							new Color((byte)(255 - num), 14, (byte)num));
					} // for

					TextureFont.WriteText(2, 30,
						"cam pos=" + BaseGame.CameraPos);

					if (Input.Keyboard.IsKeyDown(Keys.LeftControl) == false)
						testModel1.Render(Matrix.CreateScale(4));
					else
						testModel2.Render(Matrix.CreateScale(4));

					// And flush render manager to draw all objects
					BaseGame.MeshRenderManager.Render();
				});
		} // TestSingleModel()
		#endregion
#endif
		#endregion
	} // class Model
} // namespace RocketCommanderXna.Graphics
