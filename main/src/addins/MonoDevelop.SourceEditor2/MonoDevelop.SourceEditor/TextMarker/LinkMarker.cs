﻿//
// LinkMarker.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
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
using MonoDevelop.Ide.Editor;
using Mono.TextEditor;
using MonoDevelop.Core;

namespace MonoDevelop.SourceEditor
{
	class LinkMarker : UnderlineTextSegmentMarker, ITextSegmentMarker, IActionTextLineMarker
	{
		static readonly Gdk.Cursor textLinkCursor = new Gdk.Cursor (Gdk.CursorType.Hand1);
		static readonly Cairo.Color linkColor = new Cairo.Color (0, 0, 1.0);
		Action<LinkRequest> activateLink;

		public LinkMarker (int offset, int length, Action<LinkRequest> activateLink) : base (linkColor, new TextSegment (offset, length))
		{
			this.activateLink = activateLink;
			this.Wave = false;
		}

		public event EventHandler<TextMarkerMouseEventArgs> MousePressed;
		public event EventHandler<TextMarkerMouseEventArgs> MouseHover;

		object ITextSegmentMarker.Tag {
			get;
			set;
		}

		bool IActionTextLineMarker.MousePressed (MonoTextEditor editor, MarginMouseEventArgs args)
		{
			MousePressed?.Invoke (this, new TextEventArgsWrapper (args));
			if ((Platform.IsMac && (args.ModifierState & Gdk.ModifierType.Mod2Mask) == Gdk.ModifierType.Mod2Mask) ||
				(!Platform.IsMac && (args.ModifierState & Gdk.ModifierType.ControlMask) == Gdk.ModifierType.ControlMask))
				activateLink?.Invoke (LinkRequest.RequestNewView);
			else
				activateLink?.Invoke (LinkRequest.SameView);
			return false;
		}

		void IActionTextLineMarker.MouseHover (MonoTextEditor editor, MarginMouseEventArgs args, TextLineMarkerHoverResult result)
		{
			MouseHover?.Invoke (this, new TextEventArgsWrapper (args));
			result.Cursor = textLinkCursor;
		}
	}
}

