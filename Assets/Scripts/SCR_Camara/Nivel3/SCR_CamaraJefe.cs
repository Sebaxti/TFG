using UnityEngine;

public class SCR_CamaraJefe : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public Transform puntoCentro;

    [Header("Rangos de Distancia (Zoom)")]
    [Tooltip("Distancia cuando el jugador está pegado al centro")]
    public float radioMinimo = 12f;
    [Tooltip("Distancia cuando el jugador está en el borde de la arena")]
    public float radioMaximo = 22f;

    [Header("Rangos de Altura")]
    [Tooltip("Altura cuando la cámara está cerca")]
    public float alturaMinima = 5f;
    [Tooltip("Altura cuando la cámara está lejos")]
    public float alturaMaxima = 12f;

    [Header("Configuración de Arena")]
    [Tooltip("¿A qué distancia del centro consideras que el jugador está en el 'borde'?")]
    public float radioDeLaArena = 15f;
    public float alturaDeMira = 2f;

    [Header("Suavizado")]
    public float suavizadoBusqueda = 5f;

    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (puntoCentro == null) puntoCentro = GameObject.FindGameObjectWithTag("Jefe")?.transform;
    }

    private void LateUpdate()
    {
        if (jugador == null || puntoCentro == null) return;

        // 1. Calculamos la dirección y la distancia del jugador al centro
        Vector3 vectorCentroJugador = jugador.position - puntoCentro.position;
        vectorCentroJugador.y = 0;

        float distanciaJugadorAlCentro = vectorCentroJugador.magnitude;
        Vector3 direccionHaciaJugador = vectorCentroJugador.normalized;

        if (distanciaJugadorAlCentro > 0.01f)
        {
            // 2. Calculamos el "Factor de Zoom" (de 0 a 1)
            // 0 significa que el jugador está en el centro. 1 significa que está en el borde.
            float factorZoom = Mathf.Clamp01(distanciaJugadorAlCentro / radioDeLaArena);

            // 3. Aplicamos Interpolación Lineal (Lerp) para el Radio y la Altura
            // Si el factor es 0, usará los valores mínimos. Si es 1, los máximos.
            float radioActual = Mathf.Lerp(radioMinimo, radioMaximo, factorZoom);
            float alturaActual = Mathf.Lerp(alturaMinima, alturaMaxima, factorZoom);

            // 4. Calculamos la posición objetivo
            Vector3 posicionObjetivo = puntoCentro.position + (direccionHaciaJugador * radioActual) + (Vector3.up * alturaActual);

            // 5. Aplicamos el movimiento suave
            transform.position = Vector3.Lerp(transform.position, posicionObjetivo, Time.deltaTime * suavizadoBusqueda);

            // 6. Mirar siempre al centro
            Vector3 puntoDeMira = puntoCentro.position + (Vector3.up * alturaDeMira);
            transform.LookAt(puntoDeMira);
        }
    }
}