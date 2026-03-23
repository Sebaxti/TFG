using UnityEngine;
using UnityEngine.Rendering;

public class SCR_CamaraPersecucion : MonoBehaviour
{

    [Header ("Objetivo")]
    [SerializeField] private Transform jugador;

    [Header ("Movimiento Frontal (Eje Z)")]
    [SerializeField] private float distanciaObjetivo = 15f;  
    [SerializeField] private float margenActivacionZ = 3f;    
    [SerializeField] private float suavizadoZ = 5f;          

    [Header("Seguimiento Vertical (Salto)")]
    [SerializeField] private float suavizadoY = 2f;         
    [SerializeField] private float limiteSaltoCamara = 2f;  

    [Header("Seguimiento Lateral (Eje X)")]
    [SerializeField] private bool seguirX = true;
    [SerializeField] private float suavizadoX = 5f;

    private Vector3 offsetInicial;
    private float alturaOriginal;

    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player").transform;


        offsetInicial = transform.position - jugador.position;
        alturaOriginal = transform.position.y;
    }

    private void LateUpdate()
    {
        if (jugador == null) return;

        Vector3 posicionNueva = transform.position;

        float distanciaZActual = transform.position.z - jugador.position.z;

        if (distanciaZActual < (distanciaObjetivo - margenActivacionZ))
        {
            float zDestino = jugador.position.z + distanciaObjetivo;
            posicionNueva.z = Mathf.Lerp(transform.position.z, zDestino, suavizadoZ * Time.deltaTime);
        }
        
        else if (distanciaZActual > (distanciaObjetivo + margenActivacionZ))
        {
            float zDestino = jugador.position.z + distanciaObjetivo;
            posicionNueva.z = Mathf.Lerp(transform.position.z, zDestino, suavizadoZ * Time.deltaTime);
        }

        
        float alturaDeseada = alturaOriginal + (jugador.position.y * 0.3f);


        alturaDeseada = Mathf.Clamp(alturaDeseada, alturaOriginal, alturaOriginal + limiteSaltoCamara);

        posicionNueva.y = Mathf.Lerp(transform.position.y, alturaDeseada, suavizadoY * Time.deltaTime);

 
        if (seguirX)
        {
            float xDestino = jugador.position.x + offsetInicial.x;
            posicionNueva.x = Mathf.Lerp(transform.position.x, xDestino, suavizadoX * Time.deltaTime);
        }

        transform.position = posicionNueva;
    }

}
