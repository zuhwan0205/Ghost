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
        // 상호작용 감지
        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionRadius, interactableLayer);
        interactionHint.SetActive(hit != null);
        if (hit != null)
            Debug.Log($"Near: {hit.gameObject.name}");

        // 일반 상호작용
        if (Input.GetKeyDown(KeyCode.E) && hit)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null && !interactable.isHoldInteraction)
            {
                interactable.Interact();
                GameManager.Instance.AddMissionProgress();
            }
        }

        // 홀드 상호작용
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

        // 홀드 진행 바
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

        // 테스트용 아이템 추가 (1, 2, 3 키)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UIManager.Instance.AddItemToSlot(0, Resources.Load<Sprite>("Item1")); // Item1 스프라이트 필요
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