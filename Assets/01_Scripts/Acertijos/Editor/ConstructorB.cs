// =====================================================================
//  VOLVISTE · Módulo B · Constructor automático
//  Va en:  Assets/01_Scripts/Acertijos/Editor/ConstructorB.cs
//  Menú:   Volviste → B → ...
//
//  Construye los 4 prefabs Interactivos_ParedX y la escena Despertar.
//  No necesita conocer tus scripts: busca sus campos por nombre y,
//  si no encuentra alguno, lo anota en el reporte final (Console).
// =====================================================================
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Volviste.Herramientas
{
    public static class ConstructorB
    {
        const string CARPETA_PREFABS = "Assets/02_Prefabs/Acertijos";
        const string CARPETA_ARTE = "Assets/03_Arte";
        const string CARPETA_AUDIO = "Assets/04_Audio";
        const string ESCENA_DESPERTAR = "Assets/00_Scenes/Despertar.unity";
        const string INCORRECTO = "Eso no sirve aquí.";

        static readonly string[] EXITO = { "al+tener+exito", "tener+exito", "exito", "al+completar", "resuelto", "al+usar", "al+interactuar" };

        // Traducciones al inglés: cuando E las entregue, se agregan aquí
        // ("texto en español", "English text") y se vuelve a construir.
        static readonly Dictionary<string, string> INGLES = new Dictionary<string, string>
        {
            // { "Hay fósforos adentro.", "There are matches inside." },
        };

        static readonly List<string> avisos = new List<string>();
        static readonly List<string> sonidosFaltan = new List<string>();
        static readonly HashSet<string> camposUsados = new HashSet<string>();
        static readonly Dictionary<string, Sprite> cacheSprites = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, AudioClip> cacheAudio = new Dictionary<string, AudioClip>();
        static readonly Dictionary<string, Type> cacheTipos = new Dictionary<string, Type>();

        // ============================== MENÚS ==============================

        [MenuItem("Volviste/B/Construir las 4 paredes", false, 1)]
        static void MenuTodas() { ConstruirVarias(new[] { 3, 1, 2, 4 }); }

        [MenuItem("Volviste/B/Solo Pared 1 (mesa)", false, 20)]
        static void Menu1() { ConstruirVarias(new[] { 1 }); }

        [MenuItem("Volviste/B/Solo Pared 2 (puerta)", false, 21)]
        static void Menu2() { ConstruirVarias(new[] { 2 }); }

        [MenuItem("Volviste/B/Solo Pared 3 (cocina)", false, 22)]
        static void Menu3() { ConstruirVarias(new[] { 3 }); }

        [MenuItem("Volviste/B/Solo Pared 4 (ropero)", false, 23)]
        static void Menu4() { ConstruirVarias(new[] { 4 }); }

        [MenuItem("Volviste/B/Construir escena Despertar", false, 40)]
        static void MenuDespertar() { ConstruirDespertar(); }

        [MenuItem("Volviste/B/Diagnóstico (copiar campos de mis scripts)", false, 60)]
        static void Diagnostico()
        {
            string[] scripts = { "ZonaInteractiva", "ObjetoRecogible", "RuedaCombinacion", "CerraduraCombinacion", "Radio",
                                 "MesaTodosSantos", "ControlAnomalias", "SecuenciaFinParte1", "Anomalia", "RotarAguja", "SecuenciaDespertar" };
            var sb = new StringBuilder("CAMPOS DE LOS SCRIPTS DE B\n");
            foreach (var n in scripts)
            {
                var t = Tipo(n);
                if (t == null) { sb.AppendLine(n + ": NO ENCONTRADO"); continue; }
                var metodos = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                               .Where(m => !m.IsSpecialName).Select(m => m.Name + "(" + m.GetParameters().Length + ")");
                sb.AppendLine(n + " → campos: " + string.Join(", ", Campos(t).Select(f => f.Name + ":" + NombreTipo(f.FieldType)))
                              + " | métodos: " + string.Join(", ", metodos));
            }
            Debug.Log(sb.ToString());
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            EditorUtility.DisplayDialog("Volviste", "Listo. La lista quedó copiada (Ctrl+V). Pégala en el chat.", "OK");
        }

        // ============================== PAREDES ==============================

        static void ConstruirVarias(int[] paredes)
        {
            var escena = SceneManager.GetActiveScene();
            if (escena.path.EndsWith("Volviste.unity"))
            {
                EditorUtility.DisplayDialog("Volviste", "Estás en Volviste.unity, que es de A.\nAbre tu escena B_Pruebas.unity y vuelve a intentarlo.", "OK");
                return;
            }
            if (string.IsNullOrEmpty(escena.path))
            {
                EditorUtility.DisplayDialog("Volviste", "Primero guarda la escena (B_Pruebas.unity).", "OK");
                return;
            }
            var existentes = paredes.Where(n => File.Exists(RutaPrefab(n))).ToList();
            if (existentes.Count > 0 && !EditorUtility.DisplayDialog("Volviste",
                    "Ya existen: " + string.Join(", ", existentes.Select(n => "Interactivos_Pared" + n)) +
                    ".\nSe van a REEMPLAZAR por versiones nuevas (lo que hayas cambiado a mano en ellos se pierde).\n¿Seguir?",
                    "Sí, reemplazar", "Cancelar"))
                return;

            Iniciar();
            Carpeta(CARPETA_PREFABS);
            var raices = new List<GameObject>();
            foreach (int n in paredes)
            {
                try { raices.Add(ConstruirPared(n, escena)); }
                catch (Exception e) { Aviso("Error construyendo la Pared " + n + ": " + e.Message); Debug.LogException(e); }
            }
            for (int i = 1; i < raices.Count; i++) raices[i].SetActive(false); // solo se ve la primera
            ArreglarCamara();
            EditorSceneManager.MarkSceneDirty(escena);
            EditorSceneManager.SaveScene(escena);
            AssetDatabase.SaveAssets();
            Terminar("Paredes " + string.Join(", ", paredes));
        }

        static string RutaPrefab(int n) { return CARPETA_PREFABS + "/Interactivos_Pared" + n + ".prefab"; }

        static GameObject ConstruirPared(int n, Scene escena)
        {
            string nombre = "Interactivos_Pared" + n;
            foreach (var r in escena.GetRootGameObjects())
                if (r.name == nombre) Object.DestroyImmediate(r);

            var raiz = new GameObject(nombre);
            var t = raiz.transform;
            switch (n)
            {
                case 1: Pared1(t); break;
                case 2: Pared2(t); break;
                case 3: Pared3(t); break;
                case 4: Pared4(t); break;
            }
            PrefabUtility.SaveAsPrefabAssetAndConnect(raiz, RutaPrefab(n), InteractionMode.AutomatedAction);
            return raiz;
        }

        // ---------------------------- PARED 3 · COCINA ----------------------------
        static void Pared3(Transform p)
        {
            var cajonAb = Obj(p, "Cajon_Abierto", 0.6f, -1.4f, "Cajon_Abierto", 5, false, false);
            var fosforos = Obj(p, "Fosforos", 0.6f, -1.2f, "Fosforos", 6, true, false);
            Recogible(fosforos, "fosforos", "Fósforos", "Fosforos", "Fósforos.");

            Obj(p, "Horno", -4.4f, -0.7f, "Horno_Cerrado", 5);
            var hornoAb = Obj(p, "Horno_Abierto", -4.4f, -0.7f, "Horno_Abierto", 6, false, false);
            var tanta = Obj(p, "Tantawawa", -4.4f, -1.3f, "Tantawawa_Abuela", 7, true, false);
            Recogible(tanta, "tantawawa", "T'antawawa", "Tantawawa_Abuela", "Una t'antawawa… tiene la cara de la abuela.");

            var radioAb = Obj(p, "Radio_Abierta", 6.5f, 0.9f, "Radio_Abierta", 6, false, false);
            var pieza3 = Obj(p, "Escalera_Pieza3", 6.5f, 0.9f, "Escalera_Pieza3", 7, true, false);
            Recogible(pieza3, "escalera_3", "Pieza de escalera", "Escalera_Pieza3", "Un pedazo de escalera de pan.");

            // Cajón
            var cajon = Obj(p, "Cajon", 0.6f, -1.4f, "Cajon_Cerrado", 5, true);
            var ec = Evento(Zona(cajon, null, null, "Hay fósforos adentro."), true, EXITO);
            Prender(ec, cajonAb, true); Prender(ec, fosforos, true); Prender(ec, cajon, false);

            // Candado del horno (vacío) + 3 ruedas
            var candado = Obj(p, "CandadoHorno", -4.4f, 0.6f);
            var cerr = Script(candado, "CerraduraCombinacion");
            var simbolos = new Object[] { Spr("Simbolo_Sol"), Spr("Simbolo_Luna"), Spr("Simbolo_Estrella"), Spr("Simbolo_Corazon") };
            var ruedas = new List<GameObject>();
            float[] xs = { -5.8f, -4.4f, -3.0f };
            for (int i = 0; i < 3; i++)
            {
                var r = Obj(p, "Rueda" + (i + 1), xs[i], 0.6f, "Simbolo_Sol", 7, true);
                Rueda(r, simbolos, cerr, null);
                ruedas.Add(r);
            }
            Cerradura(cerr, ruedas, new[] { 0, 1, 2 }, "El candado se abrió. Huele a pan recién horneado.",
                      new[] { hornoAb, tanta }, ruedas);

            // Radio
            var radio = Obj(p, "Radio", 6.5f, 0.9f, "Radio", 5, true);
            var fuente = radio.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            var dial = Texto3D(radio.transform, "TextoDial", 0f, -0.6f, "88.0", 4f, 8, new Vector2(2.5f, 0.6f));
            var rc = Script(radio, "Radio");
            Entero(rc, 3, true, "indice+correcto", "correcto", "sintonia+correcta", "objetivo", "indice");
            Ref(rc, dial, true, "texto+dial", "dial", "texto");
            Ref(rc, fuente, false, "audio+source", "fuente", "source", "altavoz", "audio");
            Ref(rc, Clip("sfx_estatica_radio"), false, "estatica", "static", "ruido");
            Ref(rc, Clip("voz_abuela_radio"), false, "voz", "mensaje", "sintoniz", "abuela");
            Texto(rc, T("Solo se escucha estática…"), false, "estatica", "mensaje+estatica", "ruido");
            Texto(rc, T("\"¿Por qué nadie viene, wawa?… cuatro… uno… nueve…\""), false, "sintoniz", "mensaje+correcto", "mensaje", "texto+voz");
            var es = Evento(rc, true, "al+sintonizar", "sintoniz", "exito", "correcto", "resuelto");
            Prender(es, radioAb, true); Prender(es, pieza3, true);
        }

        // ---------------------------- PARED 1 · MESA ----------------------------
        static void Pared1(Transform p)
        {
            var mesa = Obj(p, "Mesa", 0f, 0f);
            var mts = Script(mesa, "MesaTodosSantos");
            Script(mesa, "ControlAnomalias");
            var fin = Script(mesa, "SecuenciaFinParte1");

            var velaOn = Obj(p, "Vela_Encendida", -4f, -0.4f, "Vela_Encendida", 5, false, false);
            var simbolos = Obj(p, "Simbolos", 0f, 2.5f, "Simbolos_Pared", 5, false, false);
            var mTanta = Obj(p, "Mesa_Tantawawa", -2.2f, -0.32f, "Tantawawa_Abuela", 5, false, false);
            var mEsc = Obj(p, "Mesa_Escalera", 2.1f, 0.08f, "Escalera_Completa", 5, false, false);
            var mFlores = Obj(p, "Mesa_Flores", 4f, -0.22f, "Flores_Cana", 5, false, false);
            var pieza1 = Obj(p, "Escalera_Pieza1", 3.4f, -3.65f, "Escalera_Pieza1", 7, true, false);
            Recogible(pieza1, "escalera_1", "Pieza de escalera", "Escalera_Pieza1", "Un pedazo de escalera de pan.");

            var retrato = Obj(p, "ZonaRetrato", 0f, -0.12f, "Retrato_Reverso", 6, true, false);
            var zr = Zona(retrato, null, "Un retrato boca abajo.", null);

            // Mesa
            Entero(mts, 4, true, "necesarios", "objetos+necesarios", "total", "cantidad", "necesario");
            Ref(mts, retrato, true, "retrato", "zona+retrato");
            Texto(mts, T("La mesa está lista. Solo falta el retrato del centro."), false, "completa", "mesa+lista", "lista", "mensaje");
            Ref(fin, retrato.GetComponent<SpriteRenderer>(), true, "dibujo+retrato", "retrato", "render", "dibujo");
            Ref(fin, Spr("Retrato_Abuela"), true, "foto+abuela", "foto", "abuela", "sprite");
            Texto(fin, T("Es… la abuela."), false, "mensaje+retrato", "mensaje", "texto");
            Ref(fin, Clip("sfx_campanada_reloj"), false, "campanada", "campana", "sonido", "clip");
            Texto(fin, "Despertar", false, "escena", "siguiente+escena", "nombre+escena");
            Enlazar(mesa);

            // Vela
            var vela = Obj(p, "Vela", -4f, -0.4f, "Vela_Apagada", 5, true);
            var ev = Evento(Zona(vela, "fosforos", "La vela está apagada. Necesito algo para encenderla.",
                                 "La llama tiembla… y en la pared aparecen un sol, una luna y una estrella."), true, EXITO);
            Prender(ev, velaOn, true); Prender(ev, simbolos, true); Prender(ev, vela, false); Llamar(ev, mts, "ColocarObjeto");

            // Huecos
            Hueco(p, "Hueco_Tantawawa", -2.2f, -0.32f, "Tantawawa_Abuela", "tantawawa",
                  "Aquí va una t'antawawa.", "La t'antawawa tiene la cara de la abuela…", mTanta, mts);
            Hueco(p, "Hueco_Escalera", 2.1f, 0.08f, "Escalera_Completa", "escalera",
                  "Aquí va la escalera de pan, para que el alma pueda bajar.", "La escalera ya está en su lugar.", mEsc, mts);
            Hueco(p, "Hueco_Flores", 4f, -0.22f, "Flores_Cana", "flores",
                  "Faltan flores y caña.", "Las flores huelen a la casa de la abuela.", mFlores, mts);

            // Mantel
            var mantel = ZonaSinDibujo(p, "Mantel", 3.4f, -3.3f, new Vector2(2.4f, 1.2f), 4, Vector2.zero);
            Prender(Evento(Zona(mantel, null, null, "Debajo del mantel hay algo…"), true, EXITO), pieza1, true);

            // Retrato → fin de la Parte 1
            Llamar(Evento(zr, true, EXITO), fin, "Iniciar");
        }

        // ---------------------------- PARED 2 · PUERTA ----------------------------
        static void Pared2(Transform p)
        {
            var pieza2 = Obj(p, "Escalera_Pieza2", -5.6f, -0.6f, "Escalera_Pieza2", 7, true, false);
            Recogible(pieza2, "escalera_2", "Pieza de escalera", "Escalera_Pieza2", "Un pedazo de escalera de pan.");

            var reloj = Obj(p, "Reloj", -5.6f, 2f, "Reloj", 5, true);
            Prender(Evento(Zona(reloj, null, null, "Detrás del reloj había algo escondido."), true, EXITO), pieza2, true);
            var agN = Aguja(p, "Aguja_Normal", -5.6f, 2f, -6f, true);
            var agR = Aguja(p, "Aguja_Reves", -5.6f, 2f, 40f, false);

            var nota = Obj(p, "Nota", -0.3f, 1f, "Nota_Abuela", 5, true);
            Zona(nota, null, "\"Arma la mesa antes de las 12, wawita. Si no, no voy a poder volver.\"", null, false, true);
            var notaR = Obj(p, "Nota_Rara", -0.3f, 1f, "Nota_Abuela", 6, true, false);
            Zona(notaR, null, "La letra cambió: \"¿Por qué nadie vino?\"", null, false, true);

            var puerta = ZonaSinDibujo(p, "Puerta", -0.3f, -0.5f, new Vector2(3f, 6f), 1, Vector2.zero);
            Zona(puerta, null, "Está cerrada. Afuera no se escucha nada.", null, false, true);

            var calN = Obj(p, "Calendario_Normal", 6.9f, 0.7f, "Calendario_Normal", 5);
            var calT1 = Obj(p, "Calendario_Tachado_1", 6.9f, 0.7f, "Calendario_Tachado_1", 6, false, false);
            var calT2 = Obj(p, "Calendario_Tachado_2", 6.9f, 0.7f, "Calendario_Tachado_2", 7, false, false);

            Anomalia(p, "Anomalia_E1_Aguja", 1, agN, agR);
            Anomalia(p, "Anomalia_E1_Calendario", 1, calN, calT1);
            Anomalia(p, "Anomalia_E3_Calendario", 3, null, calT2);
            Anomalia(p, "Anomalia_E4_Nota", 4, nota, notaR);
        }

        // ---------------------------- PARED 4 · ROPERO ----------------------------
        static void Pared4(Transform p)
        {
            Obj(p, "Ropero", 2.7f, 0.7f, "Ropero_Cerrado", 5);
            var ropAb = Obj(p, "Ropero_Abierto", 2.7f, 0.7f, "Ropero_Abierto", 6, false, false);
            var flores = Obj(p, "Flores", 2.7f, 0f, "Flores_Cana", 7, true, false);
            Recogible(flores, "flores", "Flores y caña", "Flores_Cana", "Flores y caña para la mesa.");

            var espejo = Obj(p, "Espejo_Abuela", -2.6f, 0.7f, "Espejo_Abuela", 5, false, false);
            var tela = Obj(p, "Tela", -2.6f, 0.7f, "Tela_Espejo", 6, true, false);
            var et = Evento(Zona(tela, null, null, "En el espejo, la abuela está sentada en su silla… detrás de ti."), true, EXITO);
            Prender(et, tela, false); Prender(et, espejo, true);

            var silla = Obj(p, "Silla", -6.3f, -2.2f, "Silla_Vacia", 5, true);
            Zona(silla, null, "La silla de la abuela. Está vacía.", null, false, true);

            var ffN = Obj(p, "Foto_Familia_Normal", -7.3f, 2.8f, "Foto_Familia_Normal", 5);
            var ffB = Obj(p, "Foto_Familia_Borrada", -7.3f, 2.8f, "Foto_Familia_Borrada", 6, false, false);
            var faN = Obj(p, "Foto_Abuela_Normal", -4.95f, 2.85f, "Foto_Abuela_Normal", 5);
            var faB = Obj(p, "Foto_Abuela_Borrada", -4.95f, 2.85f, "Foto_Abuela_Borrada", 6, false, false);
            Anomalia(p, "Anomalia_E2_FotoAbuela", 2, faN, faB);
            Anomalia(p, "Anomalia_E3_FotoFamilia", 3, ffN, ffB);

            var candado = Obj(p, "CandadoRopero", 2.7f, 0.7f);
            var cerr = Script(candado, "CerraduraCombinacion");
            var ruedas = new List<GameObject>();
            float[] xs = { 2.1f, 2.7f, 3.3f };
            for (int i = 0; i < 3; i++)
            {
                var r = ZonaSinDibujo(p, "NumRueda" + (i + 1), xs[i], 0.7f, new Vector2(0.5f, 0.7f), 7, Vector2.zero);
                var num = Texto3D(r.transform, "Numero", 0f, 0f, "0", 5f, 8, new Vector2(0.6f, 0.8f));
                Rueda(r, null, cerr, num);
                ruedas.Add(r);
            }
            Cerradura(cerr, ruedas, new[] { 4, 1, 9 }, "Se abrió. Adentro hay flores y caña de azúcar.",
                      new[] { ropAb, flores, tela }, ruedas);
        }

        // ============================== DESPERTAR ==============================

        static void ConstruirDespertar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ESCENA_DESPERTAR) && !EditorUtility.DisplayDialog("Volviste",
                    "Despertar.unity ya existe. ¿Reemplazarla por una nueva?", "Sí, reemplazar", "Cancelar"))
                return;

            Iniciar();
            var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            camGO.AddComponent<AudioListener>();
            var amb = camGO.AddComponent<AudioSource>();
            amb.clip = Clip("amb_noche_casa"); amb.loop = true; amb.playOnAwake = true;

            var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var cv = canvasGO.transform;

            string[] dibujos = { "Despertar_1_Cuarto", "Despertar_2_Celular", "Despertar_3_Puerta", "Despertar_4_Abuela" };
            string[] nombres = { "Pantalla0_Cuarto", "Pantalla1_Celular", "Pantalla2_Puerta", "Pantalla3_Abuela" };
            var pantallas = new List<Object>();
            for (int i = 0; i < 4; i++)
            {
                var img = Imagen(cv, nombres[i], Spr(dibujos[i]), Color.white, false);
                img.gameObject.SetActive(i == 0);
                pantallas.Add(img.gameObject);
            }

            var sub = TextoUI(cv, "Subtitulo", "", 46f, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 110f), new Vector2(-200f, 160f));
            var ind = TextoUI(cv, "IndicadorClic", "clic para continuar / click to continue", 26f, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 40f), new Vector2(480f, 50f));

            var panel = Imagen(cv, "PanelFinal", null, Color.black, true);
            var texFin = TextoUI(panel.transform, "TextoFinal", "", 48f, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1500f, 500f));
            var bGO = new GameObject("BotonVolver", typeof(RectTransform));
            bGO.transform.SetParent(panel.transform, false);
            var bImg = bGO.AddComponent<Image>();
            bImg.color = new Color(1f, 1f, 1f, 0.12f);
            var boton = bGO.AddComponent<Button>();
            boton.targetGraphic = bImg;
            var brt = (RectTransform)bGO.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.anchoredPosition = new Vector2(0f, 140f);
            brt.sizeDelta = new Vector2(560f, 90f);
            TextoUI(bGO.transform, "Texto", "Volver al inicio / Back to start", 34f, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var fundido = Imagen(cv, "ImagenFundido", null, new Color(0f, 0f, 0f, 0f), false);
            panel.gameObject.SetActive(false);

            var esGO = new GameObject("EventSystem", typeof(EventSystem));
            var modulo = Tipo("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (modulo != null) esGO.AddComponent(modulo); else esGO.AddComponent<StandaloneInputModule>();

            var seqGO = new GameObject("Secuencia");
            var seq = Script(seqGO, "SecuenciaDespertar");
            ListaTextos(seq, new[]
            {
                T("La abuela Rosa murió sola el 28 de octubre."),
                T("Te esperó para Todos Santos."),
                T("No esperes a que sea tarde para visitar a los que todavía están aquí.")
            }, true, "lineas+finales", "textos+finales", "final", "creditos", "epilogo");
            ListaTextos(seq, new[]
            {
                T("…no otra vez."),
                T("Abuelita: \"¿Vas a venir para Todos Santos, wawa?\"\n28 de octubre · Visto"),
                T("\"Volviste, wawa…\""),
                T("¿Abuela?")
            }, true, "subtitulos", "textos", "lineas", "dialogos", "textos+pantallas");
            ListaObjetos(seq, pantallas, true, "pantallas", "imagenes", "escenas", "paneles");
            Ref(seq, texFin, true, "texto+final", "final+texto");
            Ref(seq, sub, true, "subtitulo", "texto+subtitulo", "texto");
            Ref(seq, ind, false, "indicador", "clic");
            Ref(seq, panel, true, "panel+final", "panel");
            Ref(seq, fundido, true, "fundido", "imagen+fundido", "fade", "negro");
            Ref(seq, amb, false, "audio+source", "fuente", "source", "ambiente", "audio");
            Ref(seq, Clip("amb_noche_casa"), false, "ambiente", "amb");
            Ref(seq, Clip("voz_abuela_volviste"), false, "voz", "volviste");
            Ref(seq, Clip("sfx_gato_maullido"), false, "maullido", "gato+maullido", "miau");
            Ref(seq, Clip("sfx_gato_final"), false, "gato+final", "final", "bufido", "golpe");
            Texto(seq, "Volviste", false, "escena+inicio", "inicio", "escena+principal", "primera");
            Llamar(boton.onClick, seq, "VolverAlInicio");

            Carpeta("Assets/00_Scenes");
            EditorSceneManager.SaveScene(escena, ESCENA_DESPERTAR);
            Terminar("Escena Despertar");
        }

        // ============================== PIEZAS ==============================

        static Component Zona(GameObject go, string necesita, string mirar, string exito, bool unaVez = true, bool soloMirar = false)
        {
            var c = Script(go, "ZonaInteractiva");
            if (c == null) return null;
            if (!string.IsNullOrEmpty(necesita))
                Texto(c, necesita, true, "id+necesario", "objeto+necesario", "necesario", "necesita", "requerido", "id+requerido", "id+objeto", "objeto+id", "id");
            Texto(c, T(mirar), true, "mirar", "descripcion", "inspeccion", "observar");
            Texto(c, T(exito), true, "mensaje+exito", "texto+exito", "exito", "mensaje+correcto", "resuelto");
            Texto(c, T(INCORRECTO), false, "incorrecto", "equivocado", "nosirve", "error", "fallo");
            Booleano(c, unaVez, false, "solo+una+vez", "una+vez", "unica");
            if (soloMirar) Booleano(c, true, false, "solo+mirar", "solo+para+mirar", "mirar+solo");
            return c;
        }

        static Component Recogible(GameObject go, string id, string nombre, string icono, string mensaje)
        {
            var c = Script(go, "ObjetoRecogible");
            if (c == null) return null;
            Texto(c, id, true, "id", "id+objeto", "objeto+id", "item+id", "identificador", "clave");
            Texto(c, T(nombre), true, "nombre", "nombre+objeto", "titulo");
            Ref(c, Spr(icono), true, "icono", "sprite", "imagen");
            Texto(c, T(mensaje), false, "mensaje", "mensaje+recoger", "texto", "descripcion");
            Ref(c, Clip("sfx_recoger"), false, "sonido", "clip", "audio");
            return c;
        }

        static void Rueda(GameObject go, Object[] opciones, Component cerradura, Component texto)
        {
            var c = Script(go, "RuedaCombinacion");
            if (c == null) return;
            if (opciones != null)
            {
                ListaObjetos(c, opciones, true, "opciones+sprites", "opciones", "sprites", "simbolos");
                Ref(c, go.GetComponent<SpriteRenderer>(), false, "render", "sprite+renderer", "dibujo");
            }
            Ref(c, cerradura, true, "cerradura", "candado");
            if (texto != null) Ref(c, texto, true, "texto+numero", "numero", "texto");
            Ref(c, Clip("sfx_candado_clic"), false, "sonido", "clip", "audio");
        }

        static void Cerradura(Component c, List<GameObject> ruedas, int[] combinacion, string mensaje, GameObject[] prender, List<GameObject> apagar)
        {
            if (c == null) return;
            ListaObjetos(c, ruedas.Cast<Object>().ToList(), true, "ruedas", "rueda");
            ListaEnteros(c, combinacion, true, "combinacion", "correcta", "solucion", "clave", "codigo");
            Texto(c, T(mensaje), false, "mensaje", "texto");
            Ref(c, Clip("sfx_candado_abre"), false, "sonido", "clip", "audio");
            var e = Evento(c, true, "al+abrir", "abrir", "abre", "exito", "resuelto", "completar");
            foreach (var g in prender) Prender(e, g, true);
            foreach (var g in apagar) Prender(e, g, false);
        }

        static void Hueco(Transform p, string nombre, float x, float y, string spriteRef, string id, string mirar, string exito, GameObject mostrar, Component mesa)
        {
            var s = Spr(spriteRef, false);
            Vector2 tam = s != null ? (Vector2)s.bounds.size : new Vector2(1.5f, 1.5f);
            Vector2 centro = s != null ? (Vector2)s.bounds.center : Vector2.zero;
            var go = ZonaSinDibujo(p, nombre, x, y, tam, 4, centro);
            var e = Evento(Zona(go, id, mirar, exito), true, EXITO);
            Prender(e, mostrar, true);
            Llamar(e, mesa, "ColocarObjeto");
        }

        static GameObject Aguja(Transform p, string nombre, float x, float y, float valor, bool activo)
        {
            // Objeto vacío que gira + dibujo hijo con la base en el centro de giro
            // (así no hace falta cambiar el pivote del PNG).
            var go = Obj(p, nombre, x, y);
            var dib = new GameObject("Dibujo");
            dib.transform.SetParent(go.transform, false);
            var sr = dib.AddComponent<SpriteRenderer>();
            sr.sprite = Spr("Aguja");
            sr.sortingOrder = 6;
            if (sr.sprite != null)
                dib.transform.localPosition = new Vector3(-sr.sprite.bounds.center.x, -sr.sprite.bounds.min.y, 0f);
            var c = Script(go, "RotarAguja");
            Real(c, valor, true, true, "velocidad", "grados", "angulo", "rotacion", "vel");
            go.SetActive(activo);
            return go;
        }

        static void Anomalia(Transform p, string nombre, int etapa, GameObject normal, GameObject rara)
        {
            var go = Obj(p, nombre, 0f, 0f);
            var c = Script(go, "Anomalia");
            if (c == null) return;
            Entero(c, etapa, true, "etapa", "nivel", "paso", "orden");
            if (normal != null) Ref(c, normal, true, "normal", "version+normal", "original");
            Ref(c, rara, true, "rara", "raro", "version+rara", "anomalia", "extrano", "cambiado");
        }

        static string T(string es)
        {
            if (es == null) return null;
            string en;
            if (INGLES.TryGetValue(es, out en) && !string.IsNullOrEmpty(en))
                return es + "\n<size=75%><i>" + en + "</i></size>";
            return es;
        }

        // ============================== OBJETOS ==============================

        static GameObject Obj(Transform padre, string nombre, float x, float y, string sprite = null, int orden = 5, bool clic = false, bool activo = true)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(x, y, 0f);
            if (sprite != null)
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Spr(sprite);
                sr.sortingOrder = orden;
            }
            if (clic) Colisionador(go, Vector2.zero);
            go.SetActive(activo);
            return go;
        }

        static GameObject ZonaSinDibujo(Transform p, string nombre, float x, float y, Vector2 tam, int orden, Vector2 centro)
        {
            var go = Obj(p, nombre, x, y);
            var sr = go.AddComponent<SpriteRenderer>(); // vacío: solo marca el orden de clic
            sr.sortingOrder = orden;
            var bc = Colisionador(go, tam);
            bc.offset = centro;
            return go;
        }

        static BoxCollider2D Colisionador(GameObject go, Vector2 tam)
        {
            var bc = go.GetComponent<BoxCollider2D>();
            if (bc == null) bc = go.AddComponent<BoxCollider2D>();
            if (tam != Vector2.zero) { bc.size = tam; bc.offset = Vector2.zero; return bc; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null) { bc.size = sr.sprite.bounds.size; bc.offset = sr.sprite.bounds.center; }
            else bc.size = Vector2.one;
            return bc;
        }

        static Component Texto3D(Transform padre, string nombre, float x, float y, string texto, float tam, int orden, Vector2 rect)
        {
            var go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var t = Tipo("TMPro.TextMeshPro");
            if (t == null) { Aviso("No encontré TextMeshPro: crea a mano el texto '" + nombre + "'."); return null; }
            var c = go.AddComponent(t);
            go.transform.localPosition = new Vector3(x, y, 0f);
            Prop(c, "text", texto);
            Prop(c, "fontSize", tam);
            Prop(c, "color", Color.white);
            Prop(c, "sortingOrder", orden);
            PropEnum(c, "alignment", "Center");
            var rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = rect;
            return c;
        }

        static Component TextoUI(Transform padre, string nombre, string texto, float tam, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 tamRect)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos; rt.sizeDelta = tamRect;
            var t = Tipo("TMPro.TextMeshProUGUI");
            if (t == null) { Aviso("No encontré TextMeshPro UI: crea a mano el texto '" + nombre + "'."); return null; }
            var c = go.AddComponent(t);
            Prop(c, "text", texto);
            Prop(c, "fontSize", tam);
            Prop(c, "color", Color.white);
            Prop(c, "raycastTarget", false);
            PropEnum(c, "alignment", "Center");
            return c;
        }

        static Image Imagen(Transform padre, string nombre, Sprite s, Color color, bool raycast)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            var img = go.AddComponent<Image>();
            img.sprite = s; img.color = color; img.raycastTarget = raycast;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return img;
        }

        static void ArreglarCamara()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            EditorUtility.SetDirty(cam);
        }

        static void Enlazar(GameObject go)
        {
            // Si un script de la Mesa necesita a otro script de la Mesa, lo conecta solo.
            var comps = go.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                foreach (var f in Campos(c.GetType()))
                {
                    if (!typeof(Component).IsAssignableFrom(f.FieldType) || f.FieldType.Assembly != c.GetType().Assembly) continue;
                    if (!EsNulo(f.GetValue(c))) continue;
                    var otro = comps.FirstOrDefault(o => o != null && o != c && f.FieldType.IsInstanceOfType(o));
                    if (otro != null) { f.SetValue(c, otro); EditorUtility.SetDirty(c); }
                }
            }
        }

        // ============================== RECURSOS ==============================

        static Sprite Spr(string nombre, bool avisar = true)
        {
            if (string.IsNullOrEmpty(nombre)) return null;
            Sprite s;
            if (cacheSprites.TryGetValue(nombre, out s) && s != null) return s;
            s = null;
            if (AssetDatabase.IsValidFolder(CARPETA_ARTE))
            {
                foreach (var guid in AssetDatabase.FindAssets(nombre, new[] { CARPETA_ARTE }))
                {
                    var ruta = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.Equals(Path.GetFileNameWithoutExtension(ruta), nombre, StringComparison.OrdinalIgnoreCase)) continue;
                    s = AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault();
                    if (s != null) break;
                }
            }
            if (s == null && avisar) Aviso("No encontré el dibujo '" + nombre + "' en 03_Arte.");
            cacheSprites[nombre] = s;
            return s;
        }

        static AudioClip Clip(string nombre)
        {
            AudioClip c;
            if (cacheAudio.TryGetValue(nombre, out c)) return c;
            c = null;
            if (AssetDatabase.IsValidFolder(CARPETA_AUDIO))
            {
                foreach (var guid in AssetDatabase.FindAssets(nombre, new[] { CARPETA_AUDIO }))
                {
                    var ruta = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.Equals(Path.GetFileNameWithoutExtension(ruta), nombre, StringComparison.OrdinalIgnoreCase)) continue;
                    c = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
                    if (c != null) break;
                }
            }
            if (c == null) sonidosFaltan.Add(nombre);
            cacheAudio[nombre] = c;
            return c;
        }

        static Type Tipo(string nombre)
        {
            Type t;
            if (cacheTipos.TryGetValue(nombre, out t)) return t;
            t = null;
            var asms = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a =>
            {
                var n = a.GetName().Name;
                if (n.StartsWith("Assembly-CSharp")) return 0;
                if (n.StartsWith("Unity") || n.StartsWith("System") || n.StartsWith("mscorlib") || n.StartsWith("Mono")) return 2;
                return 1;
            });
            foreach (var a in asms)
            {
                Type[] ts;
                try { ts = a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { ts = e.Types.Where(x => x != null).ToArray(); }
                catch { continue; }
                t = ts.FirstOrDefault(x => (x.Name == nombre || x.FullName == nombre) && typeof(Component).IsAssignableFrom(x) && !x.IsAbstract);
                if (t != null) break;
            }
            cacheTipos[nombre] = t;
            return t;
        }

        static Component Script(GameObject go, string nombre)
        {
            var t = Tipo(nombre);
            if (t == null) { Aviso("No encontré el script '" + nombre + "' (¿hay errores rojos en la Console?). " + go.name + " quedó sin él."); return null; }
            var c = go.GetComponent(t);
            if (c == null) c = go.AddComponent(t);
            return c;
        }

        static void Carpeta(string ruta)
        {
            if (AssetDatabase.IsValidFolder(ruta)) return;
            var padre = Path.GetDirectoryName(ruta).Replace('\\', '/');
            Carpeta(padre);
            AssetDatabase.CreateFolder(padre, Path.GetFileName(ruta));
        }

        // ============================== CAMPOS ==============================

        static List<FieldInfo> Campos(Type t)
        {
            var lista = new List<FieldInfo>();
            while (t != null && t != typeof(MonoBehaviour) && t != typeof(Behaviour) && t != typeof(Component) && t != typeof(object))
            {
                foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (f.IsNotSerialized || f.IsInitOnly) continue;
                    if (!f.IsPublic && !f.IsDefined(typeof(SerializeField), true)) continue;
                    lista.Add(f);
                }
                t = t.BaseType;
            }
            return lista;
        }

        static string Norm(string s)
        {
            if (s.StartsWith("m_")) s = s.Substring(2);
            var d = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char ch in d)
                if (ch < 128 && char.IsLetterOrDigit(ch)) sb.Append(ch);
            return sb.ToString();
        }

        static bool Coincide(string campo, string patron, bool exacto)
        {
            var claves = patron.Split('+').Select(Norm).ToArray();
            if (exacto) return campo == string.Concat(claves);
            if (claves.Length == 1 && claves[0].Length <= 3) return campo.StartsWith(claves[0]) || campo.EndsWith(claves[0]);
            return claves.All(k => campo.Contains(k));
        }

        static string Clave(Component c, FieldInfo f) { return RuntimeHelpers.GetHashCode(c) + "/" + f.Name; }

        static FieldInfo Buscar(Component c, Func<FieldInfo, bool> sirve, string[] patrones)
        {
            var campos = Campos(c.GetType()).Where(f => !camposUsados.Contains(Clave(c, f)) && sirve(f)).ToList();
            foreach (bool exacto in new[] { true, false })
                foreach (var p in patrones)
                {
                    var f = campos.FirstOrDefault(x => Coincide(Norm(x.Name), p, exacto));
                    if (f != null) return f;
                }
            return null;
        }

        static bool EsNulo(object o) { return o == null || (o is Object && (Object)o == null); }

        static bool Poner(Component c, Func<Type, bool> tipoOk, Func<Type, object> convertir, bool oblig, string que, string[] patrones)
        {
            if (c == null) return false;
            var f = Buscar(c, x => tipoOk(x.FieldType) && !EsNulo(convertir(x.FieldType)), patrones);
            if (f == null) { if (oblig) Falta(c, que); return false; }
            f.SetValue(c, convertir(f.FieldType));
            camposUsados.Add(Clave(c, f));
            EditorUtility.SetDirty(c);
            return true;
        }

        static bool Texto(Component c, string v, bool oblig, params string[] p)
        {
            if (v == null) return false;
            return Poner(c, t => t == typeof(string), t => v, oblig, "texto \"" + Corto(v) + "\"", p);
        }

        static bool Entero(Component c, int v, bool oblig, params string[] p)
        {
            return Poner(c, t => t == typeof(int), t => v, oblig, "número " + v, p);
        }

        static bool Booleano(Component c, bool v, bool oblig, params string[] p)
        {
            return Poner(c, t => t == typeof(bool), t => v, oblig, "casilla '" + p[0] + "'", p);
        }

        static bool Real(Component c, float v, bool oblig, bool primeroSiNo, params string[] p)
        {
            if (c == null) return false;
            if (Poner(c, t => t == typeof(float), t => v, false, "", p)) return true;
            if (primeroSiNo)
            {
                var f = Campos(c.GetType()).FirstOrDefault(x => x.FieldType == typeof(float) && !camposUsados.Contains(Clave(c, x)));
                if (f != null)
                {
                    f.SetValue(c, v); camposUsados.Add(Clave(c, f)); EditorUtility.SetDirty(c);
                    Aviso("[" + c.gameObject.name + "] puse " + v + " en '" + f.Name + "'. Revisa que sea el campo correcto.");
                    return true;
                }
            }
            if (oblig) Falta(c, "número " + v);
            return false;
        }

        static object Convertir(Object v, Type destino)
        {
            if (EsNulo(v)) return null;
            if (destino.IsInstanceOfType(v)) return v;
            GameObject go = v as GameObject;
            if (go == null && v is Component) go = ((Component)v).gameObject;
            if (go == null) return null;
            if (destino == typeof(GameObject)) return go;
            if (typeof(Component).IsAssignableFrom(destino))
            {
                var r = go.GetComponent(destino);
                return EsNulo(r) ? null : r;
            }
            return null;
        }

        static bool Ref(Component c, Object v, bool oblig, params string[] p)
        {
            if (EsNulo(v)) return false;
            return Poner(c, t => typeof(Object).IsAssignableFrom(t), t => Convertir(v, t), oblig, "referencia a '" + v.name + "'", p);
        }

        static bool EsLista(Type t, out Type elem)
        {
            elem = null;
            if (t.IsArray) { elem = t.GetElementType(); return true; }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) { elem = t.GetGenericArguments()[0]; return true; }
            return false;
        }

        static object CrearLista(Type t, Type elem, IList<object> items)
        {
            if (t.IsArray)
            {
                var a = Array.CreateInstance(elem, items.Count);
                for (int i = 0; i < items.Count; i++) a.SetValue(items[i], i);
                return a;
            }
            var l = (IList)Activator.CreateInstance(t);
            foreach (var x in items) l.Add(x);
            return l;
        }

        static bool ListaObjetos(Component c, IList<Object> vs, bool oblig, params string[] p)
        {
            Type e0;
            return Poner(c, t => EsLista(t, out e0) && typeof(Object).IsAssignableFrom(e0), t =>
            {
                Type e;
                EsLista(t, out e);
                var items = new List<object>();
                foreach (var v in vs)
                {
                    var x = Convertir(v, e);
                    if (EsNulo(x)) return null;
                    items.Add(x);
                }
                return CrearLista(t, e, items);
            }, oblig, "lista de " + vs.Count + " objetos", p);
        }

        static bool ListaEnteros(Component c, int[] vs, bool oblig, params string[] p)
        {
            Type e0;
            return Poner(c, t => EsLista(t, out e0) && e0 == typeof(int), t =>
            {
                Type e;
                EsLista(t, out e);
                return CrearLista(t, e, vs.Cast<object>().ToList());
            }, oblig, "combinación " + string.Join(",", vs), p);
        }

        static bool ListaTextos(Component c, string[] vs, bool oblig, params string[] p)
        {
            Type e0;
            return Poner(c, t => EsLista(t, out e0) && e0 == typeof(string), t =>
            {
                Type e;
                EsLista(t, out e);
                return CrearLista(t, e, vs.Cast<object>().ToList());
            }, oblig, "lista de " + vs.Length + " textos", p);
        }

        // ============================== EVENTOS ==============================

        static UnityEvent Evento(Component c, bool oblig, params string[] p)
        {
            if (c == null) return null;
            var f = Buscar(c, x => typeof(UnityEvent).IsAssignableFrom(x.FieldType), p);
            if (f == null) { if (oblig) Falta(c, "evento (" + p[0] + ")"); return null; }
            camposUsados.Add(Clave(c, f));
            var e = f.GetValue(c) as UnityEvent;
            if (e == null) { e = (UnityEvent)Activator.CreateInstance(f.FieldType); f.SetValue(c, e); }
            EditorUtility.SetDirty(c);
            return e;
        }

        static void Prender(UnityEvent e, GameObject go, bool valor)
        {
            if (e == null || go == null) return;
            UnityEventTools.AddBoolPersistentListener(e, go.SetActive, valor);
        }

        static void Llamar(UnityEvent e, Component destino, string metodo)
        {
            if (e == null || EsNulo(destino)) return;
            var mi = destino.GetType().GetMethod(metodo, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            if (mi == null)
            {
                Aviso(destino.GetType().Name + " no tiene una función pública '" + metodo + "()'. Conéctala a mano en " + destino.gameObject.name + ".");
                return;
            }
            UnityEventTools.AddVoidPersistentListener(e, (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), destino, mi));
        }

        // ============================== UTILIDADES ==============================

        static void Prop(object o, string nombre, object v)
        {
            try
            {
                var p = o.GetType().GetProperty(nombre, BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.CanWrite) p.SetValue(o, v);
            }
            catch { }
        }

        static void PropEnum(object o, string nombre, string valor)
        {
            try
            {
                var p = o.GetType().GetProperty(nombre, BindingFlags.Public | BindingFlags.Instance);
                if (p != null && p.CanWrite && p.PropertyType.IsEnum) p.SetValue(o, Enum.Parse(p.PropertyType, valor));
            }
            catch { }
        }

        static void Falta(Component c, string que)
        {
            var lista = string.Join(", ", Campos(c.GetType()).Select(f => f.Name + ":" + NombreTipo(f.FieldType)));
            Aviso("[" + c.gameObject.name + " → " + c.GetType().Name + "] no supe dónde poner: " + que + ". Campos del script: " + lista);
        }

        static string NombreTipo(Type t)
        {
            Type e;
            return EsLista(t, out e) ? e.Name + "[]" : t.Name;
        }

        static string Corto(string s) { return s.Length > 30 ? s.Substring(0, 30) + "…" : s; }

        static void Aviso(string s) { avisos.Add(s); }

        static void Iniciar()
        {
            avisos.Clear();
            sonidosFaltan.Clear();
            camposUsados.Clear();
            cacheSprites.Clear();
            cacheAudio.Clear();
        }

        static void Terminar(string que)
        {
            var lista = avisos.Distinct().ToList();
            var sb = new StringBuilder();
            sb.AppendLine("VOLVISTE · Constructor B · " + que);
            if (lista.Count == 0) sb.AppendLine("Todo quedó asignado.");
            else
            {
                sb.AppendLine(lista.Count + " cosas para revisar:");
                foreach (var a in lista) sb.AppendLine("• " + a);
            }
            if (sonidosFaltan.Count > 0)
                sb.AppendLine("Sonidos que todavía no están en 04_Audio (vuelve a construir cuando lleguen): " + string.Join(", ", sonidosFaltan.Distinct()));
            var texto = sb.ToString();
            if (lista.Count == 0) Debug.Log(texto); else Debug.LogWarning(texto);
            EditorGUIUtility.systemCopyBuffer = texto;
            EditorUtility.DisplayDialog("Volviste · " + que,
                lista.Count == 0
                    ? "¡Listo! Todo quedó asignado.\n(El reporte quedó copiado: Ctrl+V)"
                    : "Listo, pero hay " + lista.Count + " cosas para revisar.\nEl reporte quedó copiado: pégalo (Ctrl+V) en el chat.",
                "OK");
        }
    }
}
#endif
