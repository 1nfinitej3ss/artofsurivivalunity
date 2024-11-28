using System.Collections;
using UnityEngine;

public class RectangleSpawner : MonoBehaviour
{
    public int amountOfRectangles = 10;
    public float circleRadius = 5f;
    public Vector2 rectangleSizeMin = new Vector2(0.5f, 0.5f);
    public Vector2 rectangleSizeMax = new Vector2(1f, 1f);
    public float randomness = 1f;
    public GameObject rectanglePrefab;

    private void Start()
    {
        if(rectanglePrefab == null) 
        {
            Debug.LogError("Rectangle Prefab is not assigned.");
            return;
        }

        StartCoroutine(SpawnRectangles());
    }

    private IEnumerator SpawnRectangles()
    {
        for (int i = 0; i < amountOfRectangles; i++)
        {
            float angle = (i / (float)amountOfRectangles) * 360f;
            Vector3 position = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleRadius;
            position += new Vector3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), 0);
            
            GameObject newRectangle = Instantiate(rectanglePrefab, position, Quaternion.Euler(0, 0, angle));
            newRectangle.transform.localScale = new Vector3(Random.Range(rectangleSizeMin.x, rectangleSizeMax.x), Random.Range(rectangleSizeMin.y, rectangleSizeMax.y), 1f);
            
            yield return null;
        }
    }
}
