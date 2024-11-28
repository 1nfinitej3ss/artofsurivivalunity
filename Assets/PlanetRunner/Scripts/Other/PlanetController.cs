using UnityEngine;

namespace PlanetRunner {
	public class PlanetController : MonoBehaviour {
		public GameObject[] objectPrefabs;  // Array of unique prefabs
		public float[] objectDistances;  // Array of distances for each prefab
		public int numberOfObjects = 5;
		public float rotationSpeed = .1f;
		public Vector2 objectSize = new Vector2(1f, 1f);
		public float startAngle = 0f;  // The start angle for positioning prefabs

		[System.Serializable]
		public class PrefabSpriteData {
			public Sprite defaultSprite;
			public Sprite intersectSprite;
		}

		public PrefabSpriteData[] prefabSprites;  // Array of sprite pairs for each prefab

		public float minScale = 0.7f;  // Minimum scale when furthest from player
		public float maxScale = 1.3f;  // Maximum scale when closest to player
		public float minDistance = 8.5f;  // Distance when furthest from player
		public float maxDistance = 9.0f;  // Distance when closest to player
		private GameObject player;  // Reference to the player

		void Start() {
			player = GameObject.FindGameObjectWithTag("Player");

			// Ensure the objectPrefabs and objectDistances are of the same length
			if (objectPrefabs.Length != objectDistances.Length) {
				Debug.LogError("Length of objectPrefabs and objectDistances must be the same.");
				return;
			}

			// Position the objects around the planet
			float angleStep = 360f / numberOfObjects;
			for (int i = 0; i < numberOfObjects; i++) {
				float angle = (angleStep * i + startAngle) * Mathf.Deg2Rad;  // Convert to radians and add startAngle

				// Use the corresponding distance for each object
				float objectDistance = objectDistances[i % objectDistances.Length];
				Vector2 position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * objectDistance;

				// Select a prefab
				GameObject prefab = objectPrefabs[i % objectPrefabs.Length];

				GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);

				// Set the object size
				obj.transform.localScale = objectSize;

				// Orient the object towards the planet
				Vector2 directionToPlanet = (Vector2)transform.position - position;
				float rotationZ = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg;
				obj.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90);

				// Add intersection handler to ALL prefabs
				SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
				BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
				
				// Add collider if it doesn't exist
				if (collider == null) {
					collider = obj.AddComponent<BoxCollider2D>();
					collider.isTrigger = true;
				}
				
				// Add intersection handler component
				PrefabIntersectionHandler handler = obj.AddComponent<PrefabIntersectionHandler>();
				handler.Initialize(spriteRenderer, prefabSprites[i % prefabSprites.Length]);
			}
		}

		void Update() {
			// Rotate the planet and its children
			transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

			// Update scale of all child objects
			foreach (Transform child in transform) {
				UpdateObjectScale(child.gameObject);
			}
		}

		void UpdateObjectScale(GameObject obj) {
			if (player == null) return;

			// Calculate angle between player and object relative to planet center
			Vector2 playerDir = (player.transform.position - transform.position).normalized;
			Vector2 objectDir = (obj.transform.position - transform.position).normalized;
			
			// Calculate dot product to get the cosine of the angle between them
			float dot = Vector2.Dot(playerDir, objectDir);
			
			// Remap the dot product from [-1, 1] to [0, 1] for interpolation
			float t = (dot + 1f) * 0.5f;
			
			// Interpolate both scale and distance
			float scale = Mathf.Lerp(minScale, maxScale, t);
			float currentDistance = Mathf.Lerp(minDistance, maxDistance, t);
			
			// Calculate and set position
			Vector3 directionFromCenter = (obj.transform.position - transform.position).normalized;
			Vector3 targetPosition = transform.position + (directionFromCenter * currentDistance);
			
			// Apply position and scale
			obj.transform.position = targetPosition;
			obj.transform.localScale = objectSize * scale;
			
			// Update rotation
			Vector2 directionToPlanet = (Vector2)transform.position - (Vector2)obj.transform.position;
			float rotationZ = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg;
			obj.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90);
		}
	}

	public class PrefabIntersectionHandler : MonoBehaviour {
		private SpriteRenderer spriteRenderer;
		private PlanetController.PrefabSpriteData spriteData;
		
		public void Initialize(SpriteRenderer renderer, PlanetController.PrefabSpriteData data) {
			spriteRenderer = renderer;
			spriteData = data;
		}
		
		private void OnTriggerEnter2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				spriteRenderer.sprite = spriteData.intersectSprite;
			}
		}
		
		private void OnTriggerExit2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				spriteRenderer.sprite = spriteData.defaultSprite;
			}
		}
	}
}
