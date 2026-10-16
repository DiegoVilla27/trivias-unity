# Recursos — Inventario de producción

Referencia: [GDD](GDD.md), secciones 6–9 y 13. Lista de recursos necesarios; no implica que existan o estén comprados.

## Arte 3D

Referencias específicas de modelado: [36 láminas individuales y notas](bocetos/recursos/detallados/README.md). Son bocetos, no medidas finales ni modelos terminados.

| Recurso | Prototipo | Salida |
| --- | --- | --- |
| Personaje base con esqueleto | 1 | Misma base |
| Apariencias básicas | 1 | 10 personajes/variantes disponibles |
| Cabina y bandeja | Modelos A–D integrados (bandeja en el mostrador) | Acabado del mismo conjunto |
| Tubo de expulsión | Recto, codo y anillo encadenados por código: tubo del dinero y tubo del concursante en cada cabina, con su línea central para la succión y una ráfaga de aire (partículas) | Acabado visual |
| Mesa y estación de cambio | Modelos integrados (la estación, sobre un pedestal) | Mismo conjunto pulido |
| Botón de verificar | Modelo integrado (la tapa se hunde) | Mismo objeto pulido |
| Fajo, bolsa y lingote | Modelos integrados | Valores legibles y acabado |
| Plató modular | Kit de ambiente integrado (ver abajo) | Iluminación precalculada y optimización |
| Presentador visual | Representación provisional | Diseño propio, escala por definir |
| Cheque/podio | Resultado sencillo | Ceremonia de victoria |

Los cosméticos adicionales de tienda requieren catálogo por definir. No confundir los diez personajes básicos gratuitos con atuendos que se compran usando fichas.

La referencia visual vigente del player es el catálogo de personajes completos en A-pose; las bases y los despieces anteriores fueron eliminados. El catálogo es documentación visual; el concursante jugable sigue siendo el descrito en [PLAYER](PLAYER.md). Se descartaron las piezas sueltas en el juego: el concursante será uno de unos diez personajes completos (de momento, el robot de Starter Assets).

Catálogo visual vigente: [diez personajes completos en A-pose](bocetos/player/characters/README.md), cada uno en su carpeta y con una única lámina de ocho vistas. Blue Tracksuit conserva la imagen A-pose original aprobada, ahora renombrada; los otros nueve integran sus atuendos, peinados y zapatos sobre la misma identidad visual. No se generan T-poses ni despieces como entregables del catálogo actual.

Los antiguos bocetos separados de ropa, pelo, zapatos, bases y T-poses fueron eliminados por el usuario. El [archivo histórico restante](bocetos/player/archivo/README.md) conserva únicamente otros antecedentes. Este cambio organiza los bocetos de documentación; no modifica los modelos, rigs, prefabs ni el sistema de piezas que esté implementado en Unity.

## Kit modular de ambiente

Referencia visual: [ambiente-modelado-01.png](bocetos/plato/ambiente-modelado-01.png). Mantiene el estilo del plató y propone módulos reutilizables:

- Cámara de estudio y trípode decorativos.
- Foco con viseras, foco pequeño y tira de bombillas.
- Panel de pared, esquina, columna y molduras.
- Cortina recta, lateral recogida y cortina superior.
- Baldosa de suelo, insertos dorados rectos/curvos, borde de escenario, escalera y barandilla.
- Panel de techo, viga modular, conector, soporte de foco y cable simplificado.
- Altavoz y rejilla decorativos.

Priorizar paredes, suelo, techo básico y luces para construir el volumen del plató. Añadir cortinas, cámaras y accesorios después.

Implementado (16 de octubre de 2026): todos los módulos del kit están en el plató salvo la esquina (es una esquina exterior, sin sitio en la sala), el conector de vigas y el cable (los cables son varillas, más baratas). Plataforma del concursante: es la zona de apuesta de cada cabina. Los modelos son muy detallados (la viga tiene 138.000 triángulos), así que se usan pocas copias, a escala, sin sombras en lo lejano. Si el juego va lento en equipos modestos, el siguiente paso es simplificar las mallas o añadir niveles de detalle. No requiere cámaras funcionales, telas simuladas ni iluminación dinámica en cada bombilla; la implementación visual se decidirá según rendimiento y motor.

## Tipografías

Lilita One y Poppins (SemiBold y ExtraBold), de Google Fonts con licencia SIL Open Font License; los archivos y sus licencias están en `Assets/_Project/Shared/UI/Fonts`. Se pueden incluir y distribuir con el juego.

## Animación y efectos

- Personaje: reposo, locomoción, transporte y expulsión; transiciones según [PLAYER.md](PLAYER.md).
- Cabinas: bloqueo, acierto y pérdida; pueden resolverse con luces y efectos sencillos.
- Dinero: coger, depositar, conversión y retirada, sin física compleja obligatoria.
- Acierto: confeti de colores en dos chorros a los lados de la pantalla grande (implementado).
- Victoria: lluvia de confeti dorado en toda la sala, cheque gigante en la pantalla (BANCO TRIVIAS, páguese a los supervivientes, cantidad, firma del Presentador) y podio dorado con placas de nombre (implementado, aún con primitivas). No necesita un nivel nuevo.
- Iluminación: cuatro focos de colores por ambiente (sin sombras) además de la luz principal.

## Sonido

Implementado en el MVP (módulo `Feedback`), todo **sintetizado por código al construir** (`.wav` en `Modules/Feedback/Audio/`, sin audio externo ni licencias):

| Momento | Sonido |
| --- | --- |
| Coger / dejar dinero (jugador local) | Roce con subida / golpe sordo |
| Alguien se apuesta en una cabina | Campanita |
| Pregunta nueva en pantalla | Arpegio ascendente |
| Últimos 10 s de las apuestas | Tic de reloj |
| Verificar | Golpe metálico + pitido; pitido en cada segundo del 3-2-1 |
| Trampillas | Silbido que cae |
| Acierto / multiplicador | Arpegio alegre / destellos |
| Fallo / expulsión | «Trombón triste» / caída con vibrato |
| Victoria / derrota | Fanfarria / trombón triste largo |

Plató (en `Audio/Stage/`): succión de los tubos (aire que entra y silbido que sube, 1,9 s, en 3D desde la boca del tubo), aplauso, vítores y lamento del público (sale de las gradas).

Música: un bucle por bloque de dificultad (preguntas 1–5 tranquila a 100 ppm, 6–10 con ritmo a 116 ppm, 11–15 tensa a 132 ppm), con fundido entre pistas; silencio en la sala de espera y en los resultados. Volúmenes en `Feedback/Config/FeedbackLibrary.asset`; la música va baja para no tapar la conversación. Sustituibles por audio definitivo en el acabado. Sin AudioMixer por ahora (Unity no permite crearlo por código); pendiente con el ajuste de volumen en opciones.

El presentador es exclusivamente textual. La voz de jugadores por micrófono es un sistema de comunicación, no un recurso de voz del presentador. Mantener efectos y música por debajo de la conversación.

## Interfaz y contenido

Imágenes del tema de la interfaz (generadas al construir en `Shared/UI/Sprites`): rectángulo redondeado con degradado, marco redondeado y bombilla con halo.

| Recurso | Prototipo | Salida |
| --- | --- | --- |
| Preguntas de cultura general | 30 revisadas | 200 revisadas |
| Frases del presentador | 20 | 50 por evento |
| Preguntas por partida | 15 | 15 |
| Pantallas e indicadores | Básicos | Pulidos; ver UI_UX.md |
| Iconos de fichas, dinero y comodines | Distinguibles | Estilo consistente |

Documentar fuentes de preguntas y licencias/origen de recursos externos antes de incluirlos en el juego. Para desarrollo rápido, reutilizar materiales, piezas modulares y esqueleto común.

## Orden de producción

Primero geometría y arte provisional, luego un personaje base y una cabina representativos, después sus variantes y acabado. La tienda, decorado amplio, diez apariencias y materiales de Steam vienen después de validar una partida completa.

Pendientes: herramienta de modelado, estilo final, catálogo de cosméticos, idiomas de salida, presupuesto y recursos existentes aprovechables.
