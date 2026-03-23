using UnityEngine;

public class SCR_PinchosMuerte : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_Movimiento player = other.GetComponent<SCR_Movimiento>();
            if (player != null)
            {
                player.Respawn();
            }
        }
    }
}