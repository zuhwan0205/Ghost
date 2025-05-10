using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraZoomController : MonoBehaviour
{
    [Header("카메라 설정")]
    public CinemachineCamera virtualCamera;          // Unity 6 기준 클래스
    public float zoomInSize = 3.5f;                  // 줌인할 사이즈
    public float zoomDuration = 0.5f;                // 줌인/아웃 전환 시간
    public float zoomHoldTime = 2f;                  // 줌인 유지 시간

    private float originalSize;                      // 원래 카메라 사이즈 저장
    private Coroutine zoomRoutine;

    private void Start()
    {
        if (virtualCamera == null)
            virtualCamera = Object.FindAnyObjectByType<CinemachineCamera>();

        if (virtualCamera != null)
            originalSize = virtualCamera.Lens.OrthographicSize; // 초기 크기 저장
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 줌인-줌아웃 함수
    /// </summary>
    public void ZoomInThenOut()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomSequence());
    }

    /// <summary>
    /// 줌인 → 유지 → 줌아웃 순서로 실행되는 코루틴
    /// </summary>
    private IEnumerator ZoomSequence()
    {
        var lens = virtualCamera.Lens;

        // 줌인
        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float size = Mathf.Lerp(originalSize, zoomInSize, t / zoomDuration);

            lens.OrthographicSize = size;
            virtualCamera.Lens = lens;

            yield return null;
        }

        yield return new WaitForSeconds(zoomHoldTime);

        // 줌아웃
        t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float size = Mathf.Lerp(zoomInSize, originalSize, t / zoomDuration);

            lens.OrthographicSize = size;
            virtualCamera.Lens = lens;

            yield return null;
        }
    }
}


