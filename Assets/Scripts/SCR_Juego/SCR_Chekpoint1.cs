using UnityEngine;

public class SCR_Checkpoint1 : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    [Tooltip("Arrastra aquí el objeto vacío que marca el lugar exacto donde aparecerá el jugador.")]
    [SerializeField] private Transform puntoDeReaparicion;

    [Tooltip("Opcional: Si tienes un enemigo que deba reiniciarse, arrastra su punto aquí.")]
    [SerializeField] private Transform puntoEnemigoOpcional;

    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador atraviesa este área...
        if (other.CompareTag("Player"))
        {
            SCR_Movimiento scriptJugador = other.GetComponent<SCR_Movimiento>();

            if (scriptJugador != null)
            {
                // Si por algún motivo se te olvidó asignar el punto en Unity, usa la posición actual por seguridad
                Vector3 posJugador = (puntoDeReaparicion != null) ? puntoDeReaparicion.position : transform.position;
                Vector3 posEnemigo = (puntoEnemigoOpcional != null) ? puntoEnemigoOpcional.position : Vector3.zero;

                // Le pasamos las coordenadas exactas al jugador
                scriptJugador.EstablecerCheckpoint(posJugador, posEnemigo);

                Debug.Log("¡Checkpoint activado! Reaparecerás en: " + posJugador);
            }
        }
    }
}