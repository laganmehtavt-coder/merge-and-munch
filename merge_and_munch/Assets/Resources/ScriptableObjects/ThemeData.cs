using UnityEngine;

[CreateAssetMenu(fileName = "ThemeData", menuName = "MergeGame/Theme")]
public class ThemeData : ScriptableObject {
    public enum ThemeType {
        Devlopment,
        Fruits,
        Vegetables,
        Candy
    }

    [Header("Theme Type")]
    public ThemeType themeType;

    [Header("Theme Sprites")]
    public Sprite background;
    public Sprite border;
    public Sprite line;
    public Sprite spawnPoint;
}