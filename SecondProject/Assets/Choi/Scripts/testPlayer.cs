using UnityEngine;

public class testPlayer : MonoBehaviour
{
    public float interactionTime = 0;

    private void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) transform.position = new Vector2(transform.position.x + Time.deltaTime * Input.GetAxisRaw("Horizontal") * 5, transform.position.y);
        //��ȣ�ۿ� Ű ������ ��ȣ�ۿ� Ÿ�̸� ��� ���� 0
        if (Input.GetKey(KeyCode.E)) interactionTime += Time.deltaTime;
        else interactionTime = 0;

        //�������� ��ȣ�ۿ� Ÿ�̸� 0
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) interactionTime = 0;
    }
}
