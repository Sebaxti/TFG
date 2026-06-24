using UnityEngine;

public class SCR_ObjetoCaida : MonoBehaviour
{
    public float tiempoDeVida = 4f;
    [HideInInspector] public float velocidadDescenso = 10f;

    private void Start()
    {
        Destroy(gameObject, tiempoDeVida);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.down * velocidadDescenso;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_RespawnJugador>()?.Respawn();
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Jefe") && !other.CompareTag("PilarReflector"))
        {
            Destroy(gameObject);
        }
    }
}