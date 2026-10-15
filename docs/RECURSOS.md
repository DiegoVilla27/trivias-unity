# Recursos — Inventario de producción

Referencia: [GDD](GDD.md), secciones 6–9 y 13. Lista de recursos necesarios; no implica que existan o estén comprados.

## Arte 3D

Referencias específicas de modelado: [36 láminas individuales y notas](bocetos/detallados/README.md). Son bocetos, no medidas finales ni modelos terminados.

| Recurso | Prototipo | Salida |
| --- | --- | --- |
| Personaje base con esqueleto | 1 | Misma base |
| Apariencias básicas | 1 | 10 personajes/variantes disponibles |
| Cabina y bandeja | 1 conjunto reutilizado 4 veces | Acabado del mismo conjunto |
| Tubo de expulsión | Pieza sencilla reutilizada | Acabado visual |
| Mesa y estación de cambio | 1 conjunto | Mismo conjunto pulido |
| Botón de verificar | 1 | Mismo objeto pulido |
| Fajo, bolsa y lingote | 3 objetos | Valores legibles y acabado |
| Plató modular | Geometría básica | Decoración, materiales e iluminación |
| Presentador visual | Representación provisional | Diseño propio, escala por definir |
| Cheque/podio | Resultado sencillo | Ceremonia de victoria |

Los cosméticos adicionales de tienda requieren catálogo por definir. No confundir los diez personajes básicos gratuitos con atuendos que se compran usando fichas.

## Kit modular de ambiente

Referencia visual: [ambiente-modelado-01.png](bocetos/ambiente-modelado-01.png). Mantiene el estilo del plató y propone módulos reutilizables:

- Cámara de estudio y trípode decorativos.
- Foco con viseras, foco pequeño y tira de bombillas.
- Panel de pared, esquina, columna y molduras.
- Cortina recta, lateral recogida y cortina superior.
- Baldosa de suelo, insertos dorados rectos/curvos, borde de escenario, escalera y barandilla.
- Panel de techo, viga modular, conector, soporte de foco y cable simplificado.
- Altavoz y rejilla decorativos.

Priorizar paredes, suelo, techo básico y luces para construir el volumen del plató. Añadir cortinas, cámaras y accesorios después. No requiere cámaras funcionales, telas simuladas ni iluminación dinámica en cada bombilla; la implementación visual se decidirá según rendimiento y motor.

## Animación y efectos

- Personaje: reposo, locomoción, transporte y expulsión; transiciones según [PLAYER.md](PLAYER.md).
- Cabinas: bloqueo, acierto y pérdida; pueden resolverse con luces y efectos sencillos.
- Dinero: coger, depositar, conversión y retirada, sin física compleja obligatoria.
- Victoria: confeti y cheque. No necesita un nivel nuevo.

## Sonido

Efectos propuestos para coger/depositar, convertir, pulsar botón, cuenta atrás, respuesta y expulsión. Música/ambiente para los tres bloques del concurso y resultados. Cantidad de pistas pendiente.

El presentador es exclusivamente textual. La voz de jugadores por micrófono es un sistema de comunicación, no un recurso de voz del presentador. Mantener efectos y música por debajo de la conversación.

## Interfaz y contenido

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
