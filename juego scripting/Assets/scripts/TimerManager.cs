using TMPro;
using UnityEngine;

// Temporizador sencillo: resta tiempo cada frame y actualiza el texto de la UI.
public class TimerManager
{
    public float TiempoActual { get; private set; }
    public float TiempoLimite { get; private set; }
    public bool EstaActivo { get; private set; } = true;

    private readonly TextMeshProUGUI textoTiempo;

    public TimerManager(float tiempoLimite, TextMeshProUGUI texto)
    {
        TiempoLimite = Mathf.Max(0f, tiempoLimite);
        TiempoActual = TiempoLimite;
        textoTiempo = texto;
        ActualizarTexto();
    }

    public bool Tick(float deltaTime)
    {
        if (!EstaActivo) return false;

        TiempoActual = Mathf.Max(0f, TiempoActual - deltaTime);
        ActualizarTexto();

        if (TiempoActual <= 0f)
        {
            EstaActivo = false;
            return false;
        }

        return true;
    }

    public void Reiniciar()
    {
        TiempoActual = TiempoLimite;
        EstaActivo = true;
        ActualizarTexto();
    }

    public void Detener()
    {
        EstaActivo = false;
    }

    private void ActualizarTexto()
    {
        if (textoTiempo == null) return;

        int minutos = Mathf.FloorToInt(TiempoActual / 60f);
        int segundos = Mathf.FloorToInt(TiempoActual % 60f);
        textoTiempo.text = $"{minutos:00}:{segundos:00}";

        if (TiempoActual <= 10f)
            textoTiempo.color = Color.red;
        else if (TiempoActual <= 30f)
            textoTiempo.color = Color.yellow;
        else
            textoTiempo.color = Color.white;
    }
}
