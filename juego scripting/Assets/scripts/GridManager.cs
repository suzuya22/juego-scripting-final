using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
public class GridManager
{
    private Dictionary<Vector3Int, Bloque> bloques = new Dictionary<Vector3Int, Bloque>();
    public Tilemap tilemap;
    public GameManager gameManager;

    public int Nivel { get; set; }
    public Vector3Int TamanoMalla { get; private set; }

    public GridManager(Tilemap tilemap, GameManager gameManager, int nivel)
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

        // Solo las paredes bloquean el movimiento
        // Spawn y Meta son transitables
        if (tipo == TipoBloque.Pared) return false;

        // Si hay un bloque destructible en esa posición, no es válida
        if (bloques.ContainsKey(posicion)) return false;

        // Cualquier otro caso es válido (espacio vacío, spawn, meta, etc.)
        return true;
    }

    public void ActualizarDanioBloques()
    {
        DibujarMatriz();
    }
}
