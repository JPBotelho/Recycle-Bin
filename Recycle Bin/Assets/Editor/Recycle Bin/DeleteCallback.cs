using UnityEngine;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
public class DeleteCallback : UnityEditor.AssetModificationProcessor
{
	static AssetDeleteResult OnWillDeleteAsset (string path, RemoveAssetOptions options)
    {
        RecycleBinFunctions.DeleteAndCopyToRecycleBin(new FileInfo(path));
        
        return AssetDeleteResult.DidNotDelete;
    }
}
