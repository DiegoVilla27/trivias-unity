# Trivias — Reglas del proyecto (OBLIGATORIAS)

Estas reglas se cumplen siempre, sin excepción. Si una regla de `.agents/AGENTS.md` o de una skill contradice este archivo, **prevalece este archivo**.

## 0. Contexto

- Juego: concurso de trivias cooperativo 2–6 jugadores, PC/Steam. Unity 6 (6000.6) + URP, tercera persona con Cinemachine, Input System, Netcode for GameObjects + Facepunch.Steamworks, voz de Steam enrutada por el host.
- Diseño: `docs/GDD.md` (referencia principal) y `docs/*.md` en la raíz del repositorio. Bocetos en `docs/bocetos/`.
- **Documentación siempre al día (regla del usuario):** todo cambio de reglas, contenido, interfaz o arquitectura se refleja en el mismo trabajo en el GDD (con entrada en su historial) y en los docs afectados (`LEVEL`, `PLAYER`, `SISTEMAS`, `CONTENIDO`, `UI_UX`, `RECURSOS`, `DESARROLLO`). Comprobar que los enlaces internos siguen funcionando.
- Fase actual: **MVP**. Desde el 16 de octubre de 2026 el plató y sus objetos usan los modelos 3D de cada módulo (`Modules/*/Models/`: OBJ con materiales URP; los `.glb` no se importan): cabinas y tubos, mesa, botón, estación de cambio, dinero, pantalla y decorado de concepto-01. Lo que aún no tiene modelo (presentador, cosméticos, podio, público, pedestal de la estación) sigue con primitivas. Personajes: los diez completos del usuario tal cual (1M triángulos, 1,9 m, sin retocar texturas), riggeados en Blender con el mismo esqueleto (`Player/Models/Characters/`, Humanoid), con las animaciones de Starter Assets; seis gratuitos y cuatro en la tienda.
- Agente y skills: `.agents/AGENTS.md` (importado abajo, siempre en contexto) y `.agents/skills/` (enlazadas en `.claude/skills/`, invocables con la herramienta Skill). Antes de trabajar en un área, **cargar la skill correspondiente** (p. ej. `unity-networking-multiplayer`, `unity-camera-systems`, `unity-scriptable-objects`, `unity-performance-profiling`, `unity-core-architecture`).

Instrucciones del agente Unity del proyecto:

@.agents/AGENTS.md

## 1. Arquitectura modular por carpetas

**Regla de oro: cada módulo contiene TODO lo suyo** (scripts, prefabs, materiales, modelos, animaciones, audio, configuración, tests). Quien busque algo de la cabina va a `Modules/Booth/` y lo encuentra todo ahí. No existe una carpeta global `Art/`, `Materials/` o `Scripts/` con cosas mezcladas.

```text
Assets/
├── _Project/
│   ├── Modules/
│   │   └── <Module>/                     # p. ej. Player, Camera, Booth, Money...
│   │       ├── Scripts/
│   │       │   ├── Runtime/
│   │       │   │   ├── <Proyecto>.<Module>.asmdef
│   │       │   │   ├── Data/            # ScriptableObjects de config/definición, structs, enums
│   │       │   │   ├── Logic/           # "Backend": C# puro, sin MonoBehaviour, testeable
│   │       │   │   ├── Components/      # "Frontend": MonoBehaviours finos que delegan en Logic
│   │       │   │   └── Network/         # NetworkBehaviours y mensajes de red del módulo
│   │       │   └── Editor/              # Herramientas de editor del módulo (asmdef solo Editor)
│   │       ├── Config/                  # Assets .asset de los ScriptableObjects (valores)
│   │       ├── Prefabs/
│   │       ├── Materials/
│   │       ├── Models/
│   │       ├── Animations/
│   │       ├── Audio/
│   │       ├── UI/                      # UI propia del módulo (si la tiene)
│   │       └── Tests/
│   │           ├── EditMode/            # Tests de Logic (rápidos, sin escena)
│   │           └── PlayMode/
│   ├── Shared/                          # Lo que usan 2+ módulos, con la misma estructura
│   │   ├── Scripts/Runtime/             # Trivias.Shared.asmdef: eventos, interfaces, utilidades, constantes
│   │   ├── Materials/
│   │   ├── Prefabs/
│   │   ├── Audio/
│   │   └── UI/
│   ├── Scenes/                          # Escenas del juego (Bootstrap, Lobby, Stage)
│   └── Settings/                        # URP, Input, calidad, volúmenes del proyecto
└── ThirdParty/                          # Paquetes externos sin modificar (StarterAssets, Facepunch...)
```

Reglas:

1. **Solo se crean las subcarpetas que se usan.** No crear carpetas vacías "por si acaso".
2. **Algo pasa a `Shared/` solo cuando lo usan dos o más módulos.** Si solo lo usa uno, vive en ese módulo.
3. **Un `.asmdef` por módulo** (`Trivias.<Module>`), otro para `Editor/` y otro para cada carpeta de tests. Namespace = `Trivias.<Module>` (+ subcarpeta si aplica: `Trivias.Booth.Logic`).
4. **Dependencias:** un módulo puede referenciar `Trivias.Shared` y otros módulos **solo en una dirección y sin ciclos**, declarados en la tabla de la sección 3. Lo preferido es comunicarse por interfaces y canales de eventos en `Shared`. Nunca referenciar las clases internas de otro módulo "porque es rápido".
5. **Logic = backend, Components = frontend.** Las reglas del juego (dinero, multiplicador, comodines, fichas, ranking) van en C# puro en `Logic/` y se prueban con tests EditMode. Los MonoBehaviour solo reciben eventos de Unity, leen/escriben estado y presentan.
6. **Terceros no se tocan.** Si hay que adaptar un script de `ThirdParty/`, se envuelve o se copia al módulo propio y se modifica ahí.
7. **Nombres en inglés** para carpetas, código y assets; documentación y comentarios para el usuario en español. Glosario en la sección 5.
8. Prefabs: un prefab raíz por entidad (`Booth.prefab`), variantes con Prefab Variants, nunca duplicar prefabs para cambiar un color.

## 2. Build time vs. load time vs. runtime (rendimiento)

Antes de escribir código, decidir **cuándo** se hace cada cosa. Por defecto, todo lo que se pueda hacer antes, se hace antes.

| Momento | Qué va aquí | Ejemplos en este juego |
| --- | --- | --- |
| **Edición / build** (coste 0 en juego) | Todo lo que no cambia durante la partida | Geometría del plató colocada en la escena y marcada Static; iluminación baked, light probes y reflection probes; prefabs montados; configs en ScriptableObjects; preguntas y frases importadas y **validadas en el editor**; atlas de fuentes TMP; capas y matriz de colisiones; stripping de shaders y código |
| **Carga** (una vez, al abrir escena o empezar partida) | Lo que depende de la partida pero no cambia cada frame | Spawn de jugadores; pools de fajos/bolsas/lingotes pre-instanciados; selección de las 15 preguntas; cachear referencias y `GetComponent`; reparto de comodines según el tamaño del grupo |
| **Por evento** (solo cuando algo ocurre) | Cambios de estado | Coger/depositar dinero, convertir, apostarse, verificar, resolver, actualizar textos de bandejas y de apostados, mensajes del presentador |
| **Por frame** (`Update`) | Solo lo que es continuo por naturaleza | Movimiento del personaje, cámara, interpolación de red, animaciones |

Reglas:

1. **Pregunta de control:** ¿cambia durante la partida? No → edición/build. ¿Cambia por partida pero no por frame? → carga. ¿Cambia por una acción? → evento. Solo si es continuo → `Update`.
2. **Nada de polling.** La UI y los textos se actualizan por eventos, no comprobando en `Update`.
3. **Cero allocations en `Update`/`FixedUpdate`/`LateUpdate`**: sin LINQ, sin `new` de colecciones, sin concatenar strings, sin closures, sin `GetComponent`, sin `Find*`.
4. **Pooling** para objetos que aparecen y desaparecen (dinero, textos flotantes). Nunca `Instantiate`/`Destroy` en caliente durante la ronda.
5. **Hashes y IDs precalculados**: `static readonly int` para parámetros de Animator (`Animator.StringToHash`), propiedades de shader (`Shader.PropertyToID`), y `LayerMask` cacheados.
6. **Materiales compartidos.** Cambiar color por instancia con `MaterialPropertyBlock` o materiales distintos creados en el editor; nunca `renderer.material` en runtime (duplica materiales).
7. **Validación en editor, no en runtime**: `OnValidate`, herramientas de editor y tests comprueban datos (p. ej. que cada pregunta tenga 4 opciones y una correcta). En runtime no se re-valida contenido ya validado.
8. **Código de editor fuera del build**: carpetas `Editor/` con su asmdef de editor, o `#if UNITY_EDITOR`. Logs de depuración con `[Conditional("UNITY_EDITOR")]` o un logger que se elimina en release.
9. **Red**: sincronizar estado mínimo (ids, valores, estados), no transforms innecesarios. Objetos estáticos del plató no son NetworkObjects. El host calcula, los clientes presentan.
10. **Medir antes de optimizar a ciegas**: ante dudas de rendimiento, usar el Profiler (skill `unity-performance-profiling`).

## 3. Módulos y dependencias permitidas

Se actualiza cada vez que se crea un módulo. Una flecha significa "puede referenciar".

| Módulo | Contenido | Puede depender de |
| --- | --- | --- |
| Shared | Canales de eventos, capas, logger, puntos de aparición, contratos (interacción, jugadores, sala), perfil del jugador (nombre, personaje, fichas y compras; reglas del nombre en `PlayerNameRules`), catálogo y reglas de personajes (`CharacterCatalog`, `CharacterRules`: gratuitos, elegir y no repetir en la sala), opciones del jugador (`GameSettings`), recorridos de los tubos (`TubePath`, `TubeRegistry`), animaciones de interfaz (`UiButtonFeel`, `UiPanelIntro`, `UiMarquee`, `UiPulse`); utilidades de editor (`Shared.Editor`), incluido el plano del plató (`StageLayout`: medidas y posiciones que usan todos los constructores), sus tipografías (`StageFonts`: Lilita One y Poppins, en `Shared/UI/Fonts`) el tema de la interfaz (`UiTheme`, aplicado al guardar cualquier prefab con Canvas) y el kit de los menús (`UiKit`: piezas fieles a los bocetos de `docs/bocetos/ui/menu`, medidas a 1920 × 1080 desde la esquina superior izquierda; el tema no toca lo hecho con el kit); en runtime, el fondo de los menús (`UiBackdrop`: pantallas que lo piden y ventanas que tapan) y la vista 3D de personajes y complementos (`CharacterPreview`, `CharacterPreviewImage`) | — |
| Networking | NetworkManager, conexión local (Unity Transport), admisión a sala; se publica como `IRoomConnection` | Shared |
| Player | Concursante: personaje (lo pide el dueño, el host evita repetirlo; `PlayerCharacterView` lo pinta; `CharacterShowcase` lo muestra en 3D en los menús, con complementos, y hace sus retratos al cargar), entrada, movimiento, carrera (Shift), salto, mirada, nombre (lo pide el dueño y lo acepta el host), punto de carga, red | Shared |
| Interaction | Interactor por cercanía del jugador local y aviso en pantalla | Shared |
| Money | Libro de cuentas (Logic), sesión en red, objetos visibles con pool, mesa y bandejas interactivas, bloqueo y resolución de rondas | Shared |
| Questions | Banco de preguntas (JSON en `Content/` importado y validado en edición), historial de preguntas vistas, planificación (bloques, historial del grupo y variedad de categorías) y orden de opciones | Shared |
| Match | Flujo de la partida en red (fases, verificación, revelación, victoria/derrota, resultado, volver a jugar), historial de preguntas del grupo en red, textos de cabinas y pantalla | Shared, Questions, Money, SelfBet |
| VerifyButton | Botón VERIFICAR interactivo | Shared, Match, Money, Questions |
| HUD | Pregunta, opciones, estado, cuenta atrás, dinero del equipo y comodines | Shared, Match, Money, Questions, SelfBet |
| Exchange | Estación de cambio interactiva (agrupar y dividir) | Shared, Money |
| SelfBet | Comodines: reparto, reserva, zonas de apuesta, confinamiento, multiplicador y expulsión | Shared |
| Presenter | Presentador por texto: catálogo de frases (JSON validado en edición), momentos en que habla y elección por evento sin repetir, viñeta de cómic con ánimo (letra a letra) | Shared, Match, Money, SelfBet, Questions |
| Results | Pantalla de resultados (con el puesto en los récords), volver a jugar y salir | Shared, Match, Money, Questions, Ranking |
| Menu | Pantallas como los bocetos (`MenuPrefabBuilder` con `UiKit`): menú principal (tu nombre, tu personaje en 3D, crear, unirse, cómo se juega, récords, configuración, salir), invitación, tutorial (JSON en `Content/`), tabla de récords, configuración, sala de espera y pausa (Esc) | Shared, Match, Money, Questions, Ranking |
| Ranking | Récords locales de victorias (C# puro: quién entra, orden y empates) guardados en JSON | Shared |
| Spectator | Cámara de espectador del eliminado (sigue a los activos) y su rótulo | Shared |
| Shop | Tienda del vestíbulo como los bocetos (pestañas Personajes y Accesorios, tarjetas, vista 3D; reglas en `ShopRules`), cosméticos con primitivas, personajes de pago, catálogo de cosméticos y presentador que viste a los concursantes | Shared |
| Feedback | Sonido (efectos, música y sonidos del plató sintetizados en edición), confeti, podio de la victoria, ambiente de luz por momento y plató vivo (bombillas, público, cámaras de estudio y focos) | Shared, Match, Money, SelfBet, Questions |
| Camera (`Trivias.Cameras`) | Rig Cinemachine en tercera persona, captura del ratón | Shared |
| Booth, Table, Stage | Solo constructores de prefabs (Editor) con sus modelos; Booth incluye la plataforma de apuesta y los tubos del dinero y del concursante; Stage, el decorado del plató y el volumen de posprocesado | Shared (Editor) |

Comunicación entre módulos sin referencias directas: contratos en `Shared` (`IInteractable`, `ICarrySocket`) y registros/anclajes como ScriptableObject (`InteractableRegistry`, `PlayerAvatarRegistry`, `RoomConnectionRuntime`, `MoneyRuntime`, `MatchRuntime`, `SelfBetRuntime`, `LocalRecords`, `QuestionHistory`, `TubeRegistry`). `App` une los módulos en tiempo de edición (p. ej. añade el interactor al concursante y conecta mesa y bandejas con Money). Los objetos de red de la sesión (p. ej. `MoneySession`) los crea el host al abrir la sala.
| App | Raíz de composición: `Build All`, escena Stage y pruebas de humo. Es el único módulo que conoce a todos | Todos |

Excepción de nombre: el módulo `Camera` usa el namespace `Trivias.Cameras` porque `Trivias.Camera` choca con `UnityEngine.Camera`.

Módulos previstos aún sin crear: `Voice`.

## 4. Flujo de trabajo con Unity

- **Todo se construye por código en tiempo de edición.** Cada módulo tiene un constructor en `Scripts/Editor/*Builder.cs` que crea sus prefabs con los modelos de su carpeta `Models/` (`EditorAssets.PlaceModel`, con colisiones sencillas de caja o cilindro; los modelos no traen colisiones) o con primitivas. Los modelos son pesados: el decorado lejano no proyecta sombras y todo lo estático se marca para batching. `App/ProjectBuilder` los ejecuta en orden y ensambla `Scenes/Stage.unity`. Los constructores son idempotentes: se pueden repetir sin duplicar nada.
- No editar a mano prefabs o escenas generados: cambiar el constructor.
- **La escena siempre debe estar lista para jugar.** `App/AutoBuild` guarda en `Settings/BuildStamp.txt` una huella del código y del contenido; si al compilar en el editor no coincide, reconstruye todo solo. El usuario nunca debería tener que ejecutar Build All. La reconstrucción automática **nunca cambia la escena que el usuario tiene abierta** (`OpenScenesKeeper` la restaura). Para no provocar recompilaciones mientras el usuario trabaja, el código se cambia preferiblemente con Unity cerrado.
- **Pruebas en cada cambio (regla del usuario, sin excepciones):** todo lo nuevo o modificado lleva pruebas nuevas que lo cubran (lógica en EditMode; comportamiento en juego, escena y textos en PlayMode; distribución del plató en `StageLayoutTests`). Después se ejecuta **la batería completa**, nuevas y anteriores, EditMode y PlayMode, nunca solo las nuevas. Si una prueba antigua falla, se arregla la causa antes de entregar.
- **Antes de entregar cualquier cambio (obligatorio):** con Unity cerrado, ejecutar Build All en consola, luego los tests EditMode (incluyen `WiringTests`: ninguna referencia vacía en escena y prefabs, y huella al día) y PlayMode. Si Unity está abierto, pedir que lo cierre; no dar nada por bueno sin esta verificación.
- **Pruebas rápidas:** las pruebas en juego usan un ritmo de partida corto (`FastMatchTimings`: cuenta atrás 0,5 s y revelación 1 s, sin tocar `MatchConfig` guardado). Nunca esperar duraciones del juego con números fijos: usar `PlayModeHelpers.AfterLock/AfterReveal/AfterRound`, y tras abrir sala `UntilHostReady()` en vez de esperas fijas. Si una prueba necesita el ritmo real, `PlayModeHelpers.SetPace` solo en esa prueba y restaurarlo al acabar. La verificación completa tarda unos 3 minutos; si crece, medir qué prueba la alarga.
- **Cero errores y cero avisos:** revisar en cada log toda línea de `Assets/` con `error` o `warning` (no solo `CS`: también analizadores como `UAC`). Comando: `grep -E "Assets/.*(error|warning)" <log>`.
- **Nunca pedir al usuario que abra Unity o pruebe** hasta que compilación, Build All y todos los tests estén en verde. Si hay cambios sin verificar y Unity está abierto, avisar de que se recompilará con código no verificado.
- Evitar nombres que choquen con namespaces del proyecto (`Exchange`, `Table`, `Camera`...) en métodos y variables: prefijos claros (`FindTable()`).
- Un campo `[SerializeField]` nuevo se asigna en el constructor del **prefab** que lo contiene (no solo en la escena), o `WiringTests` fallará.
- La lógica de red solo se prueba aquí con el host (un proceso). Lo que dependa de clientes remotos hay que razonarlo con cuidado y avisar al usuario de que lo verifique con Multiplayer Play Mode.
- Comandos en modo consola (Unity **cerrado**; si está abierto, usar el menú):
  - Construir: `Unity -batchmode -nographics -projectPath . -executeMethod Trivias.App.Editor.ProjectBuilder.BuildAllBatch -logFile <log>`
  - Tests EditMode: `Unity -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults <xml> -logFile <log>`
  - Tests PlayMode: igual con `-testPlatform PlayMode` (sin `-nographics`).
  - Ruta de Unity: `/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity`.
- Después de cada cambio: compilar sin errores ni warnings, pasar los tests y revisar el log.
- Prefabs de red: tras crearlos por código, llamar a `NetworkManagerPrefabBuilder.EnsureGlobalObjectIdHash` (si no, el hash queda en 0 y no aparecen en red). La lista de prefabs de red vive en `Networking/Config/DefaultNetworkPrefabs.asset`.
- Pruebas multijugador en local: **Multiplayer Play Mode** (Window > Multiplayer > Multiplayer Play Mode).
- Iluminación: de momento una luz direccional en tiempo real y luz ambiente. Precalcular la iluminación queda pendiente para el acabado.
- **GPU Resident Drawer desactivado** (`Settings/PC_RPAsset`): en Unity 6000.6 una cámara que pinta en una imagen (el expositor del personaje del menú) hace que Unity se cuelgue al descargar la escena. No reactivarlo sin comprobarlo con la batería PlayMode completa.

## 5. Glosario español → código

| Diseño | Código |
| --- | --- |
| Cabina | `Booth` |
| Bandeja | `Tray` |
| Zona de apuesta | `BetZone` |
| Fajo / bolsa / lingote | `Bundle` / `Bag` / `GoldBar` (tipo común: `MoneyItem`, denominación: `Denomination`) |
| Mesa central | `Table` |
| Estación de cambio | `ExchangeStation` |
| Botón verificar | `VerifyButton` |
| Comodín (apostarse uno mismo) | `SelfBet` |
| Reserva compartida de comodines | `SelfBetReserve` |
| Presentador | `Presenter` (no `Host`: en red, host es el jugador anfitrión) |
| Pregunta / respuesta | `Question` / `Answer` |
| Partida / ronda | `Match` / `Round` |
| Fichas | `Tokens` |
| Espectador / eliminado | `Spectator` / `Eliminated` |
| Plató | `Stage` |

## 6. Otras reglas

- No hacer commits salvo que el usuario lo pida. Mensajes con Conventional Commits (skill `conventional-commits`).
- No dejar valores de balance (capital, denominaciones, comodines, fichas) escritos en el código: van en ScriptableObjects de `Config/`.
- Seguir las prácticas prohibidas de `.agents/AGENTS.md` (sin `Find*`, sin `SendMessage`, sin `async void`, sin campos públicos en MonoBehaviour, física en `FixedUpdate`).
- Diferencia con `.agents/AGENTS.md`: allí el arte va en una carpeta global `Art/`; **aquí no**: el arte vive dentro de cada módulo.

## Bocetos: precisión y alcance acordados con el usuario

- Flujo vigente: personajes completos con pelo, ropa y zapatos integrados. Catálogo en `docs/bocetos/player/characters/README.md`; cada personaje tiene su propia carpeta.
- Una sola imagen final por personaje, con ocho vistas como Blue Tracksuit, siempre en **A-pose**. No generar T-pose, despieces, variantes ni láminas adicionales salvo petición explícita.
- Blue Tracksuit es la imagen original aprobada, trasladada a `docs/bocetos/player/characters/01-blue-tracksuit/blue-tracksuit-a-pose.png`. Mantener identidad, proporciones y estilo en los otros personajes.
- El usuario eliminó los antiguos clothes, hair, shoes, bases y T-poses: no restaurarlos ni referenciarlos como disponibles. El archivo restante contiene solo la referencia general original y un brief histórico. El flujo de personajes completos sustituye las anteriores instrucciones de entregar piezas separadas.
- Revisar referencias y alcance antes de generar para evitar gasto innecesario. Aplicar correcciones al archivo existente sin duplicar entregables. En esta fase el trabajo permanece en docs.
- No usar Higgsfield.
