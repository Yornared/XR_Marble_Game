using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MarbleGame : MonoBehaviour
{
    [Header("Marble Settings")]
    public GameObject marblePrefab;
    public float marbleSize = 0.03f;
    public float forceMultiplier = 5f;
    public bool autoSpawnOnStart = true;
    public float autoSpawnDelay = 1f;
    public float spawnHeight = 0.05f;
    public int numberOfMarbles = 3;
    public Color[] marbleColors = new Color[3] { Color.red, Color.blue, Color.green };
    
    [Header("Physics Settings")]
    public float marbleMass = 1f;
    public float dynamicFriction = 0.2f;
    public float staticFriction = 0.2f;
    public float bounciness = 0.5f;
    
    [Header("References")]
    [Tooltip("Optional: If not set, will try to find in the scene")]
    public ARRaycastManager arRaycastManager;
    
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private List<GameObject> activeMarbles = new List<GameObject>();
    private Dictionary<int, int> touchToMarbleMap = new Dictionary<int, int>(); // Maps touch ID to marble index
    private Dictionary<int, Vector2> touchStartPositions = new Dictionary<int, Vector2>(); // Maps touch ID to start position
    private Transform marbleContainer;

    void Start()
    {
        if (arRaycastManager == null)
        {
            arRaycastManager = FindAnyObjectByType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.LogError("ARRaycastManager not found! Make sure it's added to the scene.");
            }
        }
        
        // Create a container for our marbles to keep the hierarchy clean
        marbleContainer = new GameObject("Marble Container").transform;
        
        // Initialize the marble list
        for (int i = 0; i < numberOfMarbles; i++)
        {
            activeMarbles.Add(null);
        }
        
        if (autoSpawnOnStart)
        {
            // Auto-spawn marbles after a short delay
            Invoke("SpawnInitialMarbles", autoSpawnDelay);
        }
    }

    void SpawnInitialMarbles()
    {
        // Try to spawn marbles at different positions on the screen
        float screenDivision = Screen.width / (numberOfMarbles + 1);
        bool atLeastOneSpawned = false;
        
        for (int i = 0; i < numberOfMarbles; i++)
        {
            Vector2 spawnPosition = new Vector2(screenDivision * (i + 1), Screen.height / 2);
            
            if (arRaycastManager.Raycast(spawnPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                SpawnMarbleAtScreenPosition(spawnPosition, i);
                atLeastOneSpawned = true;
            }
            else
            {
                // Try center of screen for first marble
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    // Add offset to separate marbles
                    Vector3 offset = Vector3.right * (i - (numberOfMarbles - 1) * 0.5f) * 0.2f;
                    SpawnMarbleAtWorldPosition(hitPose.position + offset, i);
                    atLeastOneSpawned = true;
                }
            }
        }
        
        // If no marbles were spawned, try again later
        if (!atLeastOneSpawned)
        {
            Invoke("SpawnInitialMarbles", 1f);
        }
    }

    void Update()
    {
        // Skip input processing if interacting with UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
            
        // Process touch input
        ProcessTouchInput();
        
    #if UNITY_EDITOR
        // Process mouse input in editor
        ProcessMouseInput();
    #endif
    }

    void ProcessTouchInput()
    {
        // Skip if touch support isn't available
        if (Touchscreen.current == null)
            return;
            
        // Get all active touches
        var touches = Touchscreen.current.touches;
        
        // Track which touch IDs we've seen this frame
        HashSet<int> processedTouchIds = new HashSet<int>();
        
        // Process all touches
        for (int i = 0; i < touches.Count; i++)
        {
            var touch = touches[i];
            
            // Only process active touches
            if (!touch.press.isPressed)
                continue;
                
            int touchId = touch.touchId.ReadValue();
            Vector2 position = touch.position.ReadValue();
            bool isNewTouch = touch.press.wasPressedThisFrame;
            
            processedTouchIds.Add(touchId);
            
            if (isNewTouch)
            {
                // Handle new touch
                HandleNewTouch(touchId, position);
            }
            else if (touchToMarbleMap.ContainsKey(touchId) && touchStartPositions.ContainsKey(touchId))
            {
                // Handle continued touch
                int marbleIndex = touchToMarbleMap[touchId];
                if (marbleIndex >= 0 && marbleIndex < activeMarbles.Count && 
                    activeMarbles[marbleIndex] != null)
                {
                    Vector2 startPos = touchStartPositions[touchId];
                    Vector2 direction = position - startPos;
                    
                    // Apply force to the marble
                    Rigidbody rb = activeMarbles[marbleIndex].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 force = new Vector3(direction.x, 0, direction.y).normalized * forceMultiplier;
                        rb.AddForce(force, ForceMode.Force);
                    }
                }
            }
        }
        
        // Remove any touch mappings for touches that are no longer active
        List<int> touchesToRemove = new List<int>();
        foreach (int touchId in touchToMarbleMap.Keys)
        {
            if (!processedTouchIds.Contains(touchId))
            {
                touchesToRemove.Add(touchId);
            }
        }
        
        foreach (int touchId in touchesToRemove)
        {
            touchToMarbleMap.Remove(touchId);
            touchStartPositions.Remove(touchId);
        }
    }
    
    void HandleNewTouch(int touchId, Vector2 position)
    {
        // Find the nearest marble
        int nearestMarbleIndex = GetNearestMarbleIndex(position);
        
        // If no marble is near, try to spawn a new one
        if (nearestMarbleIndex == -1)
        {
            // Find an empty slot
            for (int i = 0; i < activeMarbles.Count; i++)
            {
                if (activeMarbles[i] == null)
                {
                    nearestMarbleIndex = i;
                    SpawnMarbleAtScreenPosition(position, i);
                    break;
                }
            }
            
            // If no empty slots, use the first marble
            if (nearestMarbleIndex == -1 && activeMarbles.Count > 0)
            {
                nearestMarbleIndex = 0;
            }
        }
        
        // Register the touch with this marble
        if (nearestMarbleIndex != -1)
        {
            touchToMarbleMap[touchId] = nearestMarbleIndex;
            touchStartPositions[touchId] = position;
        }
    }
    
    void ProcessMouseInput()
    {
    #if UNITY_EDITOR
        // Skip if mouse isn't available
        if (Mouse.current == null)
            return;
            
        // Handle mouse input in editor for testing
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            
            // Find the nearest marble
            int nearestMarbleIndex = GetNearestMarbleIndex(mousePosition);
            
            // If no marble is near, spawn a new one
            if (nearestMarbleIndex == -1)
            {
                // Find an empty slot
                for (int i = 0; i < activeMarbles.Count; i++)
                {
                    if (activeMarbles[i] == null)
                    {
                        nearestMarbleIndex = i;
                        SpawnMarbleAtScreenPosition(mousePosition, i);
                        break;
                    }
                }
                
                // If no empty slots, use the first marble
                if (nearestMarbleIndex == -1 && activeMarbles.Count > 0)
                {
                    nearestMarbleIndex = 0;
                }
            }
            
            // Store the start position for this marble
            if (nearestMarbleIndex != -1)
            {
                // Use -1 as the "mouse touch ID"
                touchToMarbleMap[-1] = nearestMarbleIndex;
                touchStartPositions[-1] = mousePosition;
            }
        }
        else if (Mouse.current.leftButton.isPressed && 
                 touchToMarbleMap.ContainsKey(-1) && 
                 touchStartPositions.ContainsKey(-1))
        {
            // Mouse is being held down, control the associated marble
            int marbleIndex = touchToMarbleMap[-1];
            if (marbleIndex >= 0 && marbleIndex < activeMarbles.Count && 
                activeMarbles[marbleIndex] != null)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                Vector2 startPos = touchStartPositions[-1];
                Vector2 direction = mousePosition - startPos;
                
                // Apply force to the marble
                Rigidbody rb = activeMarbles[marbleIndex].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 force = new Vector3(direction.x, 0, direction.y).normalized * forceMultiplier;
                    rb.AddForce(force, ForceMode.Force);
                }
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Mouse button released, remove mapping
            touchToMarbleMap.Remove(-1);
            touchStartPositions.Remove(-1);
        }
    #endif
    }

    // Find the index of the marble closest to the given screen position
    private int GetNearestMarbleIndex(Vector2 screenPosition)
    {
        float nearestDistance = float.MaxValue;
        int nearestIndex = -1;
        
        for (int i = 0; i < activeMarbles.Count; i++)
        {
            GameObject marble = activeMarbles[i];
            if (marble != null && marble.activeSelf)
            {
                // Convert marble position to screen space
                Vector2 marbleScreenPos = Camera.main.WorldToScreenPoint(marble.transform.position);
                float distance = Vector2.Distance(marbleScreenPos, screenPosition);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }
        }
        
        // Only return if it's close enough to be considered a selection
        float selectionThreshold = Screen.width * 0.2f; // 20% of screen width
        return (nearestDistance < selectionThreshold) ? nearestIndex : -1;
    }
    
    // Spawn a marble at a screen position using raycasting
    private int SpawnMarbleAtScreenPosition(Vector2 screenPosition, int marbleIndex)
    {
        if (arRaycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            return SpawnMarbleAtWorldPosition(hitPose.position, marbleIndex);
        }
        
        return -1; // Failed to spawn
    }

    // Spawn a marble at a specific world position
    public int SpawnMarbleAtWorldPosition(Vector3 worldPosition, int marbleIndex)
    {
        // Make sure index is valid
        if (marbleIndex < 0 || marbleIndex >= numberOfMarbles)
        {
            Debug.LogWarning("Invalid marble index: " + marbleIndex + ". Using index 0.");
            marbleIndex = 0;
        }
        
        // Spawn position is slightly above the given position
        Vector3 spawnPosition = worldPosition + Vector3.up * spawnHeight;
        
        // If we already have a marble at this index, destroy it
        if (marbleIndex < activeMarbles.Count && activeMarbles[marbleIndex] != null)
        {
            Destroy(activeMarbles[marbleIndex]);
        }
        
        // Create the new marble
        GameObject marble = Instantiate(marblePrefab, spawnPosition, Quaternion.identity, marbleContainer);
        marble.name = "Marble_" + marbleIndex;
        
        // Set the marble size
        marble.transform.localScale = new Vector3(marbleSize, marbleSize, marbleSize);
        
        // Set up the marble color
        SetMarbleColor(marble, marbleIndex);
        
        // Set up physics components
        SetupMarblePhysics(marble);
        
        // Store the marble
        while (activeMarbles.Count <= marbleIndex)
        {
            activeMarbles.Add(null);
        }
        activeMarbles[marbleIndex] = marble;
        
        return marbleIndex;
    }
    
    // Set up the physics components for a marble
    private void SetupMarblePhysics(GameObject marble)
    {
        // Set up rigidbody
        Rigidbody rb = marble.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = marble.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.mass = marbleMass;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection
        
        // Set up sphere collider and physics material
        SphereCollider collider = marble.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = marble.AddComponent<SphereCollider>();
        }
        
        PhysicsMaterial physMaterial = new PhysicsMaterial
        {
            dynamicFriction = dynamicFriction,
            staticFriction = staticFriction,
            bounciness = bounciness,
            frictionCombine = PhysicsMaterialCombine.Average,
            bounceCombine = PhysicsMaterialCombine.Average
        };
        collider.material = physMaterial;
    }
    
    // Set the marble color based on its index
    private void SetMarbleColor(GameObject marble, int index)
    {
        // Make sure we have valid colors
        if (marbleColors == null || marbleColors.Length == 0)
        {
            marbleColors = new Color[] { Color.red, Color.blue };
        }
        
        // Use modulo to cycle through available colors
        Color marbleColor = (index < marbleColors.Length) ? 
            marbleColors[index] : 
            marbleColors[index % marbleColors.Length];
        
        // Find all renderers attached to the marble
        Renderer[] renderers = marble.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Create a new material instance to avoid affecting the prefab
            Material material = new Material(renderer.material);
            material.color = marbleColor;
            renderer.material = material;
        }
        
        // Add trail renderer with matching color
        TrailRenderer trail = marble.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = marble.AddComponent<TrailRenderer>();
            trail.time = 0.5f; // Trail duration
            trail.startWidth = 0.03f;
            trail.endWidth = 0.01f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        // Set trail color to match marble color
        trail.startColor = marbleColor;
        trail.endColor = new Color(marbleColor.r, marbleColor.g, marbleColor.b, 0f); // Fade to transparent
    }

    /// <summary>
    /// Updates properties of all existing marbles based on current settings.
    /// Called by the debug panel when property values change.
    /// </summary>
    public void UpdateExistingMarbleProperties()
    {
        if (activeMarbles == null)
            return;

        foreach (GameObject marble in activeMarbles)
        {
            if (marble != null)
            {
                // Update size
                marble.transform.localScale = new Vector3(marbleSize, marbleSize, marbleSize);
                
                // Update physics properties
                Rigidbody rb = marble.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.mass = marbleMass;
                }
                
                // Update collider material
                SphereCollider collider = marble.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    PhysicsMaterial physMaterial = new PhysicsMaterial
                    {
                        dynamicFriction = dynamicFriction,
                        staticFriction = staticFriction,
                        bounciness = bounciness,
                        frictionCombine = PhysicsMaterialCombine.Average,
                        bounceCombine = PhysicsMaterialCombine.Average
                    };
                    collider.material = physMaterial;
                }
            }
        }
    }

    /// <summary>
    /// Destroys all existing marbles and spawns new ones.
    /// Called by the debug panel when the respawn button is pressed.
    /// </summary>
    public void RespawnAllMarbles()
    {
        // Clear existing marbles
        foreach (GameObject marble in activeMarbles)
        {
            if (marble != null)
            {
                Destroy(marble);
            }
        }
        
        // Reset the list but keep its size
        for (int i = 0; i < activeMarbles.Count; i++)
        {
            activeMarbles[i] = null;
        }
        
        // Clear touch mappings
        touchToMarbleMap.Clear();
        touchStartPositions.Clear();
        
        // Spawn new marbles
        SpawnInitialMarbles();
    }
}