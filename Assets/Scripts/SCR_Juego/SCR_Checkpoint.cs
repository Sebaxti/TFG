using UnityEngine;

public class SCR_Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform puntoAparicionPlayer;
    [SerializeField] private Transform puntoAparicionEnemigo;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_Movimiento player = other.GetComponent<SCR_Movimiento>();
            if (player != null)
            {
                player.EstablecerCheckpoint(puntoAparicionPlayer.position, puntoAparicionEnemigo.position);
            }
        }
    }
}