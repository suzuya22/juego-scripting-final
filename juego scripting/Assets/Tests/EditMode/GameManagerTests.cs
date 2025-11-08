using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManagerTests
{
    private GameObject gameManagerGO;
    private GameManager gameManager;
    private GameObject gridGO;
    private GameObject tilemapGO;
    private Tilemap tilemap;

    [SetUp]
    public void SetUp()
    {
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();

        gridGO = new GameObject("Grid", typeof(Grid));
        tilemapGO = new GameObject("Tilemap", typeof(Tilemap));
        tilemapGO.transform.SetParent(gridGO.transform);
        tilemap = tilemapGO.GetComponent<Tilemap>();

        gameManager.tilemapNivel = tilemap;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameManagerGO);
        Object.DestroyImmediate(tilemapGO);
        Object.DestroyImmediate(gridGO);
    }

    private void InicializarMapping(GameManager.TileAssetMapping mapping, TileBase[] tilesPared = null)
    {
        gameManager.mappingTiles = new[] { mapping };

        if (tilesPared != null)
        {
            var field = typeof(GameManager).GetField("tilesPared", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(gameManager, tilesPared);
        }

        var method = typeof(GameManager).GetMethod("InicializarMappingTiles", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(gameManager, null);
    }

    [Test]
    public void EsBloqueDificil_TrueParaTilesMarcados()
    {
        var tileNormal = ScriptableObject.CreateInstance<Tile>();
        var tileDanio = ScriptableObject.CreateInstance<Tile>();

        var mapping = new GameManager.TileAssetMapping
        {
            tipo = TipoBloque.Piedra,
            tileNormal = tileNormal,
            tilesProgresoDanio = new TileBase[] { tileDanio },
            esBloqueDificil = true
        };

        InicializarMapping(mapping);

    Assert.That(gameManager.EsBloqueDificil(tileNormal), Is.True);
    Assert.That(gameManager.EsBloqueDificil(tileDanio), Is.True);

        var tileOtro = ScriptableObject.CreateInstance<Tile>();
    Assert.That(gameManager.EsBloqueDificil(tileOtro), Is.False);
    }

    [Test]
    public void NivelCompletado_SetFlagsAndStopsTimer()
    {
        var timer = new TimerManager(5f, null);
        var fieldTimer = typeof(GameManager).GetField("temporizador", BindingFlags.NonPublic | BindingFlags.Instance);
        fieldTimer.SetValue(gameManager, timer);

        var fieldTerminado = typeof(GameManager).GetField("juegoTerminado", BindingFlags.NonPublic | BindingFlags.Instance);
        fieldTerminado.SetValue(gameManager, false);

        gameManager.NivelCompletado();

    Assert.That(gameManager.NivelActualCompletado, Is.True);
    Assert.That(gameManager.NivelesCompletados, Is.EqualTo(1));
    Assert.That(timer.EstaActivo, Is.False);
    Assert.That((bool)fieldTerminado.GetValue(gameManager), Is.True);

        gameManager.NivelCompletado();
    Assert.That(gameManager.NivelesCompletados, Is.EqualTo(1));
    }

    [Test]
    public void ActualizarAparienciaBloque_UsaSpriteSegunProgreso()
    {
        var tileNormal = ScriptableObject.CreateInstance<Tile>();
        var tileDanioSuave = ScriptableObject.CreateInstance<Tile>();
        var tileDanioFuerte = ScriptableObject.CreateInstance<Tile>();

        tilemap.SetTile(Vector3Int.zero, tileNormal);

        var mapping = new GameManager.TileAssetMapping
        {
            tipo = TipoBloque.Piedra,
            tileNormal = tileNormal,
            tilesProgresoDanio = new TileBase[] { tileDanioSuave, tileDanioFuerte },
            esBloqueDificil = true
        };

        InicializarMapping(mapping);

        var bloque = new Bloque(TipoBloque.Piedra, true, 2, Vector3Int.zero, tileNormal);
        bloque.Golpear(TipoHerramienta.Pico);

        gameManager.ActualizarAparienciaBloque(bloque);

    Assert.That(tilemap.GetTile(Vector3Int.zero), Is.EqualTo(tileDanioFuerte));
    }
}
