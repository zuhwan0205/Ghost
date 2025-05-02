using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRadius = 1f;
    public LayerMask interactableLayer;
    public float holdTime = 2f;
    public Image holdProgressBar;
    public GameObject interactionHint;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private Interactable currentInteractable;

    void Start()
    {
        if (holdProgressBar == null)
            Debug.LogError("HoldProgressBar is not assigned!");
        if (interactionHint == null)
            Debug.LogError("InteractionHint is not assigned!");
        holdProgressBar.gameObject.SetActive(false);
    }

    void Update()
    {
        // ��ȣ�ۿ� ����
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionRadius, interactableLayer);
        interactionHint.SetActive(hit != null);
        if (hit != null)
            Debug.Log($"Near: {hit.gameObject.name}");

        // �Ϲ� ��ȣ�ۿ�
        if (Input.GetKeyDown(KeyCode.E) && hit)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null && !interactable.isHoldInteraction)
            {
                interactable.Interact();
                GameManager.Instance.AddMissionProgress();
            }
        }

        // Ȧ�� ��ȣ�ۿ�
        if (Input.GetKey(KeyCode.E) && hit)
        {
            currentInteractable = hit.GetComponent<Interactable>();
            if (currentInteractable != null && currentInteractable.isHoldInteraction)
            {
                isHolding = true;
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdTime)
                {
                    currentInteractable.Interact();
                    GameManager.Instance.AddMissionProgress();
                    holdTimer = 0f;
                    isHolding = false;
                }
                Debug.Log($"Holding: {holdTimer}/{holdTime}");
            }
        }
        else
        {
            isHolding = false;
            holdTimer = 0f;
        }

        // Ȧ�� ���� ��
        if (isHolding && currentInteractable != null && currentInteractable.isHoldInteraction)
        {
            holdProgressBar.gameObject.SetActive(true);
            holdProgressBar.fillAmount = holdTimer / holdTime;
            Debug.Log($"Gauge Active: {holdProgressBar.gameObject.activeSelf}, Fill: {holdProgressBar.fillAmount}");
        }
        else
        {
            holdProgressBar.gameObject.SetActive(false);
            holdProgressBar.fillAmount = 0f;
        }

        // �׽�Ʈ�� ������ �߰� (1, 2, 3 Ű)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UIManager.Instance.AddItemToSlot(0, Resources.Load<Sprite>("Item1")); // Item1 ��������Ʈ �ʿ�
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UIManager.Instance.AddItemToSlot(1, Resources.Load<Sprite>("Item2"));
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UIManager.Instance.AddItemToSlot(2, Resources.Load<Sprite>("Item3"));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}