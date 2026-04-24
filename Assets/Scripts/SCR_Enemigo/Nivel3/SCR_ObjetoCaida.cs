using UnityEngine;

public class SCR_ObjetoCaida : MonoBehaviour
{
    public float tiempoDeVida = 4f;
    [HideInInspector] public float velocidadDescenso = 10f; // La controlará el jefe

    private void Start()
    {
        Destroy(gameObject, tiempoDeVida);
        // Si no tiene Rigidbody, se lo añadimos o usamos velocidad manual
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.down * velocidadDescenso;
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