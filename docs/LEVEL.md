# Nivel — Plató del concurso

Referencia: [GDD](GDD.md), secciones 3, 4, 7 y 13. Documento de diseño; aún no existe nivel implementado.

## Objetivo

Un único plató 3D bonito, legible y reutilizable para las 15 preguntas. El humor procede de un concurso aparentemente barato y peligrosamente mal construido. No se necesitan escenarios de experimentos ni zonas jugables de mantenimiento.

## Zonas necesarias

| Zona | Función | Prototipo |
| --- | --- | --- |
| Mesa central | Dinero inicial y retorno entre preguntas | Mesa sencilla con superficie clara |
| Estación de cambio | Conversión manual de denominaciones | Interacción junto a la mesa |
| Cuatro cabinas | Respuestas, bandejas y zona de apuesta personal | Una cabina base reutilizada cuatro veces |
| Botón de verificar | Bloqueo inmediato y cuenta atrás | Objeto separado de las bandejas |
| Presentación de pregunta | Texto y cuatro opciones | Pantalla grande del plató (14 × 7,4 m) detrás de las cabinas, elevada para que nada la tape desde la zona de inicio |
| Presentador | Personaje visual y comentarios de texto | A un lado de la pantalla; habla en una viñeta sobre su cabeza; sin voz |
| Tubos de expulsión | Decoración que comunica el peligro | Solo decorativos; el jugador no entra en ellos |
| Podio de resultados | Victoria y cheque gigante | Encuadre reutilizando el plató |

## Distribución propuesta

Mesa en el centro y cabinas en semicírculo frente a ella. Presentador y pantalla detrás de las cabinas; botón cerca de la mesa, separado de coger dinero y convertir. Es una propuesta de bloqueo inicial, pendiente de pruebas con la cámara en tercera persona.

- Cada cabina tiene identificador A/B/C/D y texto de respuesta, además de color. No depender solo del color.
- Bandeja de dinero y zona de apuesta deben distinguirse. La zona de apuesta está delante de la cabina y admite hasta seis jugadores.
- Encima de cada cabina, un texto muestra el nombre de cada jugador apostado en ella. Se actualiza al confirmar o cancelar.
- El jugador apostado no puede salir del círculo de su zona hasta cancelar (antes de la cuenta atrás) o resolverse la pregunta. Si todos los activos están apostados, verifican desde su zona.
- No hay cámara interna en la cabina: el jugador nunca entra en la carcasa ni en el tubo.
- Las rutas admiten seis jugadores sin pasos estrechos ni dinero inaccesible.
- La decoración no oculta respuestas, bandejas o botón.
- No fijar metros definitivos hasta probar escala del personaje y cámara.
- Las distancias deben permitir el transporte dentro del tiempo por pregunta (90 s) con margen.

## Ciclo visual

Durante apuestas, se distinguen objetos disponibles, valores de bandejas y jugadores comprometidos. Al verificar, señal de bloqueo y cuenta atrás visible. En resolución, respuesta correcta claramente marcada y expulsiones breves: el personaje sale disparado hacia arriba y pasa a espectador (su cámara sigue después a un activo). Entre rondas, el dinero vuelve a la mesa.

Bloques 1–5, 6–10 y 11–15 cambian iluminación, música y ambiente, usando la misma geometría y reglas (implementado: focos cálidos, morados y rojos; el plató se oscurece en las difíciles). Victoria: podio dorado centrado delante de la pantalla (z ≈ 12, entre cabinas y pantalla, por debajo de la línea de visión) y cheque gigante en la propia pantalla. Todos esos cambios cuentan dentro del tiempo total de partida.

## Recursos y comprobación

Usar piezas modulares: cabina, bandeja, tubo, mesa, botón y pantallas. Priorizar primero un nivel de bloques. Comprobar con dos y seis jugadores que pueden leer, alcanzar, depositar y verificar sin confundir interacciones; que las expulsiones no bloquean rutas; y que todos pueden distinguir el cierre.

Cámara confirmada: tercera persona. Pendientes: escala, dimensiones, ubicación exacta de textos, colisión entre jugadores y entrada al vestíbulo. No se confirma un vestíbulo 3D adicional; una pantalla puede cumplir esa función inicialmente.
