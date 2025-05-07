using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WireConnection : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Image wireImage;
    public Transform startNode;
    public Transform endNode;
    private Vector2 startPos, endPos;
    public bool isConnected = false;

    public void ResetWire()
    {
        isConnected = false;
        startPos = Vector2.zero;
        endPos = Vector2.zero;
        if (wireImage != null)
        {
            wireImage.gameObject.SetActive(false);
            wireImage.rectTransform.sizeDelta = new Vector2(0, 10); // �⺻ ũ��
            wireImage.rectTransform.position = Vector3.zero;
            wireImage.rectTransform.rotation = Quaternion.identity;
            Debug.Log($"{gameObject.name} ����: isConnected={isConnected}, wireImage.active={wireImage.gameObject.activeSelf}, sizeDelta={wireImage.rectTransform.sizeDelta}, rotation={wireImage.rectTransform.rotation}");
        }
        else
        {
            Debug.LogError($"{gameObject.name}�� wireImage�� null�Դϴ�!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = startNode.position;
        wireImage.gameObject.SetActive(true);
        Debug.Log($"{gameObject.name} �巡�� ����: wireImage Ȱ��ȭ - Ȱ��ȭ ����: {wireImage.gameObject.activeSelf}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        endPos = eventData.position;
        UpdateWire();
        Debug.Log($"{gameObject.name} �巡�� ��: �� ��ġ ������Ʈ");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(eventData.position, endNode.position) < 50f)
        {
            endPos = endNode.position;
            isConnected = true;
            UpdateWire();
            FindFirstObjectByType<WiringGameManager>().CheckAllWires();
            Debug.Log($"{gameObject.name} ���� ����");
        }
        else
        {
            wireImage.gameObject.SetActive(false);
            isConnected = false;
            Debug.Log($"{gameObject.name} ���� ����: wireImage ��Ȱ��ȭ");
        }
    }

    void UpdateWire()
    {
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        wireImage.rectTransform.sizeDelta = new Vector2(distance, 10);
        wireImage.rectTransform.position = startPos + direction / 2;
        wireImage.rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }
}