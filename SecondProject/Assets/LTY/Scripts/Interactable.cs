using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isHoldInteraction = false;
    public Item item;
    public Coin coin;
    public bool isVendingMachine = false;
    public bool isDirtyMirror = false;
    public bool isWireBox = false;
    public GameObject cleanMirror;
    public GameObject closedWireBox; // ���� ����ġ ���� ������Ʈ
    [SerializeField] private MirrorCleaningGame cleaningGame;
    [SerializeField] private WiringGameManager wiringGameManager;
    private bool isMiniGameCompleted = false; // �̴ϰ��� �Ϸ� ����

    void Start()
    {
        // �ſ� û�� �̴ϰ��� �ʱ�ȭ
        if (isDirtyMirror && cleaningGame == null)
        {
            cleaningGame = FindFirstObjectByType<MirrorCleaningGame>();
            if (cleaningGame == null)
            {
                Debug.LogWarning("MirrorCleaningGame ��ũ��Ʈ�� Scene���� ã�� �� �����ϴ�!");
            }
        }

        // ���� ���� �̴ϰ��� �ʱ�ȭ
        if (isWireBox && wiringGameManager == null)
        {
            wiringGameManager = FindFirstObjectByType<WiringGameManager>();
            if (wiringGameManager == null)
            {
                Debug.LogWarning("WiringGameManager ��ũ��Ʈ�� Scene���� ã�� �� �����ϴ�!");
            }
        }

        // ���� ����ġ ���� �ʱ� ��Ȱ��ȭ
        if (isWireBox && closedWireBox != null)
        {
            closedWireBox.SetActive(false);
        }
    }

    public virtual void Interact()
    {
        // �̴ϰ��� �Ϸ� �� ���ȣ�ۿ� ����
        if ((isDirtyMirror || isWireBox) && isMiniGameCompleted)
        {
            Debug.Log("�̴ϰ��� �̹� �Ϸ��: �߰� ��ȣ�ۿ� �Ұ�!");
            return;
        }

        Player player = Object.FindFirstObjectByType<Player>();

        // ���� ���ڿ� ��ȣ�ۿ�
        if (isWireBox)
        {
            Debug.Log("���� ���� ��ȣ�ۿ� ����!");
            if (wiringGameManager != null)
            {
                Debug.Log("���� ���� �̴ϰ��� ���� ȣ��!");
                wiringGameManager.StartMiniGame(this);
            }
            else
            {
                Debug.LogError("WiringGameManager ��ũ��Ʈ�� �������� �ʾҽ��ϴ�!");
            }
            return;
        }

        // ������ �ſ�� ��ȣ�ۿ�
        if (isDirtyMirror)
        {
            Debug.Log("������ �ſ� ��ȣ�ۿ� ����!");
            if (cleaningGame != null)
            {
                Debug.Log("�̴ϰ��� ���� ȣ��!");
                cleaningGame.StartMiniGame(this);
            }
            else
            {
                Debug.LogError("MirrorCleaningGame ��ũ��Ʈ�� �������� �ʾҽ��ϴ�!");
            }
            return;
        }

        // ����, ������, ���Ǳ� ��ȣ�ۿ�
        if (coin != null)
        {
            coin.Collect(player);
        }
        else if (item != null)
        {
            if (player.AddItem(item))
            {
                Destroy(gameObject);
            }
        }
        else if (isVendingMachine)
        {
            if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
            {
                PhoneManager.Instance.TogglePhoneScreen();
                Debug.Log("���Ǳ� ��ȣ�ۿ�: �޴��� UI ����");
            }
            player.ShowVendingMachinePanel();
        }
    }

    public void OnMiniGameCompleted()
    {
        isMiniGameCompleted = true; // �Ϸ� ���·� ����

        if (isDirtyMirror)
        {
            Debug.Log("�̴ϰ��� �Ϸ�: �ſ� ��ü!");
            gameObject.SetActive(false);
            if (cleanMirror != null)
            {
                cleanMirror.SetActive(true);
            }
        }
        else if (isWireBox)
        {
            Debug.Log("���� ���� �̴ϰ��� �Ϸ�: ����ġ ���� ����!");
            gameObject.SetActive(false); // ���� ����ġ ���� ��Ȱ��ȭ
            if (closedWireBox != null)
            {
                closedWireBox.SetActive(true); // ���� ����ġ ���� Ȱ��ȭ
            }
            else
            {
                Debug.LogWarning("closedWireBox�� �������� �ʾҽ��ϴ�!");
            }
        }
    }
}