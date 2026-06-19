using UnityEngine;
using System.Collections;

public class SCR_TemblorCamara : MonoBehaviour
{
    public static SCR_TemblorCamara Instancia;

    [Header("Ajustes por Defecto")]
    [Tooltip("Tiempo en segundos que durará la vibración estándar")]
    [SerializeField] private float duracionDefecto = 0.3f;

    [Tooltip("Fuerza del temblor (cuánto se desplaza la cámara)")]
    [SerializeField] private float magnitudDefecto = 0.4f;

    private Vector3 posicionOriginal;
    private Coroutine rutinaActual;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    // OPCIÓN 1: Llamas a esto sin poner números, y usará lo que hayas puesto en el Inspector
    public void AgitarCamara()
    {
        IniciarTemblor(duracionDefecto, magnitudDefecto);
    }

    // OPCIÓN 2: Llamas a esto pasándole números distintos si quieres un golpe especial (ej: explosión)
    public void AgitarCamaraPersonalizada(float duracionEspecial, float magnitudEspecial)
    {
        IniciarTemblor(duracionEspecial, magnitudEspecial);
    }

    private void IniciarTemblor(float duracion, float magnitud)
    {
        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            transform.localPosition = posicionOriginal;
        }

        rutinaActual = StartCoroutine(Temblar(duracion, magnitud));
    }

    private IEnumerator Temblar(float duracion, float magnitud)
    {
        posicionOriginal = transform.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            transform.localPosition = new Vector3(posicionOriginal.x + x, posicionOriginal.y + y, posicionOriginal.z);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = posicionOriginal;
    }
}