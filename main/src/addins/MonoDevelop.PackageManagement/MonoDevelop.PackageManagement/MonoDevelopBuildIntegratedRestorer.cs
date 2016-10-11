﻿//
// MonoDevelopBuildIntegratedRestorer.cs
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
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.LibraryModel;
using NuGet.Logging;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.Protocol.Core.Types;
using MonoDevelop.Ide;

namespace MonoDevelop.PackageManagement
{
	internal class MonoDevelopBuildIntegratedRestorer
	{
		IPackageManagementEvents packageManagementEvents;
		List<SourceRepository> sourceRepositories;
		string packagesFolder;
		ExternalProjectReferenceContext context;

		public MonoDevelopBuildIntegratedRestorer (
			ISourceRepositoryProvider repositoryProvider,
			ISettings settings,
			string solutionDirectory)
		{
			sourceRepositories = repositoryProvider.GetRepositories ().ToList ();

			packageManagementEvents = PackageManagementServices.PackageManagementEvents;

			packagesFolder = BuildIntegratedProjectUtility.GetEffectiveGlobalPackagesFolder (
				solutionDirectory,
				settings);

			context = CreateRestoreContext ();
		}

		public async Task RestorePackages (
			IEnumerable<BuildIntegratedNuGetProject> projects,
			CancellationToken cancellationToken)
		{
			var changedLocks = new List<FilePath> ();
			var affectedProjects = new List<BuildIntegratedNuGetProject> ();

			foreach (BuildIntegratedNuGetProject project in projects) {
				var changedLock = await RestorePackagesInternal (project, cancellationToken);
				if (changedLock != null) {
					changedLocks.Add (changedLock);
					affectedProjects.Add (project);
				}
			}

			if (changedLocks.Count > 0) {
				await Runtime.RunInMainThread (() => {
					FileService.NotifyFilesChanged (changedLocks);
					foreach (var project in affectedProjects) {
						NotifyProjectReferencesChanged (project);
					}
				});
			}
		}

		public async Task RestorePackages (
			BuildIntegratedNuGetProject project,
			CancellationToken cancellationToken)
		{
			var changedLock = await RestorePackagesInternal (project, cancellationToken);

			if (changedLock != null) {
				await Runtime.RunInMainThread (() => {
					FileService.NotifyFileChanged (changedLock);
					NotifyProjectReferencesChanged (project);
				});
			}
		}

		//returns the lock file, if it changed
		async Task<string> RestorePackagesInternal (
			BuildIntegratedNuGetProject project,
			CancellationToken cancellationToken)
		{
			RestoreResult restoreResult = await BuildIntegratedRestoreUtility.RestoreAsync (
				project,
				context,
				sourceRepositories, 
				packagesFolder, 
				cancellationToken);

			if (restoreResult.Success) {
				if (!object.Equals (restoreResult.LockFile, restoreResult.PreviousLockFile)) {
					return restoreResult.LockFilePath;
				}
			} else {
				ReportRestoreError (restoreResult);
			}
			return null;
		}

		static void NotifyProjectReferencesChanged (BuildIntegratedNuGetProject project)
		{
			var bips = project as BuildIntegratedProjectSystem;
			if (bips != null) {
				bips.Project.RefreshProjectBuilder ();
				bips.Project.DotNetProject.NotifyModified ("References");
			}
		}

		ILogger CreateLogger ()
		{
			return new PackageManagementLogger (packageManagementEvents);
		}

		ExternalProjectReferenceContext CreateRestoreContext ()
		{
			return new ExternalProjectReferenceContext (CreateLogger ());
		}

		void ReportRestoreError (RestoreResult restoreResult)
		{
			foreach (LibraryRange libraryRange in restoreResult.GetAllUnresolved ()) {
				packageManagementEvents.OnPackageOperationMessageLogged (
					NuGet.MessageLevel.Info,
					GettextCatalog.GetString ("Restore failed for '{0}'."),
					libraryRange.ToString ());
			}
			throw new ApplicationException (GettextCatalog.GetString ("Restore failed."));
		}

		public Task<bool> IsRestoreRequired (BuildIntegratedNuGetProject project)
		{
			var pathResolver = new VersionFolderPathResolver (packagesFolder);
			var projects = new BuildIntegratedNuGetProject[] { project };
			return BuildIntegratedRestoreUtility.IsRestoreRequired (projects, pathResolver, context);
		}

		public async Task<IEnumerable<BuildIntegratedNuGetProject>> GetProjectsRequiringRestore (
			IEnumerable<BuildIntegratedNuGetProject> projects)
		{
			var projectsToBeRestored = new List<BuildIntegratedNuGetProject> ();

			foreach (BuildIntegratedNuGetProject project in projects) {
				bool restoreRequired = await IsRestoreRequired (project);
				if (restoreRequired) {
					projectsToBeRestored.Add (project);
				}
			}

			return projectsToBeRestored;
		}
	}
}

