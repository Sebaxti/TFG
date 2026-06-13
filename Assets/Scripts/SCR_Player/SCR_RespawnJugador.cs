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

    private void Awake()
    {
        scriptMovimiento = GetComponent<SCR_Movimiento>();
        rb = GetComponent<Rigidbody>();

        // Inicializar con la posición de inicio del nivel por seguridad
        posRespawnPlayer = transform.position;
    }
    private void Start()
    {
        // Buscamos al enemigo al iniciar el nivel y guardamos su posición original
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
        if (scriptMovimiento != null) scriptMovimiento.BloquearMovimiento();

        if (SCR_GestorEscena.Instancia != null)
        {
            SCR_GestorEscena.Instancia.ProcesarMuerte(this);
        }
        else
        {
            EjecutarTeletransporte();
            FinalizarRespawn();
        }
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
    }

    public Vector3 GetEnemigoRespawn()
    {
        return posRespawnEnemigo;
    }
}