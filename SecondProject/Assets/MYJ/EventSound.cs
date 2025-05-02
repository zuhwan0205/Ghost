using UnityEngine;

public class EventSound : MonoBehaviour
{
    public string soundName = "laughing"; // AudioManager�� ��ϵ� ���� �̸�
    //private bool hasPlayed = false;       // �� ���� ����ϵ��� �����ϴ� �÷���

     public float cooldown = 5f;           // ��� ��Ÿ�� (��)
    private float lastPlayTime = -Mathf.Infinity; // ���������� ����� �ð�

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*if (!hasPlayed && other.CompareTag("Player"))
        {
            AudioManager.Instance.Play(soundName);
            hasPlayed = true; // �������ʹ� ������� �ʵ��� ����
        }*/

        if (other.CompareTag("Player") && Time.time >= lastPlayTime + cooldown)
        {
            AudioManager.Instance.Play(soundName);
            lastPlayTime = Time.time;
        }
    }
}
