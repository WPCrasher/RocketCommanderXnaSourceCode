// Author: abi
// Project: RocketCommanderXna
// Path: X:\Xna\RocketCommanderXna\Shaders
// Creation date: 27.01.2008 20:42
// Last modified: 30.01.2008 00:15

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using RocketCommanderXna.Game;
using RocketCommanderXna.Helpers;
using Model = RocketCommanderXna.Graphics.Model;
using Texture = RocketCommanderXna.Graphics.Texture;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
using RocketCommanderXna.Graphics;
#endregion

namespace RocketCommanderXna.Shaders
{
	/// <summary>
	/// Render to texture helper class based on the Texture class.
	/// This class allows to render stuff onto textures, if thats not
	/// supported, it will just not work and report an engine log message.
	/// This class is required for most PostScreenShaders.
	/// </summary>
	public class RenderToTexture : Texture
	{
		#region Variables
		/// <summary>
		/// Our render target we are going to render to. Much easier than in MDX
		/// where you have to use Surfaces, etc. Also supports the Xbox360 model
		/// of resolving the render target texture before we can use it, otherwise
		/// the RenderToTexture class would not work on the Xbox360.
		/// </summary>
		RenderTarget2D renderTarget = null;

		/// <summary>
		/// Z buffer surface for shadow mapping render targets that do not
		/// fit in our resolution. Usually unused!
		/// </summary>
		DepthStencilBuffer zBufferSurface = null;
		/// <summary>
		/// ZBuffer surface
		/// </summary>
		/// <returns>Surface</returns>
		public DepthStencilBuffer ZBufferSurface
		{
			get
			{
				return zBufferSurface;
			} // get
		} // ZBufferSurface

		/// <summary>
		/// Posible size types for creating a RenderToTexture object.
		/// </summary>
		public enum SizeType
		{
			/// <summary>
			/// Uses the full screen size for this texture
			/// </summary>
			FullScreen,
			/// <summary>
			/// Use full screen and force z buffer (sometimes needed for correct
			/// rendering).
			/// </summary>
			FullScreenWithZBuffer,
			/// <summary>
			/// Uses half the full screen size, e.g. 800x600 becomes 400x300
			/// </summary>
			HalfScreen,
			/// <summary>
			/// Use half screen and force z buffer (sometimes needed for correct
			/// rendering).
			/// </summary>
			HalfScreenWithZBuffer,
			/// <summary>
			/// Uses a quarter of the full screen size, e.g. 800x600 becomes 200x150
			/// </summary>
			QuarterScreen,
			/// <summary>
			/// Shadow map texture, usually 1024x1024, but can also be better
			/// like 2048x2048 or 4096x4096.
			/// </summary>
			ShadowMap,
		} // enum SizeTypes

		/// <summary>
		/// Size type
		/// </summary>
		private SizeType sizeType;

		/// <summary>
		/// Use full screen and render into a HDR 16 bit per channel texture.
		/// If 16bit is not available, we try 32 bit per channel and if that
		/// does not work either we just use 8bit (non-hdr) rendering!
		/// In HDR mode all rendering operations are devided by 16, this
		/// gives us 16 times brighter lights if desired or we can go 16 times
		/// darker without losing percision (16 brighter*16 darker*256 colors
		/// results to 65536, which is 16 bit).
		/// </summary>
		private bool useHdrRenderTarget = false;

		/// <summary>
		/// Calc size
		/// </summary>
		private void CalcSize()
		{
			switch (sizeType)
			{
				case SizeType.FullScreen:
				case SizeType.FullScreenWithZBuffer:
					texWidth = BaseGame.Width;
					texHeight = BaseGame.Height;
					break;
				case SizeType.HalfScreen:
				case SizeType.HalfScreenWithZBuffer:
					texWidth = BaseGame.Width / 2;
					texHeight = BaseGame.Height / 2;
					break;
				case SizeType.QuarterScreen:
					texWidth = BaseGame.Width / 4;
					texHeight = BaseGame.Height / 4;
					break;
				case SizeType.ShadowMap:
					texWidth = 512;// 1024;// 512;// 256;// 1024;
					texHeight = 512;// 1024;// 512;// 256;// 1024;
					break;
			} // switch
			CalcHalfPixelSize();
		} // CalcSize()

		/// <summary>
		/// Does this texture use some high percision format? Better than 8 bit color?
		/// </summary>
		private bool usesHighPercisionFormat = false;
		#endregion

		#region Properties
		/// <summary>
		/// Render target
		/// </summary>
		/// <returns>Render target 2D</returns>
		public RenderTarget2D RenderTarget
		{
			get
			{
				return renderTarget;
			} // get
		} // RenderTarget

		/// <summary>
		/// Override how to get XnaTexture, we have to resolve the render target
		/// for supporting the Xbox, which requires calling Resolve first!
		/// After that you can call this property to get the current texture.
		/// </summary>
		/// <returns>XnaTexture</returns>
		public override XnaTexture XnaTexture
		{
			get
			{
				if (alreadyResolved)
					internalXnaTexture = renderTarget.GetTexture();
				else
					internalXnaTexture = null;
				return internalXnaTexture;
			} // get
		} // XnaTexture

		/// <summary>
		/// Does this texture use some high percision format? Better than 8 bit color?
		/// </summary>
		public bool UsesHighPercisionFormat
		{
			get
			{
				return usesHighPercisionFormat;
			} // get
		} // UsesHighPercisionFormat

		/// <summary>
		/// Is render target valid? Will be false if loading failed.
		/// </summary>
		public override bool IsValid
		{
			get
			{
				return loaded &&
					renderTarget != null;
			} // get
		} // IsValid

		/// <summary>
		/// Back buffer depth format
		/// </summary>
		static DepthFormat backBufferDepthFormat = DepthFormat.Depth32;
		/// <summary>
		/// Back buffer depth format
		/// </summary>
		/// <returns>Surface format</returns>
		public static DepthFormat BackBufferDepthFormat
		{
			get
			{
				return backBufferDepthFormat;
			} // get
		} // BackBufferDepthFormat

		/// <summary>
		/// Remember multi sample type in Initialize for later use
		/// in RenderToTexture!
		/// </summary>
		static MultiSampleType remMultiSampleType = MultiSampleType.None;
		/// <summary>
		/// MultiSampleType
		/// </summary>
		public static MultiSampleType MultiSampleType
		{
			get
			{
				return remMultiSampleType;
			} // get
		} // MultiSampleType

		/// <summary>
		/// Remember multi sample quality.
		/// </summary>
		static int remMultiSampleQuality = 0;
		/// <summary>
		/// Multi sample quality
		/// </summary>
		public static int MultiSampleQuality
		{
			get
			{
				return remMultiSampleQuality;
			} // get
		} // MultiSampleQuality

		/// <summary>
		/// Remember depth stencil buffer in case we have to reset it!
		/// </summary>
		static DepthStencilBuffer remDepthBuffer = null;

		/// <summary>
		/// Initialize depth buffer format and multisampling
		/// </summary>
		/// <param name="setPreferredDepthStencilFormat">Set preferred depth
		/// stencil format</param>
		public static void InitializeDepthBufferFormatAndMultisampling(
			DepthFormat setPreferredDepthStencilFormat)
		{
			backBufferDepthFormat = setPreferredDepthStencilFormat;
			remMultiSampleType =
				BaseGame.Device.PresentationParameters.MultiSampleType;
			if (remMultiSampleType == MultiSampleType.NonMaskable)
				remMultiSampleType = MultiSampleType.None;
			remMultiSampleQuality =
				BaseGame.Device.PresentationParameters.MultiSampleQuality;
			remDepthBuffer = BaseGame.Device.DepthStencilBuffer;
		} // InitializeDepthBufferFormatAndMultisampling(setPreferredDepthStenci)
		#endregion

		#region Constructors
		/// <summary>
		/// Id for each created RenderToTexture for the generated filename.
		/// </summary>
		private static int RenderToTextureGlobalInstanceId = 0;
		/// <summary>
		/// Creates an offscreen texture with the specified size which
		/// can be used for render to texture.
		/// </summary>
		public RenderToTexture(SizeType setSizeType, bool setUseHdrRenderTarget)
		{
			sizeType = setSizeType;
			useHdrRenderTarget = setUseHdrRenderTarget;

			CalcSize();

			texFilename = "RenderToTexture instance " +
				RenderToTextureGlobalInstanceId++;

			Create();

			AddRemRenderToTexture(this);
		} // RenderToTexture(setSizeType, setUseHdrRenderTarget)

		/// <summary>
		/// Create render to texture, using no HDR here.
		/// </summary>
		/// <param name="setSizeType">Set size type</param>
		public RenderToTexture(SizeType setSizeType)
			: this(setSizeType, false)
		{
		} // RenderToTexture(setSizeType)
		#endregion

		#region Handle device reset
		/// <summary>
		/// Handle the DeviceReset event for a certain render target, re-create
		/// our render target.
		/// </summary>
		private void DeviceReset()
		{
			// Just recreate it.
			Create();
			alreadyResolved = false;
		} // DeviceReset()

		/// <summary>
		/// Handle the DeviceReset event, we have to re-create all our render
		/// targets.
		/// </summary>
		public static void HandleDeviceReset()
		{
			foreach (RenderToTexture renderToTexture in remRenderToTextures)
				renderToTexture.DeviceReset();
		} // HandleDeviceReset()
		#endregion

		#region Create
		/// <summary>
		/// Check if we can use a specific surface format for render targets.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		private bool CheckRenderTargetFormat(SurfaceFormat format)
		{
			return BaseGame.Device.CreationParameters.Adapter.CheckDeviceFormat(
				BaseGame.Device.CreationParameters.DeviceType,
				BaseGame.Device.DisplayMode.Format,
				TextureUsage.None,
				QueryUsages.None,
				ResourceType.RenderTarget,
				format);
		} // CheckRenderTargetFormat(format)

		/// <summary>
		/// Always create render target depth buffer
		/// </summary>
		public static bool alwaysCreateRenderTargetDepthBuffer = false;
		/// <summary>
		/// Create
		/// </summary>
		private void Create()
		{
			/*obs
#if XBOX360
			// Limit to current resolution on Xbox360 (we won't create a depth
			// buffer below)
			if (texWidth > BaseGame.Width)
				texWidth = BaseGame.Width;
			if (texHeight > BaseGame.Height)
				texHeight = BaseGame.Height;
#endif
			 */

			SurfaceFormat format = SurfaceFormat.Color;
			// Try to use R32F format for shadow mapping if possible (ps20),
			// else just use A8R8G8B8 format for shadow mapping and
			// for normal RenderToTextures too.
			if (sizeType == SizeType.ShadowMap)
			{
				// Can do R32F format?
				if (CheckRenderTargetFormat(SurfaceFormat.Single))
					format = SurfaceFormat.Single;
				// Else try R16F format, thats still much better than A8R8G8B8
				else if (CheckRenderTargetFormat(SurfaceFormat.HalfSingle))
					format = SurfaceFormat.HalfSingle;
				// And check a couple more formats (mainly for the Xbox360 support)
				else if (CheckRenderTargetFormat(SurfaceFormat.HalfVector2))
					format = SurfaceFormat.HalfVector2;
				else if (CheckRenderTargetFormat(SurfaceFormat.Luminance16))
					format = SurfaceFormat.Luminance16;
				// Else nothing found, well, then just use the 8 bit Color format.
			} // if (sizeType)
			// HDR Format? We want to use a floating point format to make it
			// easier in the shaders, so R10G10B10A2 is not a good choice
			// and usually that will crash anyway once we resolve the texture!
			else if (useHdrRenderTarget)
			{
				// Can we do 16 bits per channel?
				if (CheckRenderTargetFormat(SurfaceFormat.HalfVector4))
					format = SurfaceFormat.HalfVector4;
				// Else try to use 32 bits per channel.
				else if (CheckRenderTargetFormat(SurfaceFormat.HalfVector4))
					format = SurfaceFormat.Vector4;
				// If that does not work, do not use any HDR at all, just leave
				// the 4*8 bit default color mode for the surface format.
			} // else if

			try
			{
				// Create render target of specified size.
				renderTarget = new RenderTarget2D(BaseGame.Device,
					texWidth, texHeight, 1, format,
					MultiSampleType, MultiSampleQuality);
				if (format != SurfaceFormat.Color)
					usesHighPercisionFormat = true;

				// Unsupported on Xbox360, will crash with InvalidOperationException
				//obs: #if !XBOX360
				//*always required, some gfx cards want always the same depth buffer size
				// * as the renderTarget size!
				// Create z buffer surface for shadow map render targets
				// if they don't fit in our current resolution.
				if (sizeType == SizeType.FullScreenWithZBuffer ||
					sizeType == SizeType.HalfScreenWithZBuffer ||
					sizeType == SizeType.ShadowMap &&
					(texWidth > BaseGame.Width ||
					texHeight > BaseGame.Height))// ||
				//alwaysCreateRenderTargetDepthBuffer)
				//*/
				{
					zBufferSurface = new DepthStencilBuffer(BaseGame.Device,
						texWidth, texHeight,
						// Lets use the same stuff as the back buffer.
						BackBufferDepthFormat,
						// Don't use multisampling, render target does not support that.
						//obs: MultiSampleType.None, 0);
						MultiSampleType, MultiSampleQuality);
				} // if (sizeType)
				//#endif
				loaded = true;
			} // try
			catch (Exception ex)
			{
				// Everything failed, make this unuseable.
				Log.Write("Creating RenderToTexture failed: " + ex.ToString());
				renderTarget = null;
				internalXnaTexture = null;
				loaded = false;
			} // catch
		} // Create()
		#endregion

		#region Clear
		/// <summary>
		/// Clear render target (call SetRenderTarget first)
		/// </summary>
		public void Clear(Color clearColor)
		{
			if (loaded == false ||
				renderTarget == null)
				return;

			BaseGame.Device.Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer,
				clearColor, 1.0f, 0);
		} // Clear(clearColor)
		#endregion

		#region Resolve
		/// <summary>
		/// Make sure we don't call XnaTexture before resolving for the first time!
		/// </summary>
		bool alreadyResolved = false;
		/// <summary>
		/// Resolve render target. For windows developers this method may seem
		/// strange, why not just use the rendertarget's texture? Well, this is
		/// just for the Xbox360 support. The Xbox requires that you call Resolve
		/// first before using the rendertarget texture. The reason for that is
		/// copying the data over from the EPRAM to the video memory, for more
		/// details read the XNA docs.
		/// Note: This method will only work if the render target was set before
		/// with SetRenderTarget, else an exception will be thrown to ensure
		/// correct calling order.
		/// </summary>
		public void Resolve(bool fullResetToBackBuffer)
		{
			// Make sure this render target is currently set!
			if (RenderToTexture.CurrentRenderTarget != renderTarget)
				throw new Exception(
					"You can't call Resolve without first setting the render target!");

			alreadyResolved = true;
			//does not exist anymore: BaseGame.Device.res.ResolveRenderTarget(0);

			// Instead just restore the back buffer (will automatically resolve)
			ResetRenderTarget(fullResetToBackBuffer);
		} // Resolve()
		#endregion

		#region Set and reset render targets
		/// <summary>
		/// Remember scene render target. This is very important because
		/// for our post screen shaders we have to render our whole scene
		/// to this render target. But in the process we will use many other
		/// shaders and they might set their own render targets and then
		/// reset it, but we need to have this scene still to be set.
		/// Don't reset to the back buffer (with SetRenderTarget(0, null), this
		/// would stop rendering to our scene render target and the post screen
		/// shader will not be able to process our screen.
		/// The whole reason for this is that we can't use StrechRectangle
		/// like in Rocket Commander because XNA does not provide that function
		/// (the reason for that is cross platform compatibility with the XBox360).
		/// Instead we could use ResolveBackBuffer, but that method is VERY SLOW.
		/// Our framerate would drop from 600 fps down to 20, not good.
		/// However, multisampling will not work, so we will disable it anyway!
		/// </summary>
		static RenderTarget2D remSceneRenderTarget = null;
		/// <summary>
		/// Remember the last render target we set, this way we can check
		/// if the rendertarget was set before calling resolve!
		/// </summary>
		static RenderTarget2D lastSetRenderTarget = null;

		/// <summary>
		/// Remember render to texture instances to allow recreating them all
		/// when DeviceReset is called.
		/// </summary>
		static List<RenderToTexture> remRenderToTextures =
			new List<RenderToTexture>();

		/// <summary>
		/// Add render to texture instance to allow recreating them all
		/// when DeviceReset is called with help of the remRenderToTextures list. 
		/// </summary>
		public static void AddRemRenderToTexture(RenderToTexture renderToTexture)
		{
			remRenderToTextures.Add(renderToTexture);
		} // AddRemRenderToTexture(renderToTexture)

		/// <summary>
		/// Current render target we have set, null if it is just the back buffer.
		/// </summary>
		public static RenderTarget2D CurrentRenderTarget
		{
			get
			{
				return lastSetRenderTarget;
			} // get
		} // CurrentRenderTarget

		/// <summary>
		/// Set render target to this texture to render stuff on it.
		/// </summary>
		public bool SetRenderTarget()
		{
			if (loaded == false ||
				renderTarget == null)
				return false;

			SetRenderTarget(renderTarget, false);
			if (zBufferSurface != null)
				BaseGame.Device.DepthStencilBuffer = zBufferSurface;
			return true;
		} // SetRenderTarget()

		/// <summary>
		/// Set render target
		/// </summary>
		/// <param name="isSceneRenderTarget">Is scene render target</param>
		internal static void SetRenderTarget(RenderTarget2D renderTarget,
			bool isSceneRenderTarget)
		{
			BaseGame.Device.SetRenderTarget(0, renderTarget);
			if (isSceneRenderTarget)
				remSceneRenderTarget = renderTarget;
			lastSetRenderTarget = renderTarget;
		} // SetRenderTarget(renderTarget, isSceneRenderTarget)

		/// <summary>
		/// Reset render target
		/// </summary>
		/// <param name="fullResetToBackBuffer">Full reset to back buffer</param>
		internal static void ResetRenderTarget(bool fullResetToBackBuffer)
		{
			if (remSceneRenderTarget == null ||
				fullResetToBackBuffer)
			{
				remSceneRenderTarget = null;
				lastSetRenderTarget = null;
				BaseGame.Device.SetRenderTarget(0, null);
				BaseGame.Device.DepthStencilBuffer = remDepthBuffer;
			} // if (remSceneRenderTarget)
			else
			{
				BaseGame.Device.SetRenderTarget(0, remSceneRenderTarget);
				lastSetRenderTarget = remSceneRenderTarget;
			} // else
		} // ResetRenderTarget(fullResetToBackBuffer)
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test create render to texture
		/// </summary>
		static public void TestCreateRenderToTexture()
		{
			Model testModel = null;
			RenderToTexture renderToTexture = null;

			TestGame.Start(
				"TestCreateRenderToTexture",
				delegate
				{
					testModel = new Model("asteroid1");
					renderToTexture = new RenderToTexture(
						//SizeType.QuarterScreen);
						SizeType.HalfScreen);
						//SizeType.HalfScreenWithZBuffer);
						//SizeType.FullScreen);
						//SizeType.ShadowMap);
				},
				delegate
				{
					bool renderToTextureWay =
						Input.Keyboard.IsKeyUp(Keys.Space) &&
						Input.GamePadAPressed == false;
					BaseGame.Device.RenderState.DepthBufferEnable = true;

					if (renderToTextureWay)
					{
						// Set render target to our texture
						renderToTexture.SetRenderTarget();

						// Clear background
						renderToTexture.Clear(Color.Blue);

						// Draw background lines
						//Line.DrawGrid();
						//Ui.LineManager.RenderAll3DLines();

						// And draw object
						testModel.Render(Matrix.CreateScale(7.5f));
						//BaseGame.RenderManager.RenderAllMeshes();

						// Do we need to resolve?
						renderToTexture.Resolve(true);
						//BaseGame.Device.ResolveRenderTarget(0);

						// Reset background buffer
						//not longer required, done in Resolve now:
						//RenderToTexture.ResetRenderTarget(true);
					} // if (renderToTextureWay)
					else
					{
						// Copy backbuffer way, render stuff normally first
						// Clear background
						BaseGame.Device.Clear(Color.Blue);

						// Draw background lines
						//Line.DrawGrid();
						//Ui.LineManager.RenderAll3DLines();

						// And draw object
						testModel.Render(Matrix.CreateScale(7.5f));
						//BaseGame.RenderManager.RenderAllMeshes();
					} // else

					// Show render target in a rectangle on our screen
					renderToTexture.RenderOnScreen(
						//tst:
						new Rectangle(100, 100, 256, 256));
					//BaseGame.ScreenRectangle);
					//no need: BaseGame.UI.FlushUI();

					TextureFont.WriteText(2, 0,
						"               Press Space to toogle full screen rendering");
					TextureFont.WriteText(2, 30,
						"renderToTexture.Width=" + renderToTexture.Width);
					TextureFont.WriteText(2, 60,
						"renderToTexture.Height=" + renderToTexture.Height);
					TextureFont.WriteText(2, 90,
						"renderToTexture.Valid=" + renderToTexture.IsValid);
					TextureFont.WriteText(2, 120,
						"renderToTexture.XnaTexture=" + renderToTexture.XnaTexture);
					TextureFont.WriteText(2, 150,
						"renderToTexture.ZBufferSurface=" + renderToTexture.ZBufferSurface);
					TextureFont.WriteText(2, 180,
						"renderToTexture.Filename=" + renderToTexture.Filename);
				});
		} // TestCreateRenderToTexture()
		//*/
#endif
		#endregion
	} // class RenderToTexture
} // namespace RocketCommanderXna.Shaders
