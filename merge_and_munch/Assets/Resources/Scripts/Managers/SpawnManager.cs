using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    public static SpawnManager Instance;

    [Header("Settings")]
    [SerializeField] List<ItemData> items = new List<ItemData>();
    [SerializeField] Transform spawnPoint;
    [SerializeField] float nextSpawnDelay = 0.75f; // ✅ Aapki demand ke mutabiq 0.75s set kiya hai

    [Header("Effects")]
    [SerializeField] GameObject clickEffectPrefab;

    Queue<ItemData> upcomingItems = new Queue<ItemData>();
    Item currentItem;
    bool canTakeInput = true;
    Camera cam;

    void Awake() { Instance = this; cam = Camera.main; }

    void Start() {
        if (items.Count < 3) {
            Debug.LogError("Kam se kam 3 Items list mein add karein!");
            return;
        }
        for (int i = 0; i < 3; i++)
            upcomingItems.Enqueue(items[Random.Range(0, Mathf.Min(3, items.Count))]);

        SpawnLatestItem();
    }

    void Update() {
        // ✅ Agar canTakeInput false hai, toh movement aur drop block rahega
        if (currentItem == null || !canTakeInput || currentItem.isDropped)
            return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z - spawnPoint.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        // Horizontal movement logic
        currentItem.transform.position = new Vector2(Mathf.Clamp(worldPos.x, -1.3f, 1.3f), spawnPoint.position.y);

        if (Input.GetMouseButtonUp(0)) {
            DropCurrentItem(worldPos);
        }
    }

    void DropCurrentItem(Vector3 clickPos) {
        canTakeInput = false; // ✅ Turant input band taaki spam na ho

        // Click Effect spawn karein
        if (clickEffectPrefab != null) {
            Instantiate(clickEffectPrefab, clickPos, Quaternion.identity);
        }

        currentItem.ActivatePhysics();

        // 0.75 sec ka delay aur phir naya spawn
        StartCoroutine(WaitAndSpawn());
    }

    void SpawnLatestItem() {
        if (upcomingItems.Count == 0)
            return;

        ItemData data = upcomingItems.Dequeue();
        GameObject obj = Instantiate(data.itemPrefab, spawnPoint.position, Quaternion.identity);
        currentItem = obj.GetComponent<Item>();
        currentItem.Initialize(data, false);

        upcomingItems.Enqueue(items[Random.Range(0, Mathf.Min(3, items.Count))]);
    }

    IEnumerator WaitAndSpawn() {
        yield return new WaitForSeconds(nextSpawnDelay); // ✅ 0.75 Seconds Wait
        canTakeInput = true; // ✅ Delay ke baad input wapas on
        SpawnLatestItem();
    }

    public void SpawnMergedItem(ItemData nextData, Vector2 pos) {
        GameObject obj = Instantiate(nextData.itemPrefab, pos, Quaternion.identity);
        Item item = obj.GetComponent<Item>();
        item.Initialize(nextData, true);
    }
}