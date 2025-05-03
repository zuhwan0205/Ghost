using UnityEngine;

public static class InputHandler
{
    public static bool GetInputPosition(out Vector2 position, Camera cam, bool useCanvas = false, RectTransform canvasRect = null)
    {
        position = Vector2.zero;
        bool isTouching = false;

        if (Input.GetMouseButton(0))
        {
            position = Input.mousePosition;
            isTouching = true;
        }
        else if (Input.touchCount > 0)
        {
            position = Input.GetTouch(0).position;
            isTouching = true;
        }

        if (!isTouching)
            return false;

        if (useCanvas && canvasRect != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position, cam, out position);
            return true;
        }

        position = cam.ScreenToWorldPoint(position);
        return true;
    }
}