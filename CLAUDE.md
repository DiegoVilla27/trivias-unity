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
7. **Nombres en inglés** para carpetas, código y assets; documentación y comentarios para el usuario en español. Glosario en la sección 4.
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
| Shared | Eventos, interfaces, utilidades, constantes | — |
| _(se rellena al crear cada módulo)_ | | |

Módulos previstos para el MVP: `Networking`, `Voice`, `Player`, `Camera`, `Interaction`, `Money`, `Exchange`, `Table`, `Booth`, `SelfBet`, `VerifyButton`, `Questions`, `Match`, `Presenter`, `Stage`, `HUD`, `Results`.

## 4. Glosario español → código

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

## 5. Otras reglas

- No hacer commits salvo que el usuario lo pida. Mensajes con Conventional Commits (skill `conventional-commits`).
- No dejar valores de balance (capital, denominaciones, comodines, fichas) escritos en el código: van en ScriptableObjects de `Config/`.
- Seguir las prácticas prohibidas de `.agents/AGENTS.md` (sin `Find*`, sin `SendMessage`, sin `async void`, sin campos públicos en MonoBehaviour, física en `FixedUpdate`).
- Diferencia con `.agents/AGENTS.md`: allí el arte va en una carpeta global `Art/`; **aquí no**: el arte vive dentro de cada módulo.
