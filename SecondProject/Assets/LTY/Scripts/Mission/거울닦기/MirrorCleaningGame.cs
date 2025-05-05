using UnityEngine;
using UnityEngine.UI;

public class MirrorCleaningGame : MonoBehaviour, IMiniGame
{
    [SerializeField] private RawImage dirtImage;
    [SerializeField] private RawImage ragImage;
    [SerializeField] private float cleanRadius = 0.4f;
    [SerializeField] private float cleanSpeed = 0.1f;
    [SerializeField] private float cleanThreshold = 0.1f;

    private Texture2D dirtTexture;
    private Texture2D originalTexture;
    private bool isGameActive = false;
    private float totalAlpha;
    private float currentAlpha;
    private RectTransform dirtRect;
    private float cleanInterval = 0.2f;
    private float cleanTimer = 0f;
    private Interactable currentInteractable;
    private Camera mainCamera;
    private bool isInitialized = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        Debug.Log($"Awake: {gameObject.name} Ȱ��ȭ ���� - {gameObject.activeSelf}");
        Debug.Log($"Canvas �θ� ������Ʈ Ȱ��ȭ ����: {transform.root.gameObject.activeSelf}");
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        Debug.Log("Initialize �޼��� ���� ����.");

        if (dirtImage == null)
        {
            Debug.LogError("DirtImage�� �ν����Ϳ��� �������� �ʾҽ��ϴ�!");
            return;
        }
        if (dirtImage.texture == null)
        {
            Debug.LogError("DirtImage�� �ؽ�ó�� �������� �ʾҽ��ϴ�!");
            return;
        }

        Texture2D tempTexture = dirtImage.texture as Texture2D;
        if (tempTexture == null)
        {
            Debug.LogError("DirtImage �ؽ�ó�� Texture2D ������ �ƴմϴ�!");
            return;
        }
        if (!tempTexture.isReadable)
        {
            Debug.LogError("DirtImage �ؽ�ó�� Read/Write Enabled�� ���� �ֽ��ϴ�! �ؽ�ó ������ Ȯ���ϼ���.");
            return;
        }

        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log("OriginalTexture �ʱ�ȭ �Ϸ�.");
        isInitialized = true;
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
        {
            Debug.Log("�̴ϰ��� �̹� ���� ��!");
            return;
        }

        Initialize();

        if (originalTexture == null)
        {
            Debug.LogError("OriginalTexture�� �ʱ�ȭ���� �ʾҽ��ϴ�! ����Ƽ ���� Ȯ�� �ʿ�.");
            return;
        }

        currentInteractable = interactable;
        gameObject.SetActive(true);
        dirtRect = dirtImage.GetComponent<RectTransform>();

        if (dirtRect == null)
        {
            Debug.LogError("DirtImage�� RectTransform ������Ʈ�� �����ϴ�!");
            return;
        }

        Debug.Log($"StartMiniGame: {gameObject.name} Ȱ��ȭ - {gameObject.activeSelf}, DirtImage Ȱ��ȭ - {dirtImage.gameObject.activeSelf}");
        Debug.Log($"DirtImage ��ġ: {dirtRect.anchoredPosition}, ũ��: {dirtRect.sizeDelta}, ��Ŀ: {dirtRect.anchorMin}, {dirtRect.anchorMax}");
        Debug.Log($"DirtImage Texture: {dirtImage.texture != null}, Color: {dirtImage.color}");

        if (dirtImage.gameObject.activeSelf)
        {
            Debug.Log("DirtImage�� Ȱ��ȭ��.");
        }
        else
        {
            dirtImage.gameObject.SetActive(true);
            Debug.Log("DirtImage�� �������� Ȱ��ȭ��.");
        }

        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            Debug.Log($"RagImage ��ġ: {ragRect.anchoredPosition}, ũ��: {ragRect.sizeDelta}");
            Debug.Log($"RagImage Texture: {ragImage.texture != null}, Color: {ragImage.color}");
            ragImage.gameObject.SetActive(true);
        }

        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
        }

        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        dirtTexture.SetPixels(originalTexture.GetPixels());
        dirtTexture.Apply();
        dirtImage.texture = dirtTexture;
        Debug.Log($"DirtImage�� �� �ؽ�ó �����: {dirtImage.texture != null}");

        totalAlpha = 0f;
        foreach (var pixel in dirtTexture.GetPixels())
        {
            totalAlpha += pixel.a;
        }
        currentAlpha = totalAlpha;

        isGameActive = true;
        cleanTimer = 0f;
        Debug.Log("MirrorCleaningGame ����!");
    }

    public void CancelGame()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        gameObject.SetActive(false);
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
        dirtImage.texture = originalTexture;
        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
    }

    public void CompleteGame()
    {
        isGameActive = false;
        gameObject.SetActive(false);
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
        dirtImage.texture = originalTexture;
    }

    public bool IsActive => isGameActive;

    private void Update()
    {
        if (!isGameActive)
        {
            Debug.Log("���� ��Ȱ��ȭ ����: isGameActive == false");
            return;
        }

        Vector2 mousePos;
        bool inputSuccess = InputHandler.GetInputPosition(out mousePos, mainCamera, true, dirtRect);
        Debug.Log($"InputHandler ���: {inputSuccess}, ���콺 ��ġ: {mousePos}, Canvas Rect: {dirtRect.rect}, DirtRect Ȱ��ȭ: {dirtRect.gameObject.activeSelf}");

        if (!inputSuccess)
        {
            return;
        }

        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
                Debug.Log($"RagImage ��ġ ������Ʈ: {mousePos}, RagImage Ȱ��ȭ: {ragImage.gameObject.activeSelf}");
            }
            else
            {
                Debug.LogWarning("RagImage�� RectTransform ����");
            }
        }
        else
        {
            Debug.LogWarning("RagImage�� null��");
        }

        Rect rect = dirtRect.rect;
        Vector2 anchoredPos = dirtRect.anchoredPosition;
        Rect adjustedRect = new Rect(anchoredPos.x + rect.x, anchoredPos.y + rect.y, rect.width, rect.height);
        bool isMouseInRect = adjustedRect.Contains(mousePos);
        Debug.Log($"���콺 ���� üũ: {isMouseInRect}, ������ Rect: {adjustedRect}");

        cleanTimer += Time.deltaTime;
        bool isMouseButtonDown = Input.GetMouseButton(0);
        bool isTimerReady = cleanTimer >= cleanInterval;

        if (isMouseInRect && isMouseButtonDown && isTimerReady)
        {
            CleanDirt(mousePos);
            cleanTimer = 0f;
            Debug.Log("CleanDirt ����");
        }
    }

    private void CleanDirt(Vector2 localPos)
    {
        Rect rect = dirtRect.rect;
        Vector2 normalizedPos = new Vector2((localPos.x - rect.x) / rect.width, (localPos.y - rect.y) / rect.height);
        Vector2 uv = new Vector2(normalizedPos.x * dirtTexture.width, normalizedPos.y * dirtTexture.height);

        int radius = Mathf.FloorToInt(cleanRadius * dirtTexture.width * 0.5f);
        int xMin = Mathf.Clamp((int)uv.x - radius, 0, dirtTexture.width - 1);
        int xMax = Mathf.Clamp((int)uv.x + radius, 0, dirtTexture.width - 1);
        int yMin = Mathf.Clamp((int)uv.y - radius, 0, dirtTexture.height - 1);
        int yMax = Mathf.Clamp((int)uv.y + radius, 0, dirtTexture.height - 1);

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
            dirtTexture.SetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1, pixels);
            dirtTexture.Apply();
        }

        if (currentAlpha / totalAlpha <= cleanThreshold)
        {
            FindFirstObjectByType<MiniGameManager>().CompleteMiniGame("MirrorCleaning", currentInteractable);
        }
    }

    private void OnDestroy()
    {
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
        }
    }
}