// Project: RocketCommanderXna, File: IGraphicContent.cs
// Namespace: RocketCommanderXna.Graphics, Class: IGraphicContent
// Path: C:\code\RocketCommanderXna\Graphics, Author: Abi
// Code lines: 496, Size of file: 14,89 KB
// Creation date: 23.09.2006 10:56
// Last modified: 07.10.2006 13:56
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RocketCommanderXna.Graphics
{
	/// <summary>
	/// Helper interface to collect all graphic content files we have created
	/// in BaseGame and allowing to dispose and recreating them there.
	/// </summary>
	public interface IGraphicContent : IDisposable
	{
		/// <summary>
		/// Load the content, dispose it again with Dispose.
		/// This will be called by BaseGame.LoadGraphicsContent as often
		/// as the device needs recreation!
		/// </summary>
		void Load();
	} // interface IGraphicContent
} // namespace RocketCommanderXna.Graphics
