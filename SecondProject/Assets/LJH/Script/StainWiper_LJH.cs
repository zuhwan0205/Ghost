using UnityEngine;

public class StainWiper : MonoBehaviour
{
    private Camera cam;
    private Vector2 lastSwipePos;
    private float swipeThreshold = 0.1f;
    private bool hasSwipedOnce = false; // <- 처음 스와이프 감지 여부
    public static StainWiper Instance;
    public int cleanCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    
    private void Start()
    {
        cam = Camera.main;
    }
    
    private void Update()
    {
        Vector2 inputPos = Vector2.zero;
        bool isTouching = false;

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            inputPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isTouching = true;
        }
#else
    if (Input.touchCount > 0)
    {
        inputPos = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        isTouching = true;
    }
#endif

        if (isTouching)
        {
            if (!hasSwipedOnce)
            {
                lastSwipePos = inputPos;
                hasSwipedOnce = true;
            }

            if (Vector2.Distance(inputPos, lastSwipePos) > swipeThreshold)
            {
                TryClean(inputPos);
                lastSwipePos = inputPos;
            }
        }
        else
        {
            hasSwipedOnce = false; // 손을 뗐을 때 초기화
        }
    }

    void TryClean(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out Stain stain))
        {
            stain.Clean(position);
        }
        else if (hit != null && hit.TryGetComponent(out Dust_LJH dust))
        {
            dust.Clean(position);
        }
    }
}