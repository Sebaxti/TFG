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
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_Movimiento>()?.Respawn();
            Destroy(gameObject); 
        }
        else if (!other.CompareTag("Jefe") && !other.CompareTag("PilarReflector"))
        {
            Destroy(gameObject);
        }
    }
}