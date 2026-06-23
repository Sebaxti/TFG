using UnityEngine;
using System;

public class SCR_RespawnJugador : MonoBehaviour
{
    // El evento global ahora vive en este componente especializado
    public static event Action OnGlobalRespawn;

    private Vector3 posRespawnPlayer;
    private Vector3 posRespawnEnemigo;

    private SCR_Movimiento scriptMovimiento;
    private Rigidbody rb;

    [Header("Animaci�n de Muerte")]
    [Tooltip("Segundos de espera antes de teletransportar al jugador")]
    [SerializeField] private float tiempoEsperaMuerte = 2f;

    private void Awake()
    {
        scriptMovimiento = GetComponent<SCR_Movimiento>();
        rb = GetComponent<Rigidbody>();

        // Inicializar con la posici�n de inicio del nivel por seguridad
        posRespawnPlayer = transform.position;
    }
    private void Start()
    {
        // Buscamos al enemigo al iniciar el nivel y guardamos su posici�n original
        SCR_EnemigoPersecucion enemigo = FindFirstObjectByType<SCR_EnemigoPersecucion>();
        if (enemigo != null)
        {
            posRespawnEnemigo = enemigo.transform.position;
        }
    }
    public void EstablecerCheckpoint(Vector3 posicionPlayer, Vector3 posicionEnemigo)
    {
        posRespawnPlayer = posicionPlayer;
        posRespawnEnemigo = posicionEnemigo;
    }

    public void Respawn()
    {
        StartCoroutine(SecuenciaDeMuerte());
    
    }

    public void EjecutarTeletransporte()
    {
        transform.position = posRespawnPlayer;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Notificar a los oyentes globales (enemigos, trampas)
        OnGlobalRespawn?.Invoke();
    }

    public void FinalizarRespawn()
    {
        if (scriptMovimiento != null) scriptMovimiento.DesbloquearMovimiento();

        // Buscamos el script de animaciones y lo devolvemos a la vida limpio
        SCR_AnimacionesJugador animaciones = GetComponent<SCR_AnimacionesJugador>();
        if (animaciones != null)
        {
            animaciones.ResetearAnimaciones();
        }
    }

    public Vector3 GetEnemigoRespawn()
    {
        return posRespawnEnemigo;
    }

    private System.Collections.IEnumerator SecuenciaDeMuerte()
    {
        if (scriptMovimiento != null) scriptMovimiento.BloquearPorMuerte();

        if (SCR_TemblorCamara.Instancia != null)
        {
            SCR_TemblorCamara.Instancia.AgitarCamara();
        }

        yield return new WaitForSeconds(tiempoEsperaMuerte);

    
        if (SCR_GestorEscena.Instancia != null)
        {
            // GestorEscena llama a FinalizarRespawn al final de su secuencia
            SCR_GestorEscena.Instancia.ProcesarMuerte(this);
        }
        else
        {
            EjecutarTeletransporte();
            FinalizarRespawn();
        }
    }
}