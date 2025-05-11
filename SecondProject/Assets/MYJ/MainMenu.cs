using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string startSceneName = "stage1"; // ������ �� �̸�

    public void OnStartButton()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // �����Ϳ����� ���� ����
#endif
    }
}
