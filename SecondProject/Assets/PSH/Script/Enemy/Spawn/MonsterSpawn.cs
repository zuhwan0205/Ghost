using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    public GameObject Ghost;
    public GameObject Mutation;
    public GameObject Mannequin;
    public GameObject shadow;

    public GameObject player; // 플레이어를 반드시 연결하거나 자동으로 찾습니다

    void Start()
    {
        // 플레이어가 수동으로 연결되지 않았을 경우 자동으로 찾음
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[MonsterSpawn] Player 오브젝트를 찾을 수 없습니다! 'Player' 태그가 지정되어 있어야 합니다.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("Ghost 소환"); spawn("Ghost"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("Mutation 소환"); spawn("Mutation"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("Mannequin 소환"); spawn("Mannequin"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Debug.Log("Shadow 소환"); spawn("shadow"); }
    }

    public void spawn(string monster)
    {
        if (player == null)
        {
            Debug.LogWarning("[MonsterSpawn] 플레이어가 존재하지 않아 소환을 취소합니다.");
            return;
        }

        // 플레이어 위치 기준 +5f 오른쪽에 소환
        Vector3 spawnPos = new Vector3(
            player.transform.position.x + 5f,
            player.transform.position.y,
            player.transform.position.z
        );

        if (monster == "Ghost" && Ghost != null)
            Instantiate(Ghost, spawnPos, Quaternion.identity);
        else if (monster == "Mutation" && Mutation != null)
            Instantiate(Mutation, spawnPos, Quaternion.identity);
        else if (monster == "Mannequin" && Mannequin != null)
            Instantiate(Mannequin, spawnPos, Quaternion.identity);
        else if (monster == "shadow" && shadow != null)
            Instantiate(shadow, spawnPos, Quaternion.identity);
        else
            Debug.LogWarning($"[MonsterSpawn] '{monster}' 프리팹이 연결되지 않았습니다.");
    }
}
