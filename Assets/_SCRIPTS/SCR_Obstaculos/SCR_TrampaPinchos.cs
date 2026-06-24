using UnityEngine;
using System.Collections;

public class SCR_TrampaPinchos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform objetoPinchos;

    [Header("Configuración de Movimiento")]
    [SerializeField] private Vector3 posicionEscondido;
    [SerializeField] private Vector3 posicionActivado;
    [SerializeField] private float velocidadSalida = 15f;
    [SerializeField] private float velocidadEntrada = 3f;

    [Header("Tiempos")]
    [SerializeField] private float retardoActivacion = 0.5f;
    [SerializeField] private float tiempoFuera = 2f;       

    private bool activada = false;

    private void Start()
    {
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

        yield return new WaitForSeconds(retardoActivacion);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadSalida;
            objetoPinchos.localPosition = Vector3.Lerp(posicionEscondido, posicionActivado, t);
            yield return null;
        }

        yield return new WaitForSeconds(tiempoFuera);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadEntrada;
            objetoPinchos.localPosition = Vector3.Lerp(posicionActivado, posicionEscondido, t);
            yield return null;
        }

        activada = false;
    }
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