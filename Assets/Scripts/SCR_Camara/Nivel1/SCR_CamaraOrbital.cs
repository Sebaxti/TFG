using UnityEngine;

public class SCR_CamaraOrbital : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float alturaObjetivo = 1.5f;

    [Header("Configuración de Órbita")]
    [SerializeField] private float distanciaMaxima = 5f;
    [SerializeField] private float sensibilidadRaton = 3f;

    [Tooltip("Cuanto más bajo, más rápida y rígida. Cuanto más alto, más chicle (0.1f es ideal)")]
    [SerializeField] private float tiempoSuavizadoCamara = 0.1f;

    [Header("Límites Verticales")]
    [SerializeField] private float anguloMinimoY = -15f;
    [SerializeField] private float anguloMaximoY = 60f;

    [Header("Auto-Centrado")]
    [SerializeField] private float tiempoParaCentrar = 2f;
    [SerializeField] private float velocidadCentrado = 3f;
    [SerializeField] private float alturaVerticalCentrada = 15f;

    [Header("Colisiones (Evitar paredes)")]
    [SerializeField] private LayerMask capasColision;
    [SerializeField] private float radioCamara = 0.2f;

    private float rotacionActualX = 0f;
    private float rotacionActualY = 0f;
    private float distanciaActual;
    private float temporizadorCentrado = 0f;

    // Variables de referencia que necesita SmoothDamp para funcionar
    private Vector3 velocidadMovimiento = Vector3.zero;

    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player").transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        distanciaActual = distanciaMaxima;

        if (jugador != null)
        {
            rotacionActualX = jugador.eulerAngles.y;
            rotacionActualY = alturaVerticalCentrada;
        }
    }

    private void LateUpdate()
    {
        if (jugador == null) return;

        float inputRatonX = Input.GetAxis("Mouse X");
        float inputRatonY = Input.GetAxis("Mouse Y");
        float inputMovimiento = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));

        // --- 1. LÓGICA DE CONTROL ---
        if (Mathf.Abs(inputRatonX) > 0.05f || Mathf.Abs(inputRatonY) > 0.05f)
        {
            temporizadorCentrado = 0f;
            rotacionActualX += inputRatonX * sensibilidadRaton;
            rotacionActualY -= inputRatonY * sensibilidadRaton;
        }
        else if (inputMovimiento > 0.1f)
        {
            temporizadorCentrado += Time.deltaTime;
            if (temporizadorCentrado >= tiempoParaCentrar)
            {
                rotacionActualX = Mathf.LerpAngle(rotacionActualX, jugador.eulerAngles.y, Time.deltaTime * velocidadCentrado);
                rotacionActualY = Mathf.Lerp(rotacionActualY, alturaVerticalCentrada, Time.deltaTime * velocidadCentrado);
            }
        }
        else
        {
            temporizadorCentrado = 0f;
        }

        rotacionActualY = Mathf.Clamp(rotacionActualY, anguloMinimoY, anguloMaximoY);

        Vector3 puntoMira = jugador.position + Vector3.up * alturaObjetivo;
        Quaternion rotacionDeseada = Quaternion.Euler(rotacionActualY, rotacionActualX, 0);
        Vector3 direccionDeseada = rotacionDeseada * -Vector3.forward;

        // --- 2. COLISIONES ---
        float distanciaObjetivo = distanciaMaxima;
        if (Physics.SphereCast(puntoMira, radioCamara, direccionDeseada, out RaycastHit hit, distanciaMaxima, capasColision))
        {
            distanciaObjetivo = hit.distance;
        }

        // Reacción asimétrica de la distancia (instantánea al chocar, suave al alejarse)
        if (distanciaObjetivo < distanciaActual)
            distanciaActual = distanciaObjetivo;
        else
            distanciaActual = Mathf.Lerp(distanciaActual, distanciaObjetivo, Time.deltaTime * 5f);

        // --- 3. APLICAR MOVIMIENTO CON SMOOTHDAMP (LA MAGIA AQUÍ) ---
        Vector3 posicionFinal = puntoMira + (direccionDeseada * distanciaActual);

        // Sustituimos Lerp por SmoothDamp. Absorbe vibraciones como un muelle real.
        transform.position = Vector3.SmoothDamp(transform.position, posicionFinal, ref velocidadMovimiento, tiempoSuavizadoCamara);

        transform.LookAt(puntoMira);
    }
}