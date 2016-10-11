﻿//
// RestoreNuGetPackagesAction.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Projects;
using NuGet.PackageManagement;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;

namespace MonoDevelop.PackageManagement
{
	internal class RestoreNuGetPackagesAction : IPackageAction
	{
		IPackageRestoreManager restoreManager;
		MonoDevelopBuildIntegratedRestorer buildIntegratedRestorer;
		IMonoDevelopSolutionManager solutionManager;
		IPackageManagementEvents packageManagementEvents;
		Solution solution;
		List<NuGetProject> nugetProjects;

		public RestoreNuGetPackagesAction (Solution solution)
		{
			this.solution = solution;
			packageManagementEvents = PackageManagementServices.PackageManagementEvents;

			solutionManager = PackageManagementServices.Workspace.GetSolutionManager (solution);

			nugetProjects = solutionManager.GetNuGetProjects ().ToList ();

			if (AnyProjectsUsingPackagesConfig ()) {
				restoreManager = new PackageRestoreManager (
					solutionManager.CreateSourceRepositoryProvider (),
					solutionManager.Settings,
					solutionManager
				);
			}

			if (AnyProjectsUsingProjectJson ()) {
				buildIntegratedRestorer = new MonoDevelopBuildIntegratedRestorer (
					solutionManager.CreateSourceRepositoryProvider (),
					solutionManager.Settings,
					solution.BaseDirectory);
			}
		}


		bool AnyProjectsUsingPackagesConfig ()
		{
			return nugetProjects.Any (project => !(project is BuildIntegratedNuGetProject));
		}

		bool AnyProjectsUsingProjectJson ()
		{
			return GetBuildIntegratedNuGetProjects ().Any ();
		}

		IEnumerable<BuildIntegratedNuGetProject> GetBuildIntegratedNuGetProjects ()
		{
			return nugetProjects.OfType<BuildIntegratedNuGetProject> ();
		}

		public void Execute ()
		{
			Execute (CancellationToken.None);
		}

		public void Execute (CancellationToken cancellationToken)
		{
			Task task = ExecuteAsync (cancellationToken);
			using (var restoreTask = new PackageRestoreTask (task)) {
				task.Wait ();
			}
		}

		public bool HasPackageScriptsToRun ()
		{
			return false;
		}

		async Task ExecuteAsync (CancellationToken cancellationToken)
		{
			if (restoreManager != null) {
				using (var monitor = new PackageRestoreMonitor (restoreManager)) {
					await restoreManager.RestoreMissingPackagesInSolutionAsync (
						solutionManager.SolutionDirectory,
						new NuGetProjectContext (),
						cancellationToken);
				}
			}

			if (buildIntegratedRestorer != null) {
				await buildIntegratedRestorer.RestorePackages (
					GetBuildIntegratedNuGetProjects (),
					cancellationToken);
			}

			await Runtime.RunInMainThread (() => RefreshProjectReferences ());

			packageManagementEvents.OnPackagesRestored ();
		}

		void RefreshProjectReferences ()
		{
			foreach (DotNetProject dotNetProject in solution.GetAllDotNetProjects ()) {
				dotNetProject.RefreshReferenceStatus ();
			}
		}
	}
}

