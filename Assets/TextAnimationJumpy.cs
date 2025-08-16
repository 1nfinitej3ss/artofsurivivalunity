using System.Collections;
using UnityEngine;
using TMPro;

public class TextAnimationJumpy : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    [SerializeField] private float bounceHeight = 0.5f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float offsetBetweenChars = 0.1f;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh != null)
        {
            StartCoroutine(AnimateText());
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found!");
        }
    }

    IEnumerator AnimateText()
    {
        while (true)
        {
            textMesh.ForceMeshUpdate();
            TMP_TextInfo textInfo = textMesh.textInfo;

            for (float t = 0; t < Mathf.PI * 2; t += Time.deltaTime * bounceSpeed)
            {
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible) continue;

                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                    float charOffset = t + (i * offsetBetweenChars);
                    float yOffset = Mathf.Sin(charOffset) * bounceHeight;

                    for (int j = 0; j < 4; j++)
                    {
                        vertices[vertexIndex + j].y += yOffset;
                    }
                }

                // Update the mesh
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                yield return null;
            }
        }
    }
}
