using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace JPBotelho
{
	public static class RecycleBinFunctions
	{
		public static string recycleBinPath
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
				string unusedReturnsPath;
				return RecycleBinPreferences.Create(out unusedReturnsPath);
			}
		}


		/// <summary>
		/// Clears the trash directory.
		/// </summary>
		public static void ClearRecycleBinDirectory()
		{
			string folderPath = recycleBinPath;
			FileFunctions.ClearDirectory(new DirectoryInfo(folderPath));
			//This is here for the sake of it, might just set preferences.trash to an empty list.
			RefreshSearch("");
		}

		/// <summary>
		/// Copies a File / Directory at a given path to the Trash folder if eligible
		/// </summary>
		public static void DeleteAndCopyToRecycleBin(FileInfo file)
		{
			//The input parameter is relative to the project folder e.g.: /Assets/MyScript.cs
			string assetPath = Path.Combine(projectFolder, file.FullName); 

			DirectoryInfo recycleBinDirectory = new DirectoryInfo(recycleBinPath);

			if (FileFunctions.IsDirectory(assetPath))
			{
				FileFunctions.CopyFileOrDirectory(assetPath, recycleBinDirectory);
			}
			else
			{
				if (recycleBinPreferences.IsEligibleToSave(file))
				{
					FileFunctions.CopyFileOrDirectory(assetPath, recycleBinDirectory);
				}
			}

			FileUtil.DeleteFileOrDirectory(assetPath);

			RefreshSearch("");
		}

		//Copies all the files in the recycle bin to the assets folder.
		public static void CopyFilesFromBinToAssetsFolder()
		{
			List<string> paths = new List<string>();

			paths.AddRange(Directory.GetFiles(recycleBinPath));
			paths.AddRange(Directory.GetDirectories(recycleBinPath));

			for (int i = 0; i < paths.Count; i++)
			{
				string assetPath = Path.Combine(Application.dataPath, Path.GetFileName(paths[i]));

				FileUtil.CopyFileOrDirectory(paths[i], assetPath);
				FileUtil.DeleteFileOrDirectory(paths[i]);
				AssetDatabase.Refresh();

				RefreshSearch("");
			}
		}

		/// <summary>
		/// Gets all files in the recycle bin.
		/// </summary>
		/// <param name="search">Searches for files that contain this search. Case insensitive.</param>
		/// <returns></returns>
		public static List<TrashFile> SearchFilesInRecycleBin(string search)
		{
			List<TrashFile> trashFiles = new List<TrashFile>();

			List<string> recycleBinMembers = FileFunctions.GetFilesAndDirectories(new DirectoryInfo(recycleBinPath));
			
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

		/// <summary>
		/// Refreshes the trash search. If search string is empty (""), all objects are displayed
		/// </summary>
		/// <param name="search"></param>
		public static void RefreshSearch(string search)
		{
			recycleBinPreferences.trash = SearchFilesInRecycleBin(search);
			recycleBinPreferences.trash.Sort(new ComparerByDate());
		}

		//Returns full path to the project folder.
		public static string GetProjectFolder()
		{
			// -7 > Removes /Assets
			return Application.dataPath.Remove(Application.dataPath.ToCharArray().Length - 7);
		}

		// Formats dates to custom standard. 
		// Anything other than DD/MM/YYYY is retarded anyway. *shrug*
		public static string FormatDate(System.DateTime date)
		{
			string minute = date.Minute.ToString();

			if (minute.ToCharArray().Length == 1)
				minute = "0" + minute.ToCharArray()[0];

			return date.Day + "/" + date.Month + "/" + date.Year + "   " + date.Hour + ":" + minute;
		}
	}
}