using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Esto asegura que siempre haya un Rigidbody
public class SCR_ProyectilJefe : MonoBehaviour
{
    [Header("Configuración de Vuelo")]
    public float velocidad = 15f;
    public float rotacionHoming = 10f;

    private SCR_JefeFinal scriptJefe;
    private bool esReflejado = false;
    private Rigidbody rb;

    private void Awake()
    {
        // Obtenemos el componente Rigidbody
        rb = GetComponent<Rigidbody>();
    }

    public void Disparar(Vector3 posicionObjetivo, SCR_JefeFinal jefe)
    {
        scriptJefe = jefe;
        esReflejado = false;

        // Apuntar
        Vector3 objetivoCorregido = posicionObjetivo + Vector3.up * 1f;
        Vector3 direccionLineal = (objetivoCorregido - transform.position).normalized;
        transform.forward = direccionLineal;

        // Disparo Físico: Le damos un "empujón" constante en lugar de moverlo frame a frame
        rb.linearVelocity = transform.forward * velocidad;

        // Seguro de vida
        Destroy(gameObject, 7f);
    }

    // Usamos FixedUpdate porque estamos modificando físicas (velocity)
    private void FixedUpdate()
    {
        // FASE 2: Si rebotó, recalculamos la dirección hacia el jefe
        if (esReflejado && scriptJefe != null)
        {
            Vector3 dirAlJefe = (scriptJefe.transform.position + Vector3.up * 1.5f - transform.position).normalized;

            // Rotamos suavemente
            transform.forward = Vector3.Slerp(transform.forward, dirAlJefe, rotacionHoming * Time.fixedDeltaTime);

            // Actualizamos la velocidad física hacia la nueva dirección
            rb.linearVelocity = transform.forward * velocidad;
        }
    }

    // El evento nativo de Unity usando tu BoxCollider
    private void OnTriggerEnter(Collider other)
    {
        // --- LA LISTA BLANCA ---
        // Solo reacciona a estos 3 objetos. Ignora automáticamente el suelo o paredes invisibles.

        if (other.CompareTag("PilarReflector") && !esReflejado)
        {
            esReflejado = true;
            Debug.Log("Rebote contra Pilar");

            Renderer renderHijo = GetComponentInChildren<Renderer>();
            if (renderHijo != null) renderHijo.material.color = Color.cyan;
        }
        else if (other.CompareTag("Player") && !esReflejado)
        {
            other.GetComponent<SCR_Movimiento>()?.Respawn();
            if (scriptJefe) scriptJefe.DesbloquearJefe();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Jefe") && esReflejado)
        {
            scriptJefe.RecibirGolpe();
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (scriptJefe != null && !esReflejado) scriptJefe.DesbloquearJefe();
    }
}