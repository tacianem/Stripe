using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ObjectSpawner : MonoBehaviour {

    [Header("Object Settings")]
    public GameObject canePrefab;
    public TextMeshProUGUI gameOverText;

    float minSize = 0.2f; // Minimum object scale
    float maxSize = 1.6f; // Maximum object scale
    int minRotation = -180; // Minimum rotation angle
    int maxRotation = 220; // Maximum rotation angle

    [Header("Spawn Timing")]
    float spawnInterval = 1f; // Time between spawns
    float objectFallSpeed = 1.2f; // Speed at which objects move down

    [Header("Object Limit")]
    int maxObjects = 18; // Maximum objects allowed on screen

    List<GameObject> candyCanes = new List<GameObject>();

    bool gameOn = true;
    //bool change = false;

    Camera mainCamera;
    Vector2 screenBottomLeft;
    Vector2 screenBottomRight;
    Vector2 screenTopLeft;
    Vector2 screenTopRight;

    AudioClip magic_wand;
    AudioSource audioSource;

    void Start() {
        mainCamera = Camera.main;

         // Calculate screen bounds in world space
        screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        screenBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.nearClipPlane));
        screenTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        screenTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        magic_wand = Resources.Load<AudioClip>("Sounds/magic_wand");
        audioSource = gameObject.AddComponent<AudioSource>();

        // Start the spawning coroutine
        StartCoroutine(SpawnObjectsOverTime());
    }

    void Update() {
        // Move objects downward over time
        for (int i = candyCanes.Count - 1; i >= 0; i--) {
            GameObject cane = candyCanes[i];
            if (cane != null && gameOn) {
                // Move the object straight down
                cane.transform.position += Vector3.down * objectFallSpeed * Time.deltaTime;

                // Destroy objects that move off-screen
                if (cane.transform.position.y < screenBottomLeft.y - 1f) {
                    if(cane.GetComponent<Cane>().striped) {
                        Destroy(cane);
                        candyCanes.RemoveAt(i);
                    }
                    else { //Game Over!
                        gameOverText.gameObject.SetActive(true);
                        gameOn = false;
                    }
                }
            }
        }

        // Check for touch input
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (!gameOn) {
                gameOn = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                // Convert screen position to world position
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touch.position);
                worldPosition.z = 0f; // Ensure we ignore the Z-axis

                // Detect objects near the touch position
                DetectObjs(worldPosition);
            }
        }
    }

    void DetectObjs (Vector3 touchPosition) {
        foreach (GameObject obj in candyCanes) {
            float distance = Vector3.Distance(obj.transform.position, touchPosition);

            if (distance < 0.7f) { // Adjust this threshold to match your game's scale
                Debug.Log("TOUCHED!");
                
                Cane touchedCane = obj.GetComponent<Cane>();
                if(!touchedCane.striped)
                    audioSource.PlayOneShot(magic_wand);
                    touchedCane.ChangeSprite();
            }
        }
    }

    System.Collections.IEnumerator SpawnObjectsOverTime() {
        while (gameOn) {
            if (candyCanes.Count < maxObjects) {
                SpawnObject();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject() {
        // float middleScreen = (screenBottomRight.x - screenBottomLeft.x)/2;
        // float x;

        // if (change) {
        //     x = Random.Range(screenBottomLeft.x, middleScreen);
        // } else {
        //     x = Random.Range(middleScreen, screenBottomRight.x);
        // }
        // change = !change;

        // Generate random position, size, and rotation
        Vector2 randomPosition = new Vector2(
            Random.Range(screenBottomLeft.x, screenBottomRight.x),
            Random.Range(screenTopLeft.y, screenTopLeft.y - ((screenTopLeft.y - screenBottomLeft.y)/4)) // Spawn at the top quarter of the screen
        );

        float randomSize = Random.Range(minSize, maxSize);
        float randomRotation = Random.Range(minRotation, maxRotation);
        // Debug.Log(randomRotation); // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< console message

        // Instantiate the object
        GameObject newCandyCane = Instantiate(canePrefab, randomPosition, Quaternion.Euler(0, 0, randomRotation));
        newCandyCane.SetActive(true);
        newCandyCane.transform.localScale = Vector3.one * randomSize;

        // Add the object to the list
        candyCanes.Add(newCandyCane);
    }

}