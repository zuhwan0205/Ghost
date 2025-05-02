using UnityEngine;

public class Monitor : EventObject
{
    private Animator anim;
    [SerializeField] private float workingTime;
    [SerializeField] private float failTime;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        anim.SetBool("isWorking", isWorking);

        if (isWorking ) workingTime += Time.deltaTime;

        // needTime���� ��ȣ�ۿ� �Ϸ�� ����
        if (detected && interactionTime > needTime)
        {
            Deactivate();
        }

        // failTime���� ��ȣ�ۿ� ���ϸ� ���� �̺�Ʈ
        if (workingTime > failTime)
        {
            Deactivate();
            Debug.Log("���� ����!");
        }
    }

    private void Deactivate()
    {
        isWorking = false;
        workingTime = 0;
    }
}
