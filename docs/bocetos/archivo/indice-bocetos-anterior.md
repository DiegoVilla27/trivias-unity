# Índice visual anterior — archivo histórico

Conservado al reorganizar las carpetas el 16 de octubre de 2026. Para las referencias vigentes, usar el [nuevo índice](../README.md).

# Bocetos visuales

## Poses individuales — 15 de octubre de 2026

[Cuatro láminas de personaje y presentador en T-pose y A-pose](../archivo/poses-2026-10-15.md), con vistas por lado, superior/inferior y tres cuartos. Paquete completo (archivo retirado).

## Recursos individuales — 15 de octubre de 2026

[Índice de 36 láminas detalladas](../recursos/detallados/README.md), con vistas individuales, componentes y notas para modelado. Incluye objetos principales, cámaras, iluminación, paredes, cortinas, suelo, techo y componentes separados de la cabina.

Paquete completo de imágenes y prompts (archivo retirado).

## Referencias para modelado — 14 de octubre de 2026

Tres láminas nuevas generadas con la herramienta integrada tomando concepto-01.png como referencia de estilo e identidad:

| Lámina | Contenido | Prompt |
| --- | --- | --- |
| [Personaje](../player/archivo/referencias-originales/personaje-modelado-01.png) | Frente, perfil, espalda, T-pose, A-pose, manos, pelo, calzado y acciones | [Prompt](../player/archivo/referencias-originales/personaje-modelado-01-prompt.md) |
| [Presentador](../presentador/referencias/presentador-modelado-01.png) | Frente, perfil, espalda, T-pose, A-pose, cabeza, traje y poses de actuación; sin micrófono | [Prompt](../presentador/referencias/presentador-modelado-01-prompt.md) |
| [Recursos](../recursos/recursos-modelado-01.png) | Cabina, módulos, colores, mesa, estación, botón, fajo, bolsa, lingote, pantalla y podio | [Prompt](../recursos/recursos-modelado-01-prompt.md) |
| [Ambiente modular](../plato/ambiente-modelado-01.png) | Cámaras, focos, paredes, cortinas, suelo, bordes, escaleras, barandillas, techo, estructuras, altavoz y rejilla | [Prompt](../plato/ambiente-modelado-01-prompt.md) |

La lámina de ambiente amplía referencias decorativas, no añade sistemas de cámara operativa, vigilancia, simulación de telas o funciones nuevas de juego. Las vistas de algunos módulos son aproximadas y las etiquetas secundarias pueden tener errores; priorizar silueta y consistencia del modelo. Cámaras y estructura son decorado de estudio; altavoces no implican voz del presentador.

### Uso como referencia 3D

Importar vistas como referencias, alinear suelo y altura y construir primero volúmenes grandes. Las imágenes generadas son guías visuales: no garantizan coincidencia geométrica exacta entre vistas. Elegir la vista frontal para proporciones y ajustar perfil/espalda al mismo modelo. Las A-pose y acciones sirven para planificar rig y deformaciones, no sustituyen una malla neutra.

Cabina resuelta en GDD v0.10: el jugador se apuesta en una zona delante de la cabina (hasta seis jugadores) y un texto encima muestra los nombres de los apostados. Se descarta la cámara interna del jugador del corte de 01-cabina.png; el tubo es decorativo y la expulsión es una animación breve hacia arriba. Las plataformas laterales pueden mantenerse como decoración o retirarse. Los tubos salen del encuadre en algunas vistas; usar módulos de tubo recto y codo como referencia, no deducir un recorrido completo.

La mesa aparece con las tres denominaciones para mostrar recursos; el inicio del juego sigue siendo solo fajos. Apariencia, materiales y detalles pueden simplificarse sin modificar reglas. El presentador continúa siendo textual; las poses son animación visual, no una nueva función de voz ni gestos del jugador.

## Concepto 01

[Lámina de concepto](../plato/concepto-01.png), generada el 14 de octubre de 2026 con la herramienta integrada de generación de imágenes. Exploración artística, no captura del juego ni plano técnico aprobado.

Incluye plató, personaje base y cabina con objetos de dinero. Cuatro cabinas, tubos de expulsión, mesa central y botón de verificar.

Pendiente de revisar: distribución exacta, entrada del jugador en el espacio de expulsión, textos de respuesta, cantidad de decoración y coste de reproducir materiales e iluminación. El público decorativo y el micrófono del presentador son aportaciones de la imagen, no requisitos del GDD; el presentador sigue siendo exclusivamente textual. Los lingotes de la mesa no cambian la regla de empezar solo con fajos.

El acabado final puede simplificarse para desarrollo rápido. No se han aprobado cámara, vestíbulo o estilo definitivo con esta lámina.

Prompt completo: [PROMPT-01.md](../plato/PROMPT-01.md).
