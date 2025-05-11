using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;

    [SerializeField, Header("BGM Clips")]
    private List<AudioClipData> bgmClips;

    [SerializeField, Header("UI Clips")]
    private List<AudioClipData> uiClips;

    [SerializeField, Header("Object Clips")]
    private List<AudioClipData> objectClips;

    [SerializeField, Header("Player Clips")]
    private List<AudioClipData> playerClips;

    [SerializeField, Header("Monster Clips")]
    private List<AudioClipData> monsterClips;

    public enum SFXCategory { UI, Monster, Object, Player, BGM }

    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    private Dictionary<string, AudioClipData> clipDict;

    private List<AudioSource> sfxPool = new();
    private const int defaultPoolSize = 10;
    private int poolIndex = 0;

    #region singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion singleton

    private void Initialize()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        for (int i = 0; i < defaultPoolSize; i++)
        {
            var sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfxPool.Add(sfx);
        }

        clipDict = new Dictionary<string, AudioClipData>();
        AddClipListToDict(bgmClips);
        AddClipListToDict(uiClips);
        AddClipListToDict(objectClips);
        AddClipListToDict(playerClips);
        AddClipListToDict(monsterClips);
    }
    private void AddClipListToDict(List<AudioClipData> clipList)
    {
        if (clipList == null) return;

        foreach (var clipData in clipList)
        {
            if (clipData == null || string.IsNullOrWhiteSpace(clipData.name))
            {
                Debug.LogWarning("[AudioManager] ��� �ְų� �̸� ���� Ŭ���� �����Ǿ����ϴ�.");
                continue;
            }

            if (clipDict.ContainsKey(clipData.name))
            {
                Debug.LogWarning($"[AudioManager] �ߺ��� ���� �̸�: {clipData.name}");
                continue;
            }

            clipDict.Add(clipData.name, clipData);
        }
    }
    //���� �̸��� �޾� �ش� ���� ���
    public void Play(string name)
    {
        if (clipDict.TryGetValue(name, out var clipData))
        {
            var source = GetNextAvailableSFXSource();
            source.clip = clipData.clip;
            source.volume = clipData.volume;
            source.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Clip '{name}' not found.");
        }
    }

    //������ ��ġ���� ���� ���
    public void PlayAt(string name, Vector3 position)
    {
        if (clipDict.TryGetValue(name, out var clipData))
        {
            //3D ������ ���
            AudioSource.PlayClipAtPoint(clipData.clip, position, clipData.volume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Clip '{name}' not found.");
        }
    }
    //������ ���� �ѹ� ġ�� ������� ���忡 ����

    //����� ���
    public void PlayBGM(string name, bool fadeIn = false, float fadeTime = 1f)
    {
        if (clipDict.TryGetValue(name, out var clipData))
        {
            if (bgmSource.isPlaying)
                StopBGM();

            bgmSource.clip = clipData.clip;
            bgmSource.volume = fadeIn ? 0f : clipData.volume;
            bgmSource.Play();

            if (fadeIn)
                StartCoroutine(FadeInBGM(fadeTime, clipData.volume));
        }
        else
        {
            Debug.LogWarning($"[AudioManager] BGM '{name}' not found.");
        }
    }
    //����� ����(fadeOut�� True�� ������ �����)
    public void StopBGM(bool fadeOut = false, float fadeTime = 1f)
    {
        if (!bgmSource.isPlaying) return;

        if (fadeOut)
            StartCoroutine(FadeOutBGM(fadeTime));
        else
            bgmSource.Stop();
    }

    //ȿ���� ���
    public void PlaySFX(string soundName, Vector3 position)
    {
        if (!clipDict.ContainsKey(soundName))
        {
            Debug.LogWarning($"[AudioManager] '{soundName}' ���带 ã�� �� �����ϴ�.");
            return;
        }

        AudioClipData data = clipDict[soundName];

        // Object Pool���� AudioSource ��������
        AudioSource sfxSource = GetNextAvailableSFXSource();
        sfxSource.transform.position = position;  // ��ġ ����
        sfxSource.PlayOneShot(data.clip);
    }

    public AudioSource PlayLoopSFX(string name, Vector3 position)
    {
        if (!clipDict.TryGetValue(name, out var data))
        {
            Debug.LogWarning($"[AudioManager] '{name}' ���带 ã�� �� �����ϴ�.");
            return null;
        }

        AudioSource sfxSource = GetNextAvailableSFXSource();
        sfxSource.clip = data.clip;
        sfxSource.volume = data.volume;
        sfxSource.loop = true;

        sfxSource.transform.position = position;

        sfxSource.spatialBlend = 1f;
        sfxSource.minDistance = 1f;
        sfxSource.maxDistance = 500f;
        sfxSource.rolloffMode = AudioRolloffMode.Logarithmic;

        sfxSource.Play();
        return sfxSource;
    }



    public void StopSFX(string soundName)
    {
        // ���� ���߱� (��� ���� ���尡 �ش� ����� ����)
        foreach (var source in sfxPool)
        {
            if (source.isPlaying && source.clip.name == soundName)
            {
                source.Stop();
                break;
            }
        }
    }
    private AudioSource GetNextAvailableSFXSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            int index = (poolIndex + i) % sfxPool.Count;
            if (!sfxPool[index].isPlaying)
            {
                poolIndex = (index + 1) % sfxPool.Count;
                return sfxPool[index];
            }
        }

        var fallback = sfxPool[poolIndex];
        poolIndex = (poolIndex + 1) % sfxPool.Count;
        return fallback;
    }

    //BGM���� �ǽð� ����
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    private System.Collections.IEnumerator FadeInBGM(float time, float targetVolume)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, t / time);
            yield return null;
        }
        bgmSource.volume = targetVolume;
    }

    private System.Collections.IEnumerator FadeOutBGM(float time)
    {
        float startVolume = bgmSource.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / time);
            yield return null;
        }
        bgmSource.Stop();
    }
}