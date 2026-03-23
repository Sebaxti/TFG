using UnityEngine;
using System.Collections;

public class SCR_TrampaPinchos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform objetoPinchos; // El objeto hijo que tiene los pinchos

    [Header("Configuración de Movimiento")]
    [SerializeField] private Vector3 posicionEscondido;
    [SerializeField] private Vector3 posicionActivado;
    [SerializeField] private float velocidadSalida = 15f;
    [SerializeField] private float velocidadEntrada = 3f;

    [Header("Tiempos")]
    [SerializeField] private float retardoActivacion = 0.5f; // Tiempo desde que pisa hasta que salen
    [SerializeField] private float tiempoFuera = 2f;        // Tiempo que se quedan arriba

    private bool activada = false;

    private void Start()
    {
        // Aseguramos que empiecen escondidos
        if (objetoPinchos != null)
            objetoPinchos.localPosition = posicionEscondido;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !activada)
        {
            StartCoroutine(SecuenciaTrampa());
        }
    }

    private IEnumerator SecuenciaTrampa()
    {
        activada = true;

        // 1. Retardo (El jugador nota que algo va a pasar)
        yield return new WaitForSeconds(retardoActivacion);

        // 2. Salida Rápida
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadSalida;
            objetoPinchos.localPosition = Vector3.Lerp(posicionEscondido, posicionActivado, t);
            yield return null;
        }

        // 3. Mantenerse arriba
        yield return new WaitForSeconds(tiempoFuera);

        // 4. Esconderse Lento
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadEntrada;
            objetoPinchos.localPosition = Vector3.Lerp(posicionActivado, posicionEscondido, t);
            yield return null;
        }

        activada = false;
    }

    // Dibujar los puntos en el editor para que sea fácil configurarla
    private void OnDrawGizmosSelected()
    {
        if (objetoPinchos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(posicionActivado), 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(posicionEscondido), 0.1f);
        }
    }
}