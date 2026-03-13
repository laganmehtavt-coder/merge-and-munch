using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaHandler : MonoBehaviour {
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;
    private Vector2Int lastResolution = Vector2Int.zero;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update() {
        if (Screen.safeArea != lastSafeArea ||
            Screen.orientation != lastOrientation ||
            new Vector2Int(Screen.width, Screen.height) != lastResolution) {
            ApplySafeArea();
        }
    }

    void ApplySafeArea() {
        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        lastSafeArea = Screen.safeArea;
        lastOrientation = Screen.orientation;
        lastResolution = new Vector2Int(Screen.width, Screen.height);
    }
}