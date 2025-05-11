using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    // 생성할 몬스터 프리팹
    public GameObject monsterPrefab;
    // 플레이어의 Transform - 생성 위치 기준
    public Transform player;

    [Header("몬스터 소환조건")]
    [SerializeField] float spawnRange = 8f;   // 좌우 최대 거리
    [SerializeField] float safeZone = 1f;     // 소환 금지 거리
    [SerializeField] private float despawnTime = 6f;    // 강제 디스폰 시간 (렌더러 없을 경우)
    [SerializeField] private float RespawnTime = 20f;   // 디스폰 후 재생성 대기 시간
    public LayerMask wallLayer;     // 벽 감지 레이어

    private GameObject currentMonster;                  // 현재 존재하는 몬스터 인스턴스
    private Coroutine checkVisibilityCoroutine;         // 몬스터 시야 체크용 코루틴
    private CinemachineCamera virtualCamera;            // Cinemachine 가상 카메라


    void Start()
    {
        // 씬에 존재하는 Cinemachine 카메라를 찾음
        virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera를 찾을 수 없습니다.");
            return;
        }



        // 몬스터 생성-소멸 사이클 시작
        StartCoroutine(MonsterCycle());
    }

    // 몬스터 생성과 삭제를 반복하는 루프
    IEnumerator MonsterCycle()
    {
        while (true)
        {
            // 생성 위치 계산
            Vector3 spawnPos = CalculateSpawnPosition();

            // 몬스터 생성
            currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            Debug.Log("몬스터 리스폰 위치: " + spawnPos);

            // 보이지 않을 경우 디스폰 감시 시작
            checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());

            // 몬스터가 존재하는 동안 대기
            while (currentMonster != null)
                yield return null;

            // 디스폰 후 재생성까지 대기 시간
            yield return new WaitForSeconds(RespawnTime);
        }
    }

    // 몬스터 생성 위치를 계산하는 함수
    Vector3 CalculateSpawnPosition()
    {
        // 왼쪽 소환 가능 범위: 플레이어 기준
        float leftMin = player.position.x - spawnRange;
        float leftMax = player.position.x - safeZone;

        // 오른쪽 소환 가능 범위: 플레이어 기준
        float rightMin = player.position.x + safeZone;
        float rightMax = player.position.x + spawnRange;

        // 벽 감지로 범위 조정
        leftMin = CheckWallInRange2D(leftMin, leftMax);
        rightMax = CheckWallInRange2D(rightMax, rightMin);

        float spawnX;

        // 좌우 범위 유효성 체크
        bool canSpawnLeft = leftMin < leftMax;
        bool canSpawnRight = rightMin < rightMax;

        //양쪽 다 가능하면 랜덤 선택
        if (canSpawnLeft && canSpawnRight)
        {
            spawnX = (Random.value < 0.5f)
                ? Random.Range(leftMin, leftMax)
                : Random.Range(rightMin, rightMax);
        }
        else if (canSpawnLeft)
        {
            spawnX = Random.Range(leftMin, leftMax);
        }
        else if (canSpawnRight)
        {
            spawnX = Random.Range(rightMin, rightMax);
        }
        else
        {
            Debug.LogWarning("소환 위치가 없습니다. fallback 위치 사용");
            spawnX = player.position.x; // 기본 fallback 위치
        }

        float spawnY = player.position.y;           // Y는 플레이어 기준으로 고정
        float spawnZ = 0f;                           // 2D에서는 보통 Z는 0으로 고정

        return new Vector3(spawnX, spawnY, spawnZ);
    }


    // 소환 위치에서 벽이 있는지 확인하여 위치를 제한하는 함수
    float CheckWallInRange2D(float currentLimit, float targetLimit)
    {
        // Raycast 방향 설정
        Vector2 direction = (targetLimit - currentLimit) > 0 ? Vector2.right : Vector2.left;

        // Raycast 거리
        float distance = Mathf.Abs(currentLimit - targetLimit);

        // Ray 시작 위치
        Vector2 rayOrigin = new Vector2(targetLimit, player.position.y);

        // 실제 Raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, distance, wallLayer);

        // 충돌한 벽이 있다면, 해당 벽보다 살짝 이전 지점으로 위치 제한
        if (hit.collider != null)
        {
            // 벽 바로 앞에서 멈추도록 조정
            return hit.point.x - (0.1f * direction.x);
        }

        // 충돌이 없다면 기존 제한 위치 유지
        return currentLimit;
    }

    // 몬스터가 카메라에 보이지 않으면 제거하는 로직
    IEnumerator CheckVisibilityAndDespawn()
    {
        float invisibleTime = 0f;

        // 자식 포함 Renderer 찾기
        Renderer renderer = currentMonster.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("Renderer 없음. 강제 디스폰 처리.");
            yield return new WaitForSeconds(despawnTime);
            Destroy(currentMonster);
            yield break;
        }

        // 몬스터가 존재하는 동안 반복 체크
        while (currentMonster != null)
        {
            if (!renderer.isVisible) // 카메라에 안 보이면
            {
                invisibleTime += Time.deltaTime;

                if (invisibleTime >= 10f) // 10초 이상 안 보이면 제거
                {
                    Destroy(currentMonster);
                    Debug.Log("몬스터 디스폰됨 (10초 이상 안 보임)");
                    yield break;
                }
            }
            else
            {
                // 보이면 시간 초기화
                invisibleTime = 0f;
            }

            yield return null;
        }
    }
}
