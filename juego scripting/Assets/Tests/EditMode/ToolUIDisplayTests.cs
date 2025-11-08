using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ToolUIDisplayTests
{
    private ToolUIDisplay display;
    private GameObject root;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("ToolUIDisplayRoot");
        root.AddComponent<RectTransform>();
        display = root.AddComponent<ToolUIDisplay>();

        display.imagenPico = CrearImagen("Pico");
        display.imagenHacha = CrearImagen("Hacha");
        display.imagenTaladro = CrearImagen("Taladro");
        display.imagenMartillo = CrearImagen("Martillo");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    private Image CrearImagen(string nombre)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(root.transform);
        return go.AddComponent<Image>();
    }

    [Test]
    public void ActualizarVisualizacion_ResaltaHerramientaSeleccionada()
    {
        display.ActualizarVisualizacion(TipoHerramienta.Hacha);

    Assert.That(display.imagenHacha.color, Is.EqualTo(display.colorSeleccionado));
    Assert.That(display.imagenHacha.transform.localScale.x, Is.EqualTo(display.escalaSeleccionada).Within(0.001f));
    Assert.That(display.imagenHacha.transform.localScale.y, Is.EqualTo(display.escalaSeleccionada).Within(0.001f));

    Assert.That(display.imagenPico.color, Is.EqualTo(display.colorNoSeleccionado));
    Assert.That(display.imagenPico.transform.localScale.x, Is.EqualTo(display.escalaNormal).Within(0.001f));
    Assert.That(display.ObtenerHerramientaActual(), Is.EqualTo(TipoHerramienta.Hacha));
    }

    [Test]
    public void ActualizarVisualizacion_CuandoImagenEsNull_NoGeneraExcepcion()
    {
        display.imagenMartillo = null;

    Assert.That(() => display.ActualizarVisualizacion(TipoHerramienta.Martillo), Throws.Nothing);
    }
}
