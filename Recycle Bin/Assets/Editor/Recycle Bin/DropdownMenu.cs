using UnityEditor;


public class DropdownMenu : EditorWindow
{
    [MenuItem("Window/Recycle Bin/Show")]
    public static void ShowWindow ()
    {
        string path = AssetDatabase.GetAssetPath(RecycleBinFunctions.GetPreferences());

        if (!string.IsNullOrEmpty(path))
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }
        else
        {
            ScriptableObjectUtility.CreateAsset<Preferences>();

            path = AssetDatabase.GetAssetPath(RecycleBinFunctions.GetPreferences());

            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }

        RecycleBinFunctions.RefreshSearch("");
    }

    [MenuItem("Window/Recycle Bin/Open Folder")]
    public static void OpenTrash ()
    {
        RecycleBinFunctions.OpenFolder(RecycleBinFunctions.GetRecycleBin(true));
    }
}