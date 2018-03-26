using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace JPBotelho
{
	public static class RecycleBinFunctions
	{
		public static string recycleBin
		{
			get { return GetRecycleBinAndCreateIfNull(); }
		}

		public static RecycleBinPreferences recycleBinPreferences
		{
			get { return GetRecycleBinPreferences(); }
		}

		public static string projectFolder
		{
			get { return GetProjectFolder(); }
		}

		/// <summary>
		/// Returns the recycle bin folder.
		/// </summary>
		/// <param name="create">Create folder if it doesn't exist.</param>
		/// <returns></returns>
		public static string GetRecycleBinAndCreateIfNull()
		{
			string expectedRecycleBinPath = Path.Combine(projectFolder, recycleBinPreferences.folderName);

			if (!Directory.Exists(expectedRecycleBinPath))
			{
				return CreateRecycleBinDirectory();
			}

			return expectedRecycleBinPath;
		}

		/// <summary>
		/// Creates a Trash Folder
		/// </summary>
		public static string CreateRecycleBinDirectory()
		{
			string finalPath = Path.Combine(projectFolder, recycleBinPreferences.folderName);
			
            Directory.CreateDirectory(Path.Combine(projectFolder, recycleBinPreferences.folderName));

			return finalPath;
		}

		/// <summary>
		/// Returns the Preferences scriptable object (.asset file).
		/// </summary>
		/// <returns></returns>
		public static RecycleBinPreferences GetRecycleBinPreferences()
		{
			List<RecycleBinPreferences> prefs = ScriptableObjectUtility.FindAssetsByType<RecycleBinPreferences>();

			//Multiple instances?
			if (prefs.Count >= 1)
			{
				return prefs[0];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Refreshes the trash search. If search string is empty (""), all objects are displayed
		/// </summary>
		/// <param name="search"></param>
		public static void RefreshSearch(string search)
		{
			recycleBinPreferences.trash = GetFiles(search);
			recycleBinPreferences.trash.Sort(new ComparerByDate());
		}

		/// <summary>
		/// Clears the trash directory.
		/// </summary>
		public static void ClearTrash()
		{
			string folderPath = recycleBin;

			if (Directory.Exists(folderPath))
			{
				DirectoryInfo info = new DirectoryInfo(folderPath);

				foreach (FileInfo file in info.GetFiles())
				{
					file.Delete();
				}
				foreach (DirectoryInfo dir in info.GetDirectories())
				{
					dir.Delete(true);
				}
			}

			RefreshSearch("");
		}


		public static RecycleBinPreferences CreateRecycleBinPreferences(out string path)
		{
			return ScriptableObjectUtility.CreateAsset<RecycleBinPreferences>(out path);
		}

		/// <summary>
		/// Copies a File / Directory at a given path to the Trash folder if eligible
		/// </summary>
		public static void DeleteAndCopyToRecycleBin(FileInfo file)
		{
			string assetPath = Path.Combine(projectFolder, file.FullName); //The input parameter is relative to the project folder e.g.: /Assets/MyScript.cs

			if (IsDirectory(assetPath))
			{
				DirectoryInfo currentDirectory = new DirectoryInfo(assetPath);

				string recycleBinPath = recycleBin;
				string pathInRecycleBin = Path.Combine(recycleBinPath, assetPath);

				DirectoryInfo finalDirectory = new DirectoryInfo(pathInRecycleBin);
				CopyFilesRecursively(currentDirectory, finalDirectory, true);
			}
			else
			{   //Just a couple checks based on file's extension
				if (recycleBinPreferences.IsEligibleToSave(file))
				{
					CopyFileOrDirectory(assetPath, new DirectoryInfo(recycleBin));
				}

				FileUtil.DeleteFileOrDirectory(assetPath);
			}

			RefreshSearch("");
		}

		public static string GetProjectFolder()
		{
			// -7 > Removes /Assets
			return Application.dataPath.Remove(Application.dataPath.ToCharArray().Length - 7);
		}

		/// <summary>
		/// Copies all the files and subfolders of a directory to a destination.
		/// </summary>    
		public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, bool createFolder)
		{
			string newName = source.Name;

			//Creates parent directory for all subfolders / files (First iteration only)
			if (createFolder)
			{
				DirectoryInfo newDir;

				string path = Path.Combine(recycleBin, source.Name);

				if (Directory.Exists(path))
				{

					int i = 1;

					while (true)
					{
						newName = source.Name + " (" + i + ")";

						if (Directory.Exists(Path.Combine(recycleBin, newName)))
							i++;
						else
							break;
					}

					newDir = Directory.CreateDirectory(Path.Combine(recycleBin, newName));
				}
				else
				{
					newDir = Directory.CreateDirectory(path);
				}


				foreach (DirectoryInfo dir in source.GetDirectories())
				{
					CopyFilesRecursively(dir, newDir.CreateSubdirectory(newName), false);
				}

				foreach (FileInfo file in source.GetFiles())
				{
					string extension = file.Extension;

					if (recycleBinPreferences.IsEligibleToSave(file))
					{
						CopyFileOrDirectory(file.FullName, newDir);
					}
				}
			}
			else
			{
				foreach (DirectoryInfo dir in source.GetDirectories())
				{
					CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), false);
				}

				foreach (FileInfo file in source.GetFiles())
				{
					string extension = file.Extension;

					if (recycleBinPreferences.IsEligibleToSave(file))
					{
						CopyFileOrDirectory(file.FullName, target);
					}
				}
			}
		}

		/// <summary>
		/// Gets all files in the recycle bin.
		/// </summary>
		/// <param name="search">Searches for files that contain this search. Case insensitive.</param>
		/// <returns></returns>
		public static List<TrashFile> GetFiles(string search)
		{
			List<TrashFile> trashFiles = new List<TrashFile>();

			string pathToRecycleBin = recycleBin;

			List<string> recycleBinMembers = FileFunctions.GetFilesAndDirectories(new DirectoryInfo(pathToRecycleBin));

			
			foreach (string s in recycleBinMembers)
			{
				// "" returns everything. It's the default case.
				if (search == "" || s.ToLower().Contains(search.ToLower()))
				{
					trashFiles.Add(new TrashFile(s));
				}
			}
		
			return trashFiles;
		}

		//Copies all the files in the recycle bin to the assets folder.
		public static void CopyFilesFromBinToAssetsFolder()
		{
			List<string> paths = new List<string>();

			paths.AddRange(Directory.GetFiles(recycleBin));
			paths.AddRange(Directory.GetDirectories(recycleBin));

			for (int i = 0; i < paths.Count; i++)
			{
				FileUtil.CopyFileOrDirectory(paths[i], Path.Combine(Application.dataPath, Path.GetFileName(paths[i])));
				FileUtil.DeleteFileOrDirectory(paths[i]);

				AssetDatabase.Refresh();
				RefreshSearch("");
			}
		}

		//Checks if path is a folder. If not, it's a file.
		public static bool IsDirectory(string path)
		{
			FileAttributes attr = File.GetAttributes(path);

			return (attr & FileAttributes.Directory) == FileAttributes.Directory;
		}


		/// <summary>
		/// Formats dates to custom standard. 
		/// Anything other than DD/MM/YYYY is retarded anyway. *shrug*
		/// </summary>
		/// <returns></returns>
		public static string FormatDate(System.DateTime date)
		{
			string minute = date.Minute.ToString();

			if (minute.ToCharArray().Length == 1)
				minute = "0" + minute.ToCharArray()[0];

			return date.Day + "/" + date.Month + "/" + date.Year + "   " + date.Hour + ":" + minute;
		}

		/// <summary>
		/// Copies a file or directory (checks for already existing files/folders and adds (NUMBER) to the new file's name)
		/// </summary>
		/// <param name="path">File/Directory to copy.</param>
		/// <param name="to">Directory to copy to.</param>
		public static void CopyFileOrDirectory(string path, DirectoryInfo to)
		{
			if (!IsDirectory(path))
			{
				FileInfo info = new FileInfo(path);

				string first = Path.Combine(to.FullName, info.Name);

				if (File.Exists(first))
				{
					int i = 1;

					string replace = path.Replace(info.Extension, "");

					string newName;

					while (true)
					{
						newName = replace + " (" + i + ")" + info.Extension;

						if (File.Exists(Path.Combine(to.FullName, new FileInfo(newName).Name)))
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
			else
			{
				DirectoryInfo info = new DirectoryInfo(path);

				string first = Path.Combine(to.FullName, info.Name);

				if (Directory.Exists(first))
				{
					int i = 1;

					string replace = path.Replace(info.Extension, "");

					string newName;

					while (true)
					{
						newName = replace + " (" + i + ")" + info.Extension;

						if (Directory.Exists(Path.Combine(to.FullName, new DirectoryInfo(newName).Name)))
							i++;
						else
							break;
					}

					FileUtil.CopyFileOrDirectory(path, Path.Combine(to.FullName, new DirectoryInfo(newName).Name));
				}
				else
				{
					FileUtil.CopyFileOrDirectory(path, first);
				}
			}
		}
	}
}