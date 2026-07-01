using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Plataformas : MonoBehaviour
{
    public enum PuntoInicial { PuntoA, PuntoB }

    [Header("Configuraci�n de Ruta")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float velocidad = 3f;

    [Header("Ajustes de Inicio")]
    [SerializeField] private PuntoInicial comenzarHacia = PuntoInicial.PuntoB;

    private Rigidbody rb;
    private Transform objetivoActual;

    private Vector3 posicionAnterior;
    private Vector3 velocidadPlataforma;
    private Vector3 velocidadPlataformaAnterior;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        if (puntoA == null || puntoB == null) return;
        objetivoActual = (comenzarHacia == PuntoInicial.PuntoA) ? puntoA : puntoB;
        posicionAnterior = rb.position;
    }

    private void FixedUpdate()
    {
        if (puntoA == null || puntoB == null) return;

        velocidadPlataformaAnterior = velocidadPlataforma;

        Vector3 nuevaPosicion = Vector3.MoveTowards(
            rb.position,
            objetivoActual.position,
            velocidad * Time.fixedDeltaTime
        );

        rb.MovePosition(nuevaPosicion);

        velocidadPlataforma = (nuevaPosicion - posicionAnterior) / Time.fixedDeltaTime;
        posicionAnterior = nuevaPosicion;

        if (Vector3.Distance(rb.position, objetivoActual.position) < 0.1f)
        {
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
        SCR_Movimiento mov = collision.gameObject.GetComponent<SCR_Movimiento>();

        if (playerRb == null) return;

        if (mov != null && (mov.estadoActual == SCR_Movimiento.Estados.Jump ||
                            mov.estadoActual == SCR_Movimiento.Estados.DoubleJump)) return;

        bool estabaSubiendo = velocidadPlataformaAnterior.y > 0.1f;
        bool ahoraNoSube   = velocidadPlataforma.y <= 0.05f;
        if (estabaSubiendo && ahoraNoSube && playerRb.linearVelocity.y > 0f)
        {
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
        }

        if (velocidadPlataforma.y < -0.05f && playerRb.linearVelocity.y > velocidadPlataforma.y)
        {
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, velocidadPlataforma.y, playerRb.linearVelocity.z);
        }
    }
}