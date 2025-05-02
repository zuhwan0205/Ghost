using UnityEngine;
using UnityEngine.EventSystems;

public class NodeDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public WireConnection wireConnection;

    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        wireConnection.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            wireConnection.OnDrag(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDragging)
        {
            wireConnection.OnEndDrag(eventData);
            isDragging = false;
        }
    }
}