using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Action = System.Action;

public class LostManager : MonoBehaviour
{
    [Header("Manager INFO")]
    [SerializeField] private Transform[] spawnPoint;
    [SerializeField] private GameObject[] lostObj;
    [SerializeField] private int spawnAmount = 0;
    [SerializeField] private int fakeAmount = 0;
    [SerializeField] private bool clear = false;

    private GameObject[] lostObjs;
    private int spawnCount;
    private int fakeCount;
    private int clearCount;
    (string name, int count)[] objectIndex = new (string, int)[]
    {
        ("Ball", 0),
        ("Doll", 0),
        ("Handbag", 0),
        ("Wallet", 0),
        ("Wristwatch", 0)
    };

    public static event Action OnEndLostGame;

    private void Start()
    {
        if(MissionManager.Instance.OnLostManager == false) return;
        lostObjs = new GameObject[spawnPoint.Length];
        List<int> spawnIndices = new List<int>();
        List<int> fakeIndices = new List<int>();
        List<GameObject> spawnedList = new List<GameObject>();

        // 인덱스를 무작위로 섞어서 spawnAmount개 고름
        List<int> indices = new List<int>();
        for (int i = 0; i < spawnPoint.Length; i++) indices.Add(i);
        Shuffle(indices);

        for (int i = 0; i < spawnAmount; i++)
        {
            int index = indices[i];
            int rand = Random.Range(0, lostObj.Length);
            lostObjs[index] = Instantiate(lostObj[rand], spawnPoint[index].position, Quaternion.identity);

            for (int j = 0; j < 5; j++)
            {
                if (objectIndex[j].name == lostObj[rand].name) objectIndex[j].count++;
            }

            var lost = lostObjs[index].GetComponent<FindLost>();
            lost.isWorking = true;

            spawnedList.Add(lostObjs[index]);
            spawnIndices.Add(index);
        }

        // fakeAmount만큼 무작위로 선택하여 isFake 설정
        Shuffle(spawnedList);
        for (int i = 0; i < fakeAmount && i < spawnedList.Count; i++)
        {
            spawnedList[i].GetComponent<FindLost>().isFake = true;

            for (int j = 0; j < 5; j++)
            {
                if (objectIndex[j].name == spawnedList[i].name) objectIndex[j].count--;
            }
        }
    }

    // Fisher–Yates Shuffle 알고리즘
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }


    private GameObject TrySpawnLostObject(Vector3 position, int i)
    {
        int randIndex = Random.Range(0, lostObj.Length);

        if (RandomBool())
        {
            if (spawnCount < spawnAmount)
            {
                spawnCount++;
                return Instantiate(lostObj[randIndex], position, Quaternion.identity);
            }
        }
        else if ((spawnPoint.Length - i) <= (spawnAmount - spawnCount))
        {
            spawnCount++;
            return Instantiate(lostObj[randIndex], position, Quaternion.identity);
        }

        return null;
    }

    private bool RandomBool()
    {
        return Random.Range(0, 2) == 1;
    }

    private void Update()
    {
        if (!clear)
        {
            clearCount = 0;

            foreach (GameObject go in lostObjs)
            {
                if (go != null && !go.GetComponent<FindLost>().isWorking)
                {
                    clearCount++;
                }
            }

            if (clearCount >= lostObjs.Length - fakeAmount)
            {
                clear = true;
            }

            if (clearCount >= spawnAmount - fakeAmount)
            {
                OnEndLostGame?.Invoke();
            }
        }
    }
}
