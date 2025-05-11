using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class Monitor : EventObject
{
    private Animator anim;
    private AudioSource tvNoise;
    private bool seReady = true;
    [SerializeField] private float workingTime;
    [SerializeField] private float failTime;
    [SerializeField] private GameObject ghost;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
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
                seReady = false;
            }
        }

        // needTime���� ��ȣ�ۿ� �Ϸ�� ����
        if (detected && interactionTime > needTime)
        {
            Deactivate();
            AudioManager.Instance.Play("TVButton");
        }

        // failTime���� ��ȣ�ۿ� ���ϸ� ���� �̺�Ʈ
        if (workingTime > failTime)
        {
            Deactivate();
            Instantiate(ghost, transform.position, Quaternion.identity);
        }
    }

    private void Deactivate()
    {
        isWorking = false;
        tvNoise.Stop();
        tvNoise.loop = false;
        workingTime = 0;
        seReady = true;
    }
}
