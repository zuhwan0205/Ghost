using UnityEngine;

public class DragGlass : MonoBehaviour
{
    private static DragGlass currentlyDragging = null;

    private Vector3 offset;
    private bool isDragging = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out DragGlass target))
                {
                    target.BeginDrag(worldPos);
                    currentlyDragging = target;
                    break;
                }
            }
        }

        if (Input.GetMouseButton(0) && currentlyDragging != null)
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            currentlyDragging.Drag(worldPos);
        }

        if (Input.GetMouseButtonUp(0) && currentlyDragging != null)
        {
            currentlyDragging.EndDrag();
            currentlyDragging = null;
        }
    }

    public void BeginDrag(Vector3 startMouseWorldPos)
    {
        offset = transform.position - startMouseWorldPos;
        isDragging = true;
    }

    public void Drag(Vector3 currentMouseWorldPos)
    {
        if (!isDragging) return;
        transform.position = new Vector3(currentMouseWorldPos.x + offset.x, currentMouseWorldPos.y + offset.y, transform.position.z);
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    public void ForceStopDragging()
    {
        isDragging = false;
    }
}