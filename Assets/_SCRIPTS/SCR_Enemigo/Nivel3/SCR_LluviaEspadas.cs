using UnityEngine;

public class SCR_LluviaEspadas : MonoBehaviour
{
    [Tooltip("Tiempo en segundos que permanece activa la zona de lluvia de espadas")]
    [SerializeField] private float tiempoDeVida = 2f;

    private void OnEnable()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null) ps.Play();

        Destroy(gameObject, tiempoDeVida);
    }


}
