using UnityEngine;

public class StainWiper : MonoBehaviour
{
    private Camera cam;
    private Vector2 lastSwipePos;
    private float swipeThreshold = 0.1f;
    private bool hasSwipedOnce = false;
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
        // PC에서는 마우스 버튼만 체크
        if (Input.GetMouseButton(0))
        {
            // 화면 좌표 → 월드 좌표 변환
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = Mathf.Abs(cam.transform.position.z);
            Vector3 worldPos3 = cam.ScreenToWorldPoint(screenPos);
            Vector2 inputPos = new Vector2(worldPos3.x, worldPos3.y);

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
            hasSwipedOnce = false;
        }
    }

    void TryClean(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Dust_LJH>(out var dust))
            {
                dust.Clean(position);
                return;
            }
            if (hit.TryGetComponent<Stain>(out var stain))
            {
                stain.Clean(position);
                return;
            }
        }
    }
}