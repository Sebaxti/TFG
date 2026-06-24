using UnityEngine;

public class SCR_PinchosMuerte : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_RespawnJugador>()?.Respawn();
        }
    }
}