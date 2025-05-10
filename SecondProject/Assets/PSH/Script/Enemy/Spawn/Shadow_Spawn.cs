using UnityEngine;

public class Standing_ManSpawn : MonoBehaviour
{
    public GameObject monsterPrefab;
    [SerializeField] private bool trigger = false;

    public Player pl;

    void Awake()
    {
        pl = GetComponent<Player>();

        if (pl == null)
        {
            pl = FindFirstObjectByType<Player>();
        }
    }

    private void Update()
    {
        if (trigger)
        {
            SpawnMonsterBehindPlayer();
            trigger = false;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            trigger = true;
        }
    }

    void SpawnMonsterBehindPlayer()
    {
        if (pl == null)
        {
            Debug.LogWarning("[Standing_ManSpawn] Player 스크립트를 찾지 못했습니다!");
            return;
        }

        if (monsterPrefab == null)
        {
            Debug.LogWarning("[Standing_ManSpawn] 몬스터 프리팹이 비어있습니다!");
            return;
        }

        float direction = pl.transform.localScale.x;
        Vector3 spawnOffset = new Vector3(-direction * 2f, 0f, 0f);
        Vector3 spawnPosition = pl.transform.position + spawnOffset;

        GameObject monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);

        Vector3 monsterScale = monster.transform.localScale;
        monsterScale.x = direction * Mathf.Abs(monsterScale.x);
        monster.transform.localScale = monsterScale;
    }

    public void Standing_Man_Trigger()
    {
        trigger = true;
    }
}
