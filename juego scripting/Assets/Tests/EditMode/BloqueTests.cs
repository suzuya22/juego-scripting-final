using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BloqueTests
{
    [Test]
    public void PuedeSerDestruidoPor_DevuelveVerdaderoSoloParaHerramientaCorrecta()
    {
        var bloquePiedra = new Bloque(TipoBloque.Piedra, false, 1, Vector3Int.zero, null);

    Assert.That(bloquePiedra.PuedeSerDestruidoPor(TipoHerramienta.Pico), Is.True);
    Assert.That(bloquePiedra.PuedeSerDestruidoPor(TipoHerramienta.Hacha), Is.False);
    Assert.That(bloquePiedra.PuedeSerDestruidoPor(TipoHerramienta.Taladro), Is.False);
    Assert.That(bloquePiedra.PuedeSerDestruidoPor(TipoHerramienta.Martillo), Is.False);
    }

    [Test]
    public void Golpear_DecrementaCantidadYRetornaTrueCuandoSeDestruye()
    {
        var bloque = new Bloque(TipoBloque.Metal, false, 2, Vector3Int.zero, null);

        bool resultadoPrimerGolpe = bloque.Golpear(TipoHerramienta.Taladro);
    Assert.That(resultadoPrimerGolpe, Is.False);
    Assert.That(bloque.CantidadActual, Is.EqualTo(1));

        bool resultadoSegundoGolpe = bloque.Golpear(TipoHerramienta.Taladro);
    Assert.That(resultadoSegundoGolpe, Is.True);
    Assert.That(bloque.CantidadActual, Is.EqualTo(0));
    }

    [Test]
    public void Destruir_LimpiaTileEnTilemap()
    {
        var gridGO = new GameObject("Grid", typeof(Grid));
        var tilemapGO = new GameObject("Tilemap", typeof(Tilemap));
        tilemapGO.transform.SetParent(gridGO.transform);
        var tilemap = tilemapGO.GetComponent<Tilemap>();
        var tile = ScriptableObject.CreateInstance<Tile>();
        var posicion = Vector3Int.one;
        tilemap.SetTile(posicion, tile);

        var bloque = new Bloque(TipoBloque.Madera, false, 1, posicion, tile);

        bloque.Destruir(tilemap);

    Assert.That(tilemap.GetTile(posicion), Is.Null);

        Object.DestroyImmediate(tilemapGO);
        Object.DestroyImmediate(gridGO);
    }
}
