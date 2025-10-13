// language: csharp
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameManagerTests
{
    private GameObject go;
    private GameObject gridGo;
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private GameManager gameManager;

    private Tile tilePiedra;
    private Tile tilePared;
    private Tile tileMeta;
    private Tile tileMadera;

    [SetUp]
    public void SetUp()
    {
        // Crear GameObject contenedor y GameManager
        go = new GameObject("GM_Test");
        gameManager = go.AddComponent<GameManager>();

        // Crear Grid y Tilemap
        gridGo = new GameObject("Grid");
        gridGo.transform.parent = go.transform;
        gridGo.AddComponent<Grid>();
        GameObject tilemapObj = new GameObject("Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
        tilemapObj.transform.parent = gridGo.transform;
        tilemap = tilemapObj.GetComponent<Tilemap>();
        tilemapRenderer = tilemapObj.GetComponent<TilemapRenderer>();

        // Asignar tilemap al GameManager
        gameManager.tilemapNivel = tilemap;

        // Crear tiles concretos
        tilePiedra = ScriptableObject.CreateInstance<Tile>();
        tilePared = ScriptableObject.CreateInstance<Tile>();
        tileMeta = ScriptableObject.CreateInstance<Tile>();
        tileMadera = ScriptableObject.CreateInstance<Tile>();

        // Montar mappingTiles para que ObtenerTipoBloqueDesdeAsset funcione
        GameManager.TileAssetMapping mapPiedra = new GameManager.TileAssetMapping { tipo = TipoBloque.Piedra, tileNormal = tilePiedra, tilesProgresoDaño = new TileBase[0] };
        GameManager.TileAssetMapping mapPared = new GameManager.TileAssetMapping { tipo = TipoBloque.Pared, tileNormal = tilePared, tilesProgresoDaño = new TileBase[0] };
        GameManager.TileAssetMapping mapMeta = new GameManager.TileAssetMapping { tipo = TipoBloque.Meta, tileNormal = tileMeta, tilesProgresoDaño = new TileBase[0] };
        GameManager.TileAssetMapping mapMadera = new GameManager.TileAssetMapping { tipo = TipoBloque.Madera, tileNormal = tileMadera, tilesProgresoDaño = new TileBase[0] };

        gameManager.mappingTiles = new GameManager.TileAssetMapping[] { mapPiedra, mapPared, mapMeta, mapMadera };

        // No tiles "difíciles" por defecto
        gameManager.tilesBloquesDificiles = new TileBase[0];

        // Invocar método privado InicializarMappingTiles para poblar diccionarios internos
        MethodInfo mi = typeof(GameManager).GetMethod("InicializarMappingTiles", BindingFlags.Instance | BindingFlags.NonPublic);
        if (mi != null)
            mi.Invoke(gameManager, null);
        else
            Assert.Fail("No se encontró el método InicializarMappingTiles por reflexión");
    }

    [TearDown]
    public void TearDown()
    {
        if (go != null) UnityEngine.Object.DestroyImmediate(go);
        if (gridGo != null) UnityEngine.Object.DestroyImmediate(gridGo);
    }

    [Test]
    public void Bloque_PuedeSerDestruidoYGolpear_FuncionaCorrectamente()
    {
        Vector3Int pos = Vector3Int.zero;
        Bloque bloque = new Bloque(TipoBloque.Piedra, false, 2, pos, tilePiedra);

        // Solo pico puede destruir piedra
        Assert.IsTrue(bloque.PuedeSerDestruidoPor(TipoHerramienta.Pico));
        Assert.IsFalse(bloque.PuedeSerDestruidoPor(TipoHerramienta.Hacha));
        Assert.IsFalse(bloque.PuedeSerDestruidoPor(TipoHerramienta.Taladro));
        Assert.IsFalse(bloque.PuedeSerDestruidoPor(TipoHerramienta.Martillo));

        // Golpear dos veces: primero false (queda 1), segundo true (se destruye)
        bool first = bloque.Golpear(TipoHerramienta.Pico);
        Assert.IsFalse(first);
        Assert.AreEqual(1, bloque.CantidadActual);

        bool second = bloque.Golpear(TipoHerramienta.Pico);
        Assert.IsTrue(second);
        Assert.AreEqual(0, bloque.CantidadActual);
    }

    [Test]
    public void Nodos_SecuenciaSelectorTareas_ComportamientoEsperado()
    {
        // Tareas que devuelven true y false
        Tareas tareaTrue = new Tareas(() => true);
        Tareas tareaFalse = new Tareas(() => false);

        // Secuencia: debe fallar si algún hijo falla
        SecuenciaJuego sec = new SecuenciaJuego();
        sec.AgregarHijo(tareaTrue);
        sec.AgregarHijo(tareaFalse);
        sec.AgregarHijo(tareaTrue);
        Assert.IsFalse(sec.Ejecutar());

        // Selector sin condicion: devuelve true si algún hijo true
        SelectorJuego sel = new SelectorJuego();
        sel.AgregarHijo(tareaFalse);
        sel.AgregarHijo(tareaTrue);
        Assert.IsTrue(sel.Ejecutar());

        // Selector con condicion que evalua false: no ejecuta hijos y devuelve false
        SelectorJuego selCond = new SelectorJuego(() => false);
        selCond.AgregarHijo(tareaTrue);
        Assert.IsFalse(selCond.Ejecutar());
    }

    [Test]
    public void Malla_GetNextBlockYDestruirBloque_EscenarioBasico()
    {
        Vector3Int posPiedra = new Vector3Int(0, 0, 0);
        Vector3Int posPerson = new Vector3Int(-1, 0, 0);

        // Poner la piedra en (0,0)
        tilemap.SetTile(posPiedra, tilePiedra);

        // Poner una pared en (1,0)
        Vector3Int posPared = new Vector3Int(1, 0, 0);
        tilemap.SetTile(posPared, tilePared);

        // Crear Malla (usa gameManager para mapear)
        Malla malla = new Malla(tilemap, gameManager, 1);

        // Obtener siguiente bloque desde la posición del personaje hacia la derecha
        Bloque next = malla.GetNextBlock(posPerson, Vector3Int.right);
        Assert.IsNotNull(next);
        Assert.AreEqual(TipoBloque.Piedra, next.Tipo);

        // Destruir bloque con herramienta correcta (Pico)
        bool destruyo = malla.DestruirBloque(posPiedra, TipoHerramienta.Pico);
        Assert.IsTrue(destruyo);

        // Tilemap ya no debe tener tile en esa posición
        Assert.IsNull(tilemap.GetTile(posPiedra));

        // Ahora GetNextBlock debe devolver null para esa posición
        Bloque nextAfter = malla.GetNextBlock(posPerson, Vector3Int.right);
        Assert.IsNull(nextAfter);

        // EsPosicionValida: pared es inválida
        Assert.IsFalse(malla.EsPosicionValida(posPared));

        // Meta: colocar tileMeta en (2,0) y probar que es inválida
        Vector3Int posMeta = new Vector3Int(2, 0, 0);
        tilemap.SetTile(posMeta, tileMeta);
        Assert.IsFalse(malla.EsPosicionValida(posMeta));

        // Espacio vacío (3,0) debe ser válido
        Vector3Int posVacio = new Vector3Int(3, 0, 0);
        Assert.IsTrue(malla.EsPosicionValida(posVacio));
    }

    [Test]
    public void Temporizador_ActualizarContador_FormatoYColor()
    {
        GameObject txtGo = new GameObject("TMP");
        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();

        Temporizador temp = new Temporizador(120f, tmp);

        // 75 segundos -> 01:15 -> color blanco
        temp.ActualizarContador(75f, 120f);
        Assert.AreEqual("01:15", tmp.text);
        Assert.AreEqual(Color.white, tmp.color);

        // 25 segundos -> 00:25 -> color amarillo
        temp.ActualizarContador(25f, 120f);
        Assert.AreEqual("00:25", tmp.text);
        Assert.AreEqual(Color.yellow, tmp.color);

        // 8 segundos -> 00:08 -> color rojo
        temp.ActualizarContador(8f, 120f);
        Assert.AreEqual("00:08", tmp.text);
        Assert.AreEqual(Color.red, tmp.color);

        UnityEngine.Object.DestroyImmediate(txtGo);
    }
}