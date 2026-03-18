using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Item Prefab")]
    [SerializeField] GameObject itemPrefab;

    [Header("Items (Scriptable Objects)")]
    [SerializeField] List<ItemData> items = new List<ItemData>();

    public enum Difficulty { Easy, Medium, Hard }
    [SerializeField] Difficulty difficulty = Difficulty.Easy;

    [Header("Spawn Settings")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform spawnPointImage;
    [SerializeField] float nextSpawnDelay = 0.75f;

    Queue<ItemData> upcomingItems = new Queue<ItemData>();
    Item currentItem;
    bool canTakeInput = true;
    Camera cam;

    void Awake() {
        Instance = this;
        cam = Camera.main;
    }

    void Start() {
        GenerateInitialItems();
    }

    void Update() {
        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;
        HandleMovement();
        HandleDrop();
    }

    void HandleMovement() {
        if (currentItem == null)
            return;

        Vector3 pointerPos;

        // 1. Get Input
        if (Input.touchCount > 0) {
            pointerPos = Input.GetTouch(0).position;
        } else {
            pointerPos = Input.mousePosition;
        }

        // --- SAFETY CHECK START ---
        // 2. Check for NaN or Infinity before processing
        if (float.IsNaN(pointerPos.x) || float.IsInfinity(pointerPos.x) ||
            float.IsNaN(pointerPos.y) || float.IsInfinity(pointerPos.y)) {
            return;
        }

        // 3. Clamp pointer inside screen bounds to avoid frustum errors
        // (Camera rect 0 0 1536 2048 ke mutabiq clamp karein)
        pointerPos.x = Mathf.Clamp(pointerPos.x, 0.1f, Screen.width - 0.1f);
        pointerPos.y = Mathf.Clamp(pointerPos.y, 0.1f, Screen.height - 0.1f);
        // --- SAFETY CHECK END ---

        // Z distance from camera to spawn point
        pointerPos.z = Mathf.Abs(cam.transform.position.z - spawnPoint.position.z);

        // Convert to world position
        Vector3 worldPos = cam.ScreenToWorldPoint(pointerPos);

        // Final check for worldPos
        if (float.IsNaN(worldPos.x))
            return;

        // Clamp horizontal movement
        float xPos = Mathf.Clamp(worldPos.x, -1.31f, 1.31f);

        // Update positions
        currentItem.transform.position = new Vector2(xPos, spawnPoint.position.y);

        if (spawnPointImage != null) {
            Vector3 imgPos = spawnPointImage.position;
            imgPos.x = xPos;
            spawnPointImage.position = imgPos;
        }
    }

    void HandleDrop() {
        if (Input.GetMouseButtonUp(0))
            DropItem();
    }

    void GenerateInitialItems() {
        upcomingItems.Clear();
        for (int i = 0; i < 3; i++)
            upcomingItems.Enqueue(GetRandomItem());
        SpawnLatestItem();
    }

    void SpawnLatestItem() {
        if (itemPrefab == null || items.Count == 0)
            return;
        ItemData data = upcomingItems.Dequeue();
        GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        currentItem = obj.GetComponent<Item>();
        currentItem.Initialize(data, false);
        upcomingItems.Enqueue(GetRandomItem());
    }

    void DropItem() {
        if (currentItem == null)
            return;

        canTakeInput = false;

        // Item ki gravity activate karein
        currentItem.ActivatePhysics();

        if (currentItem.data.dropSound != null)
            SoundManager.Instance.PlaySound(currentItem.data.dropSound);

        StartCoroutine(SpawnNextItem());
    }

    IEnumerator SpawnNextItem() {
        yield return new WaitForSeconds(nextSpawnDelay);
        canTakeInput = true;
        SpawnLatestItem();
    }

    public void SpawnMergedItem(ItemData data, Vector2 position) {
        if (itemPrefab == null || data == null)
            return;

        // Instantiate at the center of the two colliding items
        GameObject obj = Instantiate(itemPrefab, position, Quaternion.identity);
        Item newItem = obj.GetComponent<Item>();

        if (newItem != null) {
            // dropped = true taaki naya item turant gravity ke saath niche gire
            newItem.Initialize(data, true);

            // Visual Pop Juice
            obj.transform.localScale = Vector3.zero;
            StartCoroutine(PopEffect(obj, data.size));
        }
    }

    IEnumerator PopEffect(GameObject obj, float targetSize) {
        float t = 0;
        while (t < 0.15f) {
            if (obj == null)
                yield break;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * (targetSize * 1.2f), t / 0.15f);
            t += Time.deltaTime;
            yield return null;
        }
        if (obj != null)
            obj.transform.localScale = Vector3.one * targetSize;
    }
    IEnumerator AnimatePop(GameObject obj, float targetSize) {
        float time = 0;
        while (time < 0.2f) {
            if (obj == null)
                yield break;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * targetSize, time / 0.2f);
            time += Time.deltaTime;
            yield return null;
        }
        if (obj != null)
            obj.transform.localScale = Vector3.one * targetSize;
    }

    ItemData GetRandomItem() {
        int maxIndex = (difficulty == Difficulty.Easy) ? 3 : (difficulty == Difficulty.Medium ? 5 : items.Count);
        return items[Random.Range(0, Mathf.Min(maxIndex, items.Count))];
    }
}