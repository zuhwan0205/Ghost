using UnityEngine;
using UnityEngine.Audio;

public class Monitor : EventObject
{
    private Animator anim;
    private AudioSource aud;
    [SerializeField] private float workingTime;
    [SerializeField] private float failTime;
    [SerializeField] private GameObject ghost;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        aud = GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();

        anim.SetBool("isWorking", isWorking);

        if (isWorking)
        {
            workingTime += Time.deltaTime;
            if (!aud.isPlaying) aud.Play();
        }

        // needTime까지 상호작용 완료시 해제
        if (detected && interactionTime > needTime)
        {
            Deactivate();
        }

        // failTime까지 상호작용 못하면 실패 이벤트
        if (workingTime > failTime)
        {
            Deactivate();
            Instantiate(ghost, transform.position, Quaternion.identity);
        }
    }

    private void Deactivate()
    {
        isWorking = false;
        workingTime = 0;
        aud.Stop();
    }
}
