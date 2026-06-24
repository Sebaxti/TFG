using UnityEngine;

public class SCR_Checkpoint : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    [SerializeField] private Transform puntoDeReaparicion;
    [SerializeField] private Transform puntoEnemigoOpcional;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_RespawnJugador respawn = other.GetComponent<SCR_RespawnJugador>();

            // Buscamos al enemigo en tiempo real para saber dónde está AHORA MISMO
            SCR_EnemigoPersecucion enemigo = FindFirstObjectByType<SCR_EnemigoPersecucion>();

            if (respawn != null)
            {
                // 1. Dónde reaparece el jugador
                Vector3 posJugador = (puntoDeReaparicion != null) ? puntoDeReaparicion.position : transform.position;

                // 2. Dónde reaparece el enemigo
                Vector3 posEnemigo;

                if (puntoEnemigoOpcional != null)
                {
                    posEnemigo = puntoEnemigoOpcional.position; // Si le pones un punto a mano, usa ese
                }
                else if (enemigo != null)
                {
                    posEnemigo = enemigo.transform.position; // Si no, coge la posición ACTUAL del enemigo
                }
                else
                {
                    posEnemigo = respawn.GetEnemigoRespawn(); // Por si el enemigo estuviera destruido, cogemos la antigua
                }

                // 3. Guardamos ambas
                respawn.EstablecerCheckpoint(posJugador, posEnemigo);
            }
        }
    }
}