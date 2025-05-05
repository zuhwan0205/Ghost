using UnityEngine;

public static class InputHandler
{
    public static bool GetInputPosition(out Vector2 position, Camera cam, bool useCanvas = false, RectTransform canvasRect = null)
    {
        position = Vector2.zero;

        // 마우스 입력 확인
        if (Input.GetMouseButton(0))
        {
            position = Input.mousePosition;
            Debug.Log($"마우스 입력 감지: {position}");
        }
        // 터치 입력 확인
        else if (Input.touchCount > 0)
        {
            position = Input.GetTouch(0).position;
            Debug.Log($"터치 입력 감지: {position}");
        }
        else
        {
            Debug.Log("입력 없음: 마우스 클릭 또는 터치 필요");
            return false;
        }

        // Canvas 좌표 변환 (Screen Space - Overlay 기준)
        if (useCanvas && canvasRect != null)
        {
            // Screen Space - Overlay에서는 카메라 필요 없음
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position, null, out position);
            if (!success)
            {
                Debug.LogWarning("ScreenPointToLocalPointInRectangle 변환 실패");
                return false;
            }

            // Canvas 크기 내로 좌표 제한
            position.x = Mathf.Clamp(position.x, -canvasRect.rect.width / 2, canvasRect.rect.width / 2);
            position.y = Mathf.Clamp(position.y, -canvasRect.rect.height / 2, canvasRect.rect.height / 2);
            Debug.Log($"Canvas 로컬 좌표 변환 성공: {position}");
            return true;
        }

        // Canvas 미사용 시 월드 좌표로 변환
        position = cam.ScreenToWorldPoint(position);
        Debug.Log($"월드 좌표 변환 성공: {position}");
        return true;
    }
}