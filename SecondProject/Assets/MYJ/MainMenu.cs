using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string startSceneName = "Stage01"; // ������ �� �̸�

    public void OnStartButton()
    {
        SceneManager.LoadScene("Stage01");
    }

    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �����Ϳ����� ���� ����
#endif
    }
}
