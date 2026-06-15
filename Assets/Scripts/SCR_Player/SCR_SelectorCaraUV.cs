using UnityEngine;

public class SelectorCaraUV : MonoBehaviour
{
    public Material materialCara;
    // El nombre debe ser exactamente igual al que pusiste en el Blackboard del Shader
    public string nombrePropiedad = "_FaceOffset";

    [Range(0, 15)]
    public int indiceCara = 0;

    void Update()
    {
        // 1. Cálculo de coordenadas (Esto no cambia)
        int columna = indiceCara % 4;
        int fila = 3 - (indiceCara / 4);

        Vector2 offset = new Vector2(columna * 0.25f, fila * 0.25f);

        // 2. Aplicamos al Shader Graph
        // Usamos SetVector porque el Shader Graph recibe un Vector2/4
        materialCara.SetVector(nombrePropiedad, offset);
    }
}
