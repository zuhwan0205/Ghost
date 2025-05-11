using UnityEngine;

public class RandomObjManager : MonoBehaviour
{
    [SerializeField] private GameObject[] secureGate;
    [SerializeField] private GameObject[] monitor;

    private float timer = 0;
    public float Stime = 60;
    public float Mtime = 90;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= Stime)
        {
            int rand = Random.Range(0, secureGate.Length);

            secureGate[rand].GetComponent<SecurityGate>().isWorking = true;

            timer = 0;
        }

        if (timer >= Mtime)
        {
            int rand = Random.Range(0, monitor.Length);

            monitor[rand].GetComponent<Monitor>().isWorking = true;

            timer = 0;
        }
    }
}
