using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManagerTests
{
    private GameObject gridGO;
    private GameObject tilemapGO;
    private Tilemap tilemap;
    private readonly List<GameObject> objetosCreados = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        gridGO = new GameObject("Grid", typeof(Grid));
        tilemapGO = new GameObject("Tilemap", typeof(Tilemap));
        tilemapGO.transform.SetParent(gridGO.transform);
        tilemap = tilemapGO.GetComponent<Tilemap>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var extra in objetosCreados)
        {
            if (extra != null)
            {
                Object.DestroyImmediate(extra);
            }
        }

        objetosCreados.Clear();
        Object.DestroyImmediate(tilemapGO);
        Object.DestroyImmediate(gridGO);
    }

    private GameManager CrearGameManager(TileBase tileNormal, TipoBloque tipo, bool esDificil = false, TileBase[] progresoDanio = null, TileBase[] tilesPared = null)
    {
        var gmGO = new GameObject("GameManager");
        var gameManager = gmGO.AddComponent<GameManager>();
        gameManager.tilemapNivel = tilemap;
    objetosCreados.Add(gmGO);

        var mapping = new GameManager.TileAssetMapping
        {
            tipo = tipo,
            tileNormal = tileNormal,
            tilesProgresoDanio = progresoDanio ?? System.Array.Empty<TileBase>(),
            esBloqueDificil = esDificil
        };

        gameManager.mappingTiles = new[] { mapping };

        if (tilesPared != null)
        {
            var fieldParedes = typeof(GameManager).GetField("tilesPared", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldParedes.SetValue(gameManager, tilesPared);
        }

        var method = typeof(GameManager).GetMethod("InicializarMappingTiles", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(gameManager, null);

        return gameManager;
    }

    [Test]
    public void GridManager_RegistraBloquesDestructibles()
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tilemap.SetTile(Vector3Int.zero, tile);

        var gameManager = CrearGameManager(tile, TipoBloque.Piedra);
        var gridManager = new GridManager(tilemap, gameManager, 1);

        var bloque = gridManager.GetNextBlock(Vector3Int.left, Vector3Int.right);

    Assert.That(bloque, Is.Not.Null);
    Assert.That(bloque.Tipo, Is.EqualTo(TipoBloque.Piedra));
    }

    [Test]
    public void DestruirBloque_EliminaTileYDevuelveTrue()
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tilemap.SetTile(Vector3Int.zero, tile);

        var gameManager = CrearGameManager(tile, TipoBloque.Piedra);
        var gridManager = new GridManager(tilemap, gameManager, 1);

        bool destruido = gridManager.DestruirBloque(Vector3Int.zero, TipoHerramienta.Pico);

    Assert.That(destruido, Is.True);
    Assert.That(tilemap.GetTile(Vector3Int.zero), Is.Null);
    }

    [Test]
    public void EsPosicionValida_FalseParaParedTrueParaEspacioVacio()
    {
        var tilePared = ScriptableObject.CreateInstance<Tile>();
        tilemap.SetTile(Vector3Int.right, tilePared);

        var gameManager = CrearGameManager(null, TipoBloque.Vacio, false, null, new[] { tilePared });
        var gridManager = new GridManager(tilemap, gameManager, 1);

    Assert.That(gridManager.EsPosicionValida(Vector3Int.right), Is.False);
    Assert.That(gridManager.EsPosicionValida(Vector3Int.left), Is.True);
    }

        [Test]
        public void RestablecerBloques_RestituyeTilesDestruidos()
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tilemap.SetTile(Vector3Int.zero, tile);

            var gameManager = CrearGameManager(tile, TipoBloque.Piedra);
            var gridManager = new GridManager(tilemap, gameManager, 1);

            gridManager.DestruirBloque(Vector3Int.zero, TipoHerramienta.Pico);
            Assert.That(tilemap.GetTile(Vector3Int.zero), Is.Null);

            gridManager.RestablecerBloques();

            Assert.That(tilemap.GetTile(Vector3Int.zero), Is.EqualTo(tile));
        }
}
