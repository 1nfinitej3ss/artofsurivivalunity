#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices; // For WebGL file handling

public class LoadJson : MonoBehaviour
{
    [SerializeField] private Text statusText;
    [SerializeField] private GameObject questionManagerPrefab;

    #if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL file handling
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
    #endif

    private void Start()
    {
        // Check if we already have a QuestionContentManager
        if (QuestionContentManager.Instance == null && questionManagerPrefab != null)
        {
            Debug.Log("Creating initial QuestionContentManager");
            GameObject qcmObject = Instantiate(questionManagerPrefab);
            DontDestroyOnLoad(qcmObject);
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            Debug.Log($"Key pressed: {Input.inputString}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("J key pressed - opening file dialog");
            OpenFileDialog();
        }
    }

    private void OpenFileDialog()
    {
        #if UNITY_EDITOR
        // Use Unity Editor file dialog
        string path = EditorUtility.OpenFilePanel("Select Questions JSON", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            OnFileSelected(path);
        }
        #elif UNITY_WEBGL && !UNITY_EDITOR
        // Use WebGL file upload
        UploadFile(gameObject.name, "OnFileSelected", ".json", false);
        #else
        // For standalone builds, you might want to use a different approach
        // For example, looking for files in a specific directory
        string defaultPath = Path.Combine(Application.persistentDataPath, "questions.json");
        if (File.Exists(defaultPath))
        {
            OnFileSelected(defaultPath);
        }
        else
        {
            Debug.LogWarning("No default questions file found at: " + defaultPath);
            if (statusText != null)
            {
                statusText.text = "No questions file found in default location";
            }
        }
        #endif
    }

    public void OnFileSelected(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            string jsonContent = LoadJsonFile(path);
            Debug.Log($"Read JSON file from {path}");

            var questionManager = QuestionContentManager.Instance;
            if (questionManager == null && questionManagerPrefab != null)
            {
                Debug.Log("Creating new QuestionContentManager instance");
                GameObject qmObject = Instantiate(questionManagerPrefab);
                questionManager = qmObject.GetComponent<QuestionContentManager>();
                DontDestroyOnLoad(qmObject);
            }

            if (questionManager != null)
            {
                Debug.Log("Setting custom JSON flag and loading JSON file");
                questionManager.SetCustomJsonAdded();
                questionManager.LoadCustomJson(jsonContent);
                if (statusText != null)
                {
                    statusText.text = "JSON file loaded successfully!";
                }
            }
            else
            {
                Debug.LogError("QuestionContentManager instance not found and prefab not assigned!");
            }
        }
        catch (System.Exception e)
        {
            if (statusText != null)
            {
                statusText.text = $"Error loading JSON: {e.Message}";
            }
            Debug.LogError($"Error loading JSON: {e}");
        }
    }

    private string LoadJsonFile(string path)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        // For WebGL, the path parameter will actually be the file content
        return path;
        #else
        // For other platforms, read the file from disk
        return File.ReadAllText(path);
        #endif
    }
} 