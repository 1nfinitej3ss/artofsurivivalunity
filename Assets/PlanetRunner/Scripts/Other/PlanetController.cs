using UnityEngine;
using System;
using System.Collections;

namespace PlanetRunner {
	public class PlanetController : MonoBehaviour {
		public GameObject[] objectPrefabs;  // Array of unique prefabs
		
		[System.Serializable]
		public class BuildingOffset {
			public float distanceFromEdge;
			public float angleOffset = 0f;
			public Vector2 positionOffset = Vector2.zero;  // Additional x,y offset for fine-tuning
		}
		
		public BuildingOffset[] buildingOffsets;  // Array of offsets for each building type
		
		public int numberOfObjects = 5;
		public float rotationSpeed = .1f;
		public Vector2 objectSize = new Vector2(1f, 1f);
		public float startAngle = 30f;
		
		private GameObject player;

		// Store rotation persistently
		private const string PLANET_ROTATION_KEY = "PlanetRotation";
		private const string PLAYER_ANGLE_KEY = "PlayerPlanetAngle";
		private const string PLAYER_DISTANCE_KEY = "PlayerPlanetDistance";

		[System.Serializable]
		public class PrefabSpriteData {
			public Sprite defaultSprite;
			public Sprite intersectSprite;
		}

		public PrefabSpriteData[] prefabSprites;

		// Add new public variables for scaling and distance control
		[Header("Dynamic Scaling Settings")]
		public float minScale = 0.8f;
		public float maxScale = 1.2f;
		public float minDistanceOffset = -0.5f;  // How much closer objects can get
		public float maxDistanceOffset = 0.5f;   // How much further objects can get
		public float scalingSmoothness = 5f;     // How smoothly to interpolate changes
		
		[Header("Building Placement")]
		public float defaultDistanceFromEdge = 3.5f;  // Add this new variable

		[Header("Building Scene Mapping")]
		[SerializeField] private string[] buildingSceneNames = { "home", "work", "studio", "social", "gallery" };

		void Start() {
			player = GameObject.FindGameObjectWithTag("Player");

			if (objectPrefabs.Length != prefabSprites.Length) {
				Debug.LogError("Length of objectPrefabs and prefabSprites must be the same.");
				return;
			}

			// Initialize building offsets only if null or wrong length
			if (buildingOffsets == null || buildingOffsets.Length != objectPrefabs.Length) {
				Debug.LogWarning("Initializing building offsets to default values");
				BuildingOffset[] newOffsets = new BuildingOffset[objectPrefabs.Length];
				
				// Preserve existing values if possible
				for (int i = 0; i < objectPrefabs.Length; i++) {
					if (buildingOffsets != null && i < buildingOffsets.Length) {
						newOffsets[i] = buildingOffsets[i] ?? new BuildingOffset { distanceFromEdge = defaultDistanceFromEdge };
					} else {
						newOffsets[i] = new BuildingOffset { distanceFromEdge = defaultDistanceFromEdge };
					}
				}
				
				buildingOffsets = newOffsets;
			}

			// Load saved rotation or use default
			if (PlayerPrefs.HasKey(PLANET_ROTATION_KEY)) {
				float savedRotation = PlayerPrefs.GetFloat(PLANET_ROTATION_KEY);
				transform.rotation = Quaternion.Euler(0f, 0f, savedRotation);
			}

			// Restore player position if saved
			if (player != null && PlayerPrefs.HasKey(PLAYER_ANGLE_KEY)) {
				float savedAngle = PlayerPrefs.GetFloat(PLAYER_ANGLE_KEY);
				float savedDistance = PlayerPrefs.GetFloat(PLAYER_DISTANCE_KEY);
				
				float angleRad = savedAngle * Mathf.Deg2Rad;
				Vector2 position = new Vector2(
					Mathf.Cos(angleRad) * savedDistance,
					Mathf.Sin(angleRad) * savedDistance
				);
				
				player.transform.position = transform.position + (Vector3)position;
			}
			
			// Create prefabs
			SpawnPrefabs();
		}

		void OnDestroy() {
			// Save the exact rotation angle
			float currentRotation = transform.rotation.eulerAngles.z;
			PlayerPrefs.SetFloat(PLANET_ROTATION_KEY, currentRotation);

			// Save player position relative to planet
			if (player != null) {
				Vector2 relativePosition = player.transform.position - transform.position;
				float angle = Mathf.Atan2(relativePosition.y, relativePosition.x) * Mathf.Rad2Deg;
				float distance = relativePosition.magnitude;
				
				PlayerPrefs.SetFloat(PLAYER_ANGLE_KEY, angle);
				PlayerPrefs.SetFloat(PLAYER_DISTANCE_KEY, distance);
			}

			PlayerPrefs.Save();
		}

		private void SpawnPrefabs() {
			// Add validation checks at the start
			if (objectPrefabs == null || objectPrefabs.Length == 0) {
				Debug.LogError("No prefabs assigned to objectPrefabs array!");
				return;
			}

			Debug.Log($"Starting spawn with {objectPrefabs.Length} prefabs, numberOfObjects: {numberOfObjects}");
			
			float angleStep = 360f / numberOfObjects;
			float currentRotation = transform.rotation.eulerAngles.z;
			float randomOffset = UnityEngine.Random.Range(-5f, 5f);
			
			CircleCollider2D planetCollider = GetComponent<CircleCollider2D>();
			if (planetCollider == null) {
				Debug.LogError("No CircleCollider2D found on planet!");
				return;
			}
			
			float colliderRadius = planetCollider.radius * transform.localScale.x;
			
			for (int i = 0; i < numberOfObjects; i++) {
				int prefabIndex = i % objectPrefabs.Length;
				BuildingOffset offset = buildingOffsets[prefabIndex];
				
				// Calculate angle with individual offset
				float angle = (angleStep * i + startAngle + currentRotation + randomOffset + offset.angleOffset);
				float distance = colliderRadius + offset.distanceFromEdge;
				float angleRad = angle * Mathf.Deg2Rad;
				
				GameObject obj = Instantiate(objectPrefabs[prefabIndex], transform);
				
				// Calculate base position
				Vector3 basePosition = transform.position + new Vector3(
					Mathf.Cos(angleRad) * distance,
					Mathf.Sin(angleRad) * distance,
					0
				);
				
				// Add individual position offset
				Vector3 finalPosition = basePosition + new Vector3(offset.positionOffset.x, offset.positionOffset.y, 0);
				
				obj.transform.position = finalPosition;
				obj.transform.localScale = objectSize;

				// Rotate to face planet center
				Vector2 directionToPlanet = ((Vector2)transform.position - (Vector2)obj.transform.position).normalized;
				float rotationZ = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg;
				obj.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90);

				SetupIntersectionHandler(obj, prefabIndex);
			}
		}

		void Update() {
			// Existing rotation code
			transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

			// Only proceed if we have a player reference
			if (player != null) {
				// Update each child object's rotation and scale
				foreach (Transform child in transform) {
					UpdateObjectRotation(child.gameObject);
					UpdateObjectScale(child.gameObject);
				}
			}
		}

		private void UpdateObjectRotation(GameObject obj) {
			// Keep object facing planet center
			Vector2 directionToPlanet = (Vector2)transform.position - (Vector2)obj.transform.position;
			float rotationZ = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg;
			obj.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ - 90);
		}

		private void UpdateObjectScale(GameObject obj) {
			// Calculate distances
			float playerDistanceFromPlanet = Vector2.Distance(player.transform.position, transform.position);
			float playerToObjDistance = Vector2.Distance(player.transform.position, obj.transform.position);
			
			// Get normalized directions from planet center
			Vector2 playerDir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
			Vector2 objDir = ((Vector2)obj.transform.position - (Vector2)transform.position).normalized;

			// Calculate dot product to determine if object is on same side as player
			float dot = Vector2.Dot(playerDir, objDir);
			
			// Calculate scale factor based on distance to player (closer = larger)
			float maxViewDistance = playerDistanceFromPlanet * 1.5f; // Reduced from 2f for tighter scaling
			float distanceFactor = 1f - Mathf.Clamp01(playerToObjDistance / maxViewDistance);
			
			// Combine dot product and distance for final scale factor
			float t = distanceFactor * (dot * 0.5f + 0.5f);
			
			// Get the building's original configuration
			PrefabIntersectionHandler handler = obj.GetComponent<PrefabIntersectionHandler>();
			if (handler == null || handler.prefabIndex >= buildingOffsets.Length) return;
			
			BuildingOffset offset = buildingOffsets[handler.prefabIndex];
			CircleCollider2D planetCollider = GetComponent<CircleCollider2D>();
			if (planetCollider == null) return;

			// Calculate base values
			float planetRadius = planetCollider.radius * transform.localScale.x;
			
			// Calculate scale
			float scaleFactor = Mathf.Lerp(minScale, maxScale, t);
			Vector3 targetScale = objectSize * scaleFactor;
			
			// Calculate the base distance using the offset - add a small constant offset
			float baseDistance = planetRadius + (offset.distanceFromEdge * scaleFactor) + 0.5f;
			
			// Calculate position offset based on scale - smaller adjustment range
			float distanceOffset = (scaleFactor - 1f) * offset.distanceFromEdge * 0.5f;
			float targetDistance = baseDistance + distanceOffset;

			// Smoothly interpolate current scale and position
			obj.transform.localScale = Vector3.Lerp(
				obj.transform.localScale, 
				targetScale, 
				Time.deltaTime * scalingSmoothness
			);

			// Calculate and update position, including the position offset from BuildingOffset
			Vector2 targetPosition = (Vector2)transform.position + 
				(objDir * targetDistance) + 
				(offset.positionOffset * scaleFactor); // Scale the position offset with the object

			obj.transform.position = Vector3.Lerp(
				obj.transform.position,
				targetPosition,
				Time.deltaTime * scalingSmoothness
			);
		}

		private void SetupIntersectionHandler(GameObject obj, int index) {
			SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
			BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
			
			if (collider == null) {
				collider = obj.AddComponent<BoxCollider2D>();
			}
			
			// Ensure collider matches the sprite bounds
			collider.isTrigger = true;
			
			// Make the collider smaller than the sprite
			Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
			collider.size = spriteSize * 0.65f;  // Reduce to 70% of sprite size
			
			// Center the collider
			collider.offset = spriteRenderer.sprite.bounds.center;
			
			// Remove any existing handler before adding a new one
			PrefabIntersectionHandler existingHandler = obj.GetComponent<PrefabIntersectionHandler>();
			if (existingHandler != null) {
				Destroy(existingHandler);
			}
			
			PrefabIntersectionHandler handler = obj.AddComponent<PrefabIntersectionHandler>();
			handler.prefabIndex = index;
			handler.Initialize(spriteRenderer, prefabSprites[index % prefabSprites.Length]);
			
			// Pass the scene names array to the handler
			handler.SetSceneNames(buildingSceneNames);
		}
	}

	public class PrefabIntersectionHandler : MonoBehaviour {
		private SpriteRenderer spriteRenderer;
		private PlanetController.PrefabSpriteData spriteData;
		private Vector3 originalScale;
		public int prefabIndex;  // Add this field
		
		// Remove the static array and make it dynamic
		[Header("Scene Management")]
		[SerializeField] private string[] sceneNames;
		[SerializeField] private bool enableDirectSceneLoading = true;
		[SerializeField] private bool requirePlayerCollision = true;
		
		private bool playerInRange = false;
		
		public void Initialize(SpriteRenderer renderer, PlanetController.PrefabSpriteData data) {
			spriteRenderer = renderer;
			spriteData = data;
			originalScale = transform.localScale;
		}

		// Add method to set scene names from PlanetController
		public void SetSceneNames(string[] _sceneNames) {
			sceneNames = _sceneNames;
		}
		
		private void OnTriggerEnter2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				playerInRange = true;
				StopAllCoroutines();
				StartCoroutine(SwitchToIntersectSprite());
			}
		}
		
		private void OnTriggerExit2D(Collider2D other) {
			if (other.CompareTag("Player")) {
				playerInRange = false;
				spriteRenderer.sprite = spriteData.defaultSprite;
			}
		}

		// Add new method for click handling
		private void OnMouseDown() {
			if (!enableDirectSceneLoading) return;
			
			// Check if player collision is required
			if (requirePlayerCollision && !playerInRange) {
				Debug.Log("Player must be in range to enter building");
				return;
			}
			
			// Validate prefab index and scene name
			if (prefabIndex >= 0 && prefabIndex < sceneNames.Length) {
				string sceneName = sceneNames[prefabIndex];
				Debug.Log($"Building clicked! Loading scene: {sceneName}");
				
				// Use direct scene loading for immediate response
				UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
			} else {
				Debug.LogWarning($"Invalid prefab index {prefabIndex} or missing scene name");
			}
		}

		private IEnumerator SwitchToIntersectSprite() {
			// Switch sprite immediately
			spriteRenderer.sprite = spriteData.intersectSprite;
			
			float elapsed = 0f;
			float duration = 0.4f;  // Slightly longer duration for smoother effect
			
			// Store initial scale in case it was modified by other effects
			Vector3 startScale = transform.localScale;
			
			while (elapsed < duration) {
				elapsed += Time.deltaTime;
				float t = elapsed / duration;
				
				// Use a gentler easing function
				float smoothT = t * t * (3f - 2f * t);  // Smoothstep function
				
				// Much smaller scale pulse - only 1% increase
				float scale = 1f + Mathf.Sin(smoothT * Mathf.PI) * 0.01f;
				transform.localScale = startScale * scale;
				
				yield return null;
			}

			// Ensure we end exactly at the original scale
			transform.localScale = startScale;
		}
	}
}
