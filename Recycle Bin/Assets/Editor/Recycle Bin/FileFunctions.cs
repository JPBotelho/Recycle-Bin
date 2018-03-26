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
			List<string> paths = new List<string>();			
			paths.AddRange(Directory.GetFiles(target.FullName));
			paths.AddRange(Directory.GetDirectories(target.FullName));

			return paths;
		}

		//Copies files or directories to a target directory. 
		//Members with the same name get (X) added after.
		//Recursion is handled by unity's CopyFileOrDirectory (doesn't handle matching names, hence this function)-
		public static void CopyFileOrDirectory(string path, DirectoryInfo to)
		{
			//Directories need some separate logic, but the implementation is nearly the same.
			bool isDirectory = IsDirectory(path);

			//If it's a file it will have extension, else it won't, no need to use DirectoryInfo
			FileInfo file = new FileInfo(path);

			string targetPath = Path.Combine(to.FullName, file.Name);

			//File does not yet exist, we can copy it right away.
			//Else: File already exists, we need to add (X) in front of it. E.g. MyFile (1).png
			if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
			{
				FileUtil.CopyFileOrDirectory(path, targetPath);
			}
			else
			{
				int i = 1;
				string newFinalName;
				string finalDestination;

				//If it is a directory, we can keep the full name and add (1) after.
				//If it is a file, we need to remove the extension.
				string name = isDirectory ? file.FullName : path.Replace(file.Extension, "");

				//In files, we need to add the extension after (1).
				string extension = isDirectory ? "" : file.Extension;
			
				while (true)
				{
					//            MyFile        (i)       .extension
					newFinalName = name + " (" + i + ")" + extension;

					//We can create a FileInfo even if it's a directory. We just need the name.
					finalDestination = Path.Combine(to.FullName, new FileInfo(newFinalName).Name);

					//If something already has that name, keep iterating.
					if (File.Exists(finalDestination) || Directory.Exists(finalDestination))
						i++;
					else
						break;
				}

				FileUtil.CopyFileOrDirectory(path, finalDestination);
			}
			
		}

		//Checks if path is a folder. If not, it's a file.
		public static bool IsDirectory(string path)
		{
			FileAttributes attr = File.GetAttributes(path);

			return (attr & FileAttributes.Directory) == FileAttributes.Directory;
		}
	}
}