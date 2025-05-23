﻿using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Audio;

public class TelePort : Interactable
{
    public CinemachineCamera virtualCamera;
    public GameObject teleport;
    public GameObject box1;
    public GameObject box2;
    public CinemachineConfiner2D cinemachineConfiner;

    [Header("오디오")]
    [SerializeField] private string player_Teleport = "player_teleport";

    public override void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        AudioManager.Instance.PlayAt(player_Teleport, player.transform.position);


        Vector3 newPosition = teleport.transform.position;
        Vector3 oldPosition = player.transform.position;

        // 1. 플레이어 위치 먼저 순간이동
        player.transform.position = newPosition;

        // 2. 시네머신 카메라도 순간이동
        if (virtualCamera != null)
        {
            Vector3 cameraOffset = virtualCamera.transform.position - oldPosition;
            Vector3 newCameraPos = newPosition + cameraOffset;

            // Cinemachine이 강제로 위치를 점프하게 함
            virtualCamera.ForceCameraPosition(newCameraPos, virtualCamera.transform.rotation);
        }

        // 3. (선택) 경계 상자 변경
        if (box1 != null) box1.SetActive(false);
        if (box2 != null) box2.SetActive(true);

        // 4. Confiner 경계 교체
        if (cinemachineConfiner != null && box2 != null)
        {
            Collider2D newBounds = box2.GetComponent<Collider2D>();
            if (newBounds != null)
            {
                newBounds.enabled = false;
                newBounds.enabled = true;

                cinemachineConfiner.enabled = false;
                cinemachineConfiner.BoundingShape2D = newBounds;
                cinemachineConfiner.enabled = true;
            }
        }
        Debug.Log("Teleport Interacted");
    }

}
