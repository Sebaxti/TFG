using UnityEngine;

public class SCR_LluviaEspadas : MonoBehaviour
{
    [SerializeField] private float tiempoDeVida = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip clipLluvia;

    private void OnEnable()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null) ps.Play();

        SCR_GestorAudio.Instancia?.ReproducirSFX(clipLluvia);

        Destroy(gameObject, tiempoDeVida);
    }
}
