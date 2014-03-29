// Project: RocketCommanderXna, File: TextureFont.cs
// Namespace: RocketCommanderXna.Graphics, Class: TextureFont
// Path: C:\code\XnaBook\RocketCommanderXna\Graphics, Author: abi
// Code lines: 631, Size of file: 18,98 KB
// Creation date: 07.12.2006 18:22
// Last modified: 07.12.2006 22:14
// Generated with Commenter by abi.exDream.com

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RocketCommanderXna.Game;
using RocketCommanderXna.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Texture font for our game, uses GameFont.png.
	/// If you want to know more details about creating bitmap fonts in XNA,
	/// how to generate the bitmaps and more details about using it, please
	/// check out the following links:
	/// http://blogs.msdn.com/garykac/archive/2006/08/30/728521.aspx
	/// http://blogs.msdn.com/garykac/articles/732007.aspx
	/// http://www.angelcode.com/products/bmfont/
	/// </summary>
	public class TextureFont : IGraphicContent
	{
		#region Constants
		/// <summary>
		/// Game font filename for our bitmap.
		/// </summary>
		const string GameFontFilename = "GameFont";

		/// <summary>
		/// TextureFont height
		/// </summary>
		const int FontHeight = 36;//first try texture: 25;
		
		/// <summary>
		/// Substract this value from the y postion when rendering.
		/// Most letters start below the CharRects, this fixes that issue.
		/// </summary>
		const int SubRenderHeight = 5;//7;//6;

		/// <summary>
		/// Char rectangles, goes from space (32) to ~ (126).
		/// Height is not used (always the same), instead we save the actual
		/// used width for rendering in the height value!
		/// This are the characters:
		///  !"#$%&'()*+,-./0123456789:;<=>?@
		/// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
		/// abcdefghijklmnopqrstuvwxyz{|}~
		/// Then we also got 4 extra rects for the XBox Buttons: A, B, X, Y
		/// </summary>
		static Rectangle[] CharRects = new Rectangle[126 - 32 + 1]
		{
			new Rectangle(0, 0, 1, 8), // space
			new Rectangle(1, 0, 11, 10),
			new Rectangle(12, 0, 14, 13),
			new Rectangle(26, 0, 20, 18),
			new Rectangle(46, 0, 20, 18),
			new Rectangle(66, 0, 24, 22),
			new Rectangle(90, 0, 25, 23),
			new Rectangle(115, 0, 8, 7),
			new Rectangle(124, 0, 10, 9),
			new Rectangle(136, 0, 10, 9),
			new Rectangle(146, 0, 20, 18),
			new Rectangle(166, 0, 20, 18),
			new Rectangle(186, 0, 10, 8),
			new Rectangle(196, 0, 10, 9),
			new Rectangle(207, 0, 10, 8),
			new Rectangle(217, 0, 18, 16),
			new Rectangle(235, 0, 20, 19),

			new Rectangle(0, 36, 20, 18), // 1
			new Rectangle(20, 36, 20, 18),
			new Rectangle(40, 36, 20, 18),
			new Rectangle(60, 36, 21, 19),
			new Rectangle(81, 36, 20, 18),
			new Rectangle(101, 36, 20, 18),
			new Rectangle(121, 36, 20, 18),
			new Rectangle(141, 36, 20, 18),
			new Rectangle(161, 36, 20, 18), // 9
			new Rectangle(181, 36, 10, 8),
			new Rectangle(191, 36, 10, 8),
			new Rectangle(201, 36, 20, 18),
			new Rectangle(221, 36, 20, 18),

			new Rectangle(0, 72, 20, 18), // >
			new Rectangle(20, 72, 19, 17),
			new Rectangle(39, 72, 26, 24),
			new Rectangle(65, 72, 22, 20),
			new Rectangle(87, 72, 22, 20),
			new Rectangle(109, 72, 22, 20),
			new Rectangle(131, 72, 23, 21),
			new Rectangle(154, 72, 20, 18),
			new Rectangle(174, 72, 19, 17),
			new Rectangle(193, 72, 23, 21),
			new Rectangle(216, 72, 23, 21),
			new Rectangle(239, 72, 11, 10),

			new Rectangle(0, 108, 15, 13), // J
			new Rectangle(15, 108, 22, 20),
			new Rectangle(37, 108, 19, 17),
			new Rectangle(56, 108, 29, 26),
			new Rectangle(85, 108, 23, 21),
			new Rectangle(108, 108, 24, 22), // O
			new Rectangle(132, 108, 22, 20),
			new Rectangle(154, 108, 24, 22),
			new Rectangle(178, 108, 24, 22),
			new Rectangle(202, 108, 21, 19),
			new Rectangle(223, 108, 16, 15),

			new Rectangle(0, 144, 22, 20), // U
			new Rectangle(22, 144, 22, 20),
			new Rectangle(44, 144, 30, 28),
			new Rectangle(74, 144, 22, 20),
			new Rectangle(96, 144, 20, 18),
			new Rectangle(116, 144, 20, 18),
			new Rectangle(136, 144, 10, 9),
			new Rectangle(146, 144, 18, 16),
			new Rectangle(167, 144, 10, 9),
			new Rectangle(177, 144, 17, 16),
			new Rectangle(194, 144, 17, 16),
			new Rectangle(211, 144, 17, 16),
			new Rectangle(228, 144, 20, 18),

			new Rectangle(0, 180, 20, 18), // b
			new Rectangle(20, 180, 18, 16),
			new Rectangle(38, 180, 20, 18),
			new Rectangle(58, 180, 20, 18), // e
			new Rectangle(79, 180, 14, 12), // f
			new Rectangle(94, 180, 19, 18), // g
			new Rectangle(114, 180, 19, 18), // h
			new Rectangle(133, 180, 11, 9),
			new Rectangle(145, 180, 11, 9), // j
			new Rectangle(156, 180, 20, 18),
			new Rectangle(176, 180, 11, 9),
			new Rectangle(187, 180, 29, 27),
			new Rectangle(216, 180, 20, 18),
			new Rectangle(236, 180, 20, 19),

			new Rectangle(0, 216, 20, 18), // p
			new Rectangle(20, 216, 20, 18),
			new Rectangle(40, 216, 13, 12), // r
			new Rectangle(53, 216, 18, 16),
			new Rectangle(71, 216, 13, 11), // t
			new Rectangle(84, 216, 19, 18),
			new Rectangle(104, 216, 17, 16),
			new Rectangle(122, 216, 25, 23),
			new Rectangle(148, 216, 19, 17),
			new Rectangle(168, 216, 18, 16),
			new Rectangle(186, 216, 16, 15),
			new Rectangle(203, 216, 10, 9),
			new Rectangle(214, 216, 12, 11), // |
			new Rectangle(227, 216, 10, 9),
			new Rectangle(237, 216, 18, 17),
		};
		/*obs, first try
		/// <summary>
		/// XBox 360 Button rects, can just be added to your text :)
		/// Check out the unit test below.
		/// </summary>
		public const char XBoxAButton = (char)(126 + 1),
			XBoxBButton = (char)(126 + 2),
			XBoxXButton = (char)(126 + 3),
			XBoxYButton = (char)(126 + 4);

		/// <summary>
		/// Char rectangles, goes from space (32) to ~ (126).
		/// Height is not used (always the same), instead we save the actual
		/// used width for rendering in the height value!
		/// This are the characters:
		///  !"#$%&'()*+,-./0123456789:;<=>?@
		/// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
		/// abcdefghijklmnopqrstuvwxyz{|}~
		/// Then we also got 4 extra rects for the XBox Buttons: A, B, X, Y
		/// </summary>
		static Rectangle[] CharRects = new Rectangle[126 - 32 + 4 + 1]
		{
			new Rectangle(0, 0, 1, 6), // space
			new Rectangle(1, 0, 6, 5),
			new Rectangle(7, 0, 8, 6),
			new Rectangle(15, 0, 10, 9),
			new Rectangle(25, 0, 11, 10),
			new Rectangle(36, 0, 16, 14),
			new Rectangle(52, 0, 14, 12),
			new Rectangle(66, 0, 5, 3),
			new Rectangle(71, 0, 9, 7),
			new Rectangle(80, 0, 9, 7),
			new Rectangle(89, 0, 11, 9),
			new Rectangle(100, 0, 10, 9),
			new Rectangle(110, 0, 6, 5),
			new Rectangle(116, 0, 7, 5),
			new Rectangle(123, 0, 6, 5),
			new Rectangle(129, 0, 5, 3),
			new Rectangle(134, 0, 13, 11),
			new Rectangle(147, 0, 13, 11),
			new Rectangle(160, 0, 13, 11),
			new Rectangle(173, 0, 13, 11),
			new Rectangle(186, 0, 13, 11),
			new Rectangle(199, 0, 13, 11),
			new Rectangle(212, 0, 13, 11),
			new Rectangle(225, 0, 13, 11),
			new Rectangle(238, 0, 13, 11),
			new Rectangle(0, 25, 13, 11), // 9
			new Rectangle(13, 25, 6, 5),
			new Rectangle(19, 25, 6, 5),
			new Rectangle(25, 25, 10, 9),
			new Rectangle(35, 25, 10, 9),
			new Rectangle(45, 25, 10, 9),
			new Rectangle(55, 25, 9, 7),
			new Rectangle(64, 25, 16, 14),
			new Rectangle(80, 25, 15, 13),
			new Rectangle(95, 25, 13, 11),
			new Rectangle(108, 25, 12, 10),
			new Rectangle(120, 25, 13, 12),
			new Rectangle(133, 25, 11, 9),
			new Rectangle(144, 25, 11, 9),
			new Rectangle(155, 25, 13, 11),
			new Rectangle(168, 25, 14, 13),
			new Rectangle(182, 25, 8, 6),
			new Rectangle(190, 25, 9, 7),
			new Rectangle(199, 25, 14, 12),
			new Rectangle(213, 25, 11, 9),
			new Rectangle(224, 25, 16, 14),
			new Rectangle(240, 25, 13, 12),
			new Rectangle(0, 50, 13, 11), // O
			new Rectangle(13, 50, 12, 11),
			new Rectangle(25, 50, 13, 11),
			new Rectangle(38, 50, 13, 12),
			new Rectangle(51, 50, 11, 9),
			new Rectangle(62, 50, 11, 9),
			new Rectangle(73, 50, 13, 11),
			new Rectangle(86, 50, 13, 11),
			new Rectangle(99, 50, 18, 16),
			new Rectangle(117, 50, 14, 13),
			new Rectangle(131, 50, 14, 12),
			new Rectangle(145, 50, 12, 11),
			new Rectangle(157, 50, 8, 7),
			new Rectangle(165, 50, 5, 3),
			new Rectangle(170, 50, 8, 7),
			new Rectangle(178, 50, 10, 8),
			new Rectangle(188, 50, 8, 6),
			new Rectangle(196, 50, 5, 4),
			new Rectangle(201, 50, 11, 10),
			new Rectangle(212, 50, 12, 10),
			new Rectangle(224, 50, 10, 9),
			new Rectangle(234, 50, 12, 10),
			new Rectangle(0, 75, 11, 9), // e
			new Rectangle(11, 75, 9, 7),
			new Rectangle(20, 75, 11, 10),
			new Rectangle(31, 75, 12, 10),
			new Rectangle(43, 75, 7, 5),
			new Rectangle(50, 75, 8, 6),
			new Rectangle(58, 75, 12, 10),
			new Rectangle(70, 75, 7, 5),
			new Rectangle(77, 75, 18, 16),
			new Rectangle(95, 75, 12, 10),
			new Rectangle(107, 75, 11, 9),
			new Rectangle(118, 75, 12, 10),
			new Rectangle(130, 75, 12, 10),
			new Rectangle(142, 75, 10, 9),
			new Rectangle(152, 75, 9, 8),
			new Rectangle(161, 75, 9, 7),
			new Rectangle(170, 75, 12, 10),
			new Rectangle(182, 75, 11, 9),
			new Rectangle(193, 75, 15, 13),
			new Rectangle(208, 75, 13, 11),
			new Rectangle(221, 75, 11, 10),
			new Rectangle(232, 75, 10, 8),
			new Rectangle(242, 75, 6, 5),
			new Rectangle(0, 100, 10, 9), // |
			new Rectangle(10, 100, 6, 5),
			new Rectangle(16, 100, 10, 8),
			// 4 extra chars for the xbox controller buttons
			new Rectangle(29, 100, 20, 21),
			new Rectangle(49, 100, 20, 21),
			new Rectangle(69, 100, 20, 21),
			new Rectangle(89, 100, 20, 21),
		};
		 */
		#endregion

		#region Variables
		/// <summary>
		/// TextureFont texture
		/// </summary>
		Texture fontTexture;
		/// <summary>
		/// TextureFont sprite
		/// </summary>
		SpriteBatch fontSprite; 
		#endregion

		#region Properties
		/// <summary>
		/// Height
		/// </summary>
		/// <returns>Int</returns>
		public static int Height
		{
			get
			{
				return FontHeight - SubRenderHeight;
			} // get
		} // Height
		#endregion

		#region Constructor
		/// <summary>
		/// Create texture font
		/// </summary>
		public TextureFont()
		{
			Load();

			BaseGame.RegisterGraphicContentObject(this);
		} // TextureFont()
    #endregion

		#region Load
		/// <summary>
		/// Load the font, also used for reloading.
		/// </summary>
		public void Load()
		{
			if (fontTexture == null)
				fontTexture = new Texture(GameFontFilename);
			if (fontSprite == null)
				fontSprite = new SpriteBatch(BaseGame.Device);
		} // Load()
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (fontTexture != null)
				fontTexture.Dispose();
			fontTexture = null;
			if (fontSprite != null)
				fontSprite.Dispose();
			fontSprite = null;
		} // Dispose()
		#endregion

		#region Get text width
		/// <summary>
		/// Get the text width of a given text.
		/// </summary>
		/// <param name="text">Text</param>
		/// <returns>Width (in pixels) of the text</returns>
		public int GetTextWidth(string text)
		{
			int width = 0;
			//foreach (char c in text)
			char[] chars = text.ToCharArray();
			for (int num = 0; num < chars.Length; num++)
			{
				int charNum = (int)chars[num];
				if (charNum >= 32 &&
					charNum - 32 < CharRects.Length)
					width += CharRects[charNum - 32].Height;
			} // foreach
			return width;
		} // GetTextWidth(text)
		#endregion

		#region Write (unused, use UIRenderer.WriteText!)
		/*
		/// <summary>
		/// Write the given text at the specified position.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="color">Color</param>
		/// <returns>Write of font we wrote</returns>
		public int Write(int x, int y, string text, Color color)
		{
			int width = 0;

			// Start rendering
			fontSprite.Begin(SpriteBlendMode.AlphaBlend);

			// Draw each character in the text
			foreach (char c in text)
			{
				int charNum = (int)c;
				if (charNum >= 32 &&
					charNum - 32 < CharRects.Length)
				{
					// Draw this char
					Rectangle rect = CharRects[charNum - 32];
					rect.Height = Height;
					Rectangle destRect = new Rectangle(x, y - SubRenderHeight,
						rect.Width, rect.Height);
					//TODO: if we want upscaling, just use destRect
					fontSprite.Draw(fontTexture.XNA2DTexture, destRect, rect, color);//,
						// Make sure fonts are always displayed at the front of everything
						//0, Vector2.Zero, SpriteEffects.None, 1.1f);

					// Increase x pos by width we use for this character
					int charWidth = CharRects[charNum - 32].Height;
					x += charWidth;
					width += charWidth;
				} // if (charNum)
			} // foreach

			// End rendering
			fontSprite.End();

			return width;
		} // Write(x, y, text)

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <returns>Int</returns>
		public int Write(int x, int y, string text)
		{
			return Write(x, y, text, Color.White);
		} // Write(x, y, text)
		 */
		#endregion

		#region Write all
		/// <summary>
		/// TextureFont to render
		/// </summary>
		internal class FontToRender
		{
			#region Variables
			/// <summary>
			/// X and y position
			/// </summary>
			public int x, y;
			/// <summary>
			/// Text
			/// </summary>
			public string text;
			/// <summary>
			/// Color
			/// </summary>
			public Color color;
			#endregion

			#region Constructor
			/// <summary>
			/// Create font to render
			/// </summary>
			/// <param name="setX">Set x</param>
			/// <param name="setY">Set y</param>
			/// <param name="setText">Set text</param>
			/// <param name="setColor">Set color</param>
			public FontToRender(int setX, int setY, string setText, Color setColor)
			{
				x = setX;
				y = setY;
				text = setText;
				color = setColor;
			} // FontToRender(setX, setY, setText)
			#endregion
		} // class FontToRender

		/// <summary>
		/// Remember font texts to render to render them all at once
		/// in our Render method (beween rest of the ui and the mouse cursor).
		/// </summary>
		static List<FontToRender> remTexts = new List<FontToRender>();

		/// <summary>
		/// Write the given text at the specified position.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="color">Color</param>
		public static void WriteText(int x, int y, string text, Color color)
		{
			remTexts.Add(new FontToRender(x, y, text, color));
		} // WriteText(x, y, text)

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteText(int x, int y, string text)
		{
			remTexts.Add(new FontToRender(x, y, text, Color.White));
		} // WriteText(x, y, text)

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteText(Point pos, string text)
		{
			remTexts.Add(new FontToRender(pos.X, pos.Y, text, Color.White));
		} // WriteText(pos, text)

		/// <summary>
		/// Write small text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteSmallTextCentered(int x, int y, string text,
			Color col, float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(col, alpha));
		} // WriteSmallTextCentered(x, y, text)

		/// <summary>
		/// Write small text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteSmallTextCentered(int x, int y, string text,
			float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(Color.White, alpha));
		} // WriteSmallTextCentered(x, y, text)

		/// <summary>
		/// Write text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteTextCentered(int x, int y, string text)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text);
		} // WriteTextCentered(x, y, text)

		/// <summary>
		/// Write text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteTextCentered(int x, int y, string text,
			Color col, float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(col, alpha));
		} // WriteTextCentered(x, y, text)

		/// <summary>
		/// Write all
		/// </summary>
		/// <param name="texts">Texts</param>
		public void WriteAll()
		{
			if (remTexts.Count == 0)
				return;

			// Start rendering
			fontSprite.Begin(SpriteBlendMode.AlphaBlend);

			// Draw each character in the text
			//foreach (UIRenderer.FontToRender fontText in texts)
			for (int textNum = 0; textNum < remTexts.Count; textNum++)
			{
				FontToRender fontText = remTexts[textNum];

				int x = fontText.x;
				int y = fontText.y;
				Color color = fontText.color;
				//foreach (char c in fontText.text)
				char[] chars = fontText.text.ToCharArray();
				for (int num = 0; num < chars.Length; num++)
				{
					int charNum = (int)chars[num];
					if (charNum >= 32 &&
						charNum - 32 < CharRects.Length)
					{
						// Draw this char
						Rectangle rect = CharRects[charNum - 32];
						// Reduce height to prevent overlapping pixels
						rect.Y += 1;
						rect.Height = FontHeight;// Height;// - 1;
						// Add 2 pixel for g, j, y
						//if (c == 'g' || c == 'j' || c == 'y')
						//	rect.Height+=2;
						Rectangle destRect = new Rectangle(x,
							y - SubRenderHeight,
							rect.Width, rect.Height);

						// Scale destRect (1600x1200 is the base size)
						//destRect.Width = destRect.Width;
						//destRect.Height = destRect.Height;

						//TODO: if we want upscaling, just use destRect
						fontSprite.Draw(fontTexture.XnaTexture, //obs:destRect,
							// Rescale to fit resolution
							destRect,
							//new Rectangle(
							//destRect.X * BaseGame.Width / 1024,
							//destRect.Y * BaseGame.Height / 768,
							//destRect.Width * BaseGame.Width / 1024,
							//destRect.Height * BaseGame.Height / 768),
							rect, color);
						//,
						// Make sure fonts are always displayed at the front of everything
						//0, Vector2.Zero, SpriteEffects.None, 1.1f);

						// Increase x pos by width we use for this character
						int charWidth = CharRects[charNum - 32].Height;
						x += charWidth;
					} // if (charNum)
				} // foreach
			} // foreach (fontText)

			// End rendering
			fontSprite.End();

			remTexts.Clear();
		} // WriteAll(texts)    
    #endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test render font
		/// </summary>
		public static void TestRenderFont()
		{
			TestGame.Start("TestRenderFont",
				delegate
				{
					TextureFont.WriteText(30, 30,
						"Hi there .. whats up?");
				});
		} // TestRenderFont()
#endif
		#endregion
	} // class TextureFont
} // namespace RocketCommanderXna.Graphics
