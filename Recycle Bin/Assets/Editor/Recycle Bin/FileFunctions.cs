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

		//Copies files or directories to a target directory. 
		//Members with the same name get (X) added after.
		//Recursive behaviour is handled by unity's CopyFileOrDirectory (doesn't handle matching names, hence this function)-
		public static void CopyFileOrDirectory(string path, DirectoryInfo to)
		{
			//Directories need some separate logic, but the implementation is the same.
			bool isDirectory = RecycleBinFunctions.IsDirectory(path);

			//If it's a file it will have extension, else it won't, so no need to use DirectoryInfo
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
	}
}