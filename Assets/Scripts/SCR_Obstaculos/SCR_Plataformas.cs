using UnityEngine;

public class SCR_Plataformas : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float velocidad = 3f;

    private Transform objetivoActual;

    private void Start()
    {
        // Empezar moviéndose hacia el punto B
        objetivoActual = puntoB;
    }

    private void FixedUpdate()
    {
        // Mover la plataforma hacia el objetivo
        transform.position = Vector3.MoveTowards(
            transform.position,
            objetivoActual.position,
            velocidad * Time.deltaTime
        );

        // Si llegó al objetivo, cambiar de dirección
        if (Vector3.Distance(transform.position, objetivoActual.position) < 0.1f)
        {
            CambiarObjetivo();
        }
    }

    private void CambiarObjetivo()
    {
        // Alternar entre punto A y punto B
        objetivoActual = objetivoActual == puntoA ? puntoB : puntoA;
    }

    // Opcional: Visualizar los puntos en el editor
    private void OnDrawGizmos()
    {
        if (puntoA != null && puntoB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(puntoA.position, puntoB.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(puntoA.position, 0.3f);
            Gizmos.DrawWireSphere(puntoB.position, 0.3f);
        }
    }
}
