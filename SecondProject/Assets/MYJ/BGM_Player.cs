using UnityEngine;

public class BGM_Player : MonoBehaviour
{ 
    void Start()
    {
        AudioManager.Instance.PlayBGM("BGM_Opening");
    }
}
