using UnityEngine;
using MergeGame;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/Item")]
public class ItemData : ScriptableObject {
    [Header("Basic")]
    public string itemName;
    public int level;

    [Header("Visual")]
    public Sprite sprite;
    public GameObject prefab;

    [Header("Shape")]
    public ItemShape shape;

    [Header("Size")]
    public Vector2 size = Vector2.one;

    [Header("Effect")]
    public ItemEffect effect;

    [Header("Sounds")]
    public AudioClip dropSound;
    public AudioClip mergeSound;
    public AudioClip destroySound;
}