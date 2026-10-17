# Nivel — Plató del concurso

Referencia: [GDD](GDD.md), secciones 3, 4, 7 y 13. Documento de diseño; aún no existe nivel implementado.

## Objetivo

Un único plató 3D bonito, legible y reutilizable para las 15 preguntas. El humor procede de un concurso aparentemente barato y peligrosamente mal construido. No se necesitan escenarios de experimentos ni zonas jugables de mantenimiento.

## Zonas necesarias

| Zona | Función | Prototipo |
| --- | --- | --- |
| Mesa central | Dinero inicial y retorno entre preguntas | Modelo de mesa redonda (tablero a 0,95 m) en el centro del plató, sobre un disco oscuro con aro dorado |
| Estación de cambio | Conversión manual de denominaciones | Máquina de rodillo sobre un pedestal, al pie de la pantalla a la izquierda del presentador |
| Cuatro cabinas | Respuestas, bandejas y zona de apuesta personal | Modelo de cabina por letra (escala 1,3: 2,8 m de alto); el dinero se apila en la bandeja del mostrador y su cantidad se ve en grande en el hueco alto (la respuesta solo está en la pantalla). La zona de apuesta es la plataforma redonda con barandilla (modelo, 2,6 m de ancho, 22 cm de alto, se sube andando) al lado derecho de la cabina mirándola desde el centro (A y C hacia la pantalla, B y D hacia la zona de inicio), para no tapar el mueble, con la entrada hacia el centro y el color de la respuesta. La barandilla tiene colisión (nueve tramos a lo largo de su arco, hasta 1,4 m para que no se salte): solo se entra por la abertura de 90° hacia el centro |
| Botón de verificar | Bloqueo inmediato y cuenta atrás | Modelo sin su placa (decisión del usuario); la tapa roja se hunde al cerrar |
| Presentación de pregunta | Texto y cuatro opciones | Pantalla del plató al fondo y en el centro (modelo a escala 1,12: 16,6 × 8,6 m con marco, de 2,1 a 10,7 m de alto); fondo oscuro translúcido sobre su resplandor y bombillas debajo |
| Presentador | Personaje visual y comentarios de texto | Modelo del usuario tal cual (esmoquin rojo, 1,9 m, un millón de triángulos; `Presenter/Models/Presenter`), Humanoid y en reposo con la animación de Starter Assets, con una cápsula para que nadie lo atraviese. En el centro, entre la pantalla y las dos estaciones, a pie de suelo sobre una marca dorada y mirando a la zona de inicio. Habla en su rótulo de la pantalla (ver [UI_UX](UI_UX.md#hud-de-la-partida)), no en el plató; él no tapa la pantalla desde la zona de inicio; sin voz |
| Tubos del dinero | Por ellos sube el dinero perdido | Uno por cabina: sale del techo del mueble, se retira hacia la pared y sube hasta el techo; al fallar, los fajos de la bandeja suben por dentro siguiendo los codos, con aire y sonido |
| Tubos del concursante | Por ellos sale el expulsado | Uno por plataforma: cuelga del techo con su boca dorada a 3,4 m (por encima de la cabeza, aun saltando); el expulsado es succionado hacia arriba y desaparece por el techo |
| Podio de resultados | Victoria y cheque gigante | Encuadre reutilizando el plató |

## Distribución propuesta

Implementada (plano único en `Shared/Scripts/Editor/StageLayout.cs`, que usan todos los constructores). Sala compacta de 25 × 29,5 m y 11 m de alto, sin espacio muerto. +Z es el fondo:

- Pantalla al fondo y en el centro (z = 13,6); telón detrás.
- Mesa en el centro (z = 4,75).
- Al pie de la pantalla (z = 9,39, colocados por el usuario): estación de cambio a la izquierda (x = −2,7) y botón de verificar a la derecha (x = 2,7), mirando a la zona de inicio; el presentador en medio (0; 11,3).
- Cabinas en dos filas a los lados mirando al centro, como la cuadrícula de la pantalla: A y B al fondo (z = 7,5), C y D delante (z = 2); A y C a la izquierda (x = −8,6), B y D a la derecha (x = 8,6). Plataformas de apuesta a 2,7 m delante de cada una (x = ±5,9).
- El pasillo central queda libre: nada tapa la pantalla desde la zona de inicio.
- Zona de inicio en fila (z = −2,5); detrás, la barandilla del público (z = −7) y tres gradas.
- Esquina derecha junto a la pantalla (9,6; 11,6): plataforma redonda con escalera y una cámara de estudio encima.
- Podio de victoria en el centro, delante de la mesa (z = 0,6), entre ella y la zona de inicio.

- Cada cabina tiene identificador A/B/C/D y texto de respuesta, además de color. No depender solo del color.
- Bandeja de dinero y zona de apuesta deben distinguirse. La zona de apuesta está al lado de la cabina y admite hasta seis jugadores.
- Encima de cada cabina, un texto muestra el nombre de cada jugador apostado en ella. Se actualiza al confirmar o cancelar.
- El jugador apostado no puede salir del círculo de su zona hasta cancelar (antes de la cuenta atrás) o resolverse la pregunta. Si todos los activos están apostados, verifican desde su zona.
- No hay cámara interna en la cabina: el jugador nunca entra en la carcasa ni en el tubo.
- Las rutas admiten seis jugadores sin pasos estrechos ni dinero inaccesible.
- La decoración no oculta respuestas, bandejas o botón.
- No fijar metros definitivos hasta probar escala del personaje y cámara.
- Las distancias deben permitir el transporte dentro del tiempo por pregunta (90 s) con margen.

Decorado (concepto-01, implementado): suelo de baldosas (modelo, a ras del suelo, con una colisión plana), aro dorado de incrustaciones alrededor de la mesa y línea dorada delante del público. Telón rojo con bambalinas, cortinas recogidas, columnas y altavoces junto a la pantalla; paredes con paneles, columnas, rejillas y altavoces (también detrás del público). Techo con paneles (cara lisa con marco dorado hacia abajo), dos líneas de vigas (z = −1 y z = 6, por encima de la línea de visión de la pantalla) con focos pequeños y los cuatro focos de verdad sujetos con su abrazadera (las luces del ambiente salen de ellos). Tres cámaras de estudio en el suelo y otra sobre una plataforma con escalera (borde curvo de escenario); público en tres gradas detrás de una barandilla dorada con colisión de pared a pared; marca dorada del presentador.

Tipografías del plató (licencia OFL, `Shared/UI/Fonts`): Lilita One para titulares (logo TRIVIAS, cabecera de la pantalla, cheque, el cartel «CAMBIO») y Poppins SemiBold, con Poppins ExtraBold como negrita real, para preguntas, opciones, nombres y totales; la viñeta del presentador usa Lilita One. Sus tablas de caracteres (Latin-1 y puntuación tipográfica) se generan en edición y no cambian al jugar; la pregunta y las opciones se ajustan de tamaño si son largas. Posprocesado cómodo: resplandor solo en bombillas y focos encendidos (umbral 1,25, intensidad 0,3), colores naturales, exposición algo más baja, tonemapping neutro y viñeta suave.

Plató vivo (módulo Feedback, implementado): bombillas de cabinas y marquesina que persiguen, parpadean en la cuenta atrás y dejan encendida solo la cabina correcta; público que respira y reacciona (aplauso, vítores, lamento); cámaras de estudio que giran hacia la mesa, el botón o la cabina correcta; focos pequeños que se balancean y focos grandes que barren la sala en la victoria. Las piezas que se mueven no son estáticas; el resto del decorado sí.

## Ciclo visual

Durante apuestas, se distinguen objetos disponibles, valores de bandejas y jugadores comprometidos. Al verificar, señal de bloqueo y cuenta atrás visible. En resolución, respuesta correcta claramente marcada y expulsiones breves: el personaje sale disparado hacia arriba y pasa a espectador (su cámara sigue después a un activo). Entre rondas, el dinero vuelve a la mesa.

Bloques 1–5, 6–10 y 11–15 cambian iluminación, música y ambiente, usando la misma geometría y reglas (implementado: focos cálidos, morados y rojos; el plató se oscurece en las difíciles). Victoria: podio dorado centrado delante de la mesa (z = 0,6) y cheque gigante en la propia pantalla. Todos esos cambios cuentan dentro del tiempo total de partida.

## Recursos y comprobación

Usar piezas modulares: cabina, bandeja, tubo, mesa, botón y pantallas. Priorizar primero un nivel de bloques. Comprobar con dos y seis jugadores que pueden leer, alcanzar, depositar y verificar sin confundir interacciones; que las expulsiones no bloquean rutas; y que todos pueden distinguir el cierre.

Cámara confirmada: tercera persona. Pendientes: escala, dimensiones, ubicación exacta de textos, colisión entre jugadores y entrada al vestíbulo. No se confirma un vestíbulo 3D adicional; una pantalla puede cumplir esa función inicialmente.
