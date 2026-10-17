// =====================================================================
// ControlParedes.cs                      MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// El cuarto de la abuela tiene 4 paredes. Solo una se ve a la vez.
// Las flechas de la interfaz llaman a ParedSiguiente() y ParedAnterior().
//
//   0 = Pared 1 · La mesa
//   1 = Pared 2 · La puerta
//   2 = Pared 3 · La cocina
//   3 = Pared 4 · El ropero
//
// Al girar de pared se borra el mensaje de abajo, para que no se quede
// un texto de otra pared (ej: "Hay fósforos adentro" en el ropero).
// =====================================================================

using UnityEngine;

public class ControlParedes : MonoBehaviour
{
    public static ControlParedes Instancia;

    [Tooltip("Arrastra aquí las 4 paredes EN ORDEN: Mesa, Puerta, Cocina, Ropero.")]
    public GameObject[] paredes;

    public AudioClip sonidoAlGirar;

    private int paredActual = 0;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        MostrarPared(0);
    }

    public void ParedSiguiente()
    {
        if (EstaBloqueado())
        {
            return;
        }
        int nuevaPared = paredActual + 1;
        if (nuevaPared >= paredes.Length)
        {
            nuevaPared = 0;
        }
        MostrarPared(nuevaPared);
        BorrarMensaje();
        ReproducirSonidoGiro();
    }

    public void ParedAnterior()
    {
        if (EstaBloqueado())
        {
            return;
        }
        int nuevaPared = paredActual - 1;
        if (nuevaPared < 0)
        {
            nuevaPared = paredes.Length - 1;
        }
        MostrarPared(nuevaPared);
        BorrarMensaje();
        ReproducirSonidoGiro();
    }

    public void MostrarPared(int indice)
    {
        for (int i = 0; i < paredes.Length; i++)
        {
            if (paredes[i] != null)
            {
                bool esLaElegida = (i == indice);
                paredes[i].SetActive(esLaElegida);
            }
        }
        paredActual = indice;
    }

    public int ObtenerParedActual()
    {
        return paredActual;
    }

    private bool EstaBloqueado()
    {
        if (GameManager.Instancia != null && GameManager.Instancia.juegoBloqueado)
        {
            return true;
        }
        return false;
    }

    private void BorrarMensaje()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.OcultarMensaje();
        }
    }

    private void ReproducirSonidoGiro()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirSonido(sonidoAlGirar);
        }
    }
}