#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CursorTextureValidator : EditorWindow
{
    [MenuItem("Tools/Validate Cursor Textures")]
    public static void ValidateCursorTextures()
    {
        CursorManager[] cursorManagers = GameObject.FindObjectsOfType<CursorManager>();
        
        foreach (CursorManager cm in cursorManagers)
        {
            ValidateTexture(cm.gameObject.name, "m_DefaultCursor");
            ValidateTexture(cm.gameObject.name, "m_ClickableCursor");
        }
    }

    private static void ValidateTexture(string objectName, string fieldName)
    {
        SerializedObject so = new SerializedObject(FindObjectOfType<CursorManager>());
        SerializedProperty prop = so.FindProperty(fieldName);
        
        if (prop != null && prop.objectReferenceValue != null)
        {
            Texture2D texture = prop.objectReferenceValue as Texture2D;
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                bool hasIssues = false;
                
                if (!importer.isReadable)
                {
                    Debug.LogError($"Cursor texture {texture.name} in {objectName} needs Read/Write Enabled!");
                    hasIssues = true;
                }
                
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    Debug.LogError($"Cursor texture {texture.name} in {objectName} should use uncompressed format!");
                    hasIssues = true;
                }
                
                if (!hasIssues)
                {
                    Debug.Log($"Cursor texture {texture.name} in {objectName} is properly configured.");
                }
            }
        }
        else
        {
            Debug.LogError($"Missing cursor texture reference for {fieldName} in {objectName}!");
        }
    }
}
#endif 