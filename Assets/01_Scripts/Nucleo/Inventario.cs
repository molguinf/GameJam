// =====================================================================
// Inventario.cs                          MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Guarda los objetos que recoge el jugador y los muestra en la barra de
// abajo. El jugador hace clic en una casilla para SELECCIONAR un objeto
// y luego hace clic en el mundo para usarlo.
//
// También combina piezas automáticamente (recetas):
//   escalera_1 + escalera_2 + escalera_3  =>  escalera
//
// Cada casilla de la UI es un Button. Su OnClick llama a
// Inventario.SeleccionarCasilla(numero de casilla: 0, 1, 2...).
// =====================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ObjetoInventario
{
    public string id;
    public string nombre;
    public Sprite icono;
}

[System.Serializable]
public class RecetaCombinacion
{
    [Tooltip("Ids de las piezas que se combinan. Ejemplo: escalera_1, escalera_2, escalera_3")]
    public string[] idsPiezas;
    public string idResultado;
    public string nombreResultado;
    public Sprite iconoResultado;
    [TextArea]
    public string mensajeAlCombinar;
}

public class Inventario : MonoBehaviour
{
    public static Inventario Instancia;

    [Header("Interfaz (barra de abajo)")]
    [Tooltip("Las imágenes de los iconos de cada casilla, en orden.")]
    public Image[] iconosCasillas;
    [Tooltip("Marcos que se encienden cuando la casilla está seleccionada (opcional).")]
    public Image[] marcosSeleccion;

    [Header("Recetas para combinar piezas")]
    public RecetaCombinacion[] recetas;

    private List<ObjetoInventario> objetos = new List<ObjetoInventario>();
    private int indiceSeleccionado = -1;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        ActualizarInterfaz();
    }

    // ---------------------------------------------------------------
    // AGREGAR / QUITAR
    // ---------------------------------------------------------------
    public void AgregarObjeto(string id, string nombre, Sprite icono)
    {
        AgregarSinRevisarRecetas(id, nombre, icono);
        RevisarRecetas();
        ActualizarInterfaz();
    }

    private void AgregarSinRevisarRecetas(string id, string nombre, Sprite icono)
    {
        ObjetoInventario nuevo = new ObjetoInventario();
        nuevo.id = id;
        nuevo.nombre = nombre;
        nuevo.icono = icono;
        objetos.Add(nuevo);
    }

    public bool TieneObjeto(string id)
    {
        foreach (ObjetoInventario objeto in objetos)
        {
            if (objeto.id == id)
            {
                return true;
            }
        }
        return false;
    }

    public void QuitarObjeto(string id)
    {
        int indiceEncontrado = -1;
        for (int i = 0; i < objetos.Count; i++)
        {
            if (objetos[i].id == id)
            {
                indiceEncontrado = i;
                break;
            }
        }

        if (indiceEncontrado == -1)
        {
            return;
        }

        objetos.RemoveAt(indiceEncontrado);

        // Ajustar la selección para que siga apuntando al objeto correcto.
        if (indiceSeleccionado == indiceEncontrado)
        {
            indiceSeleccionado = -1;
        }
        else if (indiceSeleccionado > indiceEncontrado)
        {
            indiceSeleccionado = indiceSeleccionado - 1;
        }

        ActualizarInterfaz();
    }

    // ---------------------------------------------------------------
    // SELECCIÓN
    // ---------------------------------------------------------------
    // La llama el Button de cada casilla (OnClick) con su número: 0, 1, 2...
    public void SeleccionarCasilla(int numeroCasilla)
    {
        if (numeroCasilla < 0 || numeroCasilla >= objetos.Count)
        {
            return;
        }

        if (indiceSeleccionado == numeroCasilla)
        {
            // Volver a hacer clic en la misma casilla la deselecciona.
            indiceSeleccionado = -1;
        }
        else
        {
            indiceSeleccionado = numeroCasilla;
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.MostrarMensaje(objetos[numeroCasilla].nombre);
            }
        }

        ActualizarInterfaz();
    }

    // Devuelve el id del objeto seleccionado, o "" si no hay ninguno.
    public string ObtenerIdSeleccionado()
    {
        if (indiceSeleccionado >= 0 && indiceSeleccionado < objetos.Count)
        {
            return objetos[indiceSeleccionado].id;
        }
        return "";
    }

    public void Deseleccionar()
    {
        indiceSeleccionado = -1;
        ActualizarInterfaz();
    }

    // ---------------------------------------------------------------
    // RECETAS
    // ---------------------------------------------------------------
    private void RevisarRecetas()
    {
        if (recetas == null)
        {
            return;
        }

        foreach (RecetaCombinacion receta in recetas)
        {
            bool tieneTodasLasPiezas = true;
            foreach (string idPieza in receta.idsPiezas)
            {
                if (TieneObjeto(idPieza) == false)
                {
                    tieneTodasLasPiezas = false;
                    break;
                }
            }

            if (tieneTodasLasPiezas)
            {
                foreach (string idPieza in receta.idsPiezas)
                {
                    QuitarObjeto(idPieza);
                }
                AgregarSinRevisarRecetas(receta.idResultado, receta.nombreResultado, receta.iconoResultado);

                if (GameManager.Instancia != null)
                {
                    GameManager.Instancia.MostrarMensaje(receta.mensajeAlCombinar);
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // INTERFAZ
    // ---------------------------------------------------------------
    private void ActualizarInterfaz()
    {
        for (int i = 0; i < iconosCasillas.Length; i++)
        {
            if (iconosCasillas[i] == null)
            {
                continue;
            }

            if (i < objetos.Count)
            {
                iconosCasillas[i].sprite = objetos[i].icono;
                iconosCasillas[i].enabled = true;
            }
            else
            {
                iconosCasillas[i].sprite = null;
                iconosCasillas[i].enabled = false;
            }
        }

        if (marcosSeleccion != null)
        {
            for (int i = 0; i < marcosSeleccion.Length; i++)
            {
                if (marcosSeleccion[i] != null)
                {
                    marcosSeleccion[i].enabled = (i == indiceSeleccionado);
                }
            }
        }
    }
}
