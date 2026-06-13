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

            if (respawn != null)
            {
                Vector3 posJugador = (puntoDeReaparicion != null) ? puntoDeReaparicion.position : transform.position;

                Vector3 posEnemigo = (puntoEnemigoOpcional != null) ? puntoEnemigoOpcional.position : respawn.GetEnemigoRespawn();

                respawn.EstablecerCheckpoint(posJugador, posEnemigo);
            }
        }
    }
}