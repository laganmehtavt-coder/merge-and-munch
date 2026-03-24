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

    [Header("Follow Image")]
    [SerializeField] Transform followImage;

    [Header("Click Hide Image")]
    [SerializeField] GameObject clickHideImage;

    [Header("Spawn UI")]
    [SerializeField] GameObject spawnUIObject;
    [SerializeField] Image spawnUIImage;

    // 💣 Bomb
    [Header("Bomb System")]
    [SerializeField] GameObject bombEffect;
    [SerializeField] GameObject bombModeUIHide;
    [SerializeField] GameObject extraHideObject;
    bool isBombMode = false;

    // ⚡ Upgrade
    [Header("Upgrade System")]
    [SerializeField] GameObject upgradeEffect;
    [SerializeField] GameObject upgradeUIHide;
    bool isUpgradeMode = false;

    // 🔥 FIX VARIABLES
    bool isSpecialModeActive = false;
    float inputBlockTimer = 0f;

    // 🎥 SHAKE HOLDER
    [Header("Camera Shake")]
    [SerializeField] Transform shakeHolder;
    [SerializeField] float shakeDuration = 0.1f;
    [SerializeField] float shakeAmountX = 0.01f;
    [SerializeField] float shakeAmountY = 0.01f;
    [SerializeField] float shakeSpeed = 25f;

    bool isShaking = false;
    Vector3 shakeDefaultPos;

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

        if (cam == null) {
            Debug.LogError("❌ Camera not found!");
            return;
        }

        if (shakeHolder != null)
            shakeDefaultPos = shakeHolder.localPosition;
    }

    void Start() {
        GenerateInitialItems();
    }

    void Update() {

        if (cam == null)
            return;

        // 🔥 SAME CLICK BLOCK FIX
        if (inputBlockTimer > 0f) {
            inputBlockTimer -= Time.deltaTime;
            return;
        }

        if (isBombMode) {
            HandleBombClick();
            return;
        }

        if (isUpgradeMode) {
            HandleUpgradeClick();
            return;
        }

        if (isSpecialModeActive)
            return;

        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;

        HandleMovement();
        HandleDrop();
    }

    // ================= MOVEMENT =================
    void HandleMovement() {

        Vector3 pointerPos = Input.touchCount > 0
            ? Input.GetTouch(0).position
            : Input.mousePosition;

        pointerPos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 worldPos = cam.ScreenToWorldPoint(pointerPos);

        if (float.IsNaN(worldPos.x) || float.IsInfinity(worldPos.x))
            return;

        float xPos = Mathf.Clamp(worldPos.x, minX, maxX);

        currentItem.transform.position = new Vector2(xPos, spawnPoint.position.y);

        if (followImage != null) {
            Vector3 pos = followImage.position;
            pos.x = xPos;
            followImage.position = pos;
        }
    }

    // ================= DROP =================
    void HandleDrop() {

        if (Input.GetMouseButtonUp(0) ||
           (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)) {

            SpawnClickEffect();
            SetClickImage(false);

            if (spawnUIObject != null)
                spawnUIObject.SetActive(false);

            if (spawnPoint != null)
                spawnPoint.gameObject.SetActive(false);

            DropItem();
        }
    }

    // ================= BOMB =================
    void HandleBombClick() {

        if (Input.GetMouseButtonDown(0)) {

            Item item = GetClickedItem();

            if (item != null) {

                if (bombEffect != null)
                    Instantiate(bombEffect, item.transform.position, Quaternion.identity);

                Destroy(item.gameObject);
                StartShake(0.2f);

                inputBlockTimer = 0.2f; // 🔥 FIX
            }

            isBombMode = false;
            isSpecialModeActive = false;

            SetBombUI(true);
        }
    }

    public void ActivateBomb() {
        isBombMode = true;
        isSpecialModeActive = true;

        if (spawnUIObject != null)
            spawnUIObject.SetActive(false);

        SetBombUI(false);
    }

    void SetBombUI(bool state) {

        if (bombModeUIHide != null)
            bombModeUIHide.SetActive(state);

        if (extraHideObject != null)
            extraHideObject.SetActive(state);

        if (state && spawnUIObject != null)
            spawnUIObject.SetActive(true);
    }

    // ================= UPGRADE =================
    void HandleUpgradeClick() {

        if (Input.GetMouseButtonDown(0)) {

            Item item = GetClickedItem();

            if (item != null && item.data != null && item.data.nextItem != null) {

                Vector2 pos = item.transform.position;

                if (upgradeEffect != null)
                    Instantiate(upgradeEffect, pos, Quaternion.identity);

                SpawnMergedItem(item.data.nextItem, pos);

                Destroy(item.gameObject);
                StartShake(0.2f);

                inputBlockTimer = 0.2f; // 🔥 FIX
            }

            isUpgradeMode = false;
            isSpecialModeActive = false;

            SetUpgradeUI(true);
        }
    }

    public void ActivateUpgrade() {
        isUpgradeMode = true;
        isSpecialModeActive = true;

        if (spawnUIObject != null)
            spawnUIObject.SetActive(false);

        SetUpgradeUI(false);
    }

    void SetUpgradeUI(bool state) {

        if (upgradeUIHide != null)
            upgradeUIHide.SetActive(state);

        if (state && spawnUIObject != null)
            spawnUIObject.SetActive(true);
    }

    // ================= CLICK =================
    Item GetClickedItem() {

        if (cam == null)
            return null;

        Vector2 worldPos;

        if (Input.touchCount > 0)
            worldPos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        else
            worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D col = Physics2D.OverlapPoint(worldPos);

        if (col != null)
            return col.GetComponent<Item>();

        return null;
    }

    // ================= SPAWN =================
    void GenerateInitialItems() {

        if (items.Count == 0) {
            Debug.LogError("❌ Items list empty!");
            return;
        }

        upcomingItems.Clear();
        upcomingItems.Enqueue(GetRandomItem());
        upcomingItems.Enqueue(GetRandomItem());

        SpawnLatestItem();
    }

    void SpawnLatestItem() {

        if (spawnPoint == null || itemPrefab == null)
            return;

        spawnPoint.gameObject.SetActive(true);

        ItemData data = upcomingItems.Dequeue();

        GameObject obj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);

        currentItem = obj.GetComponent<Item>();

        if (currentItem != null)
            currentItem.Initialize(data, false);

        if (spawnUIObject != null)
            spawnUIObject.SetActive(true);

        if (spawnUIImage != null && data != null)
            spawnUIImage.sprite = data.sprite;

        upcomingItems.Enqueue(GetRandomItem());
        SetClickImage(true);
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

        if (currentItem.data != null && currentItem.data.dropSound != null)
            SoundManager.Instance?.PlaySound(currentItem.data.dropSound);

        StartCoroutine(SpawnNextItem());
    }

    void SetClickImage(bool state) {
        if (clickHideImage != null)
            clickHideImage.SetActive(state);
    }

    void SpawnClickEffect() {

        if (clickEffect == null || cam == null)
            return;

        Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        Instantiate(clickEffect, pos, Quaternion.identity);
    }

    ItemData GetRandomItem() {
        return items[Random.Range(0, items.Count)];
    }

    // ================= MERGE =================
    public void SpawnMergedItem(ItemData data, Vector2 pos) {

        if (itemPrefab == null)
            return;

        GameObject obj = Instantiate(itemPrefab, pos, Quaternion.identity);

        Item item = obj.GetComponent<Item>();

        if (item != null) {
            item.Initialize(data, true);
            item.ActivatePhysics();
        }

        GameManager.Instance?.AddScore(data.mergeScore);

        if (mergePopupTextPrefab != null) {
            GameObject popup = Instantiate(mergePopupTextPrefab, pos, Quaternion.identity);
            popup.GetComponent<FloatingText>()?.SetScore(data.mergeScore);
        }

        if (data.mergeEffect != null)
            Instantiate(data.mergeEffect, pos, Quaternion.identity);

        StartShake(0.25f);
    }

    // ================= SHAKE =================
    public void StartShake(float durationOverride = -1f) {
        if (!isShaking)
            StartCoroutine(CameraShake(durationOverride));
    }

    IEnumerator CameraShake(float durationOverride) {

        if (shakeHolder == null)
            yield break;

        isShaking = true;

        float duration = durationOverride > 0 ? durationOverride : shakeDuration;
        float elapsed = 0f;

        while (elapsed < duration) {

            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmountX;
            float y = Mathf.Cos(Time.time * shakeSpeed) * shakeAmountY;

            shakeHolder.localPosition = shakeDefaultPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeHolder.localPosition = shakeDefaultPos;
        isShaking = false;
    }
}