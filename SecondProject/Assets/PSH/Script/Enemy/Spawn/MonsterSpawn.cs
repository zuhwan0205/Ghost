using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterSpawn : MonoBehaviour
{
    [Header("∏ÛΩ∫≈Õ")]
    [SerializeField] public GameObject Ghost;
    [SerializeField] public GameObject Mutation;
    [SerializeField] public GameObject Mannequin;
    [SerializeField] public GameObject shadow;

    private Player player;

    public void Start()
    {
        player =GetComponent<Player>();
    }


    public void Update()
    {


        if (Input.GetKeyDown(KeyCode.Alpha1))
            spawn("Ghost");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            spawn("Mutation");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            spawn("Mannequin");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            spawn("shadow");

    }

    public void spawn(string monster)
    {

 

        if (monster == "Ghost")
        {
            Instantiate(Ghost, transform.position, Quaternion.identity);
        }
        if (monster == "Mutation")
        {
            Instantiate(Mutation, transform.position, Quaternion.identity);
        }
        if (monster == "Mannequin")
        {
            Instantiate(Mannequin, transform.position, Quaternion.identity);
        }
        if (monster == "shadow")
        {
            Instantiate(shadow, transform.position, Quaternion.identity);
        }
    }


}
