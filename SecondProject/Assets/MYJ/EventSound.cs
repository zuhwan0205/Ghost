using UnityEngine;

public class EventSound : MonoBehaviour
{
    public string soundName = "laughing"; // AudioManager에 등록된 사운드 이름
    //private bool hasPlayed = false;       // 한 번만 재생하도록 제어하는 플래그

     public float cooldown = 5f;           // 재생 쿨타임 (초)
    private float lastPlayTime = -Mathf.Infinity; // 마지막으로 재생된 시간

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*if (!hasPlayed && other.CompareTag("Player"))
        {
            AudioManager.Instance.Play(soundName);
            hasPlayed = true; // 다음부터는 재생되지 않도록 설정
        }*/

        if (other.CompareTag("Player") && Time.time >= lastPlayTime + cooldown)
        {
            AudioManager.Instance.Play(soundName);
            lastPlayTime = Time.time;
        }
    }
}
