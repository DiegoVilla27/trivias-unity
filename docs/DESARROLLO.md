# Desarrollo — Alcance y orden de trabajo

Referencia principal: [GDD](GDD.md). Estado: documentación inicial, sin implementación ni calendario confirmado.

## Antes de construir

Decidido: Unity 6 con URP, tercera persona con Cinemachine, Netcode for GameObjects con Facepunch.Steamworks y voz de Steam enrutada por el host. Todo el MVP se desarrolla y prueba en local con Unity Transport y Multiplayer Play Mode (varios jugadores en el mismo editor). Steam (transporte, salas e invitaciones) y la voz se integran al final, sobre un transporte intercambiable preparado desde el inicio. No comenzar diez variantes antes de acordar la base.

Mantener configurables las propuestas económicas: capital 100.000, denominaciones 5.000/50.000/500.000 y seis comodines por equipo. No tratarlas como reglas finales ocultas en componentes distintos.

## Hitos propuestos

MVP: el plató y sus objetos usan los modelos 3D de cada módulo desde el 16 de octubre de 2026; lo que aún no tiene modelo (presentador, cosméticos, podio, público) sigue con primitivas. Personaje: provisionalmente el robot de Starter Assets; se preparan unos diez personajes completos.

| Orden | Entregable | Criterio para avanzar |
| --- | --- | --- |
| 0 | Red local | Dos jugadores conectados en local con Multiplayer Play Mode |
| 1 | Nivel de bloques y personaje base | Recorrer en tercera persona, coger un objeto y colocarlo en cabina |
| 2 | Una pregunta completa | Apostar con tiempo límite, pulsar botón o agotar el tiempo, bloquear y resolver correctamente |
| 3 | Sala con dos jugadores | Mismo dinero/estado visible; sin coger o convertir dos veces el mismo objeto |
| 4 | Conversión y comodines | Conservación de valor, consumo correcto y eliminación hasta el final |
| 5 | Partida de 15 preguntas | Victoria/derrota, tiempo total y resultados consistentes |
| 6 | Contenido de prototipo y micrófono | 30 preguntas, 20 frases y separación de activos/eliminados |
| 7 | Pruebas con dos y seis personas | Entender reglas, observar ritmo y registrar problemas reales |
| 8 | Preparación de salida | Banco de 200 preguntas, 50 frases, diez variantes, tienda y persistencia |
| 9 | Integración de Steam y voz | Transporte de Steam, salas, invitaciones y voz con canales de activos/eliminados |
| 10 | Ranking | Victorias válidas y desempate por tiempo total |

Comprobar pronto los riesgos de red y micrófono, aunque el acabado se haga después. Se puede probar una ronda antes de producir las 30 preguntas. No estimar semanas hasta conocer motor, recursos y equipo de desarrollo.

## Estado (16 de octubre de 2026)

| Hito | Estado |
| --- | --- |
| 0 Red local | Hecho: Netcode for GameObjects, salas de 2 a 6, menú principal y sala de espera |
| 1 Plató y personaje | Hecho: tercera persona, interacción por cercanía, dinero |
| Plató con modelos | Hecho: sala compacta (plano en `StageLayout`) con cabinas a los lados, plataformas de apuesta, tubos del dinero y del concursante, mesa, botón, estación, dinero y pantalla con sus modelos; suelo de baldosas y decorado de concepto-01 (telón, columnas, paredes, techo con vigas y focos, cámaras, público, tarima del presentador) y brillo de bombillas. Pendiente: modelos de presentador, podio y cosméticos; medir rendimiento en equipos modestos |
| Concursante | Provisional: robot de Starter Assets con sus animaciones. Pendiente: unos diez personajes completos (en preparación por el usuario), riggeados con el mismo esqueleto Humanoid; elegir personaje; medir rendimiento con seis jugadores y comprobarlo en Multiplayer Play Mode |
| Salto y carrera | Hecho: Espacio, 0,9 m, con la animación de Starter Assets; correr con Shift (5,6 m/s) |
| Presentador y tipografías | Hecho: presentador al pie de la pantalla entre cambio y verificar, viñeta de cómic junto a la cabeza (letra a letra, cuatro ánimos); Lilita One y Poppins en el plató y en toda la interfaz |
| Plató vivo e interfaz | Hecho: succión del expulsado y del dinero perdido por sus tubos (aire y sonido), bombillas, público con reacciones, cámaras de estudio y focos que se mueven; tema de la interfaz (paneles con marco dorado, botones con tacto, bombillas, logo que late). Pendiente: comprobar en Multiplayer Play Mode que los clientes ven la succión igual que el anfitrión |
| 2 Una pregunta completa | Hecho: pantalla grande, tiempo límite, verificar, cuenta atrás, resolución |
| 3 Cambio y comodines | Hecho: estación de cambio, comodín con zona, multiplicador y expulsión |
| 4 Partida de 15 preguntas | Hecho: victoria/derrota, resultados, fichas por tramo, volver a jugar, presentador |
| 6 Contenido | Hecho: 313 preguntas (60 de cálculo mental) con historial de vistas del grupo y 104 frases en 17 momentos. Pendiente: micrófono (con Steam) |
| Nombre del jugador | Hecho: se elige en el menú, se guarda en el perfil y es único en la sala. Con Steam, el de la cuenta |
| 8 Tienda y fichas guardadas | Hecho: perfil local, entrega única, tienda con 9 cosméticos sincronizados |
| Robustez para pruebas en grupo | Hecho: abandonos a mitad de partida, reapertura de sala y build para compartir con un clic (Tools → Trivias → Crear build) |
| Sonido y sensaciones | Hecho: 14 efectos, 4 sonidos del plató (succión y público) y 3 pistas sintetizados al construir, confeti por acierto y victoria, iluminación por bloque y momento, cheque y podio en la victoria |
| Menú y opciones | Hecho: menú principal con velo semioscuro sobre el plató, unirse con validación, invitar (dirección y copiar), tutorial breve, configuración guardada y pausa con Esc |
| Espectador | Hecho: cámara que sigue a los activos, cambiar con A/D, vista general con Espacio y rótulo |
| 10 Ranking | Récords locales hechos (por tamaño de grupo, dinero y tiempo, empates compartidos). Ranking global pendiente (con Steam) |
| 9 Steam y voz | Pendiente (AppID 480 para pruebas, sin coste) |

Cada cambio se valida con Build All y la batería completa de pruebas (EditMode y PlayMode).

## Pruebas imprescindibles

- Cierre único al pulsar verificar, con bloqueo inmediato y tres segundos.
- Dinero y valor conservados en conversiones y recogida simultánea.
- Reserva de comodines sin doble consumo ni redistribución por eliminación.
- Fallar con comodines restantes elimina igualmente.
- Derrota anticipada y derrota en pregunta 15; fichas por límites de tramo.
- Fichas completas por participante y entrega única.
- Tiempo desde inicio hasta resolución final, incluyendo discusión y animaciones.
- Derrotas excluidas del ranking y desempate por dinero/tiempo correctos.
- Activos no reciben comunicación de eliminados por canales del juego.
- Una apariencia nueva reutiliza prefab, animaciones y anclajes correctamente.

## Pendientes de decisión

| Decisión | Situación |
| --- | --- |
| Motor y cámara | Confirmado: Unity 6 + URP, tercera persona |
| Red y micrófono | Confirmado: NGO + Facepunch.Steamworks, voz de Steam; MVP en local, Steam y voz al final |
| Comodines por grupo y reserva | Propuesta por validar |
| Capital y denominaciones | Configuración propuesta |
| Dividir bolsas/lingotes | Confirmado |
| Dinero fuera de bandejas al cerrar | Pérdida propuesta, por validar |
| Emisión de bonos grandes | Confirmado: denominación más grande posible |
| Abandonos/reconexiones | Pendientes para fichas, usos y ranking |
| Selección de preguntas e historial | Hecho: historial por jugador combinado por el host; 313 preguntas |
| Ranking comparable y validación | Récords locales hechos; ranking global, identidad del equipo y validación pendientes |
| Nombre del juego, atuendos y catálogo de tienda | Pendientes (el nombre del jugador ya está hecho) |

## Aplazado

IA textual en vivo es opcional después del catálogo local. Observación, memoria y gestos son futuros. Experimentos retirados. Logros de Steam en espera. No incorporar rescates ni poderes.

## Mantenimiento de documentación

Al cerrar un pendiente, actualizar el GDD y los documentos afectados. Registrar cambios de alcance y no confundir un hito planificado con una funcionalidad terminada.

## Probar una pregunta concreta

Para saltar directamente a una pregunta (p. ej. la última, para ver la victoria):

1. Con Unity abierto, seleccionar `Assets/_Project/Modules/Match/Config/MatchConfig.asset`.
2. En el Inspector, sección **Pruebas**, poner **Test Start Question** en el número deseado (1 = normal, 15 = la última).
3. Play → Crear sala → Empezar partida. La consola avisa «MODO PRUEBA: empieza en la pregunta N».
4. Al terminar, volver a ponerlo en **1**.

Las preguntas siguen yendo de fácil a difícil (la 15 es difícil). Solo funciona en el editor y en builds de desarrollo: la build para compartir (Tools → Trivias → Crear build) lo ignora y siempre empieza en la 1. Build All conserva el valor. Las fichas se entregan como en una partida normal (sirve para probar la tienda).
