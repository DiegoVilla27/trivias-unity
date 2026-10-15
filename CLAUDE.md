# Trivias — Reglas del proyecto (OBLIGATORIAS)

Estas reglas se cumplen siempre, sin excepción. Si una regla de `.agents/AGENTS.md` o de una skill contradice este archivo, **prevalece este archivo**.

## 0. Contexto

- Juego: concurso de trivias cooperativo 2–6 jugadores, PC/Steam. Unity 6 (6000.6) + URP, tercera persona con Cinemachine, Input System, Netcode for GameObjects + Facepunch.Steamworks, voz de Steam enrutada por el host.
- Diseño: `~/Desktop/Trivias/GDD.md` (referencia principal) y `~/Desktop/Trivias/docs/*.md`. Bocetos en `~/Desktop/Trivias/docs/bocetos/`. Una regla de juego nueva va primero al GDD y luego a los docs afectados.
- Fase actual: **MVP**. Todo el arte son primitivas de Unity (cubo, cilindro, esfera, cápsula, plano) con materiales simples. Única excepción: el personaje `PlayerArmature` de Starter Assets.
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
| Shared | Canales de eventos, capas, logger, puntos de aparición; utilidades de editor (`Shared.Editor`) | — |
| Networking | NetworkManager, conexión local (Unity Transport), admisión a sala, panel de depuración | Shared |
| Player | Concursante: entrada, movimiento, mirada, nombre, punto de carga, red | Shared |
| Interaction | Interactor por cercanía del jugador local y aviso en pantalla | Shared |
| Money | Libro de cuentas (Logic), sesión en red, objetos visibles con pool, mesa y bandejas interactivas, bloqueo y resolución de rondas | Shared |
| Questions | Banco de preguntas (JSON en `Content/` importado y validado en edición), planificación y orden de opciones | Shared |
| Match | Flujo de la partida en red (fases, verificación, revelación, victoria/derrota), textos de cabinas y pantalla | Shared, Questions, Money |
| VerifyButton | Botón VERIFICAR interactivo | Shared, Match, Money, Questions |
| HUD | Pregunta, opciones, estado, cuenta atrás y dinero del equipo | Shared, Match, Money, Questions |
| Camera (`Trivias.Cameras`) | Rig Cinemachine en tercera persona, captura del ratón | Shared |
| Booth, Table, Exchange, Presenter, Stage | De momento solo constructores de prefabs (Editor) | Shared (Editor) |

Comunicación entre módulos sin referencias directas: contratos en `Shared` (`IInteractable`, `ICarrySocket`) y registros/anclajes como ScriptableObject (`InteractableRegistry`, `CarrySocketRegistry`, `MoneyRuntime`, `MatchRuntime`). `App` une los módulos en tiempo de edición (p. ej. añade el interactor al concursante y conecta mesa y bandejas con Money). Los objetos de red de la sesión (p. ej. `MoneySession`) los crea el host al abrir la sala.
| App | Raíz de composición: `Build All`, escena Stage y pruebas de humo. Es el único módulo que conoce a todos | Todos |

Excepción de nombre: el módulo `Camera` usa el namespace `Trivias.Cameras` porque `Trivias.Camera` choca con `UnityEngine.Camera`.

Módulos previstos aún sin crear: `SelfBet`, `Results`, `Voice`.

## 4. Flujo de trabajo con Unity

- **Todo se construye por código en tiempo de edición.** Cada módulo tiene un constructor en `Scripts/Editor/*Builder.cs` que crea sus materiales y prefabs con primitivas. `App/ProjectBuilder` los ejecuta en orden y ensambla `Scenes/Stage.unity`. Los constructores son idempotentes: se pueden repetir sin duplicar nada.
- No editar a mano prefabs o escenas generados: cambiar el constructor.
- **La escena siempre debe estar lista para jugar.** `App/AutoBuild` guarda en `Settings/BuildStamp.txt` una huella del código y del contenido; si al compilar en el editor no coincide, reconstruye todo solo. El usuario nunca debería tener que ejecutar Build All.
- **Antes de entregar cualquier cambio (obligatorio):** con Unity cerrado, ejecutar Build All en consola, luego los tests EditMode (incluyen `WiringTests`: ninguna referencia vacía en escena y prefabs, y huella al día) y PlayMode. Si Unity está abierto, pedir que lo cierre; no dar nada por bueno sin esta verificación.
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
