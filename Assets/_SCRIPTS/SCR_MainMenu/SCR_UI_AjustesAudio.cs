using UnityEngine;
using UnityEngine.UI;

public class SCR_UI_AjustesAudio : MonoBehaviour
{
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    private void OnEnable()
    {
        // Cada vez que este panel se enciende, actualiza los sliders visualmente
        if (sliderMaster != null) sliderMaster.value = PlayerPrefs.GetFloat("VolumenMaster", 0.75f);
        if (sliderMusica != null) sliderMusica.value = PlayerPrefs.GetFloat("VolumenMusica", 0.75f);
        if (sliderSFX != null) sliderSFX.value = PlayerPrefs.GetFloat("VolumenSFX", 0.75f);
    }
}