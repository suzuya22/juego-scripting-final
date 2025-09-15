using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;


public enum TipoBloque
{
    Vacio = 0,
    Pared = 1,
    Piedra = 2,
    Madera = 3,
    Metal = 4,
    Cristal = 5,
    Spawn = 6,
    Meta = 7
}

public enum TipoHerramienta
{
    Ninguna = 0,
    Pico = 1,     
    Hacha = 2,     
    Taladro = 3,   
    Martillo = 4   
}


public abstract class Nodo
{
    public abstract bool Ejecutar();
}

public class Raiz
{
    private Nodo hijo;

    public Raiz(Nodo hijo)
    {
        this.hijo = hijo;
    }

    public bool Ejecutar()
    {
        return hijo?.Ejecutar() ?? false;
    }
}

// Clase base abstracta para nodos compuestos (que pueden tener hijos, junto al flujo del arbol)
public abstract class JuegoComposite : Nodo
{
    protected List<Nodo> hijos = new List<Nodo>();

    public void AgregarHijo(Nodo hijo)
    {
        hijos.Add(hijo);
    }

    public List<Nodo> ObtenerHijos()
    {
        return new List<Nodo>(hijos);
    }

    public abstract override bool Ejecutar();
}


public class SecuenciaJuego : JuegoComposite
{
    public override bool Ejecutar()
    {
        foreach (var hijo in hijos)
        {
            if (!hijo.Ejecutar())
            {
                return false;
            }
        }
        return true;
    }
}


public class SelectorJuego : JuegoComposite
{
    private System.Func<bool> condicion;

    public SelectorJuego(System.Func<bool> condicion = null)
    {
        this.condicion = condicion;
    }

    public override bool Ejecutar()
    {
        if (condicion != null && !Evaluar())
        {
            return false;
        }

        foreach (var hijo in hijos)
        {
            if (hijo.Ejecutar())
            {
                return true;
            }
        }
        return false;
    }

    public bool Evaluar()
    {
        return condicion?.Invoke() ?? true;
    }
}


public class Tareas : Nodo
{
    private System.Func<bool> tarea;

    public Tareas(System.Func<bool> tarea)
    {
        this.tarea = tarea;
    }

    public override bool Ejecutar()
    {
        return tarea?.Invoke() ?? false;
    }
}

// Clase que representa un bloque en el juego y sus alteraciones en el inspector para manejarlo mas sencillo desde ahi
[System.Serializable]
public class Bloque
{
    public int ID { get; private set; }
    public TipoBloque Tipo { get; private set; }
    public bool BloqueDificil { get; private set; }
    public int CantidadParaRomper { get; private set; }
    public int CantidadActual { get; private set; }
    public Vector3Int Posicion { get; private set; }
    public TileBase TileOriginal { get; private set; }

    public Bloque(TipoBloque tipo, bool bloqueDificil, int cantidadRomper, Vector3Int posicion, TileBase tileOriginal)
    {
        this.Tipo = tipo;
        this.ID = (int)tipo;
        this.BloqueDificil = bloqueDificil;
        this.CantidadParaRomper = cantidadRomper;
        this.CantidadActual = cantidadRomper;
        this.Posicion = posicion;
        this.TileOriginal = tileOriginal;
    }

    public bool PuedeSerDestruidoPor(TipoHerramienta herramienta)
    {
        return (Tipo == TipoBloque.Piedra && herramienta == TipoHerramienta.Pico) ||
               (Tipo == TipoBloque.Madera && herramienta == TipoHerramienta.Hacha) ||
               (Tipo == TipoBloque.Metal && herramienta == TipoHerramienta.Taladro) ||
               (Tipo == TipoBloque.Cristal && herramienta == TipoHerramienta.Martillo);
    }

    public bool Golpear(TipoHerramienta herramienta)
    {
        if (!PuedeSerDestruidoPor(herramienta)) return false;

        CantidadActual--;
        return CantidadActual <= 0;
    }

    public void Destruir(Tilemap tilemap)
    {
        if (tilemap != null)
        {
            tilemap.SetTile(Posicion, null);
        }
    }
}

// Clase que maneja la malla/grilla del juego integrada con Tilemap
public class Malla : Nodo
{
    private Dictionary<Vector3Int, Bloque> bloques = new Dictionary<Vector3Int, Bloque>();
    public Tilemap tilemap;
    public GameManager gameManager;

    public int Nivel { get; set; }
    public Vector3Int TamanoMalla { get; private set; }

    public Malla(Tilemap tilemap, GameManager gameManager, int nivel)
    {
        this.tilemap = tilemap;
        this.gameManager = gameManager;
        this.Nivel = nivel;
        CargarBloquesDesdeTilemap();
    }

    private void CargarBloquesDesdeTilemap()
    {
        bloques.Clear();
        BoundsInt bounds = tilemap.cellBounds;
        TamanoMalla = new Vector3Int(bounds.size.x, bounds.size.y, 1);

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                TipoBloque tipo = gameManager.ObtenerTipoBloqueDesdeAsset(tile);
                if (tipo != TipoBloque.Vacio && tipo != TipoBloque.Pared)
                {
                    bool esDificil = gameManager.EsBloqueDificil(tile);
                    int cantidadRomper = esDificil ? UnityEngine.Random.Range(2, 4) : 1;

                    Bloque nuevoBloque = new Bloque(tipo, esDificil, cantidadRomper, pos, tile);
                    bloques[pos] = nuevoBloque;
                }
            }
        }
    }

    public void DibujarMatriz()
    {
        // La matriz ya está dibujada en el tilemap, esto aqui es para futura implemntacion de sprites que se ve que se estan destruyendo 
        foreach (var kvp in bloques)
        {
            Bloque bloque = kvp.Value;
            if (bloque.CantidadActual != bloque.CantidadParaRomper)
            {
                // Cambiar apariencia para mostrar daño (desde el inspector se hace la referencia a estos sprites de transicion a bloque vacio)
                gameManager.ActualizarAparienciaBloque(bloque);
            }
        }
    }

    public Bloque GetNextBlock(Vector3Int posicionPersonaje, Vector3Int direccion)
    {
        Vector3Int posicionObjetivo = posicionPersonaje + direccion;

        if (bloques.ContainsKey(posicionObjetivo))
        {
            return bloques[posicionObjetivo];
        }

        // Verificar si hay una pared (tile que no es destruible y solo funciona de bordes del mapa y caminos)
        TileBase tile = tilemap.GetTile(posicionObjetivo);
        if (tile != null)
        {
            TipoBloque tipo = gameManager.ObtenerTipoBloqueDesdeAsset(tile);
            if (tipo == TipoBloque.Pared)
            {
                return new Bloque(TipoBloque.Pared, false, int.MaxValue, posicionObjetivo, tile);
            }
        }

        return null; // Espacio vacío
    }

    public bool DestruirBloque(Vector3Int posicion, TipoHerramienta herramienta)
    {
        if (bloques.ContainsKey(posicion))
        {
            Bloque bloque = bloques[posicion];
            if (bloque.Golpear(herramienta))
            {
                bloque.Destruir(tilemap);
                bloques.Remove(posicion);
                return true;
            }
        }
        return false;
    }

    public bool EsPosicionValida(Vector3Int posicion)
    {
        TileBase tile = tilemap.GetTile(posicion);

        // Si no hay tile, es espacio vacío (válido)
        if (tile == null) return true;

        // Verificar el tipo de tile
        TipoBloque tipo = gameManager.ObtenerTipoBloqueDesdeAsset(tile);

        // Las paredes y metas no son válidas para moverse
        if (tipo == TipoBloque.Pared || tipo == TipoBloque.Meta) return false;

        // Si hay un bloque destructible en esa posición, no es válida
        if (bloques.ContainsKey(posicion)) return false;

        // Cualquier otro caso es válido (espacio vacío con tile de fondo, etc.)
        return true;
    }

    public override bool Ejecutar()
    {
        DibujarMatriz();
        return true;
    }
}

// Clase del personaje jugador
public class Personaje : Nodo
{
    public Vector3Int LadoMirar { get; private set; } = Vector3Int.right;
    public TipoHerramienta HerramientaActual { get; private set; } = TipoHerramienta.Pico;
    public Vector3Int Posicion { get; private set; }

    private Transform transformPersonaje;
    private Malla mallaReferencia;
    private GameManager gameManager;
    private bool puedeMoverse = true;

    private Dictionary<KeyCode, TipoHerramienta> teclaHerramienta = new Dictionary<KeyCode, TipoHerramienta>
    {
        { KeyCode.H, TipoHerramienta.Pico },      // Piedra
        { KeyCode.J, TipoHerramienta.Hacha },     // Madera
        { KeyCode.K, TipoHerramienta.Taladro },   // Metal
        { KeyCode.L, TipoHerramienta.Martillo }   // Cristal
    };

    private Dictionary<KeyCode, Vector3Int> teclaDireccion = new Dictionary<KeyCode, Vector3Int>
    {
        { KeyCode.W, Vector3Int.up },
        { KeyCode.S, Vector3Int.down },
        { KeyCode.A, Vector3Int.left },
        { KeyCode.D, Vector3Int.right }
    };

    public Personaje(Vector3Int posicionInicial, Transform transform, Malla malla, GameManager gameManager)
    {
        this.Posicion = posicionInicial;
        this.transformPersonaje = transform;
        this.mallaReferencia = malla;
        this.gameManager = gameManager;

        // Posicionar el transform del personaje
        if (transformPersonaje != null && mallaReferencia != null && mallaReferencia.tilemap != null)
        {
            transformPersonaje.position = mallaReferencia.tilemap.CellToWorld(Posicion) + Vector3.one * 0.5f;
        }
    }

    private void KeyPressed(KeyCode key)
    {
        if (!puedeMoverse) return;

        // Movimiento direccional (esta planteado para ir recto en la direccion que se presiono el boton, hasta chocar)
        if (teclaDireccion.ContainsKey(key))
        {
            Vector3Int nuevaDireccion = teclaDireccion[key];
            CambiarDireccion(nuevaDireccion);
            IniciarMovimientoInstantaneo();
        }

        // Acción de romper bloques 
        if (teclaHerramienta.ContainsKey(key))
        {
            CambiarDeHerramienta(teclaHerramienta[key]);
            if (gameManager != null)
            {
                gameManager.ActualizarUIHerramienta(HerramientaActual);
            }
            AccionRomper();
        }
    }

    private void CambiarDeHerramienta(TipoHerramienta herramienta)
    {
        HerramientaActual = herramienta;
    }

    private void CambiarDireccion(Vector3Int nuevaDireccion)
    {
        LadoMirar = nuevaDireccion;
    }

    private void IniciarMovimientoInstantaneo()
    {
        if (gameManager != null)
        {
            gameManager.StartCoroutine(MovimientoInstantaneoCoroutine());
        }
    }

    private IEnumerator MovimientoInstantaneoCoroutine()
    {
        puedeMoverse = false;

        // Movimiento instantáneo hasta encontrar obstáculo
        while (true)
        {
            Vector3Int siguientePosicion = Posicion + LadoMirar;

            // Verificar si puede moverse a la siguiente posición (todo esto se maneja con la malla)
            if (mallaReferencia != null && !mallaReferencia.EsPosicionValida(siguientePosicion))
            {
                break; // Se detiene al encontrar obstáculo
            }

            // Moverse instantáneamente
            Posicion = siguientePosicion;

            // Actualizar posición visual con pequeño delay para visualizar el movimiento
            if (transformPersonaje != null && mallaReferencia != null && mallaReferencia.tilemap != null)
            {
                Vector3 worldPos = mallaReferencia.tilemap.CellToWorld(Posicion) + Vector3.one * 0.5f;
                transformPersonaje.position = worldPos;
            }

            // Verificar si llegó a la meta
            if (mallaReferencia != null && mallaReferencia.tilemap != null)
            {
                TileBase tileActual = mallaReferencia.tilemap.GetTile(Posicion);
                if (tileActual != null && gameManager != null && gameManager.ObtenerTipoBloqueDesdeAsset(tileActual) == TipoBloque.Meta)
                {
                    gameManager.NivelCompletado();
                    break;
                }
            }

            // delay para visualizar el movimiento
            yield return new WaitForSeconds(0.05f);
        }

        puedeMoverse = true;
    }

    private void AccionRomper()
    {
        if (mallaReferencia != null)
        {
            Vector3Int posicionObjetivo = Posicion + LadoMirar;
            mallaReferencia.DestruirBloque(posicionObjetivo, HerramientaActual);
        }
    }

    public override bool Ejecutar()
    {
  
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                KeyPressed(key);
                break;
            }
        }
        return true;
    }
}

// Clase del temporizador
public class Temporizador : Nodo
{
    public float TiempoActual { get; private set; }
    public float TiempoLimite { get; set; }
    public TextMeshProUGUI TextoTiempo { get; private set; }   // ← mejor lo cambiamos a textmesh pro pq es mas facil de manejar xd

    public Temporizador(float tiempoLimite, TextMeshProUGUI texto)
    {
        TiempoLimite = tiempoLimite;
        TextoTiempo = texto;
        TiempoActual = tiempoLimite;
    }

    public void ActualizarContador(float tiempoActual, float tiempoLimite)
    {
        this.TiempoActual = tiempoActual;

        if (TextoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(tiempoActual / 60);
            int segundos = Mathf.FloorToInt(tiempoActual % 60);
            TextoTiempo.text = $"{minutos:00}:{segundos:00}";

            if (tiempoActual <= 10f)
                TextoTiempo.color = Color.red;
            else if (tiempoActual <= 30f)
                TextoTiempo.color = Color.yellow;
            else
                TextoTiempo.color = Color.white;
        }
    }

    public override bool Ejecutar()
    {
        TiempoActual -= Time.deltaTime;
        ActualizarContador(TiempoActual, TiempoLimite);
        return TiempoActual > 0;
    }
}


// Codigo main (esta todo aqui para aplicar las cosas de manera mas sencilla en el inspector, desde un solo script y creando un componente en la jerarquia que lo contenga se pueden referenciar todos y hacer una implementacion mas dinamica de cada tile, (creamos como un menun para crear tiles basicamente)) 
public class GameManager : MonoBehaviour
{
    [Header("Referencias del Tilemap")]
    public Tilemap tilemapNivel;
    public Transform personajeTransform;

    [Header("Configuración de Tiles")]
    public TileAssetMapping[] mappingTiles;

    [Header("UI")]
    public TextMeshProUGUI textoTiempo;
    public UnityEngine.UI.Image iconoHerramientaActual;
    public Sprite[] iconosHerramientas;

    [Header("Configuración del Juego")]
    public float tiempoLimiteNivel = 60f;
    public Vector3Int posicionInicialPersonaje = Vector3Int.zero;

    [Header("Tiles de Bloques Difíciles")]
    public TileBase[] tilesBloquesDificiles;

    private Raiz raizJuego;
    private Personaje personaje;
    private Malla malla;
    private Temporizador temporizador;
    private Dictionary<TileBase, TipoBloque> tilesToTipo;
    private Dictionary<TipoBloque, TileBase[]> tipoToTilesDañados;

    [System.Serializable]
    public class TileAssetMapping
    {
        public TipoBloque tipo;
        public TileBase tileNormal;
        public TileBase[] tilesProgresoDaño;
    }

    void Start()
    {
        InicializarMappingTiles();
        BuscarPosicionInicialPersonaje();
        InicializarJuego();
    }
    private void ActualizarPosicionUIHerramienta()
    {
        if (iconoHerramientaActual != null && personajeTransform != null)
        {
            Vector3 posPantalla = Camera.main.WorldToScreenPoint(personajeTransform.position);
            iconoHerramientaActual.transform.position = posPantalla + new Vector3(40f, 40f, 0f);
        
        }
    }

    private void InicializarMappingTiles()
    {
        tilesToTipo = new Dictionary<TileBase, TipoBloque>();
        tipoToTilesDañados = new Dictionary<TipoBloque, TileBase[]>();

        if (mappingTiles != null)
        {
            foreach (var mapping in mappingTiles)
            {
                if (mapping.tileNormal != null)
                {
                    tilesToTipo[mapping.tileNormal] = mapping.tipo;
                }
                if (mapping.tilesProgresoDaño != null && mapping.tilesProgresoDaño.Length > 0)
                {
                    tipoToTilesDañados[mapping.tipo] = mapping.tilesProgresoDaño;
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
            if (tile != null && ObtenerTipoBloqueDesdeAsset(tile) == TipoBloque.Spawn)
            {
                posicionInicialPersonaje = pos;
                tilemapNivel.SetTile(pos, null);
                break;
            }
        }
    }

    private void InicializarJuego()
    {
        malla = new Malla(tilemapNivel, this, 1);
        personaje = new Personaje(posicionInicialPersonaje, personajeTransform, malla, this);
        temporizador = new Temporizador(tiempoLimiteNivel, textoTiempo);

        SecuenciaJuego secuenciaPrincipal = new SecuenciaJuego();

        secuenciaPrincipal.AgregarHijo(new Tareas(() => temporizador.Ejecutar()));
        secuenciaPrincipal.AgregarHijo(new Tareas(() => personaje.Ejecutar()));
        secuenciaPrincipal.AgregarHijo(new Tareas(() => malla.Ejecutar()));

        SelectorJuego selectorEstado = new SelectorJuego(() => temporizador.TiempoActual > 0);
        selectorEstado.AgregarHijo(secuenciaPrincipal);
        selectorEstado.AgregarHijo(new Tareas(() => ProcesarFinJuego()));

        raizJuego = new Raiz(selectorEstado);

        ActualizarUIHerramienta(personaje.HerramientaActual);
    }

    void Update()
    {
        if (raizJuego != null)
        {
            bool resultado = raizJuego.Ejecutar();
            if (!resultado)
            {
                Debug.Log("El juego ha terminado");
            }
        }

        // Actualizar posición de UI de herramienta para que siga al personaje  (falta corregir tamaños desde la jerarquia, tambien implementar que las herramientas cambien de sprite conforme se presione la tecla adecuada de esta)
        ActualizarPosicionUIHerramienta();
    }

    public void EstablecerPosicionInicialPersonaje(Vector3Int posicion)
    {
        posicionInicialPersonaje = posicion;
    }

    public TipoBloque ObtenerTipoBloqueDesdeAsset(TileBase tile)
    {
        if (tilesToTipo != null && tilesToTipo.ContainsKey(tile))
        {
            return tilesToTipo[tile];
        }
        return TipoBloque.Pared;
    }

    public bool EsBloqueDificil(TileBase tile)
    {
        return tilesBloquesDificiles != null && System.Array.IndexOf(tilesBloquesDificiles, tile) >= 0;
    }

    public void ActualizarAparienciaBloque(Bloque bloque)
    {
        if (tipoToTilesDañados != null && tipoToTilesDañados.ContainsKey(bloque.Tipo))
        {
            TileBase[] tilesProgreso = tipoToTilesDañados[bloque.Tipo];
            if (tilesProgreso != null && tilesProgreso.Length > 0)
            {
                float progreso = 1f - ((float)bloque.CantidadActual / bloque.CantidadParaRomper);
                int indiceProgreso = Mathf.FloorToInt(progreso * tilesProgreso.Length);
                indiceProgreso = Mathf.Clamp(indiceProgreso, 0, tilesProgreso.Length - 1);

                if (tilemapNivel != null)
                {
                    tilemapNivel.SetTile(bloque.Posicion, tilesProgreso[indiceProgreso]);
                }
            }
        }
    }

    public void ActualizarUIHerramienta(TipoHerramienta herramienta)
    {
        if (iconoHerramientaActual != null && iconosHerramientas != null)
        {
            int indice = (int)herramienta - 1;
            if (indice >= 0 && indice < iconosHerramientas.Length)
            {
                iconoHerramientaActual.sprite = iconosHerramientas[indice];
            }
        }
    }

    public void NivelCompletado()
    {
        Debug.Log("Nivel completado!");
    }

    private bool ProcesarFinJuego()
    {
        Debug.Log("Tiempo agotado! Fin del juego");
        return false;
    }

    public void ReiniciarNivel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}