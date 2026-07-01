using UnityEngine;
using System;

public class SCR_RespawnJugador : MonoBehaviour
{
    public static event Action OnGlobalRespawn;

    // True desde que empieza la muerte hasta que FinalizarRespawn() termina.
    public static bool EstaMuerto { get; private set; }

    private Vector3 posRespawnPlayer;
    private Vector3 posRespawnEnemigo;

    private SCR_Movimiento scriptMovimiento;
    private Rigidbody rb;

    [Header("Animacion de Muerte")]
    [SerializeField] private float tiempoEsperaMuerte = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip clipMuerte;

    private void Awake()
    {
        EstaMuerto = false;
        scriptMovimiento = GetComponent<SCR_Movimiento>();
        rb = GetComponent<Rigidbody>();
        posRespawnPlayer = transform.position;
    }
    private void Start()
    {
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
        if (EstaMuerto) return;
        EstaMuerto = true;
        StartCoroutine(SecuenciaDeMuerte());
    }

    public void EjecutarTeletransporte()
    {
        transform.position = posRespawnPlayer;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        OnGlobalRespawn?.Invoke();
    }

    public void FinalizarRespawn()
    {
        EstaMuerto = false;
        if (scriptMovimiento != null) scriptMovimiento.DesbloquearMovimiento();

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
        SCR_GestorAudio.Instancia?.ReproducirSFX(clipMuerte);

        if (scriptMovimiento != null) scriptMovimiento.BloquearPorMuerte();

        if (SCR_TemblorCamara.Instancia != null)
        {
            SCR_TemblorCamara.Instancia.AgitarCamara();
        }

        yield return new WaitForSeconds(tiempoEsperaMuerte);

    
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
}
