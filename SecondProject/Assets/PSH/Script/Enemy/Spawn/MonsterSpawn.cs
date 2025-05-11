using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    public GameObject Ghost;
    public GameObject Mutation;
    public GameObject Mannequin;
    public GameObject shadow;

    public GameObject player; // �÷��̾ �ݵ�� �����ϰų� �ڵ����� ã���ϴ�

    void Start()
    {
        // �÷��̾ �������� ������� �ʾ��� ��� �ڵ����� ã��
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[MonsterSpawn] Player ������Ʈ�� ã�� �� �����ϴ�! 'Player' �±װ� �����Ǿ� �־�� �մϴ�.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("Ghost ��ȯ"); spawn("Ghost"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Debug.Log("Mutation ��ȯ"); spawn("Mutation"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Debug.Log("Mannequin ��ȯ"); spawn("Mannequin"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Debug.Log("Shadow ��ȯ"); spawn("shadow"); }
    }

    public void spawn(string monster)
    {
        if (player == null)
        {
            Debug.LogWarning("[MonsterSpawn] �÷��̾ �������� �ʾ� ��ȯ�� ����մϴ�.");
            return;
        }

        // �÷��̾� ��ġ ���� +5f �����ʿ� ��ȯ
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
            Debug.LogWarning($"[MonsterSpawn] '{monster}' �������� ������� �ʾҽ��ϴ�.");
    }
}
