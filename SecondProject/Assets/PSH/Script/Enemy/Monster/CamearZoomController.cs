using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraZoomController : MonoBehaviour
{
    [Header("ī�޶� ����")]
    public CinemachineCamera virtualCamera;          // Unity 6 ���� Ŭ����
    public float zoomInSize = 3.5f;                  // ������ ������
    public float zoomDuration = 0.5f;                // ����/�ƿ� ��ȯ �ð�
    public float zoomHoldTime = 2f;                  // ���� ���� �ð�

    private float originalSize;                      // ���� ī�޶� ������ ����
    private Coroutine zoomRoutine;

    private void Start()
    {
        if (virtualCamera == null)
            virtualCamera = Object.FindAnyObjectByType<CinemachineCamera>();

        if (virtualCamera != null)
            originalSize = virtualCamera.Lens.OrthographicSize; // �ʱ� ũ�� ����
    }

    /// <summary>
    /// �ܺο��� ȣ���� �� �ִ� ����-�ܾƿ� �Լ�
    /// </summary>
    public void ZoomInThenOut()
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomSequence());
    }

    /// <summary>
    /// ���� �� ���� �� �ܾƿ� ������ ����Ǵ� �ڷ�ƾ
    /// </summary>
    private IEnumerator ZoomSequence()
    {
        var lens = virtualCamera.Lens;

        // ����
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

        // �ܾƿ�
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


