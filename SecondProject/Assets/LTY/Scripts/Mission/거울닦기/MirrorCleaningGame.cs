using UnityEngine;
using UnityEngine.UI;

public class MirrorCleaningGame : MonoBehaviour
{
    // Inspector에서 설정할 UI 요소와 게임 설정값
    [SerializeField] private RawImage dirtImage; // 더러운 거울 이미지
    [SerializeField] private RawImage ragImage; // 걸레 이미지
    [SerializeField] private float cleanRadius = 0.4f; // 닦기 반경 (텍스처 기준)
    [SerializeField] private float cleanSpeed = 0.1f; // 한 번 닦을 때 투명도 감소량
    [SerializeField] private float cleanThreshold = 0.1f; // 클리어 기준 (투명도 비율)

    private Texture2D dirtTexture; // 작업용 텍스처 (닦기 처리)
    private Texture2D originalTexture; // 원본 텍스처 (복구용)
    private bool isGameActive = false; // 미니게임 활성화 여부
    private float totalAlpha; // 텍스처의 초기 투명도 합계
    private float currentAlpha; // 현재 투명도 합계
    private RectTransform dirtRect; // dirtImage의 RectTransform
    private float cleanInterval = 0.05f; // 닦기 간격 (초)
    private float cleanTimer = 0f; // 닦기 타이머
    private Interactable currentInteractable; // 미니게임을 호출한 객체

    // 초기화: 텍스처 설정 및 유효성 검사
    void Start()
    {
        // dirtImage와 텍스처가 제대로 설정되었는지 확인
        if (dirtImage == null || dirtImage.texture == null)
        {
            Debug.LogError("DirtImage 또는 텍스처가 설정되지 않았습니다!");
            return;
        }

        Texture2D tempTexture = dirtImage.texture as Texture2D;
        if (tempTexture == null || !tempTexture.isReadable)
        {
            Debug.LogError("DirtImage 텍스처가 Texture2D 형식이 아니거나 Read/Write Enabled가 꺼져 있습니다!");
            return;
        }

        // 원본 텍스처 복사 (게임 재시작 시 복구용)
        originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
        originalTexture.SetPixels(tempTexture.GetPixels());
        originalTexture.Apply();
        Debug.Log($"원본 텍스처 초기화: {originalTexture.width}x{originalTexture.height}");
    }

    // 미니게임 시작: 캔버스 활성화 및 텍스처 준비
    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive) return;

        currentInteractable = interactable;
        gameObject.SetActive(true);

        // 필수 컴포넌트 확인
        if (dirtImage == null || ragImage == null || dirtImage.GetComponent<RectTransform>() == null)
        {
            Debug.LogError("DirtImage, RagImage 또는 RectTransform이 설정되지 않았습니다!");
            gameObject.SetActive(false);
            return;
        }

        dirtRect = dirtImage.GetComponent<RectTransform>();

        // 원본 텍스처가 없으면 재설정
        if (originalTexture == null)
        {
            Texture2D tempTexture = dirtImage.texture as Texture2D;
            if (tempTexture == null || !tempTexture.isReadable)
            {
                Debug.LogError("OriginalTexture 재설정 실패!");
                gameObject.SetActive(false);
                return;
            }
            originalTexture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGBA32, false);
            originalTexture.SetPixels(tempTexture.GetPixels());
            originalTexture.Apply();
        }

        // 기존 작업용 텍스처 정리
        if (dirtTexture != null) Destroy(dirtTexture);

        // 작업용 텍스처 생성
        dirtTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        dirtTexture.filterMode = FilterMode.Bilinear;
        dirtTexture.wrapMode = TextureWrapMode.Clamp;
        dirtTexture.SetPixels(originalTexture.GetPixels());
        dirtTexture.Apply();
        dirtImage.texture = dirtTexture;

        // 초기 투명도 계산
        totalAlpha = 0f;
        foreach (var pixel in originalTexture.GetPixels())
        {
            totalAlpha += pixel.a;
        }
        currentAlpha = totalAlpha;

        if (totalAlpha <= 0)
        {
            Debug.LogError("텍스처의 투명도가 0입니다!");
            gameObject.SetActive(false);
            return;
        }

        isGameActive = true;
        cleanTimer = 0f;
        Debug.Log("미니게임 시작!");
    }

    // 미니게임 취소: 상태 초기화 및 캔버스 비활성화
    public void CancelGame()
    {
        if (!isGameActive) return;

        isGameActive = false;
        gameObject.SetActive(false);

        // 작업용 텍스처 정리
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }

        // 원본 텍스처 복구
        if (dirtImage != null && originalTexture != null)
        {
            dirtImage.texture = originalTexture;
        }

        currentInteractable = null;
        cleanTimer = 0f;
        totalAlpha = 0f;
        currentAlpha = 0f;
        Debug.Log("미니게임 취소됨!");
    }

    // 매 프레임 처리: 입력 감지 및 닦기 로직
    void Update()
    {
        if (!isGameActive) return;

        // 캔버스가 비활성화된 경우 강제 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // 마우스 좌클릭 이외의 입력 감지 (키보드, 마우스 우클릭/중간 클릭)
        bool isKeyboardInput = Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2);
        bool isInvalidMouseInput = Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);

        if (isKeyboardInput || isInvalidMouseInput)
        {
            Debug.Log("마우스 좌클릭 이외의 입력 감지! 미니게임 종료.");
            CancelGame();
            return;
        }

        // 마우스 위치를 캔버스 로컬 좌표로 변환
        Vector2 mousePos;
        bool isConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dirtRect, Input.mousePosition, null, out mousePos
        );
        if (!isConverted) return;

        // 걸레 이미지 위치 업데이트
        if (ragImage != null)
        {
            var ragRect = ragImage.GetComponent<RectTransform>();
            if (ragRect != null)
            {
                ragRect.anchoredPosition = mousePos;
            }
        }

        // 마우스가 Dirt 이미지 영역 내에 있는지 확인
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

        // 마우스 좌클릭 + 영역 내 + 타이머 조건 충족 시 닦기
        if (isMouseInRect && Input.GetMouseButton(0) && cleanTimer >= cleanInterval)
        {
            CleanDirt(mousePos);
            cleanTimer = 0f;
        }
    }

    // 거울 닦기: 마우스 위치에서 텍스처 투명도 감소
    void CleanDirt(Vector2 localPos)
    {
        // 로컬 좌표를 텍스처 UV 좌표로 변환
        Rect rect = dirtRect.rect;
        Vector2 normalizedPos = new Vector2(
            (localPos.x - rect.x) / rect.width,
            (localPos.y - rect.y) / rect.height
        );

        Vector2 uv = new Vector2(
            normalizedPos.x * dirtTexture.width,
            normalizedPos.y * dirtTexture.height
        );

        // 닦기 반경 계산
        int radius = Mathf.FloorToInt(cleanRadius * dirtTexture.width * 0.5f);
        int xMin = Mathf.Clamp((int)uv.x - radius, 0, dirtTexture.width - 1);
        int xMax = Mathf.Clamp((int)uv.x + radius, 0, dirtTexture.width - 1);
        int yMin = Mathf.Clamp((int)uv.y - radius, 0, dirtTexture.height - 1);
        int yMax = Mathf.Clamp((int)uv.y + radius, 0, dirtTexture.height - 1);

        // 픽셀 블록 가져오기
        Color[] pixels = dirtTexture.GetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1);
        bool textureChanged = false;
        float radiusSqr = radius * radius;

        // 반경 내 픽셀의 투명도 감소
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

        // 텍스처 업데이트
        if (textureChanged)
        {
            dirtTexture.SetPixels(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1, pixels);
            dirtTexture.Apply();
        }

        // 클리어 조건 체크
        if (currentAlpha / totalAlpha <= cleanThreshold)
        {
            CompleteGame();
        }
    }

    // 미니게임 완료: 상태 정리 및 Interactable 콜백 호출
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
        Debug.Log("미니게임 완료: 거울 깨끗!");
    }

    // 스크립트 종료 시 텍스처 정리
    void OnDestroy()
    {
        if (dirtTexture != null)
        {
            Destroy(dirtTexture);
            dirtTexture = null;
        }
    }
}