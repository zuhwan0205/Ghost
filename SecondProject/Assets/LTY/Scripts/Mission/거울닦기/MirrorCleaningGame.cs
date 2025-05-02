using UnityEngine;
using UnityEngine.UI;

public class MirrorCleaningGame : MonoBehaviour
{
    [SerializeField] private RawImage dirtImage;
    [SerializeField] private RawImage ragImage;
    [SerializeField] private float cleanRadius = 0.4f;
    [SerializeField] private float cleanSpeed = 0.1f;
    [SerializeField] private float cleanThreshold = 0.1f;

    private Texture2D dirtTexture;
    private Texture2D originalTexture; // ���� �ؽ�ó ���纻 ����
    private bool isGameActive = false;
    private float totalAlpha;
    private float currentAlpha;
    private RectTransform dirtRect;
    private float cleanInterval = 0.2f;
    private float cleanTimer = 0f;
    private Interactable currentInteractable;

    void Awake()
    {
        // �ʱ� ��Ȱ��ȭ�� �����Ϳ��� �������� ��ü
        // gameObject.SetActive(false);
        // Debug.Log("MiniGameCanvas �ʱ� ��Ȱ��ȭ!");
    }

    void Start()
    {
        // ���� �ؽ�ó ���� �� ������ üũ
        if (dirtImage == null)
        {
            Debug.LogError("DirtImage�� �������� �ʾҽ��ϴ�! ����Ƽ �����Ϳ��� Inspector â�� Ȯ���ϼ���.");
            return;
        }

        if (dirtImage.texture == null)
        {
            Debug.LogError("DirtImage�� �ؽ�ó�� �Ҵ���� �ʾҽ��ϴ�! ����Ƽ �����Ϳ��� DirtImage�� RawImage ������Ʈ�� Ȯ���ϼ���.");
            return;
        }

        Texture2D tempTexture = dirtImage.texture as Texture2D;
        if (tempTexture == null)
        {
            Debug.LogError("DirtImage�� �ؽ�ó�� Texture2D ������ �ƴմϴ�! �ؽ�ó ������ Ȯ���ϼ���.");
            return;
        }

        if (!tempTexture.isReadable)
        {
            Debug.LogError($"�ؽ�ó '{tempTexture.name}'�� Read/Write Enabled�� ���� �ֽ��ϴ�! ����Ƽ �����Ϳ��� �ؽ�ó ������ Ȯ���ϼ���.");
            return;
        }

        // ���� �ؽ�ó ���纻 ���� (������ RGBA32�� ����)
        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log($"���� �ؽ�ó ���� �Ϸ�: �̸�={originalTexture.name}, ũ��={originalTexture.width}x{originalTexture.height}, Readable={originalTexture.isReadable}");
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
        {
            Debug.Log("�̴ϰ��� �̹� ���� ��!");
            return;
        }

        Debug.Log("StartMiniGame ����!");
        currentInteractable = interactable;

        // ĵ���� Ȱ��ȭ ���� �õ�
        Debug.Log("ĵ���� Ȱ��ȭ �õ�!");
        gameObject.SetActive(true);
        Debug.Log($"ĵ���� Ȱ��ȭ ����: {gameObject.activeSelf}");

        if (dirtImage == null || ragImage == null)
        {
            Debug.LogError("DirtImage �Ǵ� RagImage�� �������� �ʾҽ��ϴ�!");
            gameObject.SetActive(false);
            return;
        }
        Debug.Log("DirtImage�� RagImage ���� Ȯ�� �Ϸ�!");

        dirtRect = dirtImage.GetComponent<RectTransform>();
        if (dirtRect == null)
        {
            Debug.LogError("DirtImage�� RectTransform ������Ʈ�� �����ϴ�!");
            gameObject.SetActive(false);
            return;
        }
        Debug.Log($"Dirt Rect: {dirtRect.rect}");

        // originalTexture�� null�� ��� �缳�� �õ�
        if (originalTexture == null)
        {
            Texture2D tempTexture = dirtImage.texture as Texture2D;
            if (tempTexture == null)
            {
                Debug.LogError("DirtImage�� �ؽ�ó�� Texture2D ������ �ƴմϴ�! StartMiniGame���� �缳�� ����.");
                gameObject.SetActive(false);
                return;
            }

            if (!tempTexture.isReadable)
            {
                Debug.LogError($"�ؽ�ó '{tempTexture.name}'�� Read/Write Enabled�� ���� �ֽ��ϴ�! StartMiniGame���� �缳�� ����.");
                gameObject.SetActive(false);
                return;
            }

            originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
            originalTexture.SetPixels(tempTexture.GetPixels());
            originalTexture.Apply();
            Debug.Log($"StartMiniGame���� originalTexture �缳�� �Ϸ�: �̸�={originalTexture.name}, ũ��={originalTexture.width}x{originalTexture.height}");
        }
        Debug.Log("�ؽ�ó ���� Ȯ�� �Ϸ�!");

        // ���� dirtTexture�� ������ ����
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }

        // ���ο� �۾��� �ؽ�ó ����
        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = originalTexture.GetPixels();
        dirtTexture.SetPixels(pixels);
        dirtTexture.Apply();
        Debug.Log($"Dirt Texture ũ��: {dirtTexture.width}x{dirtTexture.height}");

        // dirtImage�� �ؽ�ó ����
        if (dirtImage != null)
        {
            dirtImage.texture = dirtTexture;
            Debug.Log("dirtImage�� dirtTexture ���� �Ϸ�!");
        }
        else
        {
            Debug.LogError("dirtImage�� null�Դϴ�!");
            gameObject.SetActive(false);
            return;
        }

        totalAlpha = 0f;
        foreach (var pixel in pixels)
        {
            totalAlpha += pixel.a;
        }
        currentAlpha = totalAlpha;
        Debug.Log($"Total Alpha: {totalAlpha}");

        if (totalAlpha <= 0)
        {
            Debug.LogError("�ؽ�ó�� ������ 0�Դϴ�! �ؽ�ó�� Ȯ���ϼ���.");
            gameObject.SetActive(false);
            return;
        }

        isGameActive = true;
        cleanTimer = 0f;
        Debug.Log($"isGameActive ����: {isGameActive}");
    }

    public void CancelGame()
    {
        if (!isGameActive)
        {
            Debug.Log("�̴ϰ����� �̹� ��Ȱ��ȭ �����Դϴ�!");
            return;
        }

        isGameActive = false;
        gameObject.SetActive(false);
        Debug.Log("�̴ϰ��� ��ҵ�!");

        // �۾��� �ؽ�ó ����
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
            Debug.Log("dirtTexture �ı� �Ϸ�!");
        }

        // ���� �ؽ�ó�� ����
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
            Debug.Log("dirtImage�� originalTexture ���� �Ϸ�!");
        }
        else
        {
            Debug.LogError("dirtImage �Ǵ� originalTexture�� null�Դϴ�! ���� ����.");
        }

        // ���� �ʱ�ȭ
        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
        Debug.Log("��� �� ���� �ʱ�ȭ �Ϸ�!");
    }

    void Update()
    {
        if (!isGameActive)
        {
            Debug.Log("�̴ϰ��� ��Ȱ��ȭ ����: Update ���� �� ��!");
            return;
        }

        // ĵ������ ��Ȱ��ȭ�Ǿ��ٸ� Ȱ��ȭ ����
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("ĵ������ ��Ȱ��ȭ �����Դϴ�! ������ Ȱ��ȭ�մϴ�.");
            gameObject.SetActive(true);
        }

        // ���콺 ��ġ�� ĵ���� ���� ��ǥ�� ��ȯ
        Vector2 mousePos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dirtRect, Input.mousePosition, null, out mousePos
        );
        if (!isConverted)
        {
            Debug.LogError("���콺 ��ǥ ��ȯ ����!");
            return;
        }
        Debug.Log($"���� ���콺 ��ġ: {mousePos}");

        // Rag �̹��� ��ġ ������Ʈ
        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
                Debug.Log($"Rag �̹��� ��ġ ������Ʈ: {mousePos}, RectTransform ��ġ: {ragRect.anchoredPosition}");
            }
            else
            {
                Debug.LogError("ragImage�� RectTransform�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError("ragImage�� null�Դϴ�!");
        }

        // Dirt ���� üũ
        Rect rect = dirtRect.rect;
        Vector2 anchoredPos = dirtRect.anchoredPosition;
        Rect adjustedRect = new Rect(
            anchoredPos.x + rect.x,
            anchoredPos.y + rect.y,
            rect.width,
            rect.height
        );
        bool isMouseInRect = adjustedRect.Contains(mousePos);
        Debug.Log($"Adjusted Rect: {adjustedRect}, Contains Mouse: {isMouseInRect}");

        cleanTimer += Time.deltaTime;
        Debug.Log($"CleanTimer: {cleanTimer}");

        bool isMouseButtonDown = Input.GetMouseButton(0);
        bool isTimerReady = cleanTimer >= cleanInterval;

        Debug.Log($"���콺 ���� ��: {isMouseInRect}, ���콺 Ŭ��: {isMouseButtonDown}, Ÿ�̸� �غ�: {isTimerReady}");

        if (isMouseInRect && isMouseButtonDown && isTimerReady)
        {
            Debug.Log("���� �۱� ����!");
            CleanDirt(mousePos);
            cleanTimer = 0f;
        }
    }

    void CleanDirt(Vector2 localPos)
    {
        // localPos�� Dirt �ؽ�ó�� UV ��ǥ�� ��ȯ
        Rect rect = dirtRect.rect;
        Vector2 normalizedPos = new Vector2(
            (localPos.x - rect.x) / rect.width,
            (localPos.y - rect.y) / rect.height
        );
        Debug.Log($"Normalized Pos: {normalizedPos}");

        Vector2 uv = new Vector2(
            normalizedPos.x * dirtTexture.width,
            normalizedPos.y * dirtTexture.height
        );
        Debug.Log($"UV ��ǥ: {uv}");

        // �۱� �ݰ� ���
        int radius = Mathf.FloorToInt(cleanRadius * dirtTexture.width * 0.5f);
        int xMin = Mathf.Clamp((int)uv.x - radius, 0, dirtTexture.width - 1);
        int xMax = Mathf.Clamp((int)uv.x + radius, 0, dirtTexture.width - 1);
        int yMin = Mathf.Clamp((int)uv.y - radius, 0, dirtTexture.height - 1);
        int yMax = Mathf.Clamp((int)uv.y + radius, 0, dirtTexture.height - 1);
        Debug.Log($"����: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}");

        // �ȼ� ��� �� ���� ��������
        Color[] pixels = dirtTexture.GetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
        bool textureChanged = false;
        float radiusSqr = radius * radius;

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

        if (textureChanged)
        {
            // ����� �ȼ� ��ϸ� ������Ʈ
            dirtTexture.SetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1, pixels);
            dirtTexture.Apply();
            Debug.Log("�ؽ�ó ���� �� ���� �Ϸ�!");
        }
        else
        {
            Debug.Log("�ؽ�ó ���� ����!");
        }

        Debug.Log($"���� ���� ����: {currentAlpha / totalAlpha}");
        if (currentAlpha / totalAlpha <= cleanThreshold)
        {
            CompleteGame();
        }
    }

    void CompleteGame()
    {
        isGameActive = false;
        gameObject.SetActive(false);
        Debug.Log("���� Ŭ����! �ſ��� �����������ϴ�.");

        if (currentInteractable != null)
        {
            currentInteractable.OnMiniGameCompleted();
        }

        // �Ϸ� �ÿ��� �ؽ�ó ����
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
            Debug.Log("dirtTexture �ı� �Ϸ�!");
        }
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
            Debug.Log("dirtImage�� originalTexture ���� �Ϸ�!");
        }
        else
        {
            Debug.LogError("dirtImage �Ǵ� originalTexture�� null�Դϴ�! ���� ����.");
        }
    }

    void OnDestroy()
    {
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
    }
}