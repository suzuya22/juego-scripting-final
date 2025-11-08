using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

// Control sencillo del personaje jugador. Maneja movimiento, selección de herramienta y rotación del sprite.
public class PlayerController
{
    public Vector3Int LadoMirar { get; private set; } = Vector3Int.right;
    public TipoHerramienta HerramientaActual { get; private set; } = TipoHerramienta.Pico;
    public Vector3Int Posicion { get; private set; }

    private readonly Transform transformPersonaje;
    private readonly GridManager mallaReferencia;
    private readonly GameManager gameManager;

    private bool puedeMoverse = true;

    public PlayerController(Vector3Int posicionInicial, Transform transform, GridManager malla, GameManager manager)
    {
        Posicion = posicionInicial;
        transformPersonaje = transform;
        mallaReferencia = malla;
        gameManager = manager;

        if (transformPersonaje != null && mallaReferencia?.tilemap != null)
        {
            Vector3 posicionInicialMundo = mallaReferencia.tilemap.CellToWorld(Posicion) + Vector3.one * 0.5f;
            transformPersonaje.position = posicionInicialMundo;
        }

        gameManager?.ActualizarSpriteDireccion(LadoMirar);
    }

    public void Tick()
    {
        ProcesarMovimiento();
        ProcesarHerramientas();
    }

    public void SetHerramienta(TipoHerramienta herramienta)
    {
        HerramientaActual = herramienta;
        gameManager?.ActualizarUIHerramienta(HerramientaActual);
    }

    public void PermitirMovimiento(bool habilitado)
    {
        puedeMoverse = habilitado;
    }

    private void ProcesarMovimiento()
    {
        if (!puedeMoverse) return;

        Vector3Int direccion = Vector3Int.zero;

        if (Input.GetKeyDown(KeyCode.W)) direccion = Vector3Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) direccion = Vector3Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) direccion = Vector3Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) direccion = Vector3Int.right;

        if (direccion == Vector3Int.zero) return;

        CambiarDireccion(direccion);
        IniciarMovimientoInstantaneo();
    }

    private void ProcesarHerramientas()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SetHerramienta(TipoHerramienta.Pico);
            AccionRomper();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            SetHerramienta(TipoHerramienta.Hacha);
            AccionRomper();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            SetHerramienta(TipoHerramienta.Taladro);
            AccionRomper();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            SetHerramienta(TipoHerramienta.Martillo);
            AccionRomper();
        }
    }

    private void CambiarDireccion(Vector3Int nuevaDireccion)
    {
        LadoMirar = nuevaDireccion;
        gameManager?.ActualizarSpriteDireccion(LadoMirar);
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

        while (true)
        {
            Vector3Int siguientePosicion = Posicion + LadoMirar;

            if (mallaReferencia != null && !mallaReferencia.EsPosicionValida(siguientePosicion))
            {
                break;
            }

            Posicion = siguientePosicion;

            if (transformPersonaje != null && mallaReferencia?.tilemap != null)
            {
                Vector3 worldPos = mallaReferencia.tilemap.CellToWorld(Posicion) + Vector3.one * 0.5f;
                transformPersonaje.position = worldPos;
            }

            if (gameManager != null && gameManager.EsPosicionMeta(Posicion))
            {
                gameManager.NivelCompletado();
                break;
            }

            yield return new WaitForSeconds(0.05f);
        }

        puedeMoverse = true;
    }

    private void AccionRomper()
    {
        if (mallaReferencia == null) return;

        Vector3Int posicionObjetivo = Posicion + LadoMirar;
        mallaReferencia.DestruirBloque(posicionObjetivo, HerramientaActual);
    }
}
