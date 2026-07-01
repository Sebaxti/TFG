using UnityEngine;
using System.Collections;

public class SCR_TemblorCamara : MonoBehaviour
{
    public static SCR_TemblorCamara Instancia;

    [Header("Ajustes por Defecto")]
    [SerializeField] private float duracionDefecto = 0.3f;

    [SerializeField] private float magnitudDefecto = 0.4f;

    private Vector3 posicionOriginal;
    private Coroutine rutinaActual;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    // OPCI�N 1: Llamas a esto sin poner n�meros, y usar� lo que hayas puesto en el Inspector
    public void AgitarCamara()
    {
        IniciarTemblor(duracionDefecto, magnitudDefecto);
    }

    // OPCI�N 2: Llamas a esto pas�ndole n�meros distintos si quieres un golpe especial (ej: explosi�n)
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