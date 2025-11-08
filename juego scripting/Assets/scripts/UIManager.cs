using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Maneja la UI del nivel completado con botones para volver al lobby o siguiente nivel
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panel de Nivel Completado")]
    public GameObject panelNivelCompletado;
    public TextMeshProUGUI textoNivelCompletado;
    public Button botonVolverLobby;
    public Button botonSiguienteNivel;

    [Header("Configuración de Escenas")]
    public string nombreEscenaLobby = "Lobby";
    public string nombreEscenaSiguienteNivel = "Nivel2";

    private void Start()
    {
        // Ocultar el panel al inicio
        if (panelNivelCompletado != null)
        {
            panelNivelCompletado.SetActive(false);
        }

        // Configurar botones
        if (botonVolverLobby != null)
        {
            botonVolverLobby.onClick.AddListener(VolverAlLobby);
        }

        if (botonSiguienteNivel != null)
        {
            botonSiguienteNivel.onClick.AddListener(IrAlSiguienteNivel);
        }
    }

    /// <summary>
    /// Muestra el panel de nivel completado
    /// </summary>
    public void MostrarPanelNivelCompletado()
    {
        if (panelNivelCompletado != null)
        {
            panelNivelCompletado.SetActive(true);
            Time.timeScale = 0f; // Pausar el juego
        }

        if (textoNivelCompletado != null)
        {
            textoNivelCompletado.text = "¡NIVEL COMPLETADO!";
        }
    }

    /// <summary>
    /// Oculta el panel de nivel completado
    /// </summary>
    public void OcultarPanelNivelCompletado()
    {
        if (panelNivelCompletado != null)
        {
            panelNivelCompletado.SetActive(false);
            Time.timeScale = 1f; // Reanudar el juego
        }
    }

    /// <summary>
    /// Vuelve a la escena del lobby
    /// </summary>
    public void VolverAlLobby()
    {
        Time.timeScale = 1f; // Asegurarse de que el tiempo vuelva a la normalidad
        SceneManager.LoadScene(nombreEscenaLobby);
    }

    /// <summary>
    /// Carga la escena del siguiente nivel
    /// </summary>
    public void IrAlSiguienteNivel()
    {
        Time.timeScale = 1f; // Asegurarse de que el tiempo vuelva a la normalidad
        SceneManager.LoadScene(nombreEscenaSiguienteNivel);
    }

    /// <summary>
    /// Establece el nombre de la escena del siguiente nivel dinámicamente
    /// </summary>
    public void EstablecerSiguienteNivel(string nombreEscena)
    {
        nombreEscenaSiguienteNivel = nombreEscena;
    }

    private void OnDestroy()
    {
        // Limpiar listeners de botones
        if (botonVolverLobby != null)
        {
            botonVolverLobby.onClick.RemoveListener(VolverAlLobby);
        }

        if (botonSiguienteNivel != null)
        {
            botonSiguienteNivel.onClick.RemoveListener(IrAlSiguienteNivel);
        }
    }
}
