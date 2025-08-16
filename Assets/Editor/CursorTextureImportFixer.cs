#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CursorTextureImportFixer : EditorWindow
{
    [MenuItem("Tools/Fix Cursor Texture Settings")]
    public static void FixCursorTextures()
    {
        CursorManager cursorManager = FindObjectOfType<CursorManager>();
        if (cursorManager == null)
        {
            Debug.LogError("No CursorManager found in scene!");
            return;
        }

        SerializedObject so = new SerializedObject(cursorManager);
        FixTextureSettings(so.FindProperty("m_DefaultCursor"));
        FixTextureSettings(so.FindProperty("m_ClickableCursor"));
        
        Debug.Log("Cursor texture settings have been updated. Please rebuild your project.");
    }

    private static void FixTextureSettings(SerializedProperty textureProp)
    {
        if (textureProp != null && textureProp.objectReferenceValue != null)
        {
            Texture2D texture = textureProp.objectReferenceValue as Texture2D;
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Cursor;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.textureFormat = TextureImporterFormat.RGBA32;
                
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                
                Debug.Log($"Fixed import settings for {texture.name}");
            }
        }
    }
}
#endif 