using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private int cleanCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Glass"))
        {
            Destroy(other.gameObject);
            cleanCount++;
            if (cleanCount >= 8)
            {
                FindFirstObjectByType<MiniGameManager>().OnSocketCompleted("Glass");
            }
        }
        else if (other.CompareTag("Vent_Trash"))
        {
            Destroy(other.gameObject);
            FindFirstObjectByType<MiniGameManager>().OnVentTrashCleaned(); // ī��Ʈ ���� ��û
        }
    }

    public void ResetCount()
    {
        cleanCount = 0;
    }
}