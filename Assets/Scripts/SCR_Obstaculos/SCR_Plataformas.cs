using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Plataformas : MonoBehaviour
{
    public enum PuntoInicial { PuntoA, PuntoB }

    [Header("Configuración de Ruta")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float velocidad = 3f;

    [Header("Ajustes de Inicio")]
    [Tooltip("Selecciona a qué punto se dirigirá la plataforma al arrancar")]
    [SerializeField] private PuntoInicial comenzarHacia = PuntoInicial.PuntoB;

    private Rigidbody rb;
    private Transform objetivoActual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        if (puntoA == null || puntoB == null)
        {
            return;
        }

        objetivoActual = (comenzarHacia == PuntoInicial.PuntoA) ? puntoA : puntoB;
    }

    private void FixedUpdate()
    {
        if (puntoA == null || puntoB == null) return;

        Vector3 nuevaPosicion = Vector3.MoveTowards(
            rb.position,
            objetivoActual.position,
            velocidad * Time.fixedDeltaTime
        );

        rb.MovePosition(nuevaPosicion);

        if (Vector3.Distance(rb.position, objetivoActual.position) < 0.1f)
        {
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Cuando el jugador toca la plataforma, lo emparentamos
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.transform.parent == transform)
            {
                collision.gameObject.transform.SetParent(null);
            }
        }
    }
}