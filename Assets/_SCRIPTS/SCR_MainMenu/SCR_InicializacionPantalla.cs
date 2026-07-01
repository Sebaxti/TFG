using UnityEngine;

public class SCR_InicializacionPantalla
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void AplicarResolucionNativa()
    {
#if !UNITY_EDITOR
        Resolution res = Screen.currentResolution;
        if (res.width > 0 && res.height > 0)
            Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
#endif
    }
}
