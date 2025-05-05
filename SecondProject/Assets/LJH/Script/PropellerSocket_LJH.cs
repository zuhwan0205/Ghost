using UnityEngine;

public class PropellerSocket_LJH : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ProPeller"))
        {
            MiniGameManager manager = FindFirstObjectByType<MiniGameManager>();
            if (manager != null && manager.GetVentTrashCount() >= 4)
            {
                other.transform.position = Vector2.zero;
                other.GetComponent<DragGlass>().enabled = false;
                manager.OnSocketCompleted("Vent");
            }
        }
    }
}