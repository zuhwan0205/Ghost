using UnityEngine;

public class PropellerSocket_LJH : MonoBehaviour
{
    private int ventTrashCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vent_Trash"))
        {
            Destroy(other.gameObject);
            ventTrashCount++;
        }
        else if (other.CompareTag("ProPeller") && ventTrashCount >= 4)
        {
            other.transform.position = Vector2.zero;
            other.GetComponent<DragGlass>().enabled = false;
            FindFirstObjectByType<SocketMiniGame>().OnSocketCompleted(); // 20ÁÙ ¼öÁ¤
        }
    }
}