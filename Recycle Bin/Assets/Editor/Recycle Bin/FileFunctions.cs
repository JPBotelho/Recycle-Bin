using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace JPBotelho
{
	public static class FileFunctions
	{
		//Deletes all members of a directory.
		public static void ClearDirectory(DirectoryInfo info)
		{
			foreach (FileInfo file in info.GetFiles())
			{
				file.Delete();
			}
			foreach (DirectoryInfo dir in info.GetDirectories())
			{
				dir.Delete(true);
			}
		}

		/// Opens a folder in the windows explorer
		public static void OpenFolder(string path)
		{
			System.Diagnostics.Process.Start(path);
		}

		public static List<string> GetFilesAndDirectories(DirectoryInfo target)
		{
			List<string> paths = new List<string>(Directory.GetFiles(target.FullName)); //Gets all paths
			paths.AddRange(Directory.GetDirectories(target.FullName)); //Gets all directories

		/*		foreach (string s in paths)
				{
					// "" returns everything. It's the default case.
					if (search == "" || s.ToLower().Contains(search.ToLower()))
					{
						trash.Add(new TrashFile(s));
					}
				}*/
			
			return paths;
		}

		public static void CopyFile(string path, DirectoryInfo to)
		{
			FileInfo info = new FileInfo(path);

			string first = Path.Combine(to.FullName, info.Name);

			if (File.Exists(first))
			{
				int i = 1;

				//Remove the extension
				string replace = path.Replace(info.Extension, "");

				string newName;

				while (true)
				{
					//        MyFile         (i)        .extension
					newName = replace + " (" + i + ")" + info.Extension;

					string currentDestination = Path.Combine(to.FullName, new FileInfo(newName).Name);

					if (File.Exists(currentDestination))
						i++;
					else
						break;
				}

				FileUtil.CopyFileOrDirectory(path, Path.Combine(to.FullName, new FileInfo(newName).Name));
			}
			else
			{
				FileUtil.CopyFileOrDirectory(path, first);
			}
		}

		public static void CopyDirectory(string path, DirectoryInfo to)
		{
			DirectoryInfo info = new DirectoryInfo(path);

			string destination = Path.Combine(to.FullName, info.Name);

			if (Directory.Exists(destination))
			{
				int i = 1;

				string replace = path.Replace(info.Extension, "");

				string newName;

				while (true)
				{
					newName = replace + " (" + i + ")" + info.Extension;

					string currentDestination = Path.Combine(to.FullName, new DirectoryInfo(newName).Name);

					if (Directory.Exists(currentDestination))
						i++;
					else
						break;
				}

				FileUtil.CopyFileOrDirectory(path, Path.Combine(to.FullName, new DirectoryInfo(newName).Name));
			}
			else
			{
				FileUtil.CopyFileOrDirectory(path, destination);
			}
		}
	}
}