using System;
using UnityEngine;

public class IntatratUI : MonoBehaviour
{
    [SerializeField] private GameObject intatratUI;
    
    private void Start()
    {
        if (intatratUI == null)
            intatratUI = GameObject.Find("InteractionHint2");
        intatratUI.SetActive(true);
        intatratUI.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("dkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            intatratUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            intatratUI.SetActive(false);
        }
    }
}
