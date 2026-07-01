using UnityEngine;

public class SCR_Checkpoint : MonoBehaviour
{
    [Header("Configuraci�n de Respawn")]
    [SerializeField] private Transform puntoDeReaparicion;
    [SerializeField] private Transform puntoEnemigoOpcional;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_RespawnJugador respawn = other.GetComponent<SCR_RespawnJugador>();

            SCR_EnemigoPersecucion enemigo = FindFirstObjectByType<SCR_EnemigoPersecucion>();

            if (respawn != null)
            {
                Vector3 posJugador = (puntoDeReaparicion != null) ? puntoDeReaparicion.position : transform.position;

                Vector3 posEnemigo;

                if (puntoEnemigoOpcional != null)
                {
                    posEnemigo = puntoEnemigoOpcional.position;
                }
                else if (enemigo != null)
                {
                    posEnemigo = enemigo.transform.position;
                }
                else
                {
                    posEnemigo = respawn.GetEnemigoRespawn();
                }

                respawn.EstablecerCheckpoint(posJugador, posEnemigo);
            }
        }
    }
}