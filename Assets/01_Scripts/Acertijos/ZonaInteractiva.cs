// =====================================================================
// ZonaInteractiva.cs                  MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// El script más usado del juego. Es una zona donde se hace clic para:
//
//   a) Solo MIRAR: deja "Id Objeto Necesario" vacío.
//      Ejemplo: la notita de la abuela, levantar el mantel.
//
//   b) USAR un objeto del inventario: escribe el id en "Id Objeto Necesario".
//      El jugador primero selecciona el objeto abajo y luego hace clic aquí.
//      Ejemplo: usar "fosforos" en la vela, poner "tantawawa" en la mesa.
//
// Cuando sale bien, se ejecuta "Al Tener Exito" (UnityEvent). Ahí, desde
// el Inspector, se arrastra lo que tiene que pasar: prender un objeto
// (GameObject.SetActive), llamar a MesaTodosSantos.ColocarObjeto, etc.
//
// Necesita: BoxCollider2D (el SpriteRenderer es opcional).
// =====================================================================

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ZonaInteractiva : MonoBehaviour, IClickeable
{
    [Header("Textos")]
    [TextArea]
    [Tooltip("Lo que dice al hacer clic sin objeto (o al mirar).")]
    public string mensajeAlMirar;
    [TextArea]
    [Tooltip("Lo que dice cuando sale bien.")]
    public string mensajeExito;
    [TextArea]
    public string mensajeObjetoIncorrecto = "Eso no sirve aquí.";
    [TextArea]
    [Tooltip("Lo que dice si se vuelve a hacer clic después de usarla (opcional).")]
    public string mensajeDespuesDeUsar;

    [Header("Objeto que se necesita (vacío = no necesita nada)")]
    public string idObjetoNecesario;
    public bool consumirObjeto = true;

    [Header("Comportamiento")]
    public bool soloUnaVez = true;
    public AudioClip sonidoExito;

    [Header("Lo que pasa cuando sale bien")]
    public UnityEvent alTenerExito;

    private bool yaUsada = false;

    public void AlHacerClic()
    {
        // Ya se usó y es de una sola vez: solo mostramos un texto.
        if (yaUsada && soloUnaVez)
        {
            if (string.IsNullOrEmpty(mensajeDespuesDeUsar) == false)
            {
                Mostrar(mensajeDespuesDeUsar);
            }
            else
            {
                Mostrar(mensajeAlMirar);
            }
            return;
        }

        // No necesita objeto: el clic ya es un éxito.
        if (string.IsNullOrEmpty(idObjetoNecesario))
        {
            Exito();
            return;
        }

        string idSeleccionado = Inventario.Instancia.ObtenerIdSeleccionado();

        // No tiene nada seleccionado: solo mira.
        if (idSeleccionado == "")
        {
            Mostrar(mensajeAlMirar);
            return;
        }

        // Tiene seleccionado el objeto correcto.
        if (idSeleccionado == idObjetoNecesario)
        {
            if (consumirObjeto)
            {
                Inventario.Instancia.QuitarObjeto(idObjetoNecesario);
            }
            else
            {
                Inventario.Instancia.Deseleccionar();
            }
            Exito();
            return;
        }

        // Tiene seleccionado otro objeto.
        Mostrar(mensajeObjetoIncorrecto);
    }

    private void Exito()
    {
        yaUsada = true;

        if (string.IsNullOrEmpty(mensajeExito) == false)
        {
            Mostrar(mensajeExito);
        }
        else
        {
            Mostrar(mensajeAlMirar);
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirSonido(sonidoExito);
        }

        if (alTenerExito != null)
        {
            alTenerExito.Invoke();
        }
    }

    private void Mostrar(string texto)
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(texto);
        }
    }
}
