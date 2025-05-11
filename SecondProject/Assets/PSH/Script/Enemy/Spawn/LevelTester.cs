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


    void SetLevel(int level)
    {
        if (mannequinSpawner != null)
            mannequinSpawner.SetSpawnLevel(level);

        if (mutationSpawner != null)
            mutationSpawner.SetSpawnLevel(level);

    }
}
