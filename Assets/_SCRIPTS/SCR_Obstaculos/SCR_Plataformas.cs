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
        if (puntoA == null || puntoB == null) return;
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
}