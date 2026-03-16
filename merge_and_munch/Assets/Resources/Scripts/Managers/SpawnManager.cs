using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Item Prefab")]
    [SerializeField] GameObject itemPrefab;

    [Header("Items (Scriptable Objects)")]
    [SerializeField] List<ItemData> items = new List<ItemData>();


    public enum Difficulty {
        Easy,
        Medium,
        Hard
    }

    [Header("Spawn Difficulty")]
    [SerializeField] Difficulty difficulty = Difficulty.Easy;


    [Header("Spawn Settings")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform spawnPointImage; 
    [SerializeField] int upcomingSize = 3;
    [SerializeField] float nextSpawnDelay = 0.75f;

    [Header("Movement Limits")]
    [SerializeField] float minX = -2.45f;
    [SerializeField] float maxX = 2.45f;

    Queue<ItemData> upcomingItems = new Queue<ItemData>();

    Item currentItem;

    bool canTakeInput = true;

    Camera cam;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cam = Camera.main;
    }

    void Start() {
        GenerateInitialItems();
    }

    void Update() {
        if (currentItem == null || !canTakeInput)
            return;

        if (currentItem.isDropped)
            return;

        HandleMovement();
        HandleDrop();
    }

    void HandleMovement() {
        if (currentItem == null)
            return;

        Vector3 pointerPos;

        // Get pointer: touch or mouse
        if (Input.touchCount > 0) {
            pointerPos = Input.GetTouch(0).position;
        } else {
            pointerPos = Input.mousePosition;
        }

        // Clamp pointer inside screen (optional)
        pointerPos.x = Mathf.Clamp(pointerPos.x, 0f, Screen.width);
        pointerPos.y = Mathf.Clamp(pointerPos.y, 0f, Screen.height);

        // Z distance from camera to spawn point
        pointerPos.z = Mathf.Abs(cam.transform.position.z - spawnPoint.position.z);

        // Convert to world position
        Vector3 worldPos = cam.ScreenToWorldPoint(pointerPos);

        if (float.IsNaN(worldPos.x) || float.IsNaN(worldPos.y)) {
            Debug.LogWarning("ScreenToWorldPoint returned NaN!");
            return;
        }

        // Clamp horizontal movement to -1.31 .. 1.31
        float xPos = Mathf.Clamp(worldPos.x, -1.31f, 1.31f);

        // Move the item horizontally (Y stays fixed)
        currentItem.transform.position = new Vector2(
            xPos,
            spawnPoint.position.y
        );

        // Move spawn point image if exists
        if (spawnPointImage != null) {
            Vector3 imgPos = spawnPointImage.position;
            imgPos.x = xPos;
            spawnPointImage.position = imgPos;
        }
    }
    void HandleDrop() {
        if (Input.GetMouseButtonUp(0)) {
            DropItem();
        }
    }

    void GenerateInitialItems() {
        upcomingItems.Clear();

        for (int i = 0; i < upcomingSize; i++) {
            upcomingItems.Enqueue(GetRandomItem());
        }

        SpawnLatestItem();
    }

    void SpawnLatestItem() {
        if (itemPrefab == null || items.Count == 0) {
            Debug.LogError("SpawnManager Setup Missing!");
            return;
        }

        ItemData data = upcomingItems.Dequeue();

        GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);

        currentItem = obj.GetComponent<Item>();

        currentItem.Initialize(data, false);

        upcomingItems.Enqueue(GetRandomItem());
    }

    IEnumerator SpawnNextItem() {
        yield return new WaitForSeconds(nextSpawnDelay);

        canTakeInput = true;

        SpawnLatestItem();
    }

    void DropItem() {
        if (currentItem == null)
            return;

        canTakeInput = false;

        currentItem.isDropped = true;

        currentItem.ActivatePhysics();

        if (currentItem.data.dropSound != null) {
            SoundManager.Instance.PlaySound(currentItem.data.dropSound);
        }

        StartCoroutine(SpawnNextItem());
    }

    //  MAIN RANDOM SELECTOR
    ItemData GetRandomItem() {
        switch (difficulty) {
            case Difficulty.Easy:
                return GetEasyItem();

            case Difficulty.Medium:
                return GetMediumItem();

            case Difficulty.Hard:
                return GetHardItem();
        }

        return items[0];
    }

    //  EASY ALGORITHM
    ItemData GetEasyItem() {
        int random = Random.Range(0, items.Count);
        return items[random];
    }

    //  MEDIUM ALGORITHM
    ItemData GetMediumItem() {
        int random = Random.Range(0, items.Count);
        return items[random];
    }

    //  HARD ALGORITHM
    ItemData GetHardItem() {
        int random = Random.Range(0, items.Count);
        return items[random];
    }

    public void SpawnMergedItem(ItemData data, Vector2 position) {
        GameObject obj = Instantiate(itemPrefab, position, Quaternion.identity);

        Item item = obj.GetComponent<Item>();

        item.Initialize(data, true);

        item.ActivatePhysics();
    }
}