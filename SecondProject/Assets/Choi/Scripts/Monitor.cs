using UnityEngine;

public class Monitor : EventObject
{
    private Animator anim;
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

        if (isWorking ) workingTime += Time.deltaTime;

        // needTime까지 상호작용 완료시 해제
        if (detected && interactionTime > needTime)
        {
            Deactivate();
        }

        // failTime까지 상호작용 못하면 실패 이벤트
        if (workingTime > failTime)
        {
            Deactivate();
            Instantiate(ghost);
        }
    }

    private void Deactivate()
    {
        isWorking = false;
        workingTime = 0;
    }
}
