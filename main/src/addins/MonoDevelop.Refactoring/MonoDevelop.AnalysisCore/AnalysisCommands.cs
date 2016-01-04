// 
// AnalysisCommands.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
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
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using Gtk;
using MonoDevelop.AnalysisCore.Gui;
using MonoDevelop.AnalysisCore.Fixes;
using MonoDevelop.Ide;
using MonoDevelop.CodeIssues;
using MonoDevelop.CodeActions;
using System.Threading;
using System.IO;
using MonoDevelop.Refactoring;
using MonoDevelop.Ide.Gui.Dialogs;
using Microsoft.CodeAnalysis;
using MonoDevelop.Components.Extensions;

namespace MonoDevelop.AnalysisCore
{
	public enum AnalysisCommands
	{
		FixOperations,
		ShowFixes,
		QuickFix,
		ExportRules
	}
	
	class ShowFixesHandler : CommandHandler
	{
		protected override void Update (CommandInfo info)
		{
			var doc = MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument;
			if (doc == null || doc.Editor == null) {
				info.Enabled = false;
				return;
			}
			var codeActionExtension = doc.GetContent <CodeActionEditorExtension> ();
			if (codeActionExtension == null) {
				info.Enabled = false;
				return;
			}
			var fixes = codeActionExtension.GetCurrentFixes ();
			info.Enabled = !fixes.IsEmpty;
		}
		
		protected override void Run ()
		{
//			var doc = MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument;
//			var view = doc.GetContent<MonoDevelop.SourceEditor.SourceEditorView> ();
//			if (view == null) {
//				LoggingService.LogWarning ("ShowFixesHandler could not find a SourceEditorView");
//				return;
//			}
//			var widget = view.TextEditor;
//			var pt = view.DocumentToScreenLocation (doc.Editor.Caret.Location);
//			
//			var ces = new CommandEntrySet ();
//			ces.AddItem (AnalysisCommands.FixOperations);
//			var menu = MonoDevelop.Ide.IdeApp.CommandService.CreateMenu (ces);
//			
//			menu.Popup (null, null, delegate (Menu mn, out int x, out int y, out bool push_in) {
//				x = pt.X;
//				y = pt.Y;
//				push_in = true;
//				//if the menu would be off the bottom of the screen, "drop" it upwards
//				if (y + mn.Requisition.Height > widget.Screen.Height)
//					y -= mn.Requisition.Height + (int)widget.LineHeight;
//			}, 0, Global.CurrentEventTime);
		}
	}
	
	class FixOperationsHandler : CommandHandler
	{
		protected override void Update (CommandArrayInfo info)
		{
//			MonoDevelop.Ide.Gui.Document document;
//			IList<FixableResult> results;
//			if (!GetFixes (out document, out results))
//				return;
//			PopulateInfos (info, document, results);
		}
		
		protected override void Run (object dataItem)
		{
			if (dataItem is Result) {
				((Result)dataItem).ShowResultOptionsDialog ();
				return;
			}
			if (dataItem is System.Action)  {
				((System.Action)dataItem) ();
				return;
			}
			var action = dataItem as IAnalysisFixAction;
			if (action != null) {
				action.Fix (); 
				return;
			}
//			var ca = dataItem as CodeAction;
//			if (ca != null) {
//				var doc = MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument;
//
//				var context = doc.ParsedDocument.CreateRefactoringContext != null ? doc.ParsedDocument.CreateRefactoringContext (doc, default(CancellationToken)) : null;
//				using (var script = context.CreateScript ()) {
//					ca.Run (context, script);
//				}
//				return;
//			}


		}
		
//		public static bool GetFixes (out MonoDevelop.Ide.Gui.Document document, out IList<FixableResult> results)
//		{
//			results = null;
//			document = MonoDevelop.Ide.IdeApp.Workbench.ActiveDocument;
//			if (document == null)
//				return false;
//			
//			var ext = document.GetContent<ResultsEditorExtension> ();
//			if (ext == null)
//				return false;
//			
//			var list = ext.GetResultsAtOffset (document.Editor.CaretOffset).OfType<FixableResult> ().ToList ();
//			list.Sort (ResultCompareImportanceDesc);
//			results = list;
//
//			if (results.Count > 0)
//				return true;
//
//			var codeActionExtension = document.GetContent <CodeActionEditorExtension> ();
//			if (codeActionExtension != null) {
//				var fixes = codeActionExtension.GetCurrentFixes ();
//				if (fixes != null)
//					return !fixes.IsEmpty;
//			} 
//			return false;
//		}
//		
		static int ResultCompareImportanceDesc (Result r1, Result r2)
		{
			int c = ((int)r1.Level).CompareTo ((int)r2.Level);
			if (c != 0)
				return c;
			return string.Compare (r1.Message, r2.Message, StringComparison.Ordinal);
		}
		
//		public static void PopulateInfos (CommandArrayInfo infos, MonoDevelop.Ide.Gui.Document doc, IEnumerable<FixableResult> results)
//		{
////			//FIXME: ellipsize long messages
////			int mnemonic = 1;
////
////			var codeActionExtension = doc.GetContent <CodeActionEditorExtension> ();
////			var fixes = codeActionExtension.GetCurrentFixes ();
////			if (fixes != null) {
////				foreach (var _fix in fixes.Where (CodeActionEditorExtension.IsAnalysisOrErrorFix)) {
////					var fix = _fix;
////					if (fix is AnalysisContextActionProvider.AnalysisCodeAction)
////						continue;
////					var escapedLabel = fix.Title.Replace ("_", "__");
////					var label = (mnemonic <= 10)
////						? "_" + (mnemonic++ % 10).ToString () + " " + escapedLabel
////							: "  " + escapedLabel;
////					infos.Add (label, fix);
////				}
////			}
////
////			foreach (var result in results) {
////				bool firstAction = true;
////				foreach (var action in GetActions (doc, result)) {
////					if (firstAction) {
////						//FIXME: make this header item insensitive but not greyed out
////						infos.Add (new CommandInfo (result.Message.Replace ("_", "__"), false, false) {
////							Icon = GetIcon (result.Level)
////						}, null);
////						firstAction = false;
////					}
////					var escapedLabel = action.Label.Replace ("_", "__");
////					var label = (mnemonic <= 10)
////						? "_" + (mnemonic++ % 10).ToString () + " " + escapedLabel
////						: "  " + escapedLabel;
////					infos.Add (label, action);
////				}
////				if (result.HasOptionsDialog) {
////					var declSet = new CommandInfoSet ();
////					declSet.Text = GettextCatalog.GetString ("_Options for \"{0}\"", result.OptionsTitle);
////
////					bool hasBatchFix = false;
////					foreach (var fix in result.Fixes.OfType<IAnalysisFixAction> ().Where (f => f.SupportsBatchFix)) {
////						hasBatchFix = true;
////						var title = string.Format (GettextCatalog.GetString ("Apply in file: {0}"), fix.Label);
////						declSet.CommandInfos.Add (title, new System.Action(fix.BatchFix));
////					}
////					if (hasBatchFix)
////						declSet.CommandInfos.AddSeparator ();
////
////					var ir = result as InspectorResults;
////					if (ir != null) {
////						var inspector = ir.Inspector;
////
////						if (inspector.CanSuppressWithAttribute) {
////							declSet.CommandInfos.Add (GettextCatalog.GetString ("_Suppress with attribute"), new System.Action(delegate {
////								inspector.SuppressWithAttribute (doc, ir.Region); 
////							}));
////						}
////
////						if (inspector.CanDisableWithPragma) {
////							declSet.CommandInfos.Add (GettextCatalog.GetString ("_Suppress with #pragma"), new System.Action(delegate {
////								inspector.DisableWithPragma (doc, ir.Region); 
////							}));
////						}
////
////						if (inspector.CanDisableOnce) {
////							declSet.CommandInfos.Add (GettextCatalog.GetString ("_Disable Once"), new System.Action(delegate {
////								inspector.DisableOnce (doc, ir.Region); 
////							}));
////						}
////
////						if (inspector.CanDisableAndRestore) {
////							declSet.CommandInfos.Add (GettextCatalog.GetString ("Disable _and Restore"), new System.Action(delegate {
////								inspector.DisableAndRestore (doc, ir.Region); 
////							}));
////						}
////					}
////
////					declSet.CommandInfos.Add (GettextCatalog.GetString ("_Configure Rule"), result);
////
////					infos.Add (declSet);
////				}
////			}
//		}
//		
//		public static IEnumerable<IAnalysisFixAction> GetActions (MonoDevelop.Ide.Gui.Document doc, FixableResult result)
//		{
//			foreach (var fix in result.Fixes)
//				foreach (var handler in AnalysisExtensions.GetFixHandlers (fix.FixType))
//					foreach (var action in handler.GetFixes (doc.Editor, doc, fix))
//						yield return action;
//
//		}
//
//		static string GetIcon (Severity severity)
//		{
//			switch (severity) {
//			case Severity.Error:
//				return Ide.Gui.Stock.Error;
//			case Severity.Warning:
//				return Ide.Gui.Stock.Warning;
//			case Severity.Hint:
//				return Ide.Gui.Stock.Information;
//			default:
//				return null;
//			}
//		}
	}

	class ExportRulesHandler : CommandHandler
	{
		protected override void Run ()
		{
			var lang = "text/x-csharp";

			OpenFileDialog dlg = new OpenFileDialog ("Export Rules", MonoDevelop.Components.FileChooserAction.Save);
			dlg.InitialFileName = "rules.html";
			if (!dlg.Run ())
				return;

			Dictionary<CodeDiagnosticDescriptor, DiagnosticSeverity?> severities = new Dictionary<CodeDiagnosticDescriptor, DiagnosticSeverity?> ();

			foreach (var node in BuiltInCodeDiagnosticProvider.GetBuiltInCodeDiagnosticDecsriptorsAsync (CodeRefactoringService.MimeTypeToLanguage(lang), true).Result) {
				severities [node] = node.DiagnosticSeverity;
//				if (node.GetProvider ().SupportedDiagnostics.Length > 1) {
//					foreach (var subIssue in node.GetProvider ().SupportedDiagnostics) {
//						severities [subIssue] = node.GetSeverity (subIssue);
//					}
//				}
			}

			var grouped = severities.Keys.OfType<CodeDiagnosticDescriptor> ()
				.GroupBy (node => node.GetProvider ().SupportedDiagnostics.First ().Category)
				.OrderBy (g => g.Key, StringComparer.Ordinal);

			using (var sw = new StreamWriter (dlg.SelectedFile)) {
				sw.WriteLine ("<h1>Code Rules</h1>");
				foreach (var g in grouped) {
					sw.WriteLine ("<h2>" + g.Key + "</h2>");
					sw.WriteLine ("<table border='1'>");

					foreach (var node in g.OrderBy (n => n.Name, StringComparer.Ordinal)) {
						var title = node.Name;
						var desc = node.GetProvider ().SupportedDiagnostics.First ().Description.ToString () != title ? node.GetProvider ().SupportedDiagnostics.First ().Description : "";
						sw.WriteLine ("<tr><td>" + title + "</td><td>" + desc + "</td><td>" + node.DiagnosticSeverity + "</td></tr>");
						if (node.GetProvider ().SupportedDiagnostics.Length > 1) {
							foreach (var subIssue in node.GetProvider ().SupportedDiagnostics) {
								title = subIssue.Description.ToString ();
								desc = subIssue.Description.ToString () != title ? subIssue.Description : "";
								sw.WriteLine ("<tr><td> - " + title + "</td><td>" + desc + "</td><td>" + node.GetSeverity (subIssue) + "</td></tr>");
							}
						}
					}
					sw.WriteLine ("</table>");
				}

				var providerStates = new Dictionary<CodeRefactoringDescriptor, bool> ();
				foreach (var node in BuiltInCodeDiagnosticProvider.GetBuiltInCodeRefactoringDescriptorsAsync (CodeRefactoringService.MimeTypeToLanguage(lang), true).Result) {
					providerStates [node] = node.IsEnabled;
				}

				sw.WriteLine ("<h1>Code Actions</h1>");
				sw.WriteLine ("<table border='1'>");
				var sortedAndFiltered = providerStates.Keys.OrderBy (n => n.Name, StringComparer.Ordinal);
				foreach (var node in sortedAndFiltered) {
					sw.WriteLine ("<tr><td>" + node.IdString + "</td><td>" + node.Name + "</td></tr>");
				}
				sw.WriteLine ("</table>");
			}
		}
	}
}

