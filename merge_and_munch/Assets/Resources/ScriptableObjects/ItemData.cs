using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/ItemData")]
public class ItemData : ScriptableObject {
    public string itemName;
    public GameObject itemPrefab;
    public ItemData nextItem;
    public float size = 1f;
    public float gravityScale = 1f;
    public float mass = 1f;

    [Header("Visual Effects")]
    public GameObject mergeEffectPrefab; // Merge hone par jo particle chalega

    [Header("Animations")]
    public RuntimeAnimatorController animatorController; // Fruit ki animation

    [Header("Audio")]
    public AudioClip dropSound;
    public AudioClip mergeSound;
}