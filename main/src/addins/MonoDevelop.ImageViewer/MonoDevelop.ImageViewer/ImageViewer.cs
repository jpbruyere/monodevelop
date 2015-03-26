// 
// HexEditorView.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using Xwt;
using MonoDevelop.Ide.Fonts;

namespace MonoDevelop.ImageViewer
{
	class ImageViewer : AbstractXwtViewContent
	{
		
		ImageView window ;
		Xwt.Drawing.Image img;
		
		public override Xwt.Widget Widget {
			get {
				return window;
			}
		}
		
		public ImageViewer ()
		{			
			window = new ImageView ();
		}
		
		void SetOptions ()
		{
		}
		
		public override void Load (string fileName)
		{
			img = Xwt.Drawing.Image.FromFile (fileName);
			window.Image = img;
			ContentName = fileName;
			this.IsDirty = false;
			window.SetFocus ();
		}
	}
}
