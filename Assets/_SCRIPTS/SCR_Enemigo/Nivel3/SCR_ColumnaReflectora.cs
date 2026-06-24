using UnityEngine;
using System.Collections;

public class SCR_ColumnaReflectora : MonoBehaviour
{
    [Header("Temblor de la Columna")]
    [Tooltip("Segundos que tiembla la columna antes de hundirse")]
    [SerializeField] private float duracionTemblor = 0.8f;
    [Tooltip("Amplitud del desplazamiento horizontal del temblor (metros)")]
    [SerializeField] private float intensidadTemblor = 0.08f;

    [Header("Hundimiento")]
    [Tooltip("Pausa entre el fin del temblor y el inicio del hundimiento")]
    [SerializeField] private float retardoHundimiento = 0.2f;
    [Tooltip("Velocidad en m/s a la que se hunde la columna")]
    [SerializeField] private float velocidadHundimiento = 4f;
    [Tooltip("Distancia total que baja la columna antes de desaparecer")]
    [SerializeField] private float distanciaHundimiento = 6f;

    [Header("Desaparición")]
    [Tooltip("Segundos de espera en el fondo antes de destruirse")]
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
