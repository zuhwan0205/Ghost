using UnityEngine;

public class testPlayer : MonoBehaviour
{
    public float interactionTime = 0;

    private void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) transform.position = new Vector2(transform.position.x + Time.deltaTime * Input.GetAxisRaw("Horizontal") * 5, transform.position.y);
        //상호작용 키 누르면 상호작용 타이머 상승 때면 0
        if (Input.GetKey(KeyCode.E)) interactionTime += Time.deltaTime;
        else interactionTime = 0;

        //움직여도 상호작용 타이머 0
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) interactionTime = 0;
    }
}
