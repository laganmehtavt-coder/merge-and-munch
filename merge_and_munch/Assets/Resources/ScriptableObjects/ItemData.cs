using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/Item")]
public class ItemData : ScriptableObject {
    public enum BehaviourType {
        Bounce,
        Jelly,
        Solid
    }
    [Header("Score")]
    public int mergeScore = 10;
    [Header("Sprite")]
    public Sprite sprite;
    [Tooltip("Controls visual size")]
    public float size = 1f;
    
    [Header("Behaviour")]
    public BehaviourType behaviour;

    [Header("Physics")]
    public PhysicsMaterial2D physicsMaterial;
    public float mass = 1f;
    public float gravityScale = 1f;


    [Header("Effects")]
    public GameObject mergeEffect;  



    [Header("Sounds")]
    public AudioClip dropSound;
    public AudioClip mergeSound;

    [Header("Merge Chain")]
    public ItemData nextItem;

    [Header("Animation")]
    [Tooltip("Animator Controller for complex animations")]
    public RuntimeAnimatorController animatorController;
    [Tooltip("Single animation clip for simple animations")]
    public AnimationClip animationClip;

    [Tooltip("Time in seconds between automatic animation plays")]
    public float animationInterval = 3f; // default 3 seconds
}