using System;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private Sprite MapPhoto;
    [SerializeField] private Sprite JustPhoto1;
    [SerializeField] private Sprite JustPhoto2;
    [SerializeField] private Sprite JustPhoto3;
    [SerializeField] private Sprite JustPhoto4;

    public void CompleteQuest()
    {
        if (PhoneManager.Instance != null && MapPhoto != null)
        {
            PhoneManager.Instance.AddPhoto(MapPhoto);
            PhoneManager.Instance.AddPhoto(JustPhoto1);
            PhoneManager.Instance.AddPhoto(JustPhoto2);
            PhoneManager.Instance.AddPhoto(JustPhoto3);
            PhoneManager.Instance.AddPhoto(JustPhoto4);
            
        }
    }

    private void Start()
    {
        CompleteQuest();
    }
}