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
    private Texture2D originalTexture;
    private bool isGameActive = false;
    private float totalAlpha;
    private float currentAlpha;
    private RectTransform dirtRect;
    private float cleanInterval = 0.05f;
    private float cleanTimer = 0f;
    private Interactable currentInteractable;
    private PhoneManager phoneManager;

    void Awake()
    {
        phoneManager = FindFirstObjectByType<PhoneManager>(FindObjectsInactive.Include);
        if (phoneManager == null)
        {
            Debug.LogWarning("[MirrorCleaningGame] PhoneManager�� Awake���� ã�� �� �����ϴ�!");
        }
    }

    void Start()
    {
        if (phoneManager == null)
        {
            phoneManager = FindFirstObjectByType<PhoneManager>(FindObjectsInactive.Include);
            if (phoneManager == null)
            {
                Debug.LogWarning("[MirrorCleaningGame] PhoneManager�� Start������ ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.Log("[MirrorCleaningGame] PhoneManager�� Start���� ���������� ã��");
            }
        }

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

        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log($"���� �ؽ�ó �ʱ�ȭ: {originalTexture.width}x{originalTexture.height}");
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
        {
            Debug.Log("[MirrorCleaningGame] �̴ϰ��� �̹� ���� ��: ȣ�� ����");
            return;
        }

        currentInteractable = interactable;
        Debug.Log($"[MirrorCleaningGame] StartMiniGame ȣ���, interactable: {(interactable != null ? interactable.gameObject.name : "null")}");

        if (phoneManager == null)
        {
            phoneManager = FindFirstObjectByType<PhoneManager>(FindObjectsInactive.Include);
            if (phoneManager == null)
            {
                Debug.LogError("[MirrorCleaningGame] StartMiniGame���� PhoneManager�� ã�� �� �����ϴ�!");
            }
            else
            {
                Debug.Log("[MirrorCleaningGame] StartMiniGame���� PhoneManager�� ���������� ã��");
            }
        }

        if (phoneManager != null && phoneManager.IsPhoneOpen)
        {
            Debug.Log($"[MirrorCleaningGame] �޴��� UI Ȱ��ȭ ����: {phoneManager.IsPhoneOpen}, ��Ȱ��ȭ �õ�");
            phoneManager.TogglePhoneScreen();
            phoneManager.ForceClosePhoneScreen(); // ������ �޴��� UI �ݱ�
        }
        else
        {
            Debug.Log($"[MirrorCleaningGame] PhoneManager ����: {(phoneManager == null ? "null" : "����, �޴��� UI ��Ȱ��ȭ��")}");
        }

        gameObject.SetActive(true);

        if (dirtImage == null || ragImage == null || dirtImage.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("DirtImage, RagImage �Ǵ� RectTransform�� �������� �ʾҽ��ϴ�!");
            gameObject.SetActive(false);
            return;
        }

        dirtRect = dirtImage.GetComponent<RectTransform>();

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

        if (dirtTexture != null) Destroy(dirtTexture);

        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        dirtTexture.SetPixels(originalTexture.GetPixels());
        dirtTexture.Apply();
        dirtImage.texture = dirtTexture;

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
        Debug.Log("[MirrorCleaningGame] �̴ϰ��� ����!");
    }

    public void CancelGame()
    {
        if (!isGameActive)
        {
            Debug.Log("[MirrorCleaningGame] �̴ϰ��� �̹� ��Ȱ��ȭ ����!");
            return;
        }

        isGameActive = false;
        gameObject.SetActive(false);

        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }

        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
        }

        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
        Debug.Log("[MirrorCleaningGame] �̴ϰ��� ��ҵ�!");
    }

    void Update()
    {
        if (!isGameActive) return;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.LogWarning("[MirrorCleaningGame] ĵ������ ��Ȱ��ȭ ���¿���, ���� Ȱ��ȭ");
        }

        bool isKeyboardInput = Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2);
        bool isInvalidMouseInput = Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

        if (isKeyboardInput || isInvalidMouseInput)
        {
            Debug.Log("[MirrorCleaningGame] ���콺 ��Ŭ�� �̿��� �Է� ����! �̴ϰ��� ����.");
            CancelGame();
            return;
        }

        Vector2 mousePos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dirtRect, Input.mousePosition, null, out mousePos
        );
        if (!isConverted) return;

        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
            }
        }

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

        if (isMouseInRect && Input.GetMouseButton(0) && cleanTimer >= cleanInterval)
        {
            CleanDirt(mousePos);
            cleanTimer = 0f;
        }
    }

    void CleanDirt(Vector2 localPos)
    {
        Rect rect = dirtRect.rect;
        Vector2 normalizedPos = new Vector2(
            (localPos.x - rect.x) / rect.width,
            (localPos.y - rect.y) / rect.height
        );

        Vector2 uv = new Vector2(
            normalizedPos.x * dirtTexture.width,
            normalizedPos.y * dirtTexture.height
        );

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
            CompleteGame();
        }
    }

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
        Debug.Log("[MirrorCleaningGame] �̴ϰ��� �Ϸ�: �ſ� ����!");
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