using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class Preferences : ScriptableObject 
{
    public List<string> includeExtensions = new List<string>()
    {
        ".cs",
        ".js",
        ".shader",
        ".mat",
        ".anim",
        ".unity",
        ".obj",
        ".fbx",
        ".dae",
        ".asset",
        ".prefab"
    };

    public List<string> excludeExtensions = new List<string>()
    {
        ".meta"
    };

    public List<TrashFile> trash = new List<TrashFile>();


    [HideInInspector]
    public bool saveAll =  true;
    [HideInInspector]
    public bool saveNone = false;
    [HideInInspector]
    public string folderName = "Trash";
    [HideInInspector]
    public string search = "";

    public bool IsEligibleToSave(FileInfo file)
    {
		if (saveAll && !excludeExtensions.Contains(file.Extension) || includeExtensions.Contains(file.Extension) && !saveNone)
        {
			return true;
		}
        else
        {
			return false;
		}
    }
}
