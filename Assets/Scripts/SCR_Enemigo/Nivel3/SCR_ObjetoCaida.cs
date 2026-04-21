using UnityEngine;

public class SCR_ObjetoCaida : MonoBehaviour
{
    [Tooltip("Tiempo máximo antes de desaparecer (Seguro de vida)")]
    public float tiempoDeVida = 4f;

    private void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si le cae en la cabeza al jugador
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_Movimiento>()?.Respawn();
            Destroy(gameObject); // Nos destruimos al chocar
        }
        // Si choca con el suelo (o con los pilares)
        else if (!other.CompareTag("Jefe") && !other.CompareTag("PilarReflector"))
        {
            // Puedes añadir aquí un efecto de partículas de explosión/polvo en el futuro
            Destroy(gameObject);
        }
    }
}