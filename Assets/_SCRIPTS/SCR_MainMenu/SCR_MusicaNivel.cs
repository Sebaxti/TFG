using UnityEngine;

public class SCR_MusicaNivel : MonoBehaviour
{
    [SerializeField] private AudioClip clipMusica;

    private void Start()
    {
        SCR_GestorAudio.Instancia?.CambiarMusica(clipMusica);
    }
}
