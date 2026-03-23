using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Item Prefab")]
    [SerializeField] GameObject itemPrefab;

    [Header("Items")]
    [SerializeField] List<ItemData> items = new List<ItemData>();

    public enum Difficulty { Easy, Medium, Hard }

    [Header("Difficulty")]
    [SerializeField] Difficulty difficulty = Difficulty.Easy;

    [Header("Spawn Settings")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] Transform spawnPointImage;
    [SerializeField] int upcomingSize = 3;
    [SerializeField] float nextSpawnDelay = 0.75f;

    [Header("Movement")]
    [SerializeField] float minX = -1.7f;
    [SerializeField] float maxX = 1.7f;

    [Header("Effects")]
    [SerializeField] GameObject clickEffect;

    [Header("UI / Extra Image")]
    [SerializeField] GameObject clickHideImage;

    [Header("Idle Hint Settings")]
    [SerializeField] float idleDelay = 1.5f;     // 👈 wait time
    [SerializeField] float shakeAmount = 0.3f;   // 👈 left-right distance
    [SerializeField] float shakeSpeed = 3f;      // 👈 speed

    Queue<ItemData> upcomingItems = new Queue<ItemData>();
    Item currentItem;

    bool canTakeInput = true;
    Camera cam;

    float idleTimer = 0f;
    bool isShaking = false;

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
        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;

        HandleMovement();
        HandleDrop();
        HandleIdleHint();
    }

    // =========================
    void HandleMovement() {
        Vector3 pointerPos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : Input.mousePosition;

        pointerPos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 worldPos = cam.ScreenToWorldPoint(pointerPos);

        if (float.IsNaN(worldPos.x))
            return;

        float xPos = Mathf.Clamp(worldPos.x, minX, maxX);

        currentItem.transform.position = new Vector2(xPos, spawnPoint.position.y);

        if (spawnPointImage != null) {
            Vector3 imgPos = spawnPointImage.position;
            imgPos.x = xPos;
            spawnPointImage.position = imgPos;
        }

        // 👇 Reset idle if player moves
        idleTimer = 0f;
        isShaking = false;
    }

    void HandleDrop() {
        if (Input.GetMouseButtonUp(0) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) {

            SpawnClickEffect();
            SetClickImage(false);

            DropItem();
        }
    }

    // =========================
    // 🧠 IDLE HINT SYSTEM
    void HandleIdleHint() {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDelay) {
            isShaking = true;
        }

        if (isShaking && currentItem != null) {
            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;

            currentItem.transform.position = new Vector2(
                spawnPoint.position.x + shakeX,
                spawnPoint.position.y
            );
        }
    }

    // =========================
    void SetClickImage(bool state) {
        if (clickHideImage != null)
            clickHideImage.SetActive(state);
    }

    void GenerateInitialItems() {
        upcomingItems.Clear();

        for (int i = 0; i < upcomingSize; i++)
            upcomingItems.Enqueue(GetRandomItem());

        SpawnLatestItem();
    }

    void SpawnLatestItem() {
        ItemData data = upcomingItems.Dequeue();

        GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        currentItem = obj.GetComponent<Item>();
        currentItem.Initialize(data, false);

        upcomingItems.Enqueue(GetRandomItem());

        SetClickImage(true);

        // 👇 reset idle system
        idleTimer = 0f;
        isShaking = false;
    }

    IEnumerator SpawnNextItem() {
        yield return new WaitForSeconds(nextSpawnDelay);
        canTakeInput = true;
        SpawnLatestItem();
    }

    void DropItem() {
        canTakeInput = false;

        currentItem.isDropped = true;
        currentItem.ActivatePhysics();

        if (currentItem.data.dropSound != null) {
            SoundManager.Instance.PlaySound(currentItem.data.dropSound);
        }

        StartCoroutine(SpawnNextItem());
    }

    // =========================
    void SpawnClickEffect() {
        if (clickEffect == null)
            return;

        Vector3 pos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : Input.mousePosition;

        pos.z = Mathf.Abs(cam.transform.position.z);

        Instantiate(clickEffect, cam.ScreenToWorldPoint(pos), Quaternion.identity);
    }

    public void SpawnMergeEffect(GameObject effect, Vector2 pos) {
        if (effect != null)
            Instantiate(effect, pos, Quaternion.identity);
    }

    // =========================
    ItemData GetRandomItem() {
        return items[Random.Range(0, items.Count)];
    }

    public void SpawnMergedItem(ItemData data, Vector2 pos) {
        GameObject obj = Instantiate(itemPrefab, pos, Quaternion.identity);

        Item item = obj.GetComponent<Item>();
        item.Initialize(data, true);
        item.ActivatePhysics();
    }
}