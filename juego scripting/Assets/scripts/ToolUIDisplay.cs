using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la visualización de las herramientas en la UI
/// Muestra 4 imágenes (una por herramienta) y resalta la herramienta seleccionada
/// </summary>
public class ToolUIDisplay : MonoBehaviour
{
    [Header("Imágenes de Herramientas")]
    [Tooltip("Imagen para el Pico (Piedra) - Tecla H")]
    public Image imagenPico;
    [Tooltip("Imagen para el Hacha (Madera) - Tecla J")]
    public Image imagenHacha;
    [Tooltip("Imagen para el Taladro (Metal) - Tecla K")]
    public Image imagenTaladro;
    [Tooltip("Imagen para el Martillo (Cristal) - Tecla L")]
    public Image imagenMartillo;

    [Header("Sprites de Herramientas")]
    [Tooltip("Sprite del Pico")]
    public Sprite spritePico;
    [Tooltip("Sprite del Hacha")]
    public Sprite spriteHacha;
    [Tooltip("Sprite del Taladro")]
    public Sprite spriteTaladro;
    [Tooltip("Sprite del Martillo")]
    public Sprite spriteMartillo;

    [Header("Configuración Visual")]
    [Tooltip("Color cuando la herramienta está seleccionada")]
    public Color colorSeleccionado = Color.white;
    [Tooltip("Color cuando la herramienta NO está seleccionada")]
    public Color colorNoSeleccionado = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [Tooltip("Escala cuando está seleccionada")]
    public float escalaSeleccionada = 1.2f;
    [Tooltip("Escala normal")]
    public float escalaNormal = 1f;

    private TipoHerramienta herramientaActual = TipoHerramienta.Pico;

    private void Start()
    {
        // Asignar sprites a las imágenes
        AsignarSprites();
        
        // Actualizar la visualización inicial
        ActualizarVisualizacion(TipoHerramienta.Pico);
    }

    /// <summary>
    /// Asigna los sprites a las imágenes correspondientes
    /// </summary>
    private void AsignarSprites()
    {
        if (imagenPico != null && spritePico != null)
            imagenPico.sprite = spritePico;
        
        if (imagenHacha != null && spriteHacha != null)
            imagenHacha.sprite = spriteHacha;
        
        if (imagenTaladro != null && spriteTaladro != null)
            imagenTaladro.sprite = spriteTaladro;
        
        if (imagenMartillo != null && spriteMartillo != null)
            imagenMartillo.sprite = spriteMartillo;
    }

    /// <summary>
    /// Actualiza la visualización según la herramienta seleccionada
    /// </summary>
    public void ActualizarVisualizacion(TipoHerramienta herramienta)
    {
        Debug.Log($"ToolUIDisplay - Actualizando herramienta a: {herramienta}");
        herramientaActual = herramienta;

        // Resetear todas las herramientas
        ResetearHerramienta(imagenPico);
        ResetearHerramienta(imagenHacha);
        ResetearHerramienta(imagenTaladro);
        ResetearHerramienta(imagenMartillo);

        // Resaltar la herramienta seleccionada
        switch (herramienta)
        {
            case TipoHerramienta.Pico:
                SeleccionarHerramienta(imagenPico);
                Debug.Log("Pico seleccionado");
                break;
            case TipoHerramienta.Hacha:
                SeleccionarHerramienta(imagenHacha);
                Debug.Log("Hacha seleccionada");
                break;
            case TipoHerramienta.Taladro:
                SeleccionarHerramienta(imagenTaladro);
                Debug.Log("Taladro seleccionado");
                break;
            case TipoHerramienta.Martillo:
                SeleccionarHerramienta(imagenMartillo);
                Debug.Log("Martillo seleccionado");
                break;
        }
    }

    /// <summary>
    /// Resetea el aspecto de una herramienta a su estado no seleccionado
    /// </summary>
    private void ResetearHerramienta(Image imagen)
    {
        if (imagen != null)
        {
            imagen.color = colorNoSeleccionado;
            imagen.transform.localScale = Vector3.one * escalaNormal;
            Debug.Log($"Resetear herramienta: {imagen.name}");
        }
        else
        {
            Debug.LogWarning("Intentando resetear una imagen NULL");
        }
    }

    /// <summary>
    /// Resalta una herramienta como seleccionada
    /// </summary>
    private void SeleccionarHerramienta(Image imagen)
    {
        if (imagen != null)
        {
            imagen.color = colorSeleccionado;
            imagen.transform.localScale = Vector3.one * escalaSeleccionada;
            Debug.Log($"Seleccionar herramienta: {imagen.name} - Color: {colorSeleccionado}, Escala: {escalaSeleccionada}");
        }
        else
        {
            Debug.LogWarning("Intentando seleccionar una imagen NULL");
        }
    }

    /// <summary>
    /// Obtiene la herramienta actualmente seleccionada
    /// </summary>
    public TipoHerramienta ObtenerHerramientaActual()
    {
        return herramientaActual;
    }
}
