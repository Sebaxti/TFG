using UnityEngine;
using UnityEngine.Rendering;

public class SCR_CamaraPersecucion : MonoBehaviour
{

    [Header ("Objetivo")]
    [SerializeField] private Transform jugador;

    [Header ("Movimiento Frontal (Eje Z)")]
    [SerializeField] private float distanciaObjetivo = 15f;   // Distancia ideal entre cámara y jugador
    [SerializeField] private float margenActivacionZ = 3f;     // Cuánto puede acercarse antes de que la cámara se mueva
    [SerializeField] private float suavizadoZ = 5f;           // Velocidad de reacción de la cámara

    [Header("Seguimiento Vertical (Salto)")]
    [SerializeField] private float suavizadoY = 2f;         // Más bajo = más "mini" seguimiento
    [SerializeField] private float limiteSaltoCamara = 2f;  // Cuánto permitimos que suba la cámara máximo

    [Header("Seguimiento Lateral (Eje X)")]
    [SerializeField] private bool seguirX = true;
    [SerializeField] private float suavizadoX = 5f;

    private Vector3 offsetInicial;
    private float alturaOriginal;

    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Guardamos la configuración de inicio
        offsetInicial = transform.position - jugador.position;
        alturaOriginal = transform.position.y;
    }

    private void LateUpdate()
    {
        if (jugador == null) return;

        Vector3 posicionNueva = transform.position;

        // --- 1. LÓGICA TWINSANITY (EJE Z) ---
        // Calculamos la distancia real en Z entre cámara y jugador
        float distanciaZActual = transform.position.z - jugador.position.z;

        // Si el jugador se acerca demasiado a la "lente" (borde inferior)
        if (distanciaZActual < (distanciaObjetivo - margenActivacionZ))
        {
            float zDestino = jugador.position.z + distanciaObjetivo;
            posicionNueva.z = Mathf.Lerp(transform.position.z, zDestino, suavizadoZ * Time.deltaTime);
        }
        // Si el jugador se aleja mucho hacia el fondo
        else if (distanciaZActual > (distanciaObjetivo + margenActivacionZ))
        {
            float zDestino = jugador.position.z + distanciaObjetivo;
            posicionNueva.z = Mathf.Lerp(transform.position.z, zDestino, suavizadoZ * Time.deltaTime);
        }

        // --- 2. MINI-SEGUIMIENTO VERTICAL (EL "JUICE") ---
        // En lugar de ser fija, la altura busca un punto intermedio entre la original y la del salto
        float alturaDeseada = alturaOriginal + (jugador.position.y * 0.3f); // El 0.3f hace que sea sutil

        // Limitamos para que la cámara no suba al infinito si el nivel es muy alto
        alturaDeseada = Mathf.Clamp(alturaDeseada, alturaOriginal, alturaOriginal + limiteSaltoCamara);

        posicionNueva.y = Mathf.Lerp(transform.position.y, alturaDeseada, suavizadoY * Time.deltaTime);

        // --- 3. SEGUIMIENTO X ---
        if (seguirX)
        {
            float xDestino = jugador.position.x + offsetInicial.x;
            posicionNueva.x = Mathf.Lerp(transform.position.x, xDestino, suavizadoX * Time.deltaTime);
        }

        transform.position = posicionNueva;
    }

}
