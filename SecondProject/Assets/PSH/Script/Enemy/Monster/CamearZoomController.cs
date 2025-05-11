using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraZoomController : MonoBehaviour
{
    [Header("ī�޶� ����")]
    public CinemachineCamera virtualCamera;          // Unity 6 ���� Ŭ����
    public float zoomInSize = 3.5f;                  // ������ ������
    public float zoomDuration = 0.1f;                // ����/�ƿ� ��ȯ �ð�
    public float zoomHoldTime = 2.5f;                  // ���� ���� �ð�

    private float originalSize;                      // ���� ī�޶� ������ ����
    private Coroutine zoomRoutine;

    private void Start()
    {
        if (virtualCamera == null)
        {
            virtualCamera = Object.FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
        }

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

        // ��� ����
        lens.OrthographicSize = zoomInSize;
        virtualCamera.Lens = lens;

        yield return new WaitForSeconds(zoomHoldTime);

        // ��� �ܾƿ�
        lens.OrthographicSize = originalSize;
        virtualCamera.Lens = lens;
    }

}


