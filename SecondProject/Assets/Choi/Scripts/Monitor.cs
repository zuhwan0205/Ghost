using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class Monitor : EventObject
{
    private Animator anim;
    private AudioSource tvNoise;
    private bool seReady = true;
    private Light2D light;
    [SerializeField] private float workingTime;
    [SerializeField] private float failTime;
    [SerializeField] private GameObject ghost;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        light = GetComponentInChildren<Light2D>();
    }

    protected override void Update()
    {
        base.Update();

        anim.SetBool("isWorking", isWorking);

        if (isWorking)
        {
            workingTime += Time.deltaTime;
            if (seReady)
            {
                tvNoise = AudioManager.Instance.PlayLoopSFX("TVnoise", transform.position);
                light.enabled = true;
                seReady = false;
            }
        }

        // needTime까지 상호작용 완료시 해제
        if (detected && interactionTime > needTime)
        {
            Deactivate();
            AudioManager.Instance.Play("TVButton");
        }

        // failTime까지 상호작용 못하면 실패 이벤트
        if (workingTime > failTime)
        {
            Deactivate();
            Instantiate(ghost, new Vector2(transform.position.x, transform.position.y - 2), Quaternion.identity);
        }
    }

    private void Deactivate()
    {
        isWorking = false;
        if (tvNoise != null)
        {
            tvNoise.Stop();
            tvNoise.loop = false;
        }
        light.enabled = false;
        workingTime = 0;
        seReady = true;
    }
}
