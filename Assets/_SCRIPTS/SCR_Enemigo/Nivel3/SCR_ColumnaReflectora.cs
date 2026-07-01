using UnityEngine;
using System.Collections;

public class SCR_ColumnaReflectora : MonoBehaviour
{
    [Header("Temblor de la Columna")]
    [SerializeField] private float duracionTemblor = 0.8f;
    [SerializeField] private float intensidadTemblor = 0.08f;

    [Header("Hundimiento")]
    [SerializeField] private float retardoHundimiento = 0.2f;
    [SerializeField] private float velocidadHundimiento = 4f;
    [SerializeField] private float distanciaHundimiento = 6f;

    [Header("Desaparición")]
    [SerializeField] private float tiempoTrasHundirse = 0.3f;

    private bool hundiendo = false;

    public void IniciarHundimiento()
    {
        if (hundiendo) return;
        hundiendo = true;
        StartCoroutine(SecuenciaHundimiento());
    }

    private IEnumerator SecuenciaHundimiento()
    {
        Vector3 posOriginal = transform.position;
        float timer = 0f;

        // Fase 1: Temblar horizontalmente
        while (timer < duracionTemblor)
        {
            timer += Time.deltaTime;
            float ox = Random.Range(-1f, 1f) * intensidadTemblor;
            float oz = Random.Range(-1f, 1f) * intensidadTemblor;
            transform.position = new Vector3(posOriginal.x + ox, posOriginal.y, posOriginal.z + oz);
            yield return null;
        }
        transform.position = posOriginal;

        yield return new WaitForSeconds(retardoHundimiento);

        // Fase 2: Hundirse verticalmente
        Vector3 posDestino = posOriginal + Vector3.down * distanciaHundimiento;
        while (transform.position.y > posDestino.y)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                posDestino,
                velocidadHundimiento * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(tiempoTrasHundirse);
        Destroy(gameObject);
    }
}
