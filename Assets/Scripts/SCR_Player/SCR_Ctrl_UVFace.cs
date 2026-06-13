using UnityEngine;

public class SCR_Ctrl_UVFace : MonoBehaviour

{
    public Material materialCara;
    public string nombrePropiedadTextura = "_BaseMap";

    // Aquí definimos el número de cara (0=Feliz, 1=Enojado, 2=Triste...)
    [Range(0, 16)]
    public int indiceCara;

    void Update()
    {
        // Calculamos el offset basado en un número entero, no en una posición
        float offsetU = indiceCara * 0.25f; // Ajusta 0.25 según tu textura
        materialCara.SetTextureOffset(nombrePropiedadTextura, new Vector2(offsetU, 0));
    }
}
