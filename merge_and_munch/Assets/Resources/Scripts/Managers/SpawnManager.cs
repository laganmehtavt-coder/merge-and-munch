using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Item Prefab")]
    [SerializeField] GameObject itemPrefab;

    [Header("Merge Popup")]
    [SerializeField] GameObject mergePopupTextPrefab;


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

    [Header("Follow Image")]
    [SerializeField] Transform followImage;

    [Header("Click Hide Image")]
    [SerializeField] GameObject clickHideImage;

    // =========================
    // 💣 BOMB SYSTEM
    [Header("Bomb System")]
    [SerializeField] GameObject bombEffect;
    [SerializeField] GameObject bombModeUIHide;
    [SerializeField] GameObject extraHideObject;

    bool isBombMode = false;

    // =========================
    // ⚡ UPGRADE SYSTEM (CLICK ANY ITEM)
    [Header("Upgrade System")]
    [SerializeField] GameObject upgradeEffect;
    [SerializeField] GameObject upgradeUIHide;

    bool isUpgradeMode = false;

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

        // 💣 Bomb Mode
        if (isBombMode) {
            HandleBombClick();
            return;
        }

        // ⚡ Upgrade Mode (CLICK ANY ITEM)
        if (isUpgradeMode) {
            HandleUpgradeClick();
            return;
        }

        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;

        HandleMovement();
        HandleDrop();
    }

    // =========================
    // MOVEMENT
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
            SetClickImage(false);
            DropItem();
        }
    }

    // =========================
    // 💣 BOMB
    void HandleBombClick() {
        if (Input.GetMouseButtonDown(0) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) {

            Item item = GetClickedItem();

            if (item != null) {
                if (bombEffect != null)
                    Instantiate(bombEffect, item.transform.position, Quaternion.identity);

                Destroy(item.gameObject);
            }

            isBombMode = false;
            SetBombUI(true);
        }
    }

    public void ActivateBomb() {
        isBombMode = true;
        SetBombUI(false);
    }

    void SetBombUI(bool state) {
        if (bombModeUIHide != null)
            bombModeUIHide.SetActive(state);

        if (extraHideObject != null)
            extraHideObject.SetActive(state);
    }

    // =========================
    // ⚡ UPGRADE (CLICK ANY ITEM)
    void HandleUpgradeClick() {
        if (Input.GetMouseButtonDown(0) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) {

            Item item = GetClickedItem();

            if (item != null && item.data.nextItem != null) {

                Vector2 pos = item.transform.position;

                // ✨ effect
                if (upgradeEffect != null)
                    Instantiate(upgradeEffect, pos, Quaternion.identity);

                // 🔄 spawn next
                SpawnMergedItem(item.data.nextItem, pos);

                Destroy(item.gameObject);
            }

            isUpgradeMode = false;
            SetUpgradeUI(true);
        }
    }

    public void ActivateUpgrade() {
        isUpgradeMode = true;
        SetUpgradeUI(false);
    }

    void SetUpgradeUI(bool state) {
        if (upgradeUIHide != null)
            upgradeUIHide.SetActive(state);
    }

    // =========================
    // 🎯 COMMON CLICK DETECTION
    Item GetClickedItem() {
        Vector3 pos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : Input.mousePosition;

        pos.z = Mathf.Abs(cam.transform.position.z);

        Vector2 worldPos = cam.ScreenToWorldPoint(pos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
            return hit.collider.GetComponent<Item>();

        return null;
    }

    // =========================
    void SetClickImage(bool state) {
        if (clickHideImage != null)
            clickHideImage.SetActive(state);
    }

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

    // ✅ UPDATED — Merge hone pe score popup spawn hota hai
    public void SpawnMergedItem(ItemData data, Vector2 pos) {
        GameObject obj = Instantiate(itemPrefab, pos, Quaternion.identity);

        Item item = obj.GetComponent<Item>();
        item.Initialize(data, true);
        item.ActivatePhysics();

        // ✅ TEXT POPUP SPAWN
        SpawnMergePopup(pos, data);
    }
    void SpawnMergePopup(Vector2 pos, ItemData data) {
        if (mergePopupTextPrefab == null)
            return;

        GameObject popup = Instantiate(mergePopupTextPrefab, pos, Quaternion.identity);

        // Agar TextMeshPro use kar rahe ho
        TMPro.TextMeshPro text = popup.GetComponent<TMPro.TextMeshPro>();
        if (text != null) {
            text.text = "+" + data.mergeScore; // 👈 apna score variable use karo
        }
    }
}