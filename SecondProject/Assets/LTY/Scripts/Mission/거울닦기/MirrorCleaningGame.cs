using UnityEngine;
using UnityEngine.UI;

public class MirrorCleaningGame : MonoBehaviour
{
    // Inspector���� ������ UI ��ҿ� ���� ������
    [SerializeField] private RawImage dirtImage; // ������ �ſ� �̹���
    [SerializeField] private RawImage ragImage; // �ɷ� �̹���
    [SerializeField] private float cleanRadius = 0.4f; // �۱� �ݰ� (�ؽ�ó ����)
    [SerializeField] private float cleanSpeed = 0.1f; // �� �� ���� �� ���� ���ҷ�
    [SerializeField] private float cleanThreshold = 0.1f; // Ŭ���� ���� (���� ����)

    private Texture2D dirtTexture; // �۾��� �ؽ�ó (�۱� ó��)
    private Texture2D originalTexture; // ���� �ؽ�ó (������)
    private bool isGameActive = false; // �̴ϰ��� Ȱ��ȭ ����
    private float totalAlpha; // �ؽ�ó�� �ʱ� ���� �հ�
    private float currentAlpha; // ���� ���� �հ�
    private RectTransform dirtRect; // dirtImage�� RectTransform
    private float cleanInterval = 0.05f; // �۱� ���� (��)
    private float cleanTimer = 0f; // �۱� Ÿ�̸�
    private Interactable currentInteractable; // �̴ϰ����� ȣ���� ��ü

    // �ʱ�ȭ: �ؽ�ó ���� �� ��ȿ�� �˻�
    void Start()
    {
        // dirtImage�� �ؽ�ó�� ����� �����Ǿ����� Ȯ��
        if (dirtImage == null || dirtImage.texture == null)
        {
            Debug.LogError("DirtImage �Ǵ� �ؽ�ó�� �������� �ʾҽ��ϴ�!");
            return;
        }

        Texture2D tempTexture = dirtImage.texture as Texture2D;
        if (tempTexture == null || !tempTexture.isReadable)
        {
            Debug.LogError("DirtImage �ؽ�ó�� Texture2D ������ �ƴϰų� Read/Write Enabled�� ���� �ֽ��ϴ�!");
            return;
        }

        // ���� �ؽ�ó ���� (���� ����� �� ������)
        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log($"���� �ؽ�ó �ʱ�ȭ: {originalTexture.width}x{originalTexture.height}");
    }

    // �̴ϰ��� ����: ĵ���� Ȱ��ȭ �� �ؽ�ó �غ�
    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive) return;

        currentInteractable = interactable;
        gameObject.SetActive(true);

        // �ʼ� ������Ʈ Ȯ��
        if (dirtImage == null || ragImage == null || dirtImage.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("DirtImage, RagImage �Ǵ� RectTransform�� �������� �ʾҽ��ϴ�!");
            gameObject.SetActive(false);
            return;
        }

        dirtRect = dirtImage.GetComponent<RectTransform>();

        // ���� �ؽ�ó�� ������ �缳��
        if (originalTexture == null)
        {
            Texture2D tempTexture = dirtImage.texture as Texture2D;
            if (tempTexture == null || !tempTexture.isReadable)
            {
                Debug.LogError("OriginalTexture �缳�� ����!");
                gameObject.SetActive(false);
                return;
            }
            originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
            originalTexture.SetPixels(tempTexture.GetPixels());
            originalTexture.Apply();
        }

        // ���� �۾��� �ؽ�ó ����
        if (dirtTexture != null) Destroy(dirtTexture);

        // �۾��� �ؽ�ó ����
        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        dirtTexture.SetPixels(originalTexture.GetPixels());
        dirtTexture.Apply();
        dirtImage.texture = dirtTexture;

        // �ʱ� ���� ���
        totalAlpha = 0f;
        foreach (var pixel in originalTexture.GetPixels())
        {
            totalAlpha += pixel.a;
        }
        currentAlpha = totalAlpha;

        if (totalAlpha <= 0)
        {
            Debug.LogError("�ؽ�ó�� ������ 0�Դϴ�!");
            gameObject.SetActive(false);
            return;
        }

        isGameActive = true;
        cleanTimer = 0f;
        Debug.Log("�̴ϰ��� ����!");
    }

    // �̴ϰ��� ���: ���� �ʱ�ȭ �� ĵ���� ��Ȱ��ȭ
    public void CancelGame()
    {
        if (!isGameActive) return;

        isGameActive = false;
        gameObject.SetActive(false);

        // �۾��� �ؽ�ó ����
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }

        // ���� �ؽ�ó ����
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
        }

        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
        Debug.Log("�̴ϰ��� ��ҵ�!");
    }

    // �� ������ ó��: �Է� ���� �� �۱� ����
    void Update()
    {
        if (!isGameActive) return;

        // ĵ������ ��Ȱ��ȭ�� ��� ���� Ȱ��ȭ
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // ���콺 ��Ŭ�� �̿��� �Է� ���� (Ű����, ���콺 ��Ŭ��/�߰� Ŭ��)
        bool isKeyboardInput = Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2);
        bool isInvalidMouseInput = Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

        if (isKeyboardInput || isInvalidMouseInput)
        {
            Debug.Log("���콺 ��Ŭ�� �̿��� �Է� ����! �̴ϰ��� ����.");
            CancelGame();
            return;
        }

        // ���콺 ��ġ�� ĵ���� ���� ��ǥ�� ��ȯ
        Vector2 mousePos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dirtRect, Input.mousePosition, null, out mousePos
        );
        if (!isConverted) return;

        // �ɷ� �̹��� ��ġ ������Ʈ
        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
            }
        }

        // ���콺�� Dirt �̹��� ���� ���� �ִ��� Ȯ��
        Rect rect = dirtRect.rect;
        Vector2 anchoredPos = dirtRect.anchoredPosition;
        Rect adjustedRect = new Rect(
            anchoredPos.x + rect.x,
            anchoredPos.y + rect.y,
            rect.width,
            rect.height
        );
        bool isMouseInRect = adjustedRect.Contains(mousePos);

        cleanTimer += Time.deltaTime;

        // ���콺 ��Ŭ�� + ���� �� + Ÿ�̸� ���� ���� �� �۱�
        if (isMouseInRect && Input.GetMouseButton(0) && cleanTimer >= cleanInterval)
        {
            CleanDirt(mousePos);
            cleanTimer = 0f;
        }
    }

    // �ſ� �۱�: ���콺 ��ġ���� �ؽ�ó ���� ����
    void CleanDirt(Vector2 localPos)
    {
        // ���� ��ǥ�� �ؽ�ó UV ��ǥ�� ��ȯ
        Rect rect = dirtRect.rect;
        Vector2 normalizedPos = new Vector2(
            (localPos.x - rect.x) / rect.width,
            (localPos.y - rect.y) / rect.height
        );

        Vector2 uv = new Vector2(
            normalizedPos.x * dirtTexture.width,
            normalizedPos.y * dirtTexture.height
        );

        // �۱� �ݰ� ���
        int radius = Mathf.FloorToInt(cleanRadius * dirtTexture.width * 0.5f);
        int xMin = Mathf.Clamp((int)uv.x - radius, 0, dirtTexture.width - 1);
        int xMax = Mathf.Clamp((int)uv.x + radius, 0, dirtTexture.width - 1);
        int yMin = Mathf.Clamp((int)uv.y - radius, 0, dirtTexture.height - 1);
        int yMax = Mathf.Clamp((int)uv.y + radius, 0, dirtTexture.height - 1);

        // �ȼ� ��� ��������
        Color[] pixels = dirtTexture.GetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
        bool textureChanged = false;
        float radiusSqr = radius * radius;

        // �ݰ� �� �ȼ��� ���� ����
        for (int y = 0; y < yMax - yMin + 1; y++)
        {
            for (int x = 0; x < xMax - xMin + 1; x++)
            {
                int globalX = xMin + x;
                int globalY = yMin + y;
                float dx = globalX - uv.x;
                float dy = globalY - uv.y;
                float distSqr = dx * dx + dy * dy;
                if (distSqr > radiusSqr) continue;

                int index = y * (xMax - xMin + 1) + x;
                Color pixel = pixels[index];
                if (pixel.a > 0f)
                {
                    float newAlpha = Mathf.Max(0f, pixel.a - cleanSpeed);
                    currentAlpha -= (pixel.a - newAlpha);
                    pixel.a = newAlpha;
                    pixels[index] = pixel;
                    textureChanged = true;
                }
            }
        }

        // �ؽ�ó ������Ʈ
        if (textureChanged)
        {
            dirtTexture.SetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1, pixels);
            dirtTexture.Apply();
        }

        // Ŭ���� ���� üũ
        if (currentAlpha / totalAlpha <= cleanThreshold)
        {
            CompleteGame();
        }
    }

    // �̴ϰ��� �Ϸ�: ���� ���� �� Interactable �ݹ� ȣ��
    void CompleteGame()
    {
        isGameActive = false;
        gameObject.SetActive(false);

        if (currentInteractable != null)
        {
            currentInteractable.OnMiniGameCompleted();
        }

        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
        }
        Debug.Log("�̴ϰ��� �Ϸ�: �ſ� ����!");
    }

    // ��ũ��Ʈ ���� �� �ؽ�ó ����
    void OnDestroy()
    {
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
    }
}