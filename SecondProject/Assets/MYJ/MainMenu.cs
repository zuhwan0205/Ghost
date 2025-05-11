using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string startSceneName = "stage1"; // 시작할 씬 이름

    public void OnStartButton()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서는 강제 종료
#endif
    }
}
