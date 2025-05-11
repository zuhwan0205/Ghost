using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageStart : MonoBehaviour
{
    [SerializeField] private GameObject UICanvas;
    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject Stage1Animator;
    [SerializeField] private GameObject Stage2Animator;
    [SerializeField] private GameObject Stage3Animator;

    void Start()
    {
        UICanvas.SetActive(false);
        Player.SetActive(false);
        if(GameManager.Instance.CurrentStage == 1)
        {
            Stage1Animator.SetActive(true);
            StartCoroutine(WaitStart(Stage1Animator));
        }
        else if (GameManager.Instance.CurrentStage == 2)
        {
            Stage2Animator.SetActive(true);
            StartCoroutine(WaitStart(Stage2Animator));
        }
        else if (GameManager.Instance.CurrentStage == 3)
        {
            Stage3Animator.SetActive(true);
            StartCoroutine(WaitStart(Stage3Animator));
        }
    }

    private IEnumerator WaitStart(GameObject panel)
    {
        yield return new WaitForSeconds(5f);
        panel.SetActive(false);
        StartGame();
    }

    private void StartGame()
    {
        UICanvas.SetActive(true);
        Player.SetActive(true);
    }
}
