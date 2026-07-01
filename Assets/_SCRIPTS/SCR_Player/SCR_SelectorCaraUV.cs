using UnityEngine;

public class SelectorCaraUV : MonoBehaviour
{
    public Material materialCara;
    public string nombrePropiedad = "_FaceOffset";

    [Range(0, 15)]
    public int indiceCara = 0;

    void Update()
    {
        int columna = indiceCara % 4;
        int fila = 3 - (indiceCara / 4);

        Vector2 offset = new Vector2(columna * 0.25f, fila * 0.25f);

        materialCara.SetVector(nombrePropiedad, offset);
    }
}
