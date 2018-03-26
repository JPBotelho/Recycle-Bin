using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
	}
}