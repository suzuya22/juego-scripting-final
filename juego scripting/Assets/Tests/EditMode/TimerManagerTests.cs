using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerManagerTests
{
    private readonly List<GameObject> objetosCreados = new List<GameObject>();

    private TextMeshProUGUI CrearEtiqueta()
    {
        var go = new GameObject("TMP_Label");
        objetosCreados.Add(go);
        return go.AddComponent<TextMeshProUGUI>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in objetosCreados)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        objetosCreados.Clear();
    }

    [Test]
    public void Constructor_InicializaTiempoYTexto()
    {
        var etiqueta = CrearEtiqueta();

        var temporizador = new TimerManager(90f, etiqueta);

    Assert.That(temporizador.TiempoActual, Is.EqualTo(90f));
    Assert.That(temporizador.TiempoLimite, Is.EqualTo(90f));
    Assert.That(etiqueta.text, Is.EqualTo("01:30"));
    Assert.That(etiqueta.color, Is.EqualTo(Color.white));
    }

    [Test]
    public void Tick_AgoraTiempoHastaCero()
    {
        var etiqueta = CrearEtiqueta();
        var temporizador = new TimerManager(2f, etiqueta);

        bool resultadoPrimerTick = temporizador.Tick(1f);

    Assert.That(resultadoPrimerTick, Is.True);
    Assert.That(temporizador.TiempoActual, Is.EqualTo(1f).Within(0.001f));

        bool resultadoSegundoTick = temporizador.Tick(1f);

    Assert.That(resultadoSegundoTick, Is.False);
    Assert.That(temporizador.TiempoActual, Is.EqualTo(0f).Within(0.001f));
    Assert.That(temporizador.EstaActivo, Is.False);
    Assert.That(etiqueta.color, Is.EqualTo(Color.red));

        bool resultadoAdicional = temporizador.Tick(0.5f);
    Assert.That(resultadoAdicional, Is.False);
    }

    [Test]
    public void Reiniciar_RestableceTiempoYActivaTemporizador()
    {
        var etiqueta = CrearEtiqueta();
        var temporizador = new TimerManager(10f, etiqueta);

        temporizador.Tick(3f);
        temporizador.Reiniciar();

    Assert.That(temporizador.TiempoActual, Is.EqualTo(10f).Within(0.001f));
    Assert.That(temporizador.EstaActivo, Is.True);
    Assert.That(etiqueta.text, Is.EqualTo("00:10"));
    Assert.That(etiqueta.color, Is.EqualTo(Color.red));
    }

    [Test]
    public void Detener_InhabilitaTick()
    {
        var etiqueta = CrearEtiqueta();
        var temporizador = new TimerManager(5f, etiqueta);

        temporizador.Detener();
        bool resultado = temporizador.Tick(1f);

    Assert.That(resultado, Is.False);
    Assert.That(temporizador.TiempoActual, Is.EqualTo(5f).Within(0.001f));
    Assert.That(temporizador.EstaActivo, Is.False);
    }
}
