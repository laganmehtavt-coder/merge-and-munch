using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Item Prefab")]
    [SerializeField] GameObject itemPrefab;

    [Header("Items")]
    [SerializeField] List<ItemData> items = new List<ItemData>();

    [Header("Spawn Settings")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] float nextSpawnDelay = 0.75f;

    [Header("Movement")]
    [SerializeField] float minX = -1.7f;
    [SerializeField] float maxX = 1.7f;

    [Header("Effects")]
    [SerializeField] GameObject clickEffect;

    [Header("UI")]
    [SerializeField] Image nextItemImage;

    [Header("Follow Image (Moves Left/Right)")]
    [SerializeField] Transform followImage; // 👈 follows item

    [Header("Click Hide Image")]
    [SerializeField] GameObject clickHideImage; // 👈 hide/show

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
        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;

        HandleMovement();
        HandleDrop();
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

        // 👉 Move item
        currentItem.transform.position = new Vector2(xPos, spawnPoint.position.y);

        // 👉 Move follow image with item
        if (followImage != null) {
            Vector3 pos = followImage.position;
            pos.x = xPos;
            followImage.position = pos;
        }
    }

    void HandleDrop() {
        if (Input.GetMouseButtonUp(0) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) {

            SpawnClickEffect();

            // 👉 hide click image
            SetClickImage(false);

            DropItem();
        }
    }

    // =========================
    void SetClickImage(bool state) {
        if (clickHideImage != null)
            clickHideImage.SetActive(state);
    }

    // =========================
    void GenerateInitialItems() {
        upcomingItems.Clear();

        upcomingItems.Enqueue(GetRandomItem());
        upcomingItems.Enqueue(GetRandomItem());

        SpawnLatestItem();
        UpdateNextItemUI();
    }

    void SpawnLatestItem() {
        ItemData data = upcomingItems.Dequeue();

        GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        currentItem = obj.GetComponent<Item>();
        currentItem.Initialize(data, false);

        upcomingItems.Enqueue(GetRandomItem());

        UpdateNextItemUI();

        // 👉 show click image again
        SetClickImage(true);
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
    void UpdateNextItemUI() {
        if (nextItemImage == null || upcomingItems.Count == 0)
            return;

        ItemData next = upcomingItems.Peek();

        if (next != null && next.sprite != null) {
            nextItemImage.sprite = next.sprite;
            nextItemImage.enabled = true;
        }
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