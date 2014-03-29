// Project: RocketCommanderXna, File: LineManager3D.cs
// Namespace: RocketCommanderXna.Graphic, Class: LineManager3D
// Creation date: 18.02.2005 11:35
// Last modified: 01.03.2005 16:36
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RocketCommanderXna;
using RocketCommanderXna.Helpers;
using RocketCommanderXna.Shaders;
using RocketCommanderXna.Game;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Helper class for game for rendering lines.
	/// This class will collect all line calls, then build a new vertex buffer
	/// if any line has changed or the line number changed and finally will
	/// render all lines in the vertex buffer at the end of the frame (so this
	/// class is obviously only for 2D lines directly on screen, no z buffer
	/// and no stuff will be in front of the lines, because everything is
	/// rendered at the end of the frame).
	/// </summary>
	internal class LineManager3D : IGraphicContent
	{
		#region Line struct
		/// <summary>
		/// Struct for a line, instances of this class will be added to lines.
		/// </summary>
		struct Line
		{
			// Positions
			public Vector3 startPoint, endPoint;
			// Colors
			public Color startColor, endColor;

			/// <summary>
			/// Constructor
			/// </summary>
			public Line(
				Vector3 setStartPoint, Color setStartColor,
				Vector3 setEndPoint, Color setEndColor)
			{
				startPoint = setStartPoint;
				startColor = setStartColor;
				endPoint = setEndPoint;
				endColor = setEndColor;
			} // Line(setStartPoint, setStartColor, setEndPoint)

			/// <summary>
			/// Are these two Lines equal?
			/// </summary>
			public static bool operator ==(Line a, Line b)
			{
				return
					a.startPoint == b.startPoint &&
					a.endPoint == b.endPoint &&
					a.startColor == b.startColor &&
					a.endColor == b.endColor;
			} // ==(a, b)

			/// <summary>
			/// Are these two Lines not equal?
			/// </summary>
			public static bool operator !=(Line a, Line b)
			{
				return
					a.startPoint != b.startPoint ||
					a.endPoint != b.endPoint ||
					a.startColor != b.startColor ||
					a.endColor != b.endColor;
			} // !=(a, b)

			/// <summary>
			/// Support Equals(.) to keep the compiler happy
			/// (because we used == and !=)
			/// </summary>
			public override bool Equals(object a)
			{
				if (a.GetType() == typeof(Line))
					return (Line)a == this;
				else
					return false; // Object is not a Line
			} // Equals(a)

			/// <summary>
			/// Support GetHashCode() to keep the compiler happy
			/// (because we used == and !=)
			/// </summary>
			public override int GetHashCode()
			{
				return 0; // Not supported or nessescary
			} // GetHashCode()
		} // struct Line
		#endregion

		#region Variables
		/// <summary>
		/// Number of lines used this frame, will be set to 0 when rendering.
		/// </summary>
		private int numOfLines = 0;

		/// <summary>
		/// The actual list for all the lines, it will NOT be reseted each
		/// frame like numOfLines! We will remember the last lines and
		/// only change this list when anything changes (new line, old
		/// line missing, changing line data).
		/// When this happens buildVertexBuffer will be set to true.
		/// </summary>
		private List<Line> lines = new List<Line>();

		/// <summary>
		/// Build vertex buffer this frame because the line list was changed?
		/// </summary>
		private bool buildVertexBuffer = false;

		/// <summary>
		/// Vertex buffer for all lines
		/// </summary>
		VertexPositionColor[] lineVertices =
			new VertexPositionColor[MaxNumOfLines*2];

		/// <summary>
		/// Real number of primitives currently used.
		/// </summary>
		private int numOfPrimitives = 0;

		/// <summary>
		/// Max. number of lines allowed to prevent to big buffer, will never
		/// be reached, but in case something goes wrong or numOfLines is not
		/// reseted each frame, we won't add unlimited lines (all new lines
		/// will be ignored if this max. number is reached).
		/// </summary>
		protected const int MaxNumOfLines =
			4096;//40096;//512;//256; // more than in 2D

		/// <summary>
		/// Vertex declaration for our lines.
		/// </summary>
		VertexDeclaration decl = null;
		#endregion

		#region Initialization
		/// <summary>
		/// Init LineManager
		/// </summary>
		public LineManager3D()
		{
			if (BaseGame.Device == null)
				throw new NullReferenceException(
					"XNA device is not initialized, can't init line manager.");

			Load();

			BaseGame.RegisterGraphicContentObject(this);
		} // LineManager()
		#endregion

		#region Load
		public void Load()
		{
			if (decl == null)
				decl = new VertexDeclaration(
					BaseGame.Device, VertexPositionColor.VertexElements);
		} // Load()
		#endregion

		#region Dispose
		public void Dispose()
		{
			if (decl != null)
				decl.Dispose();
			decl = null;
		} // Dispose()
		#endregion

		#region Reset, AddLine and Rendering
		/// <summary>
		/// Add line
		/// </summary>
		public void AddLine(
			Vector3 startPoint, Color startColor,
			Vector3 endPoint, Color endColor)
		{
			// Don't add new lines if limit is reached
			if (numOfLines >= MaxNumOfLines)
			{
				/*ignore
				Log.Write("Too many lines requested in LineManager3D. " +
					"Max lines = " + MaxNumOfLines);
				 */
				return;
			} // if (numOfLines)

			// Build line
			Line line = new Line(startPoint, startColor, endPoint, endColor);

			// Check if this exact line exists at the current lines position.
			if (lines.Count > numOfLines)
			{
				if ((Line)lines[numOfLines] != line)
				{
					// overwrite old line, otherwise just increase numOfLines
					lines[numOfLines] = line;
					// Remember to build vertex buffer in Render()
					buildVertexBuffer = true;
				} // if if
			} // if
			else
			{
				// Then just add new line
				lines.Add(line);
				// Remember to build vertex buffer in Render()
				buildVertexBuffer = true;
			} // else

			// nextUpValue line
			numOfLines++;
		} // AddLine(startPoint, startColor, endPoint)

		/// <summary>
		/// Add line (only 1 color for start and end version)
		/// </summary>
		public void AddLine(Vector3 startPoint, Vector3 endPoint,
			Color color)
		{
			AddLine(startPoint, color, endPoint, color);
		} // AddLine(startPoint, endPoint, color)

		protected void UpdateVertexBuffer()
		{
			// Don't do anything if we got no lines.
			if (numOfLines == 0 ||
				// Or if some data is invalid
				lines.Count < numOfLines)
			{
				numOfPrimitives = 0;
				return;
			} // if (numOfLines)

			// Set all lines
			for (int lineNum = 0; lineNum < numOfLines; lineNum++)
			{
				Line line = (Line)lines[lineNum];
				lineVertices[lineNum * 2 + 0] = new VertexPositionColor(
					line.startPoint, line.startColor);
				lineVertices[lineNum * 2 + 1] = new VertexPositionColor(
					line.endPoint, line.endColor);
			} // for (lineNum)
			numOfPrimitives = numOfLines;

			// Vertex buffer was build
			buildVertexBuffer = false;
		} // UpdateVertexBuffer()

		/// <summary>
		/// Render all lines added this frame
		/// </summary>
		public void Render()
		{
			// Need to build vertex buffer?
			if (buildVertexBuffer ||
				numOfPrimitives != numOfLines)
			{
				UpdateVertexBuffer();
			} // if (buildVertexBuffer)

			// Render lines if we got any lines to render
			if (numOfPrimitives > 0)
			{
				try
				{
					BaseGame.WorldMatrix = Matrix.Identity;
					BaseGame.AlphaBlending = true;
					BaseGame.Device.VertexDeclaration = decl;
					BaseGame.Device.RenderState.DepthBufferEnable = true;
					ShaderEffect.lineRendering.Render(
						"LineRendering3D",
						delegate
						{
							BaseGame.Device.DrawUserPrimitives<VertexPositionColor>(
								PrimitiveType.LineList, lineVertices, 0, numOfPrimitives);
						});
				} // try
				catch (Exception ex)
				{
					Log.Write(
						"LineManager3D.Render failed. numOfPrimitives=" + numOfPrimitives +
						", numOfLines=" + numOfLines + ". Error: " + ex.ToString());
				} // catch (ex)
			} // if (numOfVertices)

			// Ok, finally reset numOfLines for next frame
			numOfLines = 0;
		} // Render()
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestRenderLines
		/// <summary>
		/// Test render lines
		/// </summary>
		public static void TestRenderLines()
		{
			TestGame.Start(
				delegate // 3d render code
				{
					for (int num = 0; num < 200; num++)
					{
						BaseGame.DrawLine(
							new Vector3(-12.0f + num / 4.0f, 13.0f, 0),
							new Vector3(-17.0f + num / 4.0f, -13.0f, 0),
							new Color((byte)(255-num), 14, (byte)num));
					} // for

					TextureFont.WriteText(2, 30,
						"cam pos="+BaseGame.CameraPos);
				});
		} // TestRenderLines()
		#endregion
#endif
		#endregion
	}	// class LineManager3D
} // namespace RocketCommanderXna.Graphic
