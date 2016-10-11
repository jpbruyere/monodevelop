﻿//
// InformationPopoverWidget.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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
using MonoDevelop.Ide;
using MonoDevelop.Ide.Tasks;
using Xwt;

namespace MonoDevelop.Components
{
	public class InformationPopoverWidget: Widget
	{
		TaskSeverity severity;
		Xwt.ImageView imageView;
		string message;
		TooltipPopoverWindow popover;
		PopupPosition popupPosition;

		public InformationPopoverWidget ()
		{
			severity = TaskSeverity.Information;
			imageView = new Xwt.ImageView ();
			Content = imageView;
			UpdateIcon ();
		}

		public TaskSeverity Severity {
			get {
				return severity;
			}
			set {
				severity = value;
				UpdateIcon ();
				UpdatePopover ();
			}
		}

		public string Message {
			get {
				return message;
			}

			set {
				message = value;
				UpdatePopover ();
			}
		}

		public PopupPosition PopupPosition {
			get { return popupPosition; }
			set {
				popupPosition = value;
				UpdatePopover ();
			}
		}

		void UpdateIcon ()
		{
			imageView.Image = GetSeverityIcon ();
		}

		Xwt.Drawing.Image GetSeverityIcon ()
		{
			switch (severity) {
			case TaskSeverity.Error:
				return ImageService.GetIcon ("md-error");
			case TaskSeverity.Warning:
				return ImageService.GetIcon ("md-warning");
			}
			return ImageService.GetIcon ("md-information");
		}

		protected override void OnMouseEntered (EventArgs args)
		{
			base.OnMouseEntered (args);
			ShowPopover ();
		}

		void ShowPopover ()
		{
			if (popover != null)
				popover.Destroy ();
			popover = new TooltipPopoverWindow {
				ShowArrow = true,
				Text = message,
				Severity = severity
			};
			popover.ShowPopup ((Gtk.Widget)this.Surface.NativeWidget, popupPosition);
		}

		void UpdatePopover ()
		{
			if (popover != null)
				ShowPopover ();
		}

		protected override void OnMouseExited (EventArgs args)
		{
			base.OnMouseExited (args);
			if (popover != null) {
				popover.Destroy ();
				popover = null;
			}
		}
	}
}

