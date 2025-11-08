using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

// GameManager maneja la configuración del nivel y coordina los sistemas principales.
public class GameManager : MonoBehaviour
{
    [Header("Referencias del Tilemap")]
    public Tilemap tilemapNivel;
    public Transform personajeTransform;

    [Header("Configuracion de Tiles")]
    public TileAssetMapping[] mappingTiles;

    [Header("UI")]
    public TextMeshProUGUI textoTiempo;
    public UIManager uiManager;
    public ToolUIDisplay toolUIDisplay;

    [Header("Configuracion del Juego")]
    public float tiempoLimiteNivel = 60f;
    public Vector3Int posicionInicialPersonaje = Vector3Int.zero;
    private Vector3Int posicionMeta = Vector3Int.zero;

    [Header("Tiles de Bloques Dificiles")]
    public TileBase[] tilesBloquesDificiles;

    [Header("Variantes de Pared")]
    [SerializeField] private TileBase[] tilesPared = new TileBase[10];

    [Header("Sprites Personaje Direcciones")]
    public Sprite spriteArriba;
    public Sprite spriteAbajo;
    public Sprite spriteIzquierda;
    public Sprite spriteDerecha;
    public SpriteRenderer personajeSpriteRenderer;

    private PlayerController personaje;
    private GridManager malla;
    private TimerManager temporizador;
    private Dictionary<TileBase, TipoBloque> tilesToTipo;
    private Dictionary<TipoBloque, TileBase[]> tipoToTilesDanio;
    private HashSet<TileBase> tilesDificiles;
    private bool juegoTerminado;

    public bool NivelActualCompletado { get; private set; }
    public int NivelesCompletados { get; private set; }

    [System.Serializable]
    public class TileAssetMapping
    {
        public TipoBloque tipo;
        public TileBase tileNormal;
    public TileBase[] tilesProgresoDanio = System.Array.Empty<TileBase>();
    [Tooltip("Marcar si este bloque requiere varios golpes (usa sprites de danio)")]
        public bool esBloqueDificil;
    }

    private void Start()
    {
        InicializarMappingTiles();
        BuscarPosicionInicialPersonaje();
        InicializarJuego();
    }

    private void Update()
    {
        if (juegoTerminado) return;

        bool tiempoRestante = temporizador != null && temporizador.Tick(Time.deltaTime);
        if (!tiempoRestante)
        {
            ReiniciarPorTiempoAgotado();
            return;
        }

        personaje?.Tick();
        malla?.ActualizarDanioBloques();
    }

    private void InicializarMappingTiles()
    {
        tilesToTipo = new Dictionary<TileBase, TipoBloque>();
    tipoToTilesDanio = new Dictionary<TipoBloque, TileBase[]>();
        tilesDificiles = new HashSet<TileBase>();

        if (mappingTiles != null)
        {
            foreach (var mapping in mappingTiles)
            {
                if (mapping == null) continue;

                if (mapping.tileNormal != null)
                {
                    tilesToTipo[mapping.tileNormal] = mapping.tipo;
                }

                if (mapping.tilesProgresoDanio != null && mapping.tilesProgresoDanio.Length > 0)
                {
                    tipoToTilesDanio[mapping.tipo] = mapping.tilesProgresoDanio;
                }

                if (mapping.esBloqueDificil)
                {
                    if (mapping.tileNormal != null)
                    {
                        tilesDificiles.Add(mapping.tileNormal);
                    }

                    if (mapping.tilesProgresoDanio != null)
                    {
                        foreach (var tile in mapping.tilesProgresoDanio)
                        {
                            if (tile != null)
                            {
                                tilesDificiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        if (tilesPared != null)
        {
            foreach (var tile in tilesPared)
            {
                if (tile != null)
                {
                    tilesToTipo[tile] = TipoBloque.Pared;
                }
            }
        }
    }

    private void BuscarPosicionInicialPersonaje()
    {
        if (tilemapNivel == null) return;

        BoundsInt bounds = tilemapNivel.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemapNivel.GetTile(pos);
            if (tile == null) continue;

            TipoBloque tipo = ObtenerTipoBloqueDesdeAsset(tile);

            if (tipo == TipoBloque.Spawn)
            {
                posicionInicialPersonaje = pos;
                tilemapNivel.SetTile(pos, null);
            }
            else if (tipo == TipoBloque.Meta)
            {
                posicionMeta = pos;
                tilemapNivel.SetTile(pos, null);
            }
        }
    }

    public void ActualizarSpriteDireccion(Vector3Int dir)
    {
        if (personajeSpriteRenderer == null) return;

        if (dir == Vector3Int.up && spriteArriba != null)
            personajeSpriteRenderer.sprite = spriteArriba;
        else if (dir == Vector3Int.down && spriteAbajo != null)
            personajeSpriteRenderer.sprite = spriteAbajo;
        else if (dir == Vector3Int.left && spriteIzquierda != null)
            personajeSpriteRenderer.sprite = spriteIzquierda;
        else if (dir == Vector3Int.right && spriteDerecha != null)
            personajeSpriteRenderer.sprite = spriteDerecha;
    }

    private void InicializarJuego()
    {
        malla = new GridManager(tilemapNivel, this, 1);
        personaje = new PlayerController(posicionInicialPersonaje, personajeTransform, malla, this);
        temporizador = new TimerManager(tiempoLimiteNivel, textoTiempo);

        juegoTerminado = false;
        NivelActualCompletado = false;
        personaje.PermitirMovimiento(true);
        ActualizarUIHerramienta(personaje.HerramientaActual);
        ActualizarSpriteDireccion(Vector3Int.right);
    }

    public void EstablecerPosicionInicialPersonaje(Vector3Int posicion)
    {
        posicionInicialPersonaje = posicion;
    }

    public bool EsPosicionMeta(Vector3Int posicion)
    {
        return posicion == posicionMeta;
    }

    public TipoBloque ObtenerTipoBloqueDesdeAsset(TileBase tile)
    {
        if (tile != null && tilesToTipo != null && tilesToTipo.TryGetValue(tile, out var tipo))
        {
            return tipo;
        }
        return TipoBloque.Pared;
    }

    public bool EsBloqueDificil(TileBase tile)
    {
        if (tile == null) return false;

        if (tilesDificiles != null && tilesDificiles.Contains(tile))
        {
            return true;
        }

        return tilesBloquesDificiles != null && System.Array.IndexOf(tilesBloquesDificiles, tile) >= 0;
    }

    public void ActualizarAparienciaBloque(Bloque bloque)
    {
        if (bloque == null) return;

    if (tipoToTilesDanio != null && tipoToTilesDanio.TryGetValue(bloque.Tipo, out var tilesProgreso) && tilesProgreso.Length > 0)
        {
            float progreso = 1f - ((float)bloque.CantidadActual / bloque.CantidadParaRomper);
            int indiceProgreso = Mathf.FloorToInt(progreso * tilesProgreso.Length);
            indiceProgreso = Mathf.Clamp(indiceProgreso, 0, tilesProgreso.Length - 1);

            tilemapNivel?.SetTile(bloque.Posicion, tilesProgreso[indiceProgreso]);
        }
    }

    public void ActualizarUIHerramienta(TipoHerramienta herramienta)
    {
        if (toolUIDisplay == null)
        {
            toolUIDisplay = FindObjectOfType<ToolUIDisplay>(true);
            if (toolUIDisplay == null)
            {
                Debug.LogWarning("GameManager: No se encontró ToolUIDisplay en la escena para actualizar la UI de herramientas.");
                return;
            }
        }

        toolUIDisplay.ActualizarVisualizacion(herramienta);
    }

    public void NivelCompletado()
    {
        if (juegoTerminado) return;

        Debug.Log("Nivel completado!");
        NivelActualCompletado = true;
        NivelesCompletados++;
        juegoTerminado = true;
        temporizador?.Detener();
        personaje?.PermitirMovimiento(false);

        uiManager?.MostrarPanelNivelCompletado();
    }

    private void ReiniciarPorTiempoAgotado()
    {
        if (juegoTerminado) return;

        StopAllCoroutines();
        malla?.RestablecerBloques();
        personaje?.ReiniciarPosicion(posicionInicialPersonaje);
        personaje?.PermitirMovimiento(true);
        temporizador?.Reiniciar();
        NivelActualCompletado = false;
    }

    private void ProcesarFinJuego()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        temporizador?.Detener();
        personaje?.PermitirMovimiento(false);
        Debug.Log("Tiempo agotado! Fin del juego");
    }

    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}