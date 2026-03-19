using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemUIManager : MonoBehaviour {
    public static ItemUIManager Instance;

    [System.Serializable]
    public class ItemUI {
        public ItemData data;
        public Image image;
        public bool isActivated;
    }

    public List<ItemUI> itemUIList = new List<ItemUI>();

    Coroutine currentFlashRoutine;
    Image currentFlashingImage;

    void Awake() {
        Instance = this;

        // Default gray
        foreach (var item in itemUIList) {
            if (item.image != null) {
                item.image.color = Color.gray;
                item.image.transform.localScale = Vector3.one;
            }
        }
    }

    public void OnItemSpawn(ItemData data) {
        ItemUI target = null;

        foreach (var item in itemUIList) {
            if (item.data == data) {
                target = item;
                break;
            }
        }

        if (target == null || target.image == null)
            return;

        // ✅ ACTIVATE (only once)
        if (!target.isActivated) {
            target.isActivated = true;
            target.image.color = Color.white;
        }

        // ✅ STOP OLD FLASH CLEANLY
        if (currentFlashRoutine != null) {
            StopCoroutine(currentFlashRoutine);

            if (currentFlashingImage != null) {
                currentFlashingImage.transform.localScale = Vector3.one;
                currentFlashingImage.color = Color.white;
            }
        }

        // ✅ START NEW FLASH
        currentFlashingImage = target.image;
        currentFlashRoutine = StartCoroutine(FlashPopEffect(target.image));
    }

    IEnumerator FlashPopEffect(Image img) {
        if (img == null)
            yield break;

        float time = 0f;

        // 🔥 PHASE 1: QUICK POP (Overshoot)
        float growDuration = 0.12f;
        while (time < growDuration) {
            if (img == null)
                yield break;

            float t = time / growDuration;

            // Smooth overshoot
            float scale = Mathf.Lerp(1f, 1.25f, EaseOutBack(t));
            img.transform.localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }

        // 🔥 PHASE 2: SETTLE BACK
        time = 0f;
        float settleDuration = 0.1f;

        while (time < settleDuration) {
            if (img == null)
                yield break;

            float t = time / settleDuration;

            float scale = Mathf.Lerp(1.25f, 1f, t);
            img.transform.localScale = Vector3.one * scale;

            time += Time.deltaTime;
            yield return null;
        }

        // FINAL RESET (SAFE)
        if (img != null)
            img.transform.localScale = Vector3.one;
    }

    // 🔥 Smooth pop curve
    float EaseOutBack(float t) {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}