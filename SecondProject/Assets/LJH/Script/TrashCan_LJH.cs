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
                FindFirstObjectByType<GlassMiniGame>().OnGlassMissionCompleted(); // 17ÁÙ ¼öÁ¤
            }
        }
    }

    public void ResetCount()
    {
        cleanCount = 0;
    }
}