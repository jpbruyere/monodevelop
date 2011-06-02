﻿using System;
using System.Collections.Generic;
using System.Text;
using MonoDevelop.Components.Extensions;
using System.Windows.Forms;
using MonoDevelop.Core;

namespace MonoDevelop.Platform
{
    class SelectFileDialogHandler : ISelectFileDialogHandler
    {
        public bool Run(SelectFileDialogData data)
        {
            FileDialog dlg = null;
            if (data.Action == Gtk.FileChooserAction.Open)
                dlg = new OpenFileDialog();
            else if (data.Action == Gtk.FileChooserAction.Save)
                dlg = new SaveFileDialog();		
			
			SetCommonFormProperties (data, dlg);
			
            bool result = false;
            try
            {
                WinFormsRoot root = new WinFormsRoot();
                if (dlg.ShowDialog(root) == DialogResult.Cancel)
                    result = false;
                else
                {
					FilePath[] paths = new FilePath [dlg.FileNames.Length];
					for (int n=0; n<dlg.FileNames.Length; n++)
						paths [n] = dlg.FileNames [n];
                    data.SelectedFiles = paths;
                    result = true;
                }
            }
            finally
            {
                dlg.Dispose();
            }

            return result;
        }
		
		internal static void SetCommonFormProperties (SelectFileDialogData data, FileDialog dialog)
		{
			if (!string.IsNullOrEmpty (data.Title))
				dialog.Title = data.Title;
			
			dialog.AddExtension = true;
			dialog.InitialDirectory = data.CurrentFolder;
            if (!string.IsNullOrEmpty (data.InitialFileName))
                dialog.FileName = data.InitialFileName;
			
			OpenFileDialog openDialog = dialog as OpenFileDialog;
			if (openDialog != null)
				openDialog.Multiselect = data.SelectMultiple;
		}
    }
}
