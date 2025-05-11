using System;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    [Header("Manager INFO")]
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private GameObject canObj;
    [SerializeField] private int clearCount = 0;
    [SerializeField] private bool clear = false;
    private GameObject[] canObjs;
    
    public static event Action OnEndTrashGame;

    private void Start()
    {
        canObjs = new GameObject[spawnPoint.Length];

        int i = 0;
        foreach (Transform t in spawnPoint)
        {
            canObjs[i] = Instantiate(canObj, t.position, Quaternion.identity);
            canObjs[i].GetComponent<RemoveTrash>().isWorking = true;
            i++;
        }
    }

    private void Update()
    {
        if (!clear)
        {
            foreach (GameObject go in canObjs)
            {
                if (!go.GetComponent<RemoveTrash>().isWorking)
                {
                    clearCount++;
                }
            }

            if (clearCount == canObjs.Length)
            {
                clear = true;
                OnEndTrashGame?.Invoke();
            }
            else
            {
                clearCount = 0;
            }
        }
    }
}
