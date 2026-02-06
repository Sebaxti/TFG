using UnityEngine;

// Obligamos a que la plataforma tenga un Rigidbody para moverla físicamente
[RequireComponent(typeof(Rigidbody))]
public class SCR_Plataformas : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float velocidad = 3f;

    private Rigidbody rb; // Referencia al componente físico
    private Transform objetivoActual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Configuración necesaria para plataformas móviles
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Suaviza el movimiento
    }

    private void Start()
    {
        objetivoActual = puntoB;
    }

    private void FixedUpdate()
    {
        if (puntoA == null || puntoB == null) return;

        // Calculamos la siguiente posición
        Vector3 nuevaPosicion = Vector3.MoveTowards(
            rb.position,
            objetivoActual.position,
            velocidad * Time.fixedDeltaTime
        );

        // USAMOS MovePosition: Esto es lo que hace que el jugador no se deslice
        rb.MovePosition(nuevaPosicion);

        if (Vector3.Distance(rb.position, objetivoActual.position) < 0.1f)
        {
            CambiarObjetivo();
        }
    }

    private void CambiarObjetivo()
    {
        objetivoActual = objetivoActual == puntoA ? puntoB : puntoA;
    }

    private void OnDrawGizmos()
    {
        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(puntoA.position, puntoB.position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(puntoA.position, 0.3f);
            Gizmos.DrawWireSphere(puntoB.position, 0.3f);
        }
    }
}