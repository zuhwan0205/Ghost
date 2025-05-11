using UnityEngine;

public class LevelTester : MonoBehaviour
{
    private MannequinSpawn mannequinSpawner;
    private MutationSpawner mutationSpawner;

    void Start()
    {
        mannequinSpawner = FindObjectOfType<MannequinSpawn>();
        mutationSpawner = FindObjectOfType<MutationSpawner>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetLevel(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SetLevel(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SetLevel(3);
    }

    void SetLevel(int level)
    {
        if (mannequinSpawner != null)
            mannequinSpawner.SetSpawnLevel(level);

        if (mutationSpawner != null)
            mutationSpawner.SetSpawnLevel(level);

        Debug.Log($"[LevelTester] 레벨 {level} 적용 완료");
    }
}
