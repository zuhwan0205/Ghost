using UnityEngine;

public static class InputHandler
{
    public static bool GetInputPosition(out Vector2 position, Camera cam, bool useCanvas = false, RectTransform canvasRect = null)
    {
        position = Vector2.zero;

        // ���콺 �Է� Ȯ��
        if (Input.GetMouseButton(0))
        {
            position = Input.mousePosition;
            Debug.Log($"���콺 �Է� ����: {position}");
        }
        // ��ġ �Է� Ȯ��
        else if (Input.touchCount > 0)
        {
            position = Input.GetTouch(0).position;
            Debug.Log($"��ġ �Է� ����: {position}");
        }
        else
        {
            Debug.Log("�Է� ����: ���콺 Ŭ�� �Ǵ� ��ġ �ʿ�");
            return false;
        }

        // Canvas ��ǥ ��ȯ (Screen Space - Overlay ����)
        if (useCanvas && canvasRect != null)
        {
            // Screen Space - Overlay������ ī�޶� �ʿ� ����
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position, null, out position);
            if (!success)
            {
                Debug.LogWarning("ScreenPointToLocalPointInRectangle ��ȯ ����");
                return false;
            }

            // Canvas ũ�� ���� ��ǥ ����
            position.x = Mathf.Clamp(position.x, -canvasRect.rect.width / 2, canvasRect.rect.width / 2);
            position.y = Mathf.Clamp(position.y, -canvasRect.rect.height / 2, canvasRect.rect.height / 2);
            Debug.Log($"Canvas ���� ��ǥ ��ȯ ����: {position}");
            return true;
        }

        // Canvas �̻�� �� ���� ��ǥ�� ��ȯ
        position = cam.ScreenToWorldPoint(position);
        Debug.Log($"���� ��ǥ ��ȯ ����: {position}");
        return true;
    }
}