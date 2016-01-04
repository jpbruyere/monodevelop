﻿//
// AbstractUnitTestTextEditorExtension.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
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
using System.Collections.Generic;
using System.Threading;
using MonoDevelop.NUnit;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Gtk;
using MonoDevelop.Components;
using System.Threading.Tasks;
using MonoDevelop.Ide.Editor.Extension;
using MonoDevelop.Ide.Editor;

namespace MonoDevelop.NUnit
{
	public abstract class AbstractUnitTestTextEditorExtension : TextEditorExtension
	{
		protected override void Initialize ()
		{
			base.Initialize ();
			DocumentContext.DocumentParsed += HandleDocumentParsed; 
			if (IdeApp.Workbench == null)
				return;
			NUnitService.Instance.TestSessionCompleted += HandleTestSessionCompleted;
		}

		void HandleTestSessionCompleted (object sender, EventArgs e)
		{
			foreach (var marker in currentMarker)
				marker.UpdateState ();
		}

		public override void Dispose ()
		{
			src.Cancel ();
			NUnitService.Instance.TestSessionCompleted -= HandleTestSessionCompleted;
			RemoveHandler ();
			DocumentContext.DocumentParsed -= HandleDocumentParsed; 
			base.Dispose ();
		}

		CancellationTokenSource src = new CancellationTokenSource ();

		public abstract Task<IList<UnitTestLocation>> GatherUnitTests (CancellationToken token);

		void HandleDocumentParsed (object sender, EventArgs e)
		{
			if (!IdeApp.Preferences.EnableUnitTestEditorIntegration)
				return;
			src.Cancel ();
			src = new CancellationTokenSource ();
			var token = src.Token;
			ThreadPool.QueueUserWorkItem (delegate {
				if (token.IsCancellationRequested || DocumentContext == null)
					return;
				try {
					GatherUnitTests (token).ContinueWith (task => {
						var foundTests = task.Result;
						if (foundTests == null || DocumentContext == null)
							return;
						Application.Invoke (delegate {
							if (token.IsCancellationRequested || DocumentContext == null)
								return;
							foreach (var oldMarker in currentMarker)
								Editor.RemoveMarker (oldMarker);
							var newMarkers = new List<IUnitTestMarker> ();
							foreach (var foundTest in foundTests) {
								var unitTestMarker = TextMarkerFactory.CreateUnitTestMarker (Editor, new UnitTestMarkerHostImpl (this), foundTest);
								newMarkers.Add (unitTestMarker);
								var line = Editor.GetLineByOffset (foundTest.Offset);
								if (line != null) {
									Editor.AddMarker (line, unitTestMarker);
								}
							}
							this.currentMarker = newMarkers;
						});

					}, TaskContinuationOptions.ExecuteSynchronously | 
						TaskContinuationOptions.NotOnCanceled | 
						TaskContinuationOptions.NotOnFaulted);
				} catch (OperationCanceledException) {
				}
			});
		}

		static uint timeoutHandler;

		static void RemoveHandler ()
		{
			if (timeoutHandler != 0) {
				GLib.Source.Remove (timeoutHandler); 
				timeoutHandler = 0;
			}
		}

		List<IUnitTestMarker> currentMarker = new List<IUnitTestMarker>();

		class UnitTestMarkerHostImpl : UnitTestMarkerHost
		{
			readonly AbstractUnitTestTextEditorExtension ext;

			public UnitTestMarkerHostImpl (AbstractUnitTestTextEditorExtension ext)
			{
				if (ext == null)
					throw new ArgumentNullException ("ext");
				this.ext = ext;
			}

			#region implemented abstract members of UnitTestMarkerHost

			public override Xwt.Drawing.Image GetStatusIcon (string unitTestIdentifier, string caseId = null)
			{
				var test = NUnitService.Instance.SearchTestById (unitTestIdentifier + caseId);
				if (test != null)
					return test.StatusIcon;
				return TestStatusIcon.None;
			}

			public override bool IsFailure (string unitTestIdentifier, string caseId = null)
			{
				var test = NUnitService.Instance.SearchTestById (unitTestIdentifier + caseId);
				if (test != null) {
					var result = test.GetLastResult ();
					if (result != null)
						return result.IsFailure;
				}
				return false;
			}

			public override string GetMessage (string unitTestIdentifier, string caseId = null)
			{
				var test = NUnitService.Instance.SearchTestById (unitTestIdentifier + caseId);
				if (test != null) {
					var result = test.GetLastResult ();
					if (result != null)
						return result.Message;
				}
				return null;
			}

			public override bool HasResult (string unitTestIdentifier, string caseId = null)
			{
				return NUnitService.Instance.SearchTestById (unitTestIdentifier + caseId) != null;
			}

			public override void PopupContextMenu (UnitTestLocation unitTest, int x, int y)
			{
				var debugModeSet = Runtime.ProcessService.GetDebugExecutionMode ();

				var menu = new ContextMenu ();
				if (unitTest.IsFixture) {
					var menuItem = new ContextMenuItem ("_Run All");
					menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, false).Run;
					menu.Add (menuItem);
					if (debugModeSet != null) {
						menuItem = new ContextMenuItem ("_Debug All");
						menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, true).Run;
						menu.Add (menuItem);
					}
					menuItem = new ContextMenuItem ("_Select in Test Pad");
					menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, true).Select;
					menu.Add (menuItem);
				} else {
					if (unitTest.TestCases.Count == 0) {
						var menuItem = new ContextMenuItem ("_Run");
						menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, false).Run;
						menu.Add (menuItem);
						if (debugModeSet != null) {
							menuItem = new ContextMenuItem ("_Debug");
							menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, true).Run;
							menu.Add (menuItem);
						}
						menuItem = new ContextMenuItem ("_Select in Test Pad");
						menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, true).Select;
						menu.Add (menuItem);
					} else {
						var menuItem = new ContextMenuItem ("_Run All");
						menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, false).Run;
						menu.Add (menuItem);
						if (debugModeSet != null) {
							menuItem = new ContextMenuItem ("_Debug All");
							menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier, true).Run;
							menu.Add (menuItem);
						}
						menu.Add (new SeparatorContextMenuItem ());
						foreach (var id in unitTest.TestCases) {
							var submenu = new ContextMenu ();
							menuItem = new ContextMenuItem ("_Run");
							menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier + id, false).Run;
							submenu.Add (menuItem);
							if (debugModeSet != null) {
								menuItem = new ContextMenuItem ("_Debug");
								menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier + id, true).Run;
								submenu.Add (menuItem);
							}

							var label = "Test" + id;
							string tooltip = null;
							var test = NUnitService.Instance.SearchTestById (unitTest.UnitTestIdentifier + id);
							if (test != null) {
								var result = test.GetLastResult ();
								if (result != null && result.IsFailure) {
									tooltip = result.Message;
									label += "!";
								}
							}

							menuItem = new ContextMenuItem ("_Select in Test Pad");
							menuItem.Clicked += new TestRunner (unitTest.UnitTestIdentifier + id, true).Select;
							submenu.Add (menuItem);

							var subMenuItem = new ContextMenuItem (label);
							// if (!string.IsNullOrEmpty (tooltip))
							//	subMenuItem.TooltipText = tooltip;
							subMenuItem.SubMenu  = submenu;
							menu.Add (subMenuItem);
						}
					}
				}
				menu.Show (ext.Editor, x, y);
			}

			#endregion

			class TestRunner
			{
				readonly string testCase;
				readonly bool debug;

				public TestRunner (string testCase, bool debug)
				{
					this.testCase = testCase;
					this.debug = debug;
				}

				bool TimeoutHandler ()
				{
					var test = NUnitService.Instance.SearchTestById (testCase);
					if (test != null) {
						RunTest (test); 
						timeoutHandler = 0;
					} else {
						return true;
					}
					return false;
				}

				List<NUnitProjectTestSuite> testSuites = new List<NUnitProjectTestSuite>();
				internal void Run (object sender, EventArgs e)
				{
					if (IdeApp.ProjectOperations.IsBuilding (IdeApp.ProjectOperations.CurrentSelectedSolution) || 
						IdeApp.ProjectOperations.IsRunning (IdeApp.ProjectOperations.CurrentSelectedSolution))
						return;

					var foundTest = NUnitService.Instance.SearchTestById (testCase);
					if (foundTest != null) {
						RunTest (foundTest);
						return;
					}

					var tests = new Stack<UnitTest> ();
					foreach (var test in NUnitService.Instance.RootTests) {
						tests.Push (test);
					}
					while (tests.Count > 0) {
						var test = tests.Pop ();

						var solutionFolderTestGroup = test as SolutionFolderTestGroup;
						if (solutionFolderTestGroup != null) {
							foreach (var test2 in solutionFolderTestGroup.Tests) {
								tests.Push (test2); 
							}
							continue;
						}
						var nUnitProjectTestSuite = test as NUnitProjectTestSuite;
						if (nUnitProjectTestSuite != null)
							testSuites.Add (nUnitProjectTestSuite); 
					}

					foreach (var test in testSuites) {
						test.TestChanged += HandleTestChanged;
						test.ProjectBuiltWithoutTestChange += HandleTestChanged;
					}

					IdeApp.ProjectOperations.Build (IdeApp.ProjectOperations.CurrentSelectedSolution);
				}

				void HandleTestChanged (object sender, EventArgs e)
				{
					var foundTest = NUnitService.Instance.SearchTestById (testCase);
					if (foundTest != null) {
						foreach (var test in testSuites) {
							test.TestChanged -= HandleTestChanged;
							test.ProjectBuiltWithoutTestChange -= HandleTestChanged;
						}
						testSuites.Clear ();

						RunTest (foundTest); 
					}
				}

				internal void Select (object sender, EventArgs e)
				{
					var test = NUnitService.Instance.SearchTestById (testCase);
					if (test == null)
						return;
					var pad = IdeApp.Workbench.GetPad<TestPad> ();
					pad.BringToFront ();
					var content = (TestPad)pad.Content;
					content.SelectTest (test);
				}

				void RunTest (UnitTest test)
				{
					var debugModeSet = Runtime.ProcessService.GetDebugExecutionMode ();
					MonoDevelop.Core.Execution.IExecutionHandler ctx = null;
					if (debug && debugModeSet != null) {
						foreach (var executionMode in debugModeSet.ExecutionModes) {
							if (test.CanRun (executionMode.ExecutionHandler)) {
								ctx = executionMode.ExecutionHandler;
								break;
							}
						}
					}
					// NUnitService.Instance.RunTest (test, ctx);
					var pad = IdeApp.Workbench.GetPad<TestPad> ();
					var content = (TestPad)pad.Content;
					content.RunTest (test, ctx);
				}
			}
		}
	}
}

