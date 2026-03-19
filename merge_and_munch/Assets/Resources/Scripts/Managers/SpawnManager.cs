using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;
    HashSet<ItemData> spawnedOnce = new HashSet<ItemData>();
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

    // 🎯 FOLLOW MOUSE / TOUCH
    void HandleMovement() {
        Vector3 pointerPos;

        if (Input.touchCount > 0)
            pointerPos = Input.GetTouch(0).position;
        else
            pointerPos = Input.mousePosition;

        // Safety check
        if (float.IsNaN(pointerPos.x) || float.IsNaN(pointerPos.y))
            return;

        pointerPos.x = Mathf.Clamp(pointerPos.x, 0.1f, Screen.width - 0.1f);
        pointerPos.y = Mathf.Clamp(pointerPos.y, 0.1f, Screen.height - 0.1f);

        pointerPos.z = Mathf.Abs(cam.transform.position.z - spawnPoint.position.z);

        Vector3 worldPos = cam.ScreenToWorldPoint(pointerPos);

        if (float.IsNaN(worldPos.x))
            return;

        float xPos = Mathf.Clamp(worldPos.x, -1.31f, 1.31f);

        currentItem.transform.position = new Vector2(xPos, spawnPoint.position.y);

        if (spawnPointImage != null) {
            Vector3 imgPos = spawnPointImage.position;
            imgPos.x = xPos;
            spawnPointImage.position = imgPos;
        }
    }

    void HandleDrop() {
        if (Input.GetMouseButtonUp(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            DropItem();
        }
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

        // ✅ FIRST TIME SPAWN CHECK
        if (!spawnedOnce.Contains(data)) {
            spawnedOnce.Add(data);

            // UI update
            if (ItemUIManager.Instance != null) {
                ItemUIManager.Instance.OnItemSpawn(data);
            }
        }

        upcomingItems.Enqueue(GetRandomItem());
    }

    void DropItem() {
        if (currentItem == null)
            return;

        canTakeInput = false;

        currentItem.ActivatePhysics();

        if (currentItem.data.dropSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(currentItem.data.dropSound);

        StartCoroutine(SpawnNextItem());
    }

    IEnumerator SpawnNextItem() {
        yield return new WaitForSeconds(nextSpawnDelay);

        canTakeInput = true;
        SpawnLatestItem();
    }

    // 🔥 MERGE SPAWN (FIXED)
    // 🔥 MERGE SPAWN (NO SCALE EFFECT)
    public void SpawnMergedItem(ItemData data, Vector2 position) {
        if (itemPrefab == null || data == null)
            return;

        GameObject obj = Instantiate(itemPrefab, position, Quaternion.identity);
        Item newItem = obj.GetComponent<Item>();

        if (newItem != null) {
            obj.transform.localScale = Vector3.one * data.size;

            newItem.Initialize(data, true);

            // ✅ PHYSICS FIX
            StartCoroutine(FixSpawnPhysics(newItem));

            // 🔥 UI CALL (IMPORTANT)
            if (ItemUIManager.Instance != null) {
                ItemUIManager.Instance.OnItemSpawn(data);
            }
        }
    }

    // 🔧 FIX PHYSICS AFTER SPAWN
    IEnumerator FixSpawnPhysics(Item item) {
        yield return new WaitForSeconds(0.05f);

        Rigidbody2D[] rbs = item.GetComponentsInChildren<Rigidbody2D>();

        foreach (var rb in rbs) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // ✨ POP ANIMATION
    IEnumerator PopEffect(GameObject obj, float targetSize) {
        float t = 0;

        while (t < 0.15f) {
            if (obj == null)
                yield break;

            obj.transform.localScale = Vector3.Lerp(
                Vector3.zero,
                Vector3.one * (targetSize * 1.2f),
                t / 0.15f
            );

            t += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
            obj.transform.localScale = Vector3.one * targetSize;
    }

    // 🎲 RANDOM ITEM BASED ON DIFFICULTY
    ItemData GetRandomItem() {
        int maxIndex = (difficulty == Difficulty.Easy) ? 3 :
                       (difficulty == Difficulty.Medium ? 5 : items.Count);

        return items[Random.Range(0, Mathf.Min(maxIndex, items.Count))];
    }
}