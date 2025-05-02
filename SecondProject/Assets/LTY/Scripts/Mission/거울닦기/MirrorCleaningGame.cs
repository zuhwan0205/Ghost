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
    private Texture2D originalTexture; // 원본 텍스처 복사본 저장
    private bool isGameActive = false;
    private float totalAlpha;
    private float currentAlpha;
    private RectTransform dirtRect;
    private float cleanInterval = 0.2f;
    private float cleanTimer = 0f;
    private Interactable currentInteractable;

    void Awake()
    {
        // 초기 비활성화는 에디터에서 설정으로 대체
        // gameObject.SetActive(false);
        // Debug.Log("MiniGameCanvas 초기 비활성화!");
    }

    void Start()
    {
        // 원본 텍스처 저장 및 안전성 체크
        if (dirtImage == null)
        {
            Debug.LogError("DirtImage가 설정되지 않았습니다! 유니티 에디터에서 Inspector 창을 확인하세요.");
            return;
        }

        if (dirtImage.texture == null)
        {
            Debug.LogError("DirtImage에 텍스처가 할당되지 않았습니다! 유니티 에디터에서 DirtImage의 RawImage 컴포넌트를 확인하세요.");
            return;
        }

        Texture2D tempTexture = dirtImage.texture as Texture2D;
        if (tempTexture == null)
        {
            Debug.LogError("DirtImage의 텍스처가 Texture2D 형식이 아닙니다! 텍스처 형식을 확인하세요.");
            return;
        }

        if (!tempTexture.isReadable)
        {
            Debug.LogError($"텍스처 '{tempTexture.name}'의 Read/Write Enabled가 꺼져 있습니다! 유니티 에디터에서 텍스처 설정을 확인하세요.");
            return;
        }

        // 원본 텍스처 복사본 생성 (포맷을 RGBA32로 고정)
        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log($"원본 텍스처 복사 완료: 이름={originalTexture.name}, 크기={originalTexture.width}x{originalTexture.height}, Readable={originalTexture.isReadable}");
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
        {
            Debug.Log("미니게임 이미 진행 중!");
            return;
        }

        Debug.Log("StartMiniGame 시작!");
        currentInteractable = interactable;

        // 캔버스 활성화 먼저 시도
        Debug.Log("캔버스 활성화 시도!");
        gameObject.SetActive(true);
        Debug.Log($"캔버스 활성화 상태: {gameObject.activeSelf}");

        if (dirtImage == null || ragImage == null)
        {
            Debug.LogError("DirtImage 또는 RagImage가 설정되지 않았습니다!");
            gameObject.SetActive(false);
            return;
        }
        Debug.Log("DirtImage와 RagImage 설정 확인 완료!");

        dirtRect = dirtImage.GetComponent<RectTransform>();
        if (dirtRect == null)
        {
            Debug.LogError("DirtImage에 RectTransform 컴포넌트가 없습니다!");
            gameObject.SetActive(false);
            return;
        }
        Debug.Log($"Dirt Rect: {dirtRect.rect}");

        // originalTexture가 null일 경우 재설정 시도
        if (originalTexture == null)
        {
            Texture2D tempTexture = dirtImage.texture as Texture2D;
            if (tempTexture == null)
            {
                Debug.LogError("DirtImage의 텍스처가 Texture2D 형식이 아닙니다! StartMiniGame에서 재설정 실패.");
                gameObject.SetActive(false);
                return;
            }

            if (!tempTexture.isReadable)
            {
                Debug.LogError($"텍스처 '{tempTexture.name}'의 Read/Write Enabled가 꺼져 있습니다! StartMiniGame에서 재설정 실패.");
                gameObject.SetActive(false);
                return;
            }

            originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
            originalTexture.SetPixels(tempTexture.GetPixels());
            originalTexture.Apply();
            Debug.Log($"StartMiniGame에서 originalTexture 재설정 완료: 이름={originalTexture.name}, 크기={originalTexture.width}x{originalTexture.height}");
        }
        Debug.Log("텍스처 설정 확인 완료!");

        // 기존 dirtTexture가 있으면 정리
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }

        // 새로운 작업용 텍스처 생성
        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = originalTexture.GetPixels();
        dirtTexture.SetPixels(pixels);
        dirtTexture.Apply();
        Debug.Log($"Dirt Texture 크기: {dirtTexture.width}x{dirtTexture.height}");

        // dirtImage에 텍스처 설정
        if (dirtImage != null)
        {
            dirtImage.texture = dirtTexture;
            Debug.Log("dirtImage에 dirtTexture 설정 완료!");
        }
        else
        {
            Debug.LogError("dirtImage가 null입니다!");
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
            Debug.LogError("텍스처의 투명도가 0입니다! 텍스처를 확인하세요.");
            gameObject.SetActive(false);
            return;
        }

        isGameActive = true;
        cleanTimer = 0f;
        Debug.Log($"isGameActive 상태: {isGameActive}");
    }

    public void CancelGame()
    {
        if (!isGameActive)
        {
            Debug.Log("미니게임이 이미 비활성화 상태입니다!");
            return;
        }

        isGameActive = false;
        gameObject.SetActive(false);
        Debug.Log("미니게임 취소됨!");

        // 작업용 텍스처 정리
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
            Debug.Log("dirtTexture 파괴 완료!");
        }

        // 원본 텍스처로 복구
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
            Debug.Log("dirtImage에 originalTexture 복구 완료!");
        }
        else
        {
            Debug.LogError("dirtImage 또는 originalTexture가 null입니다! 복구 실패.");
        }

        // 상태 초기화
        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
        Debug.Log("취소 후 상태 초기화 완료!");
    }

    void Update()
    {
        if (!isGameActive)
        {
            Debug.Log("미니게임 비활성화 상태: Update 실행 안 됨!");
            return;
        }

        // 캔버스가 비활성화되었다면 활성화 보장
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning("캔버스가 비활성화 상태입니다! 강제로 활성화합니다.");
            gameObject.SetActive(true);
        }

        // 마우스 위치를 캔버스 로컬 좌표로 변환
        Vector2 mousePos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dirtRect, Input.mousePosition, null, out mousePos
        );
        if (!isConverted)
        {
            Debug.LogError("마우스 좌표 변환 실패!");
            return;
        }
        Debug.Log($"로컬 마우스 위치: {mousePos}");

        // Rag 이미지 위치 업데이트
        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
                Debug.Log($"Rag 이미지 위치 업데이트: {mousePos}, RectTransform 위치: {ragRect.anchoredPosition}");
            }
            else
            {
                Debug.LogError("ragImage에 RectTransform이 없습니다!");
            }
        }
        else
        {
            Debug.LogError("ragImage가 null입니다!");
        }

        // Dirt 영역 체크
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

        Debug.Log($"마우스 영역 안: {isMouseInRect}, 마우스 클릭: {isMouseButtonDown}, 타이머 준비: {isTimerReady}");

        if (isMouseInRect && isMouseButtonDown && isTimerReady)
        {
            Debug.Log("먼지 닦기 시작!");
            CleanDirt(mousePos);
            cleanTimer = 0f;
        }
    }

    void CleanDirt(Vector2 localPos)
    {
        // localPos를 Dirt 텍스처의 UV 좌표로 변환
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
        Debug.Log($"UV 좌표: {uv}");

        // 닦기 반경 계산
        int radius = Mathf.FloorToInt(cleanRadius * dirtTexture.width * 0.5f);
        int xMin = Mathf.Clamp((int)uv.x - radius, 0, dirtTexture.width - 1);
        int xMax = Mathf.Clamp((int)uv.x + radius, 0, dirtTexture.width - 1);
        int yMin = Mathf.Clamp((int)uv.y - radius, 0, dirtTexture.height - 1);
        int yMax = Mathf.Clamp((int)uv.y + radius, 0, dirtTexture.height - 1);
        Debug.Log($"영역: xMin={xMin}, xMax={xMax}, yMin={yMin}, yMax={yMax}");

        // 픽셀 블록 한 번에 가져오기
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
            // 변경된 픽셀 블록만 업데이트
            dirtTexture.SetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1, pixels);
            dirtTexture.Apply();
            Debug.Log("텍스처 변경 및 적용 완료!");
        }
        else
        {
            Debug.Log("텍스처 변경 없음!");
        }

        Debug.Log($"현재 알파 비율: {currentAlpha / totalAlpha}");
        if (currentAlpha / totalAlpha <= cleanThreshold)
        {
            CompleteGame();
        }
    }

    void CompleteGame()
    {
        isGameActive = false;
        gameObject.SetActive(false);
        Debug.Log("게임 클리어! 거울이 깨끗해졌습니다.");

        if (currentInteractable != null)
        {
            currentInteractable.OnMiniGameCompleted();
        }

        // 완료 시에도 텍스처 정리
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
            Debug.Log("dirtTexture 파괴 완료!");
        }
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
            Debug.Log("dirtImage에 originalTexture 복구 완료!");
        }
        else
        {
            Debug.LogError("dirtImage 또는 originalTexture가 null입니다! 복구 실패.");
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