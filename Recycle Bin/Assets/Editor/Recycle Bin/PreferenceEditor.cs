using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace JPBotelho
{
	[CustomEditor(typeof(RecycleBinPreferences))]
	public class PreferenceEditor : Editor
	{
		SerializedProperty all;
		SerializedProperty none;
		SerializedProperty name_;
		SerializedProperty search;

		public static GUIStyle skin = new GUIStyle();

		bool settings; //Foldout
		RecycleBinPreferences pref;

		bool showSubfolders = true;
		bool showDate = true;

		Texture folder;
		Texture file;

		string recycleBin;

		public void OnEnable()
		{
			recycleBin = RecycleBinFunctions.recycleBinPath;

			showSubfolders = EditorPrefs.GetBool("show");
			showDate = EditorPrefs.GetBool("date");

			pref = (RecycleBinPreferences)target;

			skin.alignment = TextAnchor.MiddleCenter;
			skin.fontStyle = FontStyle.Bold;

			folder = (Texture)AssetDatabase.LoadMainAssetAtPath("Assets/Gizmos/folder.png");
			file = (Texture)AssetDatabase.LoadMainAssetAtPath("Assets/Gizmos/file.png");

			all = serializedObject.FindProperty("saveAll");
			none = serializedObject.FindProperty("saveNone");
			name_ = serializedObject.FindProperty("folderName");
			search = serializedObject.FindProperty("search");

			RecycleBinFunctions.RefreshSearch("");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.LabelField("Recycle Bin", EditorStyles.centeredGreyMiniLabel);

			settings = EditorGUILayout.Foldout(settings, "Settings");

			if (settings)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.Space();

				EditorGUILayout.PropertyField(name_, new GUIContent("Folder Name"));

				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Path:");
				EditorGUILayout.LabelField(recycleBin);

				EditorGUILayout.Space();

				//Draw everything besides script name and deleted file list. This way there's no need to reimplement arrays of strings for the extensions.
				DrawPropertiesExcluding(serializedObject, new string[] { "m_Script", "trash" });

				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				EditorGUILayout.PropertyField(all, new GUIContent("Save All"));
				EditorGUILayout.PropertyField(none, new GUIContent("Discard All"));

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();

			List<TrashFile> list = pref.trash;

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Trash", skin);

			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();

			//Search field
			EditorGUILayout.PropertyField(search, new GUIContent("Search"));

			EditorGUILayout.Space();

			//View folder content/data fields
			showSubfolders = EditorGUILayout.Toggle(new GUIContent("View Folder Content"), showSubfolders);
			showDate = EditorGUILayout.Toggle(new GUIContent("View Date"), showDate);


			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
				RecycleBinFunctions.RefreshSearch(pref.search);
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			//Draws files and directories.
			for (int i = 0; i < list.Count; i++)
			{
				if (!FileFunctions.IsDirectory(list[i].path))
				{
					DrawFile(list[i].path, true, true);
				}
				else
				{
					DrawFolderRecursive(new DirectoryInfo(list[i].path), true);
				}

				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			//Draws Delete / Restore All 
			DrawButtons();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorPrefs.SetBool("show", showSubfolders);
			EditorPrefs.SetBool("date", showDate);

			serializedObject.ApplyModifiedProperties();
		}

		// Draws file. 
		// parameter box: draws bounding box.
		// parameter button: draws delete/restore buttons.
		void DrawFile(string path, bool box, bool button)
		{
			FileInfo info = new FileInfo(path);

			if (!box)
			{
				GUILayout.Label(new GUIContent("   " + Path.GetFileName(path), file));

				GUILayout.BeginHorizontal();

				if (button)
				{
					if (GUILayout.Button("Delete"))
					{
						Delete(new FileInfo(path));
					}

					if (GUILayout.Button("Restore"))
					{
						Restore(new FileInfo(path));
					}
				}

				GUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

				GUILayout.Label(new GUIContent("  " + Path.GetFileName(path), file));

				if (showDate)
				{
					GUILayoutOption date = GUILayout.Width(120);
					GUILayout.Label(RecycleBinFunctions.FormatDate(info.LastAccessTime), date);
				}

				GUILayoutOption option = GUILayout.Width(65);

				if (GUILayout.Button(new GUIContent("Delete"), option))
				{
					Delete(new FileInfo(path));
				}
				if (GUILayout.Button(new GUIContent("Restore"), option))
				{
					Restore(new FileInfo(path));
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		//Draws delete/restore/open buttons.
		private void DrawButtons()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("Delete All"))
			{
				if (EditorUtility.DisplayDialog("Delete Trash?", "Are you sure you want to complete this action?", "Yes", "No"))
				{
					RecycleBinFunctions.ClearRecycleBinDirectory();
				}
			}

			if (GUILayout.Button("Restore All"))
			{
				if (EditorUtility.DisplayDialog("Restore Trash?", "Are you sure you want to complete this action?", "Yes", "No"))
				{
					RecycleBinFunctions.CopyFilesFromBinToAssetsFolder();
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (GUILayout.Button("Open Trash"))
			{
				FileFunctions.OpenFolder(RecycleBinFunctions.GetRecycleBinAndCreateIfNull());
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawFolderRecursive(DirectoryInfo info, bool buttons)
		{
			if (Directory.Exists(info.FullName))
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				EditorGUILayout.BeginHorizontal();

				GUILayout.Label(new GUIContent("  " + info.Name, folder));


				if (buttons)
				{
					if (showDate)
					{
						GUILayoutOption date = GUILayout.Width(120);

						GUILayout.Label(RecycleBinFunctions.FormatDate(info.LastAccessTime), date);
					}

					GUILayoutOption option = GUILayout.Width(65);

					if (GUILayout.Button(new GUIContent("Delete"), option))
					{
						Delete(info);
					}

					if (GUILayout.Button(new GUIContent("Restore"), option))
					{
						Restore(info);
					}
				}

				EditorGUILayout.EndHorizontal();

				if (showSubfolders)
				{
					if (Directory.Exists(info.FullName)) //Doesnt Draw Subfolders if parent was deleted, calling GetFiles or GetDirectories on an invalid folder throws exceptions
					{
						foreach (FileInfo file in info.GetFiles())
						{
							EditorGUI.indentLevel++;

							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(30);
							DrawFile(file.FullName, false, false);
							EditorGUILayout.EndHorizontal();
							EditorGUI.indentLevel--;

							GUILayout.Space(5);
						}

						foreach (DirectoryInfo dir in info.GetDirectories())
						{
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(30);
							DrawFolderRecursive(dir, false);
							EditorGUILayout.EndHorizontal();

							GUILayout.Space(5);
						}
					}
				}
				EditorGUI.indentLevel--;

				EditorGUILayout.EndVertical();
			}
		}

		void Delete(DirectoryInfo info)
		{
			if (EditorUtility.DisplayDialog("Recycle Bin", "Delete " + info.Name + "?", "Yes", "No"))
			{
				FileUtil.DeleteFileOrDirectory(info.FullName);

				RecycleBinFunctions.RefreshSearch("");
			}
		}

		void Delete(FileInfo info)
		{
			if (EditorUtility.DisplayDialog("Recycle Bin", "Delete " + info.Name + "?", "Yes", "No"))
			{
				FileUtil.DeleteFileOrDirectory(info.FullName);

				RecycleBinFunctions.RefreshSearch("");
			}
		}

		void Restore(FileInfo info)
		{
			if (EditorUtility.DisplayDialog("Recycle Bin", "Restore " + info.Name + "?", "Yes", "No"))
			{
				FileUtil.CopyFileOrDirectory(info.FullName, Path.Combine(Application.dataPath, info.Name));
				FileUtil.DeleteFileOrDirectory(info.FullName);

				AssetDatabase.Refresh();
				RecycleBinFunctions.RefreshSearch("");
			}
		}

		void Restore(DirectoryInfo info)
		{
			if (EditorUtility.DisplayDialog("Recycle Bin", "Restore " + info.Name + "?", "Yes", "No"))
			{
				FileUtil.CopyFileOrDirectory(info.FullName, Path.Combine(Application.dataPath, info.Name));
				FileUtil.DeleteFileOrDirectory(info.FullName);

				AssetDatabase.Refresh();
				RecycleBinFunctions.RefreshSearch("");
			}
		}

	}
}
