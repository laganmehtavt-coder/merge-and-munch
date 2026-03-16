using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("BGM")]
    public AudioClip bgm;

    [Header("Destroy Sound")]
    public AudioClip destroySound;

    void Start() {
        if (bgm != null) {
            bgmSource.clip = bgm;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlaySound(AudioClip clip) {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayDestroySound() {
        PlaySound(destroySound);
    }
}