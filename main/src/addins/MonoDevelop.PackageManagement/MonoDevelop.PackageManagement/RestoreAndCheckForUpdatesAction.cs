﻿//
// RestoreAndCheckForUpdatesAction.cs
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

using System;
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
	internal class RestoreAndCheckForUpdatesAction : IPackageAction
	{
		List<PackageRestoreData> packagesToRestore;
		IPackageRestoreManager restoreManager;
		MonoDevelopBuildIntegratedRestorer buildIntegratedRestorer;
		IMonoDevelopSolutionManager solutionManager;
		IPackageManagementEvents packageManagementEvents;
		Solution solution;
		List<NuGetProject> nugetProjects;
		List<BuildIntegratedNuGetProject> buildIntegratedProjectsToBeRestored;

		public RestoreAndCheckForUpdatesAction (Solution solution)
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

		public bool CheckForUpdatesAfterRestore { get; set; }

		public async Task<bool> HasMissingPackages (CancellationToken cancellationToken = default(CancellationToken))
		{
			if (restoreManager != null) {
				var packages = await restoreManager.GetPackagesInSolutionAsync (
					solutionManager.SolutionDirectory,
					cancellationToken);

				packagesToRestore = packages.ToList ();
				if (packagesToRestore.Any (package => package.IsMissing)) {
					return true;
				}
			}

			if (buildIntegratedRestorer != null) {
				var projects = await buildIntegratedRestorer.GetProjectsRequiringRestore (GetBuildIntegratedNuGetProjects ());
				buildIntegratedProjectsToBeRestored = projects.ToList ();
				return buildIntegratedProjectsToBeRestored.Any ();
			}

			return false;
		}

		public void Execute ()
		{
		}

		public void Execute (CancellationToken cancellationToken)
		{
			Task task = RestorePackagesAsync (cancellationToken);
			using (var restoreTask = new PackageRestoreTask (task)) {
				task.Wait (cancellationToken);
			}

			if (CheckForUpdatesAfterRestore && !cancellationToken.IsCancellationRequested) {
				CheckForUpdates ();
			}
		}

		public bool HasPackageScriptsToRun ()
		{
			return false;
		}

		void CheckForUpdates ()
		{
			try {
				PackageManagementServices.UpdatedPackagesInWorkspace.CheckForUpdates (new SolutionProxy (solution));
			} catch (Exception ex) {
				LoggingService.LogError ("Check for NuGet package updates error.", ex);
			}
		}

		async Task RestorePackagesAsync (CancellationToken cancellationToken)
		{
			if (restoreManager != null) {
				using (var monitor = new PackageRestoreMonitor (restoreManager)) {
					await restoreManager.RestoreMissingPackagesAsync (
						solutionManager.SolutionDirectory,
						packagesToRestore,
						new NuGetProjectContext (),
						cancellationToken);
				}
			}

			if (buildIntegratedRestorer != null) {
				await buildIntegratedRestorer.RestorePackages (buildIntegratedProjectsToBeRestored, cancellationToken);
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

