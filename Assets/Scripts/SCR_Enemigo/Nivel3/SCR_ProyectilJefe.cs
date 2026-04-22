using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
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
        rb = GetComponent<Rigidbody>();
    }

    public void Disparar(Vector3 posicionObjetivo, SCR_JefeFinal jefe)
    {
        scriptJefe = jefe;
        esReflejado = false;

        Vector3 objetivoCorregido = posicionObjetivo + Vector3.up * 1f;
        Vector3 direccionLineal = (objetivoCorregido - transform.position).normalized;
        transform.forward = direccionLineal;

        rb.linearVelocity = transform.forward * velocidad;

        Destroy(gameObject, 7f);
    }

    private void FixedUpdate()
    {
        if (esReflejado && scriptJefe != null)
        {
            Vector3 dirAlJefe = (scriptJefe.transform.position + Vector3.up * 1.5f - transform.position).normalized;

            transform.forward = Vector3.Slerp(transform.forward, dirAlJefe, rotacionHoming * Time.fixedDeltaTime);

            rb.linearVelocity = transform.forward * velocidad;
        }
    }
    private void OnTriggerEnter(Collider other)
    {

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