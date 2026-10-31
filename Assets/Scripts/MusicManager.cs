using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> musicClips;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicClips = new Dictionary<string, AudioClip>()
            {
                { "Stage", Resources.Load<AudioClip>("Audio/BGM/ステージ") },
                { "Title", Resources.Load<AudioClip>("Audio/BGM/タイトル・メニュー") },
                { "Menu", Resources.Load<AudioClip>("Audio/BGM/タイトル・メニュー") },
                { "Result", Resources.Load<AudioClip>("Audio/BGM/リザルト") }
            };
            audioSource = GetComponent<AudioSource>();

        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        PlayMusic(SceneManager.GetActiveScene().name);
    }
    public void PlayMusic(string sceneName)
    {
        if (sceneName == "Result")
        {
            audioSource.loop = false;
        }
        else
        {
            audioSource.loop = true;
        }
        if (sceneName.Contains("Stage"))
        {
            if (audioSource.clip == musicClips["Stage"] && audioSource.isPlaying)
            {
                return;
            }
            audioSource.clip = musicClips["Stage"];
            audioSource.Play();
        }
        else
        {
            if (audioSource.clip == musicClips[sceneName] && audioSource.isPlaying)
            {
                return;
            }
            audioSource.clip = musicClips.ContainsKey(sceneName) ? musicClips[sceneName] : null;
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }
}
