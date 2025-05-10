using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Alram : MonoBehaviour
{
    private Light2D light;

    void Start()
    {
        light = GetComponentInChildren<Light2D>();
    }

    private void OffLight() { light.enabled = false; }
    private void OnLight() { light.enabled = true; }
}
