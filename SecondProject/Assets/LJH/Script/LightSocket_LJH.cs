using UnityEngine;

public class LightSocket : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Light"))
        {
            other.transform.position = transform.position;
            other.GetComponent<DragGlass>().enabled = false;
            FindObjectOfType<SocketMiniGame>().OnSocketCompleted();
        }
    }
}