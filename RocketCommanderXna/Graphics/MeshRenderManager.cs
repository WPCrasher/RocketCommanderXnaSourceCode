// Project: RocketCommanderXna, File: MeshRenderManager.cs
// Namespace: RocketCommanderXna.Graphics, Class: RenderableMesh
// Path: C:\code\RocketCommanderXna\Graphics, Author: Abi
// Code lines: 573, Size of file: 19,90 KB
// Creation date: 26.10.2006 22:48
// Last modified: 03.11.2006 09:22
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Game;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Shaders;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Mesh render manager, a little helper class which allows us to render
	/// all our models with much faster performance through sorting by
	/// material and shader techniques.
	/// 
	/// We keep a list of lists of lists for rendering our RenderableMeshes
	/// sorted by techniques first and materials second.
	/// The most outer list contains all techniques (see
	/// MeshesPerMaterialPerTechniques).
	/// Then the first inner list contains all materials (see MeshesPerMaterial).
	/// And finally the most inner list contains all meshes that use the
	/// technique and material we got.
	/// Additionally we could also sort by shaders, but since all models
	/// use the same shader (normalMapping), that is the only thing we support
	/// here. Improving it to support more shaders is easily possible.
	/// 
	/// All this is created in the Model constructor. At runtime we just
	/// go through these lists and render everything down as quickly as
	/// possible.
	/// </summary>
	public class MeshRenderManager
	{
		#region Remember vertex and index buffers
		/// <summary>
		/// Don't set vertex and index buffers again if they are already set this
		/// frame.
		/// </summary>
		static VertexBuffer lastVertexBufferSet = null;
		static IndexBuffer lastIndexBufferSet = null;
		#endregion

		#region MatrixAndAlpha helper struct
		/// <summary>
		/// Matrix and alpha, these are the 2 things that change from
		/// RenderableMesh to the next RenderableMesh with the same material
		/// and model mesh.
		/// </summary>
		public struct MatrixAndAlpha
		{
			/// <summary>
			/// Render matrix
			/// </summary>
			public Matrix matrix;
			/// <summary>
			/// Alpha
			/// </summary>
			public float alpha;

			/// <summary>
			/// Matrix and alpha helper class
			/// </summary>
			/// <param name="setMatrix">Matrix</param>
			/// <param name="setAlpha">Alpha</param>
			public MatrixAndAlpha(Matrix setMatrix, float setAlpha)
			{
				matrix = setMatrix;
				alpha = setAlpha;
			} // MatrixAndAlpha(setMatrix, setAlpha)
		} // struct MatrixAndAlpha
		#endregion

		#region RenderableMesh helper class
		/// <summary>
		/// Renderable mesh, created in the Model constructor and is rendered
		/// when we render all the models at once at the end of each frame!
		/// </summary>
		public class RenderableMesh
		{
			#region Variables
			/// <summary>
			/// Vertex buffer
			/// </summary>
			public VertexBuffer vertexBuffer;
			/// <summary>
			/// Index buffer
			/// </summary>
			public IndexBuffer indexBuffer;
			/// <summary>
			/// Material
			/// </summary>
			public Material material;
			/// <summary>
			/// Used technique
			/// </summary>
			public EffectTechnique usedTechnique;
			/// <summary>
			/// Vertex declaration
			/// </summary>
			public VertexDeclaration vertexDeclaration;
			/// <summary>
			/// Stream offset, vertex stride, etc.
			/// All parameters we need for rendering.
			/// </summary>
			public int streamOffset, vertexStride, baseVertex,
				numVertices, startIndex, primitiveCount;

			/// <summary>
			/// List of render matrices we use every frame. At creation time
			/// this list is unused and empty, but for each frame we use this
			/// list to remember which objects we want to render.
			/// Of course rendering happens only if this list is not empty.
			/// After each frame this list is cleared again.
			/// </summary>
			public List<MatrixAndAlpha>
				lastFrameRenderMatricesAndAlpha = new List<MatrixAndAlpha>(),
				thisFrameRenderMatricesAndAlpha = new List<MatrixAndAlpha>();
			#endregion

			#region Constructor
			/// <summary>
			/// Create renderable mesh
			/// </summary>
			/// <param name="setVertexBuffer">Set vertex buffer</param>
			/// <param name="setIndexBuffer">Set index buffer</param>
			/// <param name="setMaterial">Set material</param>
			/// <param name="setUsedTechnique">Set used technique</param>
			/// <param name="setWorldParameter">Set world parameter</param>
			/// <param name="setVertexDeclaration">Set vertex declaration</param>
			/// <param name="setStreamOffset">Set stream offset</param>
			/// <param name="setVertexStride">Set vertex stride</param>
			/// <param name="setBaseVertex">Set base vertex</param>
			/// <param name="setNumVertices">Set number vertices</param>
			/// <param name="setStartIndex">Set start index</param>
			/// <param name="setPrimitiveCount">Set primitive count</param>
			public RenderableMesh(VertexBuffer setVertexBuffer,
				IndexBuffer setIndexBuffer, Material setMaterial,
				EffectTechnique setUsedTechnique,
				//EffectParameter setWorldParameter,
				VertexDeclaration setVertexDeclaration,
				int setStreamOffset, int setVertexStride, int setBaseVertex,
				int setNumVertices, int setStartIndex, int setPrimitiveCount)
			{
				vertexBuffer = setVertexBuffer;
				indexBuffer = setIndexBuffer;
				material = setMaterial;
				usedTechnique = setUsedTechnique;
				vertexDeclaration = setVertexDeclaration;
				streamOffset = setStreamOffset;
				vertexStride = setVertexStride;
				baseVertex = setBaseVertex;
				numVertices = setNumVertices;
				startIndex = setStartIndex;
				primitiveCount = setPrimitiveCount;
			} // RenderableMesh(setVertexBuffer, setIndexBuffer, setMaterial)
			#endregion

			#region Render
			//float lastAlpha = 1.0f;
			/// <summary>
			/// Render this renderable mesh, MUST be called inside of the
			/// render method of ShaderEffect.normalMapping!
			/// </summary>
			/// <param name="worldMatrix">World matrix</param>
			public void RenderMesh(MatrixAndAlpha worldMatrixAndAlpha)
			{
				// Update world matrix
				BaseGame.ParallaxShader.WorldMatrix = worldMatrixAndAlpha.matrix;
				BaseGame.ParallaxShader.AlphaValue = worldMatrixAndAlpha.alpha;
				BaseGame.ParallaxShader.Effect.CommitChanges();

				// Set vertex buffer and index buffer
				if (lastVertexBufferSet != vertexBuffer ||
					lastIndexBufferSet != indexBuffer)
				{
					//tst: BaseGame.Device.VertexDeclaration = vertexDeclaration;
					lastVertexBufferSet = vertexBuffer;
					lastIndexBufferSet = indexBuffer;
					BaseGame.Device.Vertices[0].SetSource(
						vertexBuffer, streamOffset, vertexStride);
					BaseGame.Device.Indices = indexBuffer;
				} // if (vertexBuffer)

				// And render (this call takes the longest, we can't optimize
				// it any further because the vertexBuffer and indexBuffer are
				// WriteOnly, we can't combine it or optimize it any more).
				BaseGame.Device.DrawIndexedPrimitives(
					PrimitiveType.TriangleList,
					baseVertex, 0, numVertices, startIndex, primitiveCount);
			} // RenderMesh(worldMatrix)

			/// <summary>
			/// Render
			/// </summary>
			public void Render()
			{
				// Render all meshes we have requested this frame.
				//lastAlpha = 1.0f;
				//obs: foreach (Matrix matrix in renderMatrices)
				for (int matrixNum = 0;
					matrixNum < lastFrameRenderMatricesAndAlpha.Count; matrixNum++)
					RenderMesh(lastFrameRenderMatricesAndAlpha[matrixNum]);
				
				//*tst
				// Clear all meshes, don't render them again.
				// Next frame everything will be created again.
				lastFrameRenderMatricesAndAlpha.Clear();
				//*/
			} // Render()
			#endregion
		} // class RenderableMesh
		#endregion

		#region MeshesPerMaterial helper class
		/// <summary>
		/// Meshes per material
		/// </summary>
		public class MeshesPerMaterial
		{
			#region Variables
			/// <summary>
			/// Material
			/// </summary>
			public Material material;
			/// <summary>
			/// Meshes
			/// </summary>
			public List<RenderableMesh> meshes = new List<RenderableMesh>();
			#endregion
			
			#region Properties
			/// <summary>
			/// Number of render matrices this material uses this frame.
			/// </summary>
			/// <returns>Int</returns>
			public int NumberOfRenderMatrices
			{
				get
				{
					int ret = 0;
					//obs: foreach (RenderableMesh mesh in meshes)
					for (int meshNum = 0; meshNum < meshes.Count; meshNum++)
						ret += meshes[meshNum].lastFrameRenderMatricesAndAlpha.Count;
					return ret;
				} // get
			} // NumberOfRenderMatrices
			#endregion

			#region Constructor
			/// <summary>
			/// Create meshes per material for the setMaterial.
			/// </summary>
			/// <param name="setMaterial">Set material</param>
			public MeshesPerMaterial(Material setMaterial)
			{
				material = setMaterial;
			} // MeshesPerMaterial(setMaterial, setMesh)
			#endregion

			#region Add
			/// <summary>
			/// Adds a renderable mesh using this material.
			/// </summary>
			/// <param name="addMesh">Add mesh</param>
			public void Add(RenderableMesh addMesh)
			{
				// Make sure this mesh uses the correct material
				if (addMesh.material != material)
					throw new Exception("Invalid material, to add a mesh to "+
						"MeshesPerMaterial it must use the specified material="+
						material);

				meshes.Add(addMesh);
			} // Add(addMesh)
			#endregion

			#region Render
			/// <summary>
			/// Render all meshes that use this material.
			/// This method is only called if we got any meshes to render,
			/// which is determinated if NumberOfRenderMeshes is greater 0.
			/// </summary>
			public void Render()
			{
				// Set material settings. We don't have to update the shader here,
				// it will be done in RenderableMesh.Render anyway because of
				// updating the world matrix!
				BaseGame.ParallaxShader.SetParametersOptimized(material);
				// Set vertex declaration
				BaseGame.Device.VertexDeclaration = meshes[0].vertexDeclaration;

				// Render all meshes that use this material.
				//obs: foreach (RenderableMesh mesh in meshes)
				for (int meshNum = 0; meshNum < meshes.Count; meshNum++)
				{
					RenderableMesh mesh = meshes[meshNum];
					if (mesh.lastFrameRenderMatricesAndAlpha.Count > 0)
						mesh.Render();
				} // for (meshNum)
			} // Render()
			#endregion
		} // class MeshesPerMaterial
		#endregion

		#region MeshesPerMaterialsPerTechniques helper class
		/// <summary>
		/// Meshes per material per techniques
		/// </summary>
		public class MeshesPerMaterialPerTechniques
		{
			#region Variables
			/// <summary>
			/// Technique
			/// </summary>
			public EffectTechnique technique;
			/// <summary>
			/// Meshes per materials
			/// </summary>
			public List<MeshesPerMaterial> meshesPerMaterials =
				new List<MeshesPerMaterial>();
			#endregion

			#region Properties
			/// <summary>
			/// Number of render matrices this technique uses this frame.
			/// </summary>
			/// <returns>Int</returns>
			public int NumberOfRenderMatrices
			{
				get
				{
					int ret = 0;
					//obs: foreach (MeshesPerMaterial list in meshesPerMaterials)
					for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
						ret += meshesPerMaterials[listNum].NumberOfRenderMatrices;
					return ret;
				} // get
			} // NumberOfRenderMatrices
			#endregion

			#region Constructor
			/// <summary>
			/// Create meshes per material per techniques
			/// </summary>
			/// <param name="setTechnique">Set technique</param>
			public MeshesPerMaterialPerTechniques(EffectTechnique setTechnique)
			{
				technique = setTechnique;
			} // MeshesPerMaterialPerTechniques(setTechnique)
			#endregion

			#region Add
			/// <summary>
			/// Adds a renderable mesh using this technique.
			/// </summary>
			/// <param name="addMesh">Add mesh</param>
			public void Add(RenderableMesh addMesh)
			{
				// Make sure this mesh uses the correct material
				if (addMesh.usedTechnique != technique)
					throw new Exception("Invalid technique, to add a mesh to "+
						"MeshesPerMaterialPerTechniques it must use the specified "+
						"technique="+technique.Name);

				// Search for the used material, maybe we have it already in list.
				//obs: foreach (MeshesPerMaterial list in meshesPerMaterials)
				for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
				{
					MeshesPerMaterial existingList = meshesPerMaterials[listNum];
					if (existingList.material == addMesh.material)
					{
						// Just add
						existingList.Add(addMesh);
						return;
					} // if (existingList.material)
				} // for (listNum)

				// Not found, create new list and add mesh there.
				MeshesPerMaterial newList = new MeshesPerMaterial(addMesh.material);
				newList.Add(addMesh);
				meshesPerMaterials.Add(newList);
			} // Add(addMesh)
			#endregion

			#region Render
			/// <summary>
			/// Render all meshes that use this technique sorted by the materials.
			/// This method is only called if we got any meshes to render,
			/// which is determinated if NumberOfRenderMeshes is greater 0.
			/// </summary>
			/// <param name="effect">Effect</param>
			public void Render(Effect effect)
			{
				// Start effect for this technique
				effect.CurrentTechnique = technique;
				effect.Begin(SaveStateMode.None);

				// Render all pass (we always just have one)
				//obs: foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					EffectPass pass = effect.CurrentTechnique.Passes[0];
				
					pass.Begin();
					// Render all meshes sorted by all materials.
					//obs: foreach (MeshesPerMaterial list in meshesPerMaterials)
					for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
					{
						MeshesPerMaterial list = meshesPerMaterials[listNum];
						if (list.NumberOfRenderMatrices > 0)
							list.Render();
					} // for (listNum)
					pass.End();
				} // foreach (pass)

				// End shader
				effect.End();
			} // Render(effect)
			#endregion
		} // class MeshesPerMaterialPerTechniques
		#endregion
		
		#region Variables
		/// <summary>
		/// Sorted meshes we got. Everything is sorted by techniques and then
		/// sorted by materials. This all happens at construction time.
		/// For rendering use renderMatrices list, which is directly in the
		/// most inner list of sortedMeshes (the RenderableMesh objects).
		/// </summary>
		List<MeshesPerMaterialPerTechniques> sortedMeshes =
			new List<MeshesPerMaterialPerTechniques>();
		#endregion

		#region Add
		/// <summary>
		/// Add model mesh part with the used effect to our sortedMeshes list.
		/// Neither the model mesh part nor the effect is directly used,
		/// we will extract all data from the model and only render the
		/// index and vertex buffers later.
		/// The model mesh part must use the TangentVertex format.
		/// </summary>
		/// <param name="vertexBuffer">Vertex buffer</param>
		/// <param name="indexBuffer">Index buffer</param>
		/// <param name="part">Part</param>
		/// <param name="effect">Effect</param>
		/// <returns>Renderable mesh</returns>
		public RenderableMesh Add(VertexBuffer vertexBuffer,
			IndexBuffer indexBuffer, ModelMeshPart part, Effect effect)
		{
			string techniqueName = effect.CurrentTechnique.Name;

			// Does this technique already exists?
			MeshesPerMaterialPerTechniques foundList = null;
			//obs: foreach (MeshesPerMaterialPerTechniques list in sortedMeshes)
			for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
			{
				MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];

				if (list.technique != null &&
					list.technique.Name == techniqueName)
				{
					foundList = list;
					break;
				} // if (list.technique.Name)
			} // for (listNum)

			// Did not found list? Create new one
			if (foundList == null)
			{
				EffectTechnique technique =
					BaseGame.ParallaxShader.GetTechnique(techniqueName);

				// Make sure we always have a valid technique
				if (technique == null)
				{
					if (BaseGame.CanUsePS20)
						techniqueName = "Diffuse20";//"Specular20";
					else
						techniqueName = "Diffuse";//"Specular";
					technique = BaseGame.ParallaxShader.GetTechnique(techniqueName);
				} // if

				foundList = new MeshesPerMaterialPerTechniques(technique);
				sortedMeshes.Add(foundList);
			} // if (foundList)

			// Create new material from the current effect parameters.
			// This will create duplicate materials if the same material is used
			// multiple times, we check this later.
			Material material = new Material(effect);

			// Search for material inside foundList.
			//obs: foreach (MeshesPerMaterial innerList in foundList.meshesPerMaterials)
			for (int innerListNum = 0; innerListNum <
				foundList.meshesPerMaterials.Count; innerListNum++)
			{
				MeshesPerMaterial innerList =
					foundList.meshesPerMaterials[innerListNum];

				// Check if this is the same material and we can use it instead.
				// For our purposes it is sufficiant if we check textures and colors.
				if (innerList.material.diffuseTexture == material.diffuseTexture &&
					innerList.material.normalTexture == material.normalTexture &&
					innerList.material.ambientColor == material.ambientColor &&
					innerList.material.diffuseColor == material.diffuseColor &&
					innerList.material.specularColor == material.specularColor &&
					innerList.material.specularPower == material.specularPower)
				{
					// Reuse this material and quit this search
					material = innerList.material;
					break;
				} // if (innerList.material.diffuseTexture)
			} // foreach (innerList)
			
			// Build new RenderableMesh object
			RenderableMesh mesh = new RenderableMesh(
				vertexBuffer, indexBuffer, material, foundList.technique,
				part.VertexDeclaration,
				part.StreamOffset, part.VertexStride, part.BaseVertex,
				part.NumVertices, part.StartIndex, part.PrimitiveCount);
			foundList.Add(mesh);
			return mesh;
		} // Add(vertexBuffer, indexBuffer, part)
		#endregion

		#region Render
		/// <summary>
		/// Render all meshes we collected this frame sorted by techniques
		/// and materials. This method is about 3-5 times faster than just
		/// using Model's Mesh.Draw method (see commented out code there).
		/// The reason for that is that we require only very few state changes
		/// and render everthing down as fast as we can. The only optimization
		/// left would be to put vertices of several meshes together if they
		/// are static and use the same technique and material. But since
		/// meshes have WriteOnly vertex and index buffers, we can't do that
		/// without using a custom model format.
		/// </summary>
		public void Render()
		{
			// Copy over last frame's object to this frame
			for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
			{
				MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];
				for (int meshNum = 0; meshNum < list.meshesPerMaterials.Count; meshNum++)
				{
					MeshesPerMaterial list2 = list.meshesPerMaterials[meshNum];
					for (int num = 0; num < list2.meshes.Count; num++)
					{
						RenderableMesh mesh = list2.meshes[num];
						// Copy over last frame matrices for rendering now!
						mesh.lastFrameRenderMatricesAndAlpha =
							mesh.thisFrameRenderMatricesAndAlpha;
						// Clear list for this frame
						mesh.thisFrameRenderMatricesAndAlpha =
							new List<MatrixAndAlpha>();
					} // for
				} // for
			} // for

			// Make sure z buffer is on for 3D content
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

			// We always use the normalMapping shader here.
			Effect effect = BaseGame.ParallaxShader.Effect;
			// Set general parameters for the shader
			BaseGame.ParallaxShader.SetParametersOptimizedGeneral();

			// Don't set vertex buffer again if it does not change this frame.
			// Clear these remember settings.
			lastVertexBufferSet = null;
			lastIndexBufferSet = null;

			//obs: foreach (MeshesPerMaterialPerTechniques list in sortedMeshes)
			for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
			{
				MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];

				if (list.NumberOfRenderMatrices > 0)
					list.Render(effect);
			} // for (listNum)
		} // Render()
		#endregion

		#region ClearAll
		/// <summary>
		/// Clear all objects in the render list in case our device got lost.
		/// </summary>
		public void ClearAll()
		{
			// Copy over last frame's object to this frame
			for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
			{
				MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];
				for (int meshNum = 0; meshNum < list.meshesPerMaterials.Count; meshNum++)
				{
					MeshesPerMaterial list2 = list.meshesPerMaterials[meshNum];
					for (int num = 0; num < list2.meshes.Count; num++)
					{
						RenderableMesh mesh = list2.meshes[num];
						// Clear all
						mesh.lastFrameRenderMatricesAndAlpha.Clear();
						mesh.thisFrameRenderMatricesAndAlpha.Clear();
					} // for
				} // for
			} // for
		} // ClearAll()
		#endregion
	} // class MeshRenderManager
} // namespace RocketCommanderXna.Graphics
