using UnityEngine;

public class DragGlass : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;

    void OnMouseDown()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
    
    public void ForceStopDragging()
    {
        isDragging = false;
    }
}
