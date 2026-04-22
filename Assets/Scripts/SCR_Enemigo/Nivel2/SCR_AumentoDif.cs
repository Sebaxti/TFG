using UnityEngine;

public class SCR_AumentoDif : MonoBehaviour
{
    [Header("Conexión")]
    [SerializeField] private SCR_EnemigoPersecucion scriptEnemigo;

    [Header("Ajuste de Probabilidad (Suma)")]
    [Range(0f, 1f)][SerializeField] private float sumarCorte = 0.1f;
    [Range(0f, 1f)][SerializeField] private float sumarCaida = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (scriptEnemigo != null)
            {
                scriptEnemigo.SumarDificultad(sumarCorte, sumarCaida);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("¡Olvidasste asignar al Enemigo en este Trigger!");
            }
        }
    }
}