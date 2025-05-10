using Unity.VisualScripting;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    [Header("Detect INFO")]
    [SerializeField] LayerMask findObj;
    [SerializeField] float detectDistX;
    [SerializeField] float detectDistY;

    [Header("Interact INFO")]
    [SerializeField] protected bool detected;
    [HideInInspector] public float interactionTime;
    public float needTime;

    protected Collider2D detCol;

    [Header("Work INFO")]
    public bool isWorking = false;

    protected virtual void Update()
    {
        (detected, detCol) = DetectPlayer();    // ������ �÷��̾� ����
        interactionTime = detCol != null ? detCol.GetComponent<Player>().interactionTime : 0;   // �÷��̾� interactionTime ����
    }

    protected virtual (bool, Collider2D) DetectPlayer() // �÷��̾� ����
    {
        Collider2D hit = Physics2D.OverlapBox(transform.position, new Vector2(detectDistX, detectDistY), 0f, findObj);

        return (hit != null, hit);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector2(detectDistX, detectDistY));
    }
}
