//
// FileSettingsStore.cs
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
using System.IO;
using System.Linq;

using MonoDevelop.Core;
using MonoDevelop.Core.Serialization;


namespace Mono.TextEditor.Utils
{
	public static class FileSettingsStore
	{

		public class Settings
		{
			[ItemProperty]
			public int CaretOffset;
			[ItemProperty]
			public double vAdjustment;
			[ItemProperty]
			public double hAdjustment;
			[ItemProperty]
			public Dictionary<int, bool> FoldingStates = new Dictionary<int, bool> ();

			public override string ToString ()
			{
				return string.Format ("[Settings: CaretOffset={0}, vAdjustment={1}, hAdjustment={2}]", CaretOffset, vAdjustment, hAdjustment);
			}
		}

		static Dictionary<string, Settings> settingStore = new Dictionary<string, Settings> ();

		public static bool TryGetValue (string contentName, out Settings settings, bool persist)
		{
			if (contentName == null)
				throw new ArgumentNullException ("contentName");
			if (settingStore.TryGetValue (contentName, out settings))
				return true;
			if (!persist)
				return false;

			FilePath root = UserProfile.Current.CacheDir.Combine ("FileSettingsStore");
			FilePath path = root + contentName + ".fss";

			if (!File.Exists (path))
				return false;

			BinaryDataSerializer serializer = new BinaryDataSerializer (new DataContext ());
			settings = (Settings)serializer.Deserialize (path, typeof(Settings));
			return true;
		}

		public static void Store (string contentName, Settings settings, bool persist)
		{
			if (contentName == null)
				throw new ArgumentNullException ("contentName");
			if (settings == null)
				throw new ArgumentNullException ("settings");

			if (persist) {
				FilePath root = UserProfile.Current.CacheDir.Combine ("FileSettingsStore");
				FilePath path = root + contentName + ".fss";

				Directory.CreateDirectory (Path.GetDirectoryName (path));

				BinaryDataSerializer serializer = new BinaryDataSerializer (new DataContext ());
				serializer.Serialize (path, settings);
			}
			settingStore [contentName] = settings;
		}

		public static void Remove (string contentName)
		{
			FilePath root = UserProfile.Current.CacheDir.Combine ("FileSettingsStore");
			FilePath path = root + contentName + ".fss";
			if (!File.Exists (path))
				return;
			path.Delete ();
			DirectoryInfo dir = new DirectoryInfo (Path.GetDirectoryName (path));
			while(dir.ToString () != root)
			{
				if(dir.EnumerateFiles ().Any () || dir.EnumerateDirectories ().Any ())
					return;
				dir.Delete ();
				dir = dir.Parent;
			}
			
			settingStore.Remove (contentName);
		}
	}
}

