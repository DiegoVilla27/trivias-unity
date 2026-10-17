# Trivias — Guion de voz del presentador para ElevenLabs

Preparado el 17 de octubre de 2026. **104 intervenciones, 17 situaciones, un archivo por identificador.**

Fuente: [catálogo original de frases](../../Assets/_Project/Modules/Presenter/Content/phrases_es.json).

Este documento es el guion de producción solicitado. El catálogo JSON del juego sigue siendo textual y no se modifica aquí; todavía no hay audios generados ni reproducción de voz integrada. Al integrar las grabaciones habrá que actualizar los subtítulos para que coincidan con el **Texto hablado** de cada ficha, y vincular cada clip por su ID, no por su posición en una lista.

## Cómo usar este guion

1. Elige una voz masculina adulta que ya tenga español natural, presencia y capacidad expresiva. Mantén el mismo acento, voz, modelo y ajustes para todo el catálogo. Como punto de partida artístico, español de España: el texto conserva «vosotros» y «os». No cambies de acento entre clips.
2. Prueba primero `welcome-01`, `discussion-04`, `countdown-03`, `perfect-01`, `victory-03` y `defeat-04`: cubren bienvenida, ironía, urgencia, sorpresa, triunfo y cercanía. Genera una toma inicial de cada una; repite solo las que fallen. No produzcas las 104 hasta fijar esta identidad.
3. Usa un modelo que interprete etiquetas de audio, como Eleven v3 o v4 si está disponible en tu interfaz. Los bloques de este guion usan etiquetas en inglés de la documentación de ElevenLabs. Son sugerencias de interpretación, no garantías; su efecto depende de la voz. Si una etiqueta se pronuncia literalmente o produce un sonido extraño, retírala y conserva el texto y su puntuación.
4. Lee **Interpretación** antes de generar, pero pega en ElevenLabs **únicamente el bloque de texto**. No pegues el título, el nombre de archivo, las explicaciones ni el texto original con variables. No uses la función de reescritura para cambiar el diálogo aprobado sin revisar el resultado.
5. Escucha cada toma completa. Debe parecer que está reaccionando a jugadores delante de él, no leyendo una lista ni locutando un anuncio. Mantén las tomas elegidas y anota voz/modelo/ajustes para poder reproducir el criterio en otra sesión.
6. Exporta el original como WAV si está disponible; si solo puedes descargar MP3, guárdalo así sin convertirlo a WAV esperando recuperar calidad. El nombre base debe ser el ID exacto de la ficha. El destino previsto al integrarlo es `Assets/_Project/Modules/Presenter/Audio/es/`; no se crea esa carpeta vacía ahora.

## Dirección común del personaje

Presentador de concurso televisivo, adulto, carismático, seguro de su oficio. Timbre cálido y con cuerpo, dicción clara y sonrisa en la voz. Se divierte con las malas apuestas y con su decorado, pero también sabe celebrar un acierto. Su vanidad es de comedia: nunca es un matón ni se burla de rasgos personales. La voz del mismo hombre debe reconocerse en todos los estados.

Naturalidad antes que exageración. Alterna anuncios claros, comentarios al micrófono y remates más secos. La energía se modifica con intención y melodía, no subiendo el volumen en cada frase. No declames cada palabra, no alargues todas las vocales y no hagas siempre la misma subida final. Separa la premisa del chiste con una pausa pequeña; el remate suele funcionar mejor más bajo y corto. No añadas muletillas, nombres de jugadores, cifras ni texto no escrito.

Como referencia de ensayo, usa ritmo conversacional ágil; las frases de bienvenida y victoria pueden abrirse un poco más. Las pausas escritas no tienen una duración exacta garantizada. Los suspiros deben ser mínimos; no deben comerse la primera palabra. No se han añadido risas, aplausos, música ni efectos del tubo: los efectos pertenecen al juego. Una sonrisa se oye en el habla sin necesidad de una carcajada en cada clip.

Las etiquetas empleadas orientan energía (`[excited]`), curiosidad (`[curious]`), sorpresa (`[surprised]`), ironía (`[sarcastic]`), picardía (`[mischievously]`) o una reacción breve (`[sighs]`, `[exhales]`). Las fichas sin etiqueta también tienen una dirección concreta; la puntuación y el texto ayudan a conseguirla. No se utilizan etiquetas SSML `<break>` para v3/v4.

Estas indicaciones artísticas son propuestas para Trivias. El funcionamiento general de las etiquetas, la dependencia de la voz y las pausas se contrastó con la [guía oficial de ElevenLabs](https://elevenlabs.io/docs/overview/capabilities/text-to-speech/best-practices) y su [explicación de etiquetas](https://elevenlabs.io/docs/help-center/product/core-capabilities/text-to-speech/how-do-audio-tags-work-with-eleven-v3-and-v4). No se ha generado ni escuchado audio en este trabajo.

## Adaptaciones y sincronización con el juego

- **30 textos con variables:** las cantidades, ronda, multiplicador o número de supervivientes se sustituyen por una formulación completa sin cifras variables. La información exacta seguirá en el HUD; el subtítulo futuro debe usar el texto hablado de esta ficha. Se incluye el original para revisar la adaptación, nunca para pegarlo en ElevenLabs.
- **7 intervenciones de cierre:** se proporcionan versiones breves para el bloqueo actual de tres segundos. `countdown-03` conserva su texto; las otras seis se acortan. Resultado: **36 textos reformulados y 68 conservados**, con sus 104 IDs intactos. La duración debe medirse al escuchar; una redacción breve no garantiza que la toma quepa.
- Para `countdown-*`, objetivo de montaje: terminar antes de la revelación, dejando un pequeño margen dentro de los tres segundos. Si la voz no cabe con una dicción natural, no acelerarla hasta deformarla: repetir con menos pausa o revisar su integración temporal antes de aprobarla. La partida y los números visibles son la autoridad temporal.
- `countdown-05` no es una pista de sincronización automática. El montaje futuro debe alinear sus números con el cierre visible; si no encajan, no usar ese clip como cuenta oficial.
- `time-01` menciona «diez segundos» y solo se puede usar si el aviso coincide con ese umbral real. Si se configura otro, habrá que adaptar esa toma.
- **Atención a `loss-04`: pertenece al evento `eliminated`, no a `loss`.** Se conserva el identificador histórico para no romper su correspondencia. Tampoco existe `loss-10`: los ocho comentarios de pérdida tienen sus IDs reales, no una numeración inventada.
- La locución propuesta no lee preguntas ni respuestas. No anuncia opciones correctas antes de revelar y no inventa cambios de reglas. La integración futura tendrá que priorizar o interrumpir clips ante un evento nuevo para evitar solapamientos; no prolongar por voz el bloqueo de apuestas sin decidirlo expresamente.

## Índice por situación


| Situación | Evento | Clips |
| --- | --- | --- |
| [Bienvenida](#situacion-01) | `welcome` | 7 |
| [Discusión prolongada](#situacion-02) | `long_discussion` | 10 |
| [Activación de comodín](#situacion-03) | `self_bet` | 8 |
| [Apuestas cerradas y cuenta atrás](#situacion-04) | `countdown` | 7 |
| [Poco tiempo para apostar](#situacion-05) | `time_running_out` | 5 |
| [Bloque intermedio](#situacion-06) | `medium_block` | 4 |
| [Bloque difícil](#situacion-07) | `hard_block` | 4 |
| [Última pregunta](#situacion-08) | `last_question` | 4 |
| [Matemáticas](#situacion-09) | `math_question` | 6 |
| [Acierto con alguna pérdida](#situacion-10) | `success` | 7 |
| [Pleno](#situacion-11) | `perfect` | 6 |
| [Premio multiplicado](#situacion-12) | `multiplied` | 5 |
| [Pérdida importante](#situacion-13) | `loss` | 8 |
| [Expulsión](#situacion-14) | `eliminated` | 7 |
| [Abandono o desconexión](#situacion-15) | `player_left` | 4 |
| [Victoria](#situacion-16) | `victory` | 6 |
| [Derrota](#situacion-17) | `defeat` | 6 |
| **Total** | **17 eventos** | **104** |

<a id="situacion-01"></a>

## 01. Bienvenida — 7 clips

**Cuándo:** Al empezar la primera pregunta.
**Dirección del bloque:** Energía 4/5; orgullo de anfitrión y una bienvenida con trampa.

### welcome-01

**Archivo:** `welcome-01.wav` · **Evento:** `welcome`

**Interpretación:** Abre con una sonrisa audible y proyecta «Trivias» hacia el público. Tras «vuestro», baja ligeramente la voz y deja caer «por poco tiempo» con picardía.

**Texto hablado / subtítulo futuro:** ¡Bienvenidos a Trivias! Hoy el dinero es vuestro… por poco tiempo.

**Pegar en ElevenLabs:**

```text
[excited] ¡Bienvenidos a Trivias! Hoy el dinero es vuestro… por poco tiempo.
```

### welcome-02

**Archivo:** `welcome-02.wav` · **Evento:** `welcome`

**Interpretación:** Enumera los ingredientes con energía creciente. Presenta lo del decorado como un gasto perfectamente razonable y remata «¡Empezamos!» limpio, sin gritar.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Quince preguntas, {premio} y un decorado que necesita financiación. ¡Empezamos!»

**Texto hablado / subtítulo futuro:** Quince preguntas, un premio en juego y un decorado que necesita financiación. ¡Empezamos!

**Pegar en ElevenLabs:**

```text
[excited] Quince preguntas, un premio en juego y un decorado que necesita financiación. ¡Empezamos!
```

### welcome-03

**Archivo:** `welcome-03.wav` · **Evento:** `welcome`

**Interpretación:** Pide silencio con autoridad tranquila. Construye expectativa hasta «premio»; deja una pausa corta y seca antes del giro «O de nada».

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Silencio en el plató. Quince preguntas os separan de {premio}. O de nada.»

**Texto hablado / subtítulo futuro:** Silencio en el plató. Quince preguntas os separan del premio. O de nada.

**Pegar en ElevenLabs:**

```text
[mischievously] Silencio en el plató. Quince preguntas os separan del premio. O de nada.
```

### welcome-04

**Archivo:** `welcome-04.wav` · **Evento:** `welcome`

**Interpretación:** Recibe a los jugadores con cordialidad ceremoniosa. En «el tubo», baja un poco el tono y saborea «con hambre», como una travesura compartida.

**Texto hablado / subtítulo futuro:** Bienvenidos, valientes. Las cabinas están limpias y el tubo, con hambre.

**Pegar en ElevenLabs:**

```text
[mischievously] Bienvenidos, valientes. Las cabinas están limpias y el tubo, con hambre.
```

### welcome-05

**Archivo:** `welcome-05.wav` · **Evento:** `welcome`

**Interpretación:** Di el nombre del concurso con seguridad. Confiesa lo del decorado con naturalidad descarada; el chiste debe parecer una verdad que se te escapa.

**Texto hablado / subtítulo futuro:** Empieza Trivias. El dinero que apostéis mal acabará pagando mi decorado.

**Pegar en ElevenLabs:**

```text
[sarcastic] Empieza Trivias. El dinero que apostéis mal acabará pagando mi decorado.
```

### welcome-06

**Archivo:** `welcome-06.wav` · **Evento:** `welcome`

**Interpretación:** Contempla la mesa como si ya estuvieras contando los billetes. Separa las dos frases y cierra «ya veremos» con una sonrisa, no con una amenaza.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Hoy tenéis {premio} encima de la mesa. Mañana, ya veremos.»

**Texto hablado / subtítulo futuro:** Hoy tenéis una fortuna encima de la mesa. Mañana, ya veremos.

**Pegar en ElevenLabs:**

```text
[mischievously] Hoy tenéis una fortuna encima de la mesa. Mañana, ya veremos.
```

### welcome-07

**Archivo:** `welcome-07.wav` · **Evento:** `welcome`

**Interpretación:** Da la salida con impulso. Marca tres consejos distintos, con una pequeña subida en «equipo» y «cabeza»; el tubo llega como coletilla cómplice.

**Texto hablado / subtítulo futuro:** ¡Arrancamos! Pensad en equipo, apostad con cabeza y no miréis al tubo.

**Pegar en ElevenLabs:**

```text
[excited] ¡Arrancamos! Pensad en equipo, apostad con cabeza y no miréis al tubo.
```

<a id="situacion-02"></a>

## 02. Discusión prolongada — 10 clips

**Cuándo:** Si el grupo tarda en verificar en una partida sin límite de tiempo.
**Dirección del bloque:** Energía 2/5; ironía conversacional. No anunciar un plazo que no existe.

### discussion-01

**Archivo:** `discussion-01.wav` · **Evento:** `long_discussion`

**Interpretación:** Finge una paciencia exquisita. Da «cobra por horas» como un dato administrativo y ofrece «seguid debatiendo» con ironía seca, sin sonar enfadado.

**Texto hablado / subtítulo futuro:** Tranquilos, el plató cobra por horas. Seguid debatiendo.

**Pegar en ElevenLabs:**

```text
[sarcastic] Tranquilos, el plató cobra por horas. Seguid debatiendo.
```

### discussion-02

**Archivo:** `discussion-02.wav` · **Evento:** `long_discussion`

**Interpretación:** Empieza resignado, con el aire de quien mira su reloj. Deja respirar el intento imposible del botón al final y no añadas una carcajada.

**Texto hablado / subtítulo futuro:** Llevo un rato esperando. El botón no se pulsa solo, aunque lo intente.

**Pegar en ElevenLabs:**

```text
[sarcastic] Llevo un rato esperando. El botón no se pulsa solo, aunque lo intente.
```

### discussion-03

**Archivo:** `discussion-03.wav` · **Evento:** `long_discussion`

**Interpretación:** Concede la primera afirmación como un profesor diplomático. Haz una separación clara antes de «Responder» y remata corto, con media sonrisa.

**Texto hablado / subtítulo futuro:** Debatir está muy bien. Responder, todavía mejor.

**Pegar en ElevenLabs:**

```text
[sarcastic] Debatir está muy bien. Responder, todavía mejor.
```

### discussion-04

**Archivo:** `discussion-04.wav` · **Evento:** `long_discussion`

**Interpretación:** Ofrece el café de verdad, con hospitalidad. La segunda frase revela el negocio: baja un poco la voz y acentúa «descuento» con elegancia.

**Texto hablado / subtítulo futuro:** Si queréis, os traigo café. Lo descuento del premio, claro.

**Pegar en ElevenLabs:**

```text
[mischievously] Si queréis, os traigo café. Lo descuento del premio, claro.
```

### discussion-05

**Archivo:** `discussion-05.wav` · **Evento:** `long_discussion`

**Interpretación:** Cuenta la salida del público como una noticia cotidiana. Pronuncia «para ver el final» con una esperanza exageradamente profesional, sin dramatismo triste.

**Texto hablado / subtítulo futuro:** El público se ha ido a cenar. Volverá para ver el final.

**Pegar en ElevenLabs:**

```text
[sarcastic] El público se ha ido a cenar. Volverá para ver el final.
```

### discussion-06

**Archivo:** `discussion-06.wav` · **Evento:** `long_discussion`

**Interpretación:** Arranca observador, casi intrigado. La duda crece dentro de la frase; apoya «más dudo» y termina con escepticismo juguetón.

**Texto hablado / subtítulo futuro:** Cuanto más lo pensáis, más dudo de que lo sepáis.

**Pegar en ElevenLabs:**

```text
[curious] Cuanto más lo pensáis, más dudo de que lo sepáis.
```

### discussion-07

**Archivo:** `discussion-07.wav` · **Evento:** `long_discussion`

**Interpretación:** Haz creer durante un instante que vas a ayudar. Tras «pista», una pausa mínima; revela la obviedad con total seriedad y sin señalar ninguna opción.

**Texto hablado / subtítulo futuro:** Os doy una pista: la respuesta está en una de las cuatro cabinas.

**Pegar en ElevenLabs:**

```text
[mischievously] Os doy una pista: la respuesta está en una de las cuatro cabinas.
```

### discussion-08

**Archivo:** `discussion-08.wav` · **Evento:** `long_discussion`

**Interpretación:** Primera mitad calmada y precisa. El contraste está en «mi paciencia»: subráyalo con el tono, sin acelerar ni fingir una cuenta atrás real.

**Texto hablado / subtítulo futuro:** El reloj no corre, pero mi paciencia sí.

**Pegar en ElevenLabs:**

```text
[sarcastic] El reloj no corre, pero mi paciencia sí.
```

### discussion-09

**Archivo:** `discussion-09.wav` · **Evento:** `long_discussion`

**Interpretación:** Describe la reunión con respeto empresarial impostado. Una pequeña pausa antes del apéndice; «arruinado» cae bajo y seco.

**Texto hablado / subtítulo futuro:** Tanta reunión parece un consejo de administración. De uno arruinado.

**Pegar en ElevenLabs:**

```text
[sarcastic] Tanta reunión parece un consejo de administración. De uno arruinado.
```

### discussion-10

**Archivo:** `discussion-10.wav` · **Evento:** `long_discussion`

**Interpretación:** Recordatorio amable, con una impaciencia apenas contenida. Destaca «verificar» como un botón conocido y trata su deseo como si fuera una persona.

**Texto hablado / subtítulo futuro:** Os recuerdo que existe un botón de VERIFICAR. Está deseando que lo pulséis.

**Pegar en ElevenLabs:**

```text
[mischievously] Os recuerdo que existe un botón de VERIFICAR. Está deseando que lo pulséis.
```

<a id="situacion-03"></a>

## 03. Activación de comodín — 8 clips

**Cuándo:** Cuando un concursante se apuesta en una cabina.
**Dirección del bloque:** Energía 3/5; sorpresa y curiosidad por el riesgo, sin anticipar si acertará.

### selfbet-01

**Archivo:** `selfbet-01.wav` · **Evento:** `self_bet`

**Interpretación:** Reacciona al movimiento con sorpresa inmediata. Celebra primero «valor»; deja una pausa breve y gira a «poca memoria» con malicia simpática.

**Texto hablado / subtítulo futuro:** ¡Alguien se ha subido a la cabina! Qué valor… o qué poca memoria.

**Pegar en ElevenLabs:**

```text
[excited] ¡Alguien se ha subido a la cabina! Qué valor… o qué poca memoria.
```

### selfbet-02

**Archivo:** `selfbet-02.wav` · **Evento:** `self_bet`

**Interpretación:** Narra la apuesta personal como una noticia importante. La segunda frase se vuelve confidencial: el tubo es una broma, no una amenaza realista.

**Texto hablado / subtítulo futuro:** Un concursante se apuesta a sí mismo. El tubo ya se está calentando.

**Pegar en ElevenLabs:**

```text
[mischievously] Un concursante se apuesta a sí mismo. El tubo ya se está calentando.
```

### selfbet-03

**Archivo:** `selfbet-03.wav` · **Evento:** `self_bet`

**Interpretación:** Reconoce lo ocurrido sin una gran ovación. En «me encanta», que se note una satisfacción sospechosa; termina admirando esa confianza.

**Texto hablado / subtítulo futuro:** Más gente en las cabinas. Me encanta cuando confían tanto.

**Pegar en ElevenLabs:**

```text
[mischievously] Más gente en las cabinas. Me encanta cuando confían tanto.
```

### selfbet-04

**Archivo:** `selfbet-04.wav` · **Evento:** `self_bet`

**Interpretación:** Lleva la cuenta con oficio de presentador. Haz una leve pausa antes de definir la situación y carga el remate en «ciega», no en la persona.

**Texto hablado / subtítulo futuro:** Otro concursante apostado. A esto lo llamo yo confianza ciega.

**Pegar en ElevenLabs:**

```text
[sarcastic] Otro concursante apostado. A esto lo llamo yo confianza ciega.
```

### selfbet-05

**Archivo:** `selfbet-05.wav` · **Evento:** `self_bet`

**Interpretación:** Empieza como quien presenta un producto financiero. El giro «más arriesgada» lleva el peso; mantén dicción elegante y una sonrisa comercial.

**Texto hablado / subtítulo futuro:** Apostarse a uno mismo: la inversión más arriesgada del mercado.

**Pegar en ElevenLabs:**

```text
[mischievously] Apostarse a uno mismo: la inversión más arriesgada del mercado.
```

### selfbet-06

**Archivo:** `selfbet-06.wav` · **Evento:** `self_bet`

**Interpretación:** Contrasta los dos desenlaces con igual claridad. El primer «si» ilusiona; el segundo baja ligeramente de tono y termina en un saludo casi cordial.

**Texto hablado / subtítulo futuro:** Si acertáis, se multiplica el premio. Si no, el tubo os manda saludos.

**Pegar en ElevenLabs:**

```text
[mischievously] Si acertáis, se multiplica el premio. Si no, el tubo os manda saludos.
```

### selfbet-07

**Archivo:** `selfbet-07.wav` · **Evento:** `self_bet`

**Interpretación:** Anuncia la cabina ocupada con formalidad. La preocupación fingida se transforma en confesión: deja caer «porque yo no» como una pequeña indiscreción.

**Texto hablado / subtítulo futuro:** Cabina ocupada. Espero que esa persona sepa lo que hace, porque yo no.

**Pegar en ElevenLabs:**

```text
[curious] Cabina ocupada. Espero que esa persona sepa lo que hace, porque yo no.
```

### selfbet-08

**Archivo:** `selfbet-08.wav` · **Evento:** `self_bet`

**Interpretación:** Primera frase afectuosa, como un elogio sincero. Segunda con humor negro suave: separa «duran poco» de «se recuerdan», sin recrearte en una desgracia.

**Texto hablado / subtítulo futuro:** Me encantan los concursantes con fe. Duran poco, pero se recuerdan.

**Pegar en ElevenLabs:**

```text
[sarcastic] Me encantan los concursantes con fe. Duran poco, pero se recuerdan.
```

<a id="situacion-04"></a>

## 04. Apuestas cerradas y cuenta atrás — 7 clips

**Cuándo:** Durante el bloqueo de tres segundos previo a revelar.
**Dirección del bloque:** Energía 3/5; firmeza y suspense breve. La información manda sobre el chiste.

### countdown-01

**Archivo:** `countdown-01.wav` · **Evento:** `countdown`

**Interpretación:** Entrada firme y directa. Marca «tres segundos» sin alargar las sílabas y cierra con suspense contenido; debe entenderse todo antes de revelar.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Tres segundos para arrepentiros. Para cambiar, ya es tarde.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Tres segundos. Ya no hay vuelta atrás.

**Pegar en ElevenLabs:**

```text
Tres segundos. Ya no hay vuelta atrás.
```

### countdown-02

**Archivo:** `countdown-02.wav` · **Evento:** `countdown`

**Interpretación:** Comunica primero el cierre, con autoridad. La invitación a respirar es breve y humana; no insertes una inhalación interpretada que consuma el tiempo.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Apuestas cerradas. Respirad hondo, que el tubo no espera.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Apuestas cerradas. Respirad hondo.

**Pegar en ElevenLabs:**

```text
Apuestas cerradas. Respirad hondo.
```

### countdown-03

**Archivo:** `countdown-03.wav` · **Evento:** `countdown`

**Interpretación:** Ataque claro en «verificado», sin estridencia. Baja al final para dar sensación de decisión definitiva; no prolongues la última palabra.

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** ¡Verificado! Ya no hay vuelta atrás.

**Pegar en ElevenLabs:**

```text
¡Verificado! Ya no hay vuelta atrás.
```

### countdown-04

**Archivo:** `countdown-04.wav` · **Evento:** `countdown`

**Interpretación:** Tono de ceremonia breve. Da un microcambio de intención al pasar de la información a la suerte, sin una pausa larga entre las frases.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Bandejas cerradas. Que la suerte reparta lo que la cabeza no supo.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Bandejas cerradas. Que reparta la suerte.

**Pegar en ElevenLabs:**

```text
Bandejas cerradas. Que reparta la suerte.
```

### countdown-05

**Archivo:** `countdown-05.wav` · **Evento:** `countdown`

**Interpretación:** Tres números claros, descendiendo ligeramente en tono. Ritmo regular, sin dramatizar cada número ni añadir «cero» o una conclusión inventada.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Tres, dos, uno… Disfrutad de estos últimos segundos de esperanza.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Tres, dos, uno…

**Pegar en ElevenLabs:**

```text
Tres, dos, uno…
```

### countdown-06

**Archivo:** `countdown-06.wav` · **Evento:** `countdown`

**Interpretación:** Orden breve, firme y cordial. Sonríe apenas al desear suerte; suena como un anfitrión que disfruta del suspense, no como un árbitro enfadado.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Nadie toca nada. El dinero está donde lo dejasteis, para bien o para mal.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Nadie toca nada. ¡Que haya suerte!

**Pegar en ElevenLabs:**

```text
Nadie toca nada. ¡Que haya suerte!
```

### countdown-07

**Archivo:** `countdown-07.wav` · **Evento:** `countdown`

**Interpretación:** Pronuncia «bloqueado» con precisión. El segundo enunciado es una recomendación cómplice, corta y fácil de oír sobre la música.

**Adaptación:** Versión breve para el bloqueo de tres segundos.
**Original del catálogo:** «Bloqueado. Ahora solo queda mirar la pantalla y cruzar los dedos.»

**Control de duración:** aprobar solo si termina antes de revelar; comprobar el tiempo real de la toma.

**Texto hablado / subtítulo futuro:** Bloqueado. Cruzad los dedos.

**Pegar en ElevenLabs:**

```text
Bloqueado. Cruzad los dedos.
```

<a id="situacion-05"></a>

## 05. Poco tiempo para apostar — 5 clips

**Cuándo:** Con límite de tiempo, cuando se activa el aviso de pocos segundos.
**Dirección del bloque:** Energía 4/5; urgencia legible y sin pánico.

### time-01

**Archivo:** `time-01.wav` · **Evento:** `time_running_out`

**Interpretación:** Aviso urgente y nítido, no un grito de pánico. Destaca «diez» y manda mover el dinero con energía práctica; no añadas pausas teatrales.

**Condición:** verificar que el aviso del juego se dispara cuando quedan diez segundos.

**Texto hablado / subtítulo futuro:** ¡Diez segundos! El dinero no se mueve solo.

**Pegar en ElevenLabs:**

```text
[excited] ¡Diez segundos! El dinero no se mueve solo.
```

### time-02

**Archivo:** `time-02.wav` · **Evento:** `time_running_out`

**Interpretación:** Empieza ágil, informando de verdad de la urgencia. La broma del tubo ocupa el remate, más bajo, sin ralentizar la intervención.

**Texto hablado / subtítulo futuro:** Se acaba el tiempo. El tubo ya está contando con vosotros.

**Pegar en ElevenLabs:**

```text
[mischievously] Se acaba el tiempo. El tubo ya está contando con vosotros.
```

### time-03

**Archivo:** `time-03.wav` · **Evento:** `time_running_out`

**Interpretación:** Ataque inmediato de aviso. Empuja la acción en «moved» y deja «el destino» como una amenaza de opereta, con sonrisa y sin alargarla.

**Texto hablado / subtítulo futuro:** ¡Que se acaba! Moved el dinero o lo moverá el destino.

**Pegar en ElevenLabs:**

```text
[excited] ¡Que se acaba! Moved el dinero o lo moverá el destino.
```

### time-04

**Archivo:** `time-04.wav` · **Evento:** `time_running_out`

**Interpretación:** Entra con prisa controlada. La segunda frase casi se responde a sí misma; enfatiza «rápido» y corta limpio para dejar jugar.

**Texto hablado / subtítulo futuro:** Últimos segundos. Si dudáis, dudad rápido.

**Pegar en ElevenLabs:**

```text
[excited] Últimos segundos. Si dudáis, dudad rápido.
```

### time-05

**Archivo:** `time-05.wav` · **Evento:** `time_running_out`

**Interpretación:** «Tic, tac» son dos golpes vocales breves, no una imitación larga de reloj. El resto debe sonar claro, decidido y progresivamente apremiante.

**Texto hablado / subtítulo futuro:** Tic, tac. Decidid ya o decidirá el reloj.

**Pegar en ElevenLabs:**

```text
[mischievously] Tic, tac. Decidid ya o decidirá el reloj.
```

<a id="situacion-06"></a>

## 06. Bloque intermedio — 4 clips

**Cuándo:** Al empezar la pregunta seis.
**Dirección del bloque:** Energía 3/5; cambio de nivel, con advertencia cómplice.

### medium-01

**Archivo:** `medium-01.wav` · **Evento:** `medium_block`

**Interpretación:** Cambia a un registro algo más serio. Primera frase como una frontera; segunda con entusiasmo contenido por lo que empieza ahora.

**Texto hablado / subtítulo futuro:** Se acabaron las fáciles. Ahora empieza el concurso de verdad.

**Pegar en ElevenLabs:**

```text
[mischievously] Se acabaron las fáciles. Ahora empieza el concurso de verdad.
```

### medium-02

**Archivo:** `medium-02.wav` · **Evento:** `medium_block`

**Interpretación:** Anuncia el cambio de nivel con presencia. «Agarrad bien» es una advertencia cómplice, no una nueva instrucción de mecánica ni una orden agresiva.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Pregunta {ronda}: subimos la dificultad. Agarrad bien el dinero.»

**Texto hablado / subtítulo futuro:** Subimos la dificultad. Agarrad bien el dinero.

**Pegar en ElevenLabs:**

```text
[excited] Subimos la dificultad. Agarrad bien el dinero.
```

### medium-03

**Archivo:** `medium-03.wav` · **Evento:** `medium_block`

**Interpretación:** Presentación limpia de bloque, con menos broma que una bienvenida. Destaca «intermedio» y marca la consecuencia con claridad, sin parecer un tutorial monótono.

**Texto hablado / subtítulo futuro:** Bloque intermedio. Ya no vale responder lo primero que suene.

**Pegar en ElevenLabs:**

```text
Bloque intermedio. Ya no vale responder lo primero que suene.
```

### medium-04

**Archivo:** `medium-04.wav` · **Evento:** `medium_block`

**Interpretación:** Despide el calentamiento con autoridad. Después observa el dinero con curiosidad interesada; el remate «cuánto os dura» lleva una sonrisa apenas audible.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Hasta aquí el calentamiento. Tenéis {premio}; veamos cuánto os dura.»

**Texto hablado / subtítulo futuro:** Hasta aquí el calentamiento. Todavía tenéis dinero; veamos cuánto os dura.

**Pegar en ElevenLabs:**

```text
[mischievously] Hasta aquí el calentamiento. Todavía tenéis dinero; veamos cuánto os dura.
```

<a id="situacion-07"></a>

## 07. Bloque difícil — 4 clips

**Cuándo:** Al empezar la pregunta once.
**Dirección del bloque:** Energía 3/5; mayor peso y suspense, sin cambiar de voz.

### hard-01

**Archivo:** `hard-01.wav` · **Evento:** `hard_block`

**Interpretación:** Anuncia las difíciles con solemnidad corta. En la segunda frase aflora un entusiasmo indecente por el tubo, sin convertirlo en voz de villano.

**Texto hablado / subtítulo futuro:** Llegan las difíciles. Aquí es donde el tubo come bien.

**Pegar en ElevenLabs:**

```text
[mischievously] Llegan las difíciles. Aquí es donde el tubo come bien.
```

### hard-02

**Archivo:** `hard-02.wav` · **Evento:** `hard_block`

**Interpretación:** Presenta el último bloque con peso. Finge ignorancia y luego rectifica casi sin querer, con una media sonrisa al mencionar la tarjeta.

**Texto hablado / subtítulo futuro:** Último bloque. Preguntas que ni yo sé. Bueno, yo tengo la tarjeta.

**Pegar en ElevenLabs:**

```text
[sarcastic] Último bloque. Preguntas que ni yo sé. Bueno, yo tengo la tarjeta.
```

### hard-03

**Archivo:** `hard-03.wav` · **Evento:** `hard_block`

**Interpretación:** Arranque seguro, más grave por intención y no por cambiar de personaje. Destaca «experto»; el deseo de suerte es breve, sincero solo a medias.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Pregunta {ronda}. Ahora sí, nivel experto. Vais a necesitar suerte.»

**Texto hablado / subtítulo futuro:** Ahora sí, nivel experto. Vais a necesitar suerte.

**Pegar en ElevenLabs:**

```text
Ahora sí, nivel experto. Vais a necesitar suerte.
```

### hard-04

**Archivo:** `hard-04.wav` · **Evento:** `hard_block`

**Interpretación:** Advierte con una calma demasiado amable. Tras la afirmación, baja un poco el tono y convierte «con cariño» en un consuelo sospechoso.

**Texto hablado / subtítulo futuro:** Las cinco últimas son las peores. Os lo digo con cariño.

**Pegar en ElevenLabs:**

```text
[sarcastic] Las cinco últimas son las peores. Os lo digo con cariño.
```

<a id="situacion-08"></a>

## 08. Última pregunta — 4 clips

**Cuándo:** Al empezar la pregunta quince.
**Dirección del bloque:** Energía 3/5; importancia real, con pausas contenidas.

### last-01

**Archivo:** `last-01.wav` · **Evento:** `last_question`

**Interpretación:** Da importancia al comienzo sin un grito. Una pausa breve separa el anuncio del riesgo; «todo o nada» debe sonar decidido y concreto.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Última pregunta. Todo o nada por {premio}.»

**Texto hablado / subtítulo futuro:** Última pregunta. Todo o nada por el premio.

**Pegar en ElevenLabs:**

```text
Última pregunta. Todo o nada por el premio.
```

### last-02

**Archivo:** `last-02.wav` · **Evento:** `last_question`

**Interpretación:** Exhala muy brevemente, como quien también siente la tensión. «Pregunta quince» es ceremonial; la frase final abre la posibilidad de los dos desenlaces.

**Texto hablado / subtítulo futuro:** Pregunta quince. Respirad hondo: aquí se ganan o se pierden los cheques.

**Pegar en ElevenLabs:**

```text
[exhales] Pregunta quince. Respirad hondo: aquí se ganan o se pierden los cheques.
```

### last-03

**Archivo:** `last-03.wav` · **Evento:** `last_question`

**Interpretación:** Habla más cerca del micrófono, sin susurrar. Deja espacio tras «La última» y construye ilusión hacia «el premio», con contención.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «La última. Solo una respuesta os separa de llevaros {premio}.»

**Texto hablado / subtítulo futuro:** La última. Solo una respuesta os separa de llevaros el premio.

**Pegar en ElevenLabs:**

```text
La última. Solo una respuesta os separa de llevaros el premio.
```

### last-04

**Archivo:** `last-04.wav` · **Evento:** `last_question`

**Interpretación:** Reconoce el final con orgullo de anfitrión. Después calma al grupo y reserva «demasiado» para una coletilla baja, traviesa y muy corta.

**Texto hablado / subtítulo futuro:** Llegamos al final. Que nadie tiemble… demasiado.

**Pegar en ElevenLabs:**

```text
[mischievously] Llegamos al final. Que nadie tiemble… demasiado.
```

<a id="situacion-09"></a>

## 09. Matemáticas — 6 clips

**Cuándo:** Al introducir una operación, si no tiene prioridad otro anuncio.
**Dirección del bloque:** Energía 2–3/5; profesor travieso, sin pistas sobre el resultado.

### math-01

**Archivo:** `math-01.wav` · **Evento:** `math_question`

**Interpretación:** Presenta la categoría como una autoridad académica impostada. Recalca «sin calculadora» con humor; «serio» es el remate y no una reprimenda.

**Texto hablado / subtítulo futuro:** Pregunta de matemáticas. Sin calculadora, que esto es un concurso serio.

**Pegar en ElevenLabs:**

```text
[sarcastic] Pregunta de matemáticas. Sin calculadora, que esto es un concurso serio.
```

### math-02

**Archivo:** `math-02.wav` · **Evento:** `math_question`

**Interpretación:** Normaliza las cuentas con tono cercano. Ofrece el permiso de usar dedos como una concesión magnánima; evita infantilizar la voz.

**Texto hablado / subtítulo futuro:** Toca hacer cuentas. Los dedos de las manos están permitidos.

**Pegar en ElevenLabs:**

```text
[mischievously] Toca hacer cuentas. Los dedos de las manos están permitidos.
```

### math-03

**Archivo:** `math-03.wav` · **Evento:** `math_question`

**Interpretación:** Suena sinceramente encantado con los números. La segunda frase explica ese entusiasmo con una sonrisa perversa pequeña, sin reír encima del texto.

**Texto hablado / subtítulo futuro:** Números, mi parte favorita. Sobre todo cuando os equivocáis.

**Pegar en ElevenLabs:**

```text
[mischievously] Números, mi parte favorita. Sobre todo cuando os equivocáis.
```

### math-04

**Archivo:** `math-04.wav` · **Evento:** `math_question`

**Interpretación:** Nombra la categoría y tranquiliza de verdad. El pequeño giro «casi nunca» llega más bajo, después de una pausa corta y natural.

**Texto hablado / subtítulo futuro:** Matemáticas. Tranquilos, los números no muerden. Casi nunca.

**Pegar en ElevenLabs:**

```text
[curious] Matemáticas. Tranquilos, los números no muerden. Casi nunca.
```

### math-05

**Archivo:** `math-05.wav` · **Evento:** `math_question`

**Interpretación:** Pide colaboración con ritmo cotidiano y simpático. Da el peso a «alguien del grupo» y «calculadora», sin sugerir una respuesta matemática.

**Texto hablado / subtítulo futuro:** Una operación. Que alguien del grupo haga de calculadora.

**Pegar en ElevenLabs:**

```text
Una operación. Que alguien del grupo haga de calculadora.
```

### math-06

**Archivo:** `math-06.wav` · **Evento:** `math_question`

**Interpretación:** Breve anuncio de categoría y luego recuerdo de profesor divertido. La ironía apunta a aquella excusa escolar, sin echar una bronca al jugador.

**Texto hablado / subtítulo futuro:** Cálculo mental. Y luego decíais en el colegio que no servía para nada.

**Pegar en ElevenLabs:**

```text
[sarcastic] Cálculo mental. Y luego decíais en el colegio que no servía para nada.
```

<a id="situacion-10"></a>

## 10. Acierto con alguna pérdida — 7 clips

**Cuándo:** Al revelar un acierto con pérdida, si no tiene prioridad otro evento.
**Dirección del bloque:** Energía 3/5; alivio moderado, sin sonar a pleno.

### success-01

**Archivo:** `success-01.wav` · **Evento:** `success`

**Interpretación:** Celebra «correcto» con una subida rápida de energía. Recupera el aplomo al hablar del premio y añade el aviso final con picardía contenida.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «¡Correcto! Os quedan {premio}. No os acostumbréis.»

**Texto hablado / subtítulo futuro:** ¡Correcto! El premio sigue en juego. No os acostumbréis.

**Pegar en ElevenLabs:**

```text
[excited] ¡Correcto! El premio sigue en juego. No os acostumbréis.
```

### success-02

**Archivo:** `success-02.wav` · **Evento:** `success`

**Interpretación:** Reconoce el acierto sin una gran ovación. La segunda frase adelanta dificultad con tono de anfitrión que conoce la tarjeta, no de amenaza personal.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Acierto en la pregunta {ronda}. La siguiente no será tan amable.»

**Texto hablado / subtítulo futuro:** Pregunta acertada. La siguiente no será tan amable.

**Pegar en ElevenLabs:**

```text
[mischievously] Pregunta acertada. La siguiente no será tan amable.
```

### success-03

**Archivo:** `success-03.wav` · **Evento:** `success`

**Interpretación:** Di «acierto» de forma neutra, como si te costase admitirlo. El presupuesto personificado recibe una pena teatral pequeñita y un cierre seco.

**Texto hablado / subtítulo futuro:** Acierto. El presupuesto del decorado está llorando.

**Pegar en ElevenLabs:**

```text
[sarcastic] Acierto. El presupuesto del decorado está llorando.
```

### success-04

**Archivo:** `success-04.wav` · **Evento:** `success`

**Interpretación:** Felicita con sinceridad moderada. «Por el camino» reconoce la pérdida sin celebrarla; termina animándoles a cuidar lo que sigue en juego.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Bien hecho, aunque algo se ha quedado por el camino. Os quedan {premio}.»

**Texto hablado / subtítulo futuro:** Bien hecho, aunque algo se ha quedado por el camino. Cuidad lo que queda.

**Pegar en ElevenLabs:**

```text
Bien hecho, aunque algo se ha quedado por el camino. Cuidad lo que queda.
```

### success-05

**Archivo:** `success-05.wav` · **Evento:** `success`

**Interpretación:** Alivia la tensión al anunciar que superaron la pregunta. La decepción fingida por el tubo aparece en la segunda frase y no tapa el acierto.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Pregunta {ronda} superada. El tubo se queda con hambre.»

**Texto hablado / subtítulo futuro:** Pregunta superada. El tubo se queda con hambre.

**Pegar en ElevenLabs:**

```text
[mischievously] Pregunta superada. El tubo se queda con hambre.
```

### success-06

**Archivo:** `success-06.wav` · **Evento:** `success`

**Interpretación:** Resultado claro al principio. Después concede el aprobado con una pequeña vacilación expresiva en «limpio» y aceptación simpática en «suficiente».

**Texto hablado / subtítulo futuro:** Correcto. No ha sido limpio, pero ha sido suficiente.

**Pegar en ElevenLabs:**

```text
[sarcastic] Correcto. No ha sido limpio, pero ha sido suficiente.
```

### success-07

**Archivo:** `success-07.wav` · **Evento:** `success`

**Interpretación:** Elogio contenido, de profesor que concede el pase. Marca el contraste entre «mínima» y «seguís», con una esperanza real al cerrar.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Aprobados por la mínima. Seguís teniendo {premio} en juego.»

**Texto hablado / subtítulo futuro:** Aprobados por la mínima. Seguís teniendo dinero en juego.

**Pegar en ElevenLabs:**

```text
[mischievously] Aprobados por la mínima. Seguís teniendo dinero en juego.
```

<a id="situacion-11"></a>

## 11. Pleno — 6 clips

**Cuándo:** Al acertar sin perder dinero, si no tiene prioridad otro evento.
**Dirección del bloque:** Energía 4/5; admiración real disfrazada de disgusto por el decorado.

### perfect-01

**Archivo:** `perfect-01.wav` · **Evento:** `perfect`

**Interpretación:** Sorpresa alegre, genuina durante el primer instante. Confirma que no se perdió nada y luego rompe la celebración fingiendo que te han estropeado el guion.

**Texto hablado / subtítulo futuro:** ¡Pleno! Ni un billete perdido. Esto no estaba en el guion.

**Pegar en ElevenLabs:**

```text
[surprised] ¡Pleno! Ni un billete perdido. Esto no estaba en el guion.
```

### perfect-02

**Archivo:** `perfect-02.wav` · **Evento:** `perfect`

**Interpretación:** Describe el pleno con claridad y una admiración que intentas disimular. La queja del decorado suena personal y cómica, nunca indignada de verdad.

**Texto hablado / subtítulo futuro:** Todo el dinero en la respuesta buena. Me estáis arruinando el decorado.

**Pegar en ElevenLabs:**

```text
[sarcastic] Todo el dinero en la respuesta buena. Me estáis arruinando el decorado.
```

### perfect-03

**Archivo:** `perfect-03.wav` · **Evento:** `perfect`

**Interpretación:** «Perfecto» parece una concesión a regañadientes. Reconoce que conservan el dinero y remata «cara de póker» con una sequedad deliberada.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Perfecto. Seguís con {premio} y yo, con cara de póker.»

**Texto hablado / subtítulo futuro:** Perfecto. Seguís con todo vuestro dinero y yo, con cara de póker.

**Pegar en ElevenLabs:**

```text
[sarcastic] Perfecto. Seguís con todo vuestro dinero y yo, con cara de póker.
```

### perfect-04

**Archivo:** `perfect-04.wav` · **Evento:** `perfect`

**Interpretación:** Asombro breve por el tubo vacío. Haz la pregunta final con curiosidad divertida y un pequeño ascenso; no exijas una respuesta de los jugadores.

**Texto hablado / subtítulo futuro:** Ni una moneda al tubo. ¿Habéis estudiado o es pura suerte?

**Pegar en ElevenLabs:**

```text
[curious] Ni una moneda al tubo. ¿Habéis estudiado o es pura suerte?
```

### perfect-05

**Archivo:** `perfect-05.wav` · **Evento:** `perfect`

**Interpretación:** Pronuncia el elogio con satisfacción y cercanía. Saborea «limpia» y cierra el dicho popular con naturalidad, sin voz de anuncio de detergente.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Pregunta {ronda}, limpia como una patena.»

**Texto hablado / subtítulo futuro:** Una ronda limpia como una patena.

**Pegar en ElevenLabs:**

```text
Una ronda limpia como una patena.
```

### perfect-06

**Archivo:** `perfect-06.wav` · **Evento:** `perfect`

**Interpretación:** Felicitación clara y pequeña pausa para disfrutarla. La advertencia posterior recupera el personaje travieso, sin borrar el mérito del grupo.

**Texto hablado / subtítulo futuro:** Impecable. Disfrutadlo, que la siguiente será más difícil.

**Pegar en ElevenLabs:**

```text
[mischievously] Impecable. Disfrutadlo, que la siguiente será más difícil.
```

<a id="situacion-12"></a>

## 12. Premio multiplicado — 5 clips

**Cuándo:** Cuando un comodín acertado genera bonificación y tiene prioridad este comentario.
**Dirección del bloque:** Energía 4/5; celebrar el riesgo que sí ha salido bien.

### multiplied-01

**Archivo:** `multiplied-01.wav` · **Evento:** `multiplied`

**Interpretación:** Alegría inmediata por el riesgo que funcionó. Segundo golpe de energía en «multiplicado», sin inventar una cifra ni prolongar el grito.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «¡El comodín ha salido bien! Premio multiplicado por {multiplicador}.»

**Texto hablado / subtítulo futuro:** ¡El comodín ha salido bien! ¡Premio multiplicado!

**Pegar en ElevenLabs:**

```text
[excited] ¡El comodín ha salido bien! ¡Premio multiplicado!
```

### multiplied-02

**Archivo:** `multiplied-02.wav` · **Evento:** `multiplied`

**Interpretación:** Combina admiración por el valor con sorpresa por la suerte. La noticia del premio debe sonar a recompensa merecida y cerrar con amplitud.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Por {multiplicador}. Qué valor y qué suerte: ahora tenéis {premio}.»

**Texto hablado / subtítulo futuro:** Qué valor y qué suerte: ¡vuestro premio se ha multiplicado!

**Pegar en ElevenLabs:**

```text
[excited] Qué valor y qué suerte: ¡vuestro premio se ha multiplicado!
```

### multiplied-03

**Archivo:** `multiplied-03.wav` · **Evento:** `multiplied`

**Interpretación:** Suspiro corto, más envidia que tristeza. Resume la buena jugada y confiesa tu mala suerte financiera como un comentario privado al micrófono.

**Texto hablado / subtítulo futuro:** Apostarse y acertar. Ojalá mis inversiones salieran así.

**Pegar en ElevenLabs:**

```text
[sighs] Apostarse y acertar. Ojalá mis inversiones salieran así.
```

### multiplied-04

**Archivo:** `multiplied-04.wav` · **Evento:** `multiplied`

**Interpretación:** Anuncia la multiplicación con oficio de presentador. Al mencionar el decorado, baja un poco la energía y finge resignación presupuestaria.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Multiplicador por {multiplicador}. El decorado tendrá que esperar.»

**Texto hablado / subtítulo futuro:** Multiplicador activado. El decorado tendrá que esperar.

**Pegar en ElevenLabs:**

```text
[mischievously] Multiplicador activado. El decorado tendrá que esperar.
```

### multiplied-05

**Archivo:** `multiplied-05.wav` · **Evento:** `multiplied`

**Interpretación:** Primera frase satisfecha, como quien comprueba una buena cuenta. El cierre celebra el crecimiento con brillo, sin prometer que seguirá aumentando en otra ronda.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «La cabina ha pagado bien. {premio} y subiendo.»

**Texto hablado / subtítulo futuro:** La cabina ha pagado bien. ¡El premio crece!

**Pegar en ElevenLabs:**

```text
[excited] La cabina ha pagado bien. ¡El premio crece!
```

<a id="situacion-13"></a>

## 13. Pérdida importante — 8 clips

**Cuándo:** Cuando se pierde más dinero del que se conserva.
**Dirección del bloque:** Energía 2–3/5; burla de la situación y del presupuesto, no de la persona.

### loss-01

**Archivo:** `loss-01.wav` · **Evento:** `loss`

**Interpretación:** Reconoce la pérdida con calma y una compasión muy superficial. El agradecimiento al decorado es el remate, dicho como si fuera de lo más educado.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Habéis perdido {cantidad}. El decorado os lo agradece.»

**Texto hablado / subtítulo futuro:** Habéis perdido un buen pellizco. El decorado os lo agradece.

**Pegar en ElevenLabs:**

```text
[sarcastic] Habéis perdido un buen pellizco. El decorado os lo agradece.
```

### loss-02

**Archivo:** `loss-02.wav` · **Evento:** `loss`

**Interpretación:** Reformula el desastre con tono de relaciones públicas. Después agradece la aportación con una cortesía tan perfecta que resulte sospechosa.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Aquí nadie pierde dinero: financia el decorado. Hoy, {cantidad}.»

**Texto hablado / subtítulo futuro:** Aquí nadie pierde dinero: financia el decorado. Gracias por vuestra aportación.

**Pegar en ElevenLabs:**

```text
[mischievously] Aquí nadie pierde dinero: financia el decorado. Gracias por vuestra aportación.
```

### loss-03

**Archivo:** `loss-03.wav` · **Evento:** `loss`

**Interpretación:** Observación corta, seca y segura. Acentúa «certezas» y deja el contraste con el grupo para el final, sin insultos ni carcajadas.

**Texto hablado / subtítulo futuro:** El tubo tiene más certezas que vosotros.

**Pegar en ElevenLabs:**

```text
[sarcastic] El tubo tiene más certezas que vosotros.
```

### loss-05

**Archivo:** `loss-05.wav` · **Evento:** `loss`

**Interpretación:** Describe la mala distribución con claridad. En la segunda frase sonríe apenas, imaginando un tubo satisfecho; no añadas sonidos de comer.

**Texto hablado / subtítulo futuro:** Más dinero fuera que dentro. El tubo se relame.

**Pegar en ElevenLabs:**

```text
[mischievously] Más dinero fuera que dentro. El tubo se relame.
```

### loss-06

**Archivo:** `loss-06.wav` · **Evento:** `loss`

**Interpretación:** Reacciona al agujero con sorpresa breve. Recupera enseguida el pragmatismo interesado de quien ya piensa en la reforma del plató.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «{cantidad} menos. Con eso pintamos el plató de nuevo.»

**Texto hablado / subtítulo futuro:** Menudo agujero en el presupuesto. Con eso pintamos el plató de nuevo.

**Pegar en ElevenLabs:**

```text
[surprised] Menudo agujero en el presupuesto. Con eso pintamos el plató de nuevo.
```

### loss-07

**Archivo:** `loss-07.wav` · **Evento:** `loss`

**Interpretación:** Decepción ligera, sin sentenciar una derrota que todavía no ha llegado. El consejo final es cercano y útil, con más cuidado que burla.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Mala ronda. Os quedan {premio}; cuidadlos mejor.»

**Texto hablado / subtítulo futuro:** Mala ronda. Cuidad mejor el dinero que os queda.

**Pegar en ElevenLabs:**

```text
Mala ronda. Cuidad mejor el dinero que os queda.
```

### loss-08

**Archivo:** `loss-08.wav` · **Evento:** `loss`

**Interpretación:** Suspiro pequeño y tres «ay» naturales, sin canto ni lamento exagerado. La última frase observa cómo se va el dinero con una resignación burlona.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Ay, ay, ay. {cantidad} que se van y no vuelven.»

**Texto hablado / subtítulo futuro:** Ay, ay, ay. Esos billetes se van y no vuelven.

**Pegar en ElevenLabs:**

```text
[sighs] Ay, ay, ay. Esos billetes se van y no vuelven.
```

### loss-09

**Archivo:** `loss-09.wav` · **Evento:** `loss`

**Interpretación:** Entrega la primera observación como un diagnóstico elegante. La segunda frase, más corta y baja, revela el chiste; deja una pausa pequeña antes de ella.

**Texto hablado / subtítulo futuro:** La sabiduría no estaba en esa bandeja. Y el dinero tampoco.

**Pegar en ElevenLabs:**

```text
[sarcastic] La sabiduría no estaba en esa bandeja. Y el dinero tampoco.
```

<a id="situacion-14"></a>

## 14. Expulsión — 7 clips

**Cuándo:** Cuando alguien apostado en una respuesta falsa queda eliminado.
**Dirección del bloque:** Energía 3/5; despedida teatral. No confundir con una desconexión.

### loss-04

**Archivo:** `loss-04.wav` · **Evento:** `eliminated`

**Interpretación:** Haz balance de los concursantes que continúan sin dar una cifra. La alusión al tubo es una coletilla traviesa; no suenes alegre por una lesión real.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Quedan {supervivientes} concursantes en pie. El tubo pide más.»

**Correspondencia especial:** este ID se reproduce en una expulsión; no moverlo al grupo de pérdidas por su nombre.

**Texto hablado / subtítulo futuro:** Quedan concursantes en pie. El tubo pide más.

**Pegar en ElevenLabs:**

```text
[mischievously] Quedan concursantes en pie. El tubo pide más.
```

### eliminated-01

**Archivo:** `eliminated-01.wav` · **Evento:** `eliminated`

**Interpretación:** Explica lo ocurrido con claridad y aplomo. Pausa breve entre el error y su consecuencia; el tubo suena implacable dentro de la ficción de concurso.

**Texto hablado / subtítulo futuro:** Alguien se apostó a la respuesta equivocada. El tubo no perdona.

**Pegar en ElevenLabs:**

```text
Alguien se apostó a la respuesta equivocada. El tubo no perdona.
```

### eliminated-02

**Archivo:** `eliminated-02.wav` · **Evento:** `eliminated`

**Interpretación:** Reconoce la baja con falsa solemnidad. La aparente condolencia gira a una ventaja absurda en «más espacio», con una sonrisa muy contenida.

**Texto hablado / subtítulo futuro:** Un concursante menos. Los demás seguirán… con más espacio.

**Pegar en ElevenLabs:**

```text
[mischievously] Un concursante menos. Los demás seguirán… con más espacio.
```

### eliminated-03

**Archivo:** `eliminated-03.wav` · **Evento:** `eliminated`

**Interpretación:** Anuncia la expulsión con impacto corto. Después vuelve al tono conversacional y concede que la fe era bonita, con ironía suave en «acierta».

**Texto hablado / subtítulo futuro:** ¡Expulsión! La fe es bonita, pero no siempre acierta.

**Pegar en ElevenLabs:**

```text
[surprised] ¡Expulsión! La fe es bonita, pero no siempre acierta.
```

### eliminated-04

**Archivo:** `eliminated-04.wav` · **Evento:** `eliminated`

**Interpretación:** Distingue a quienes siguen jugando de quienes pasan a la grada. Primera parte alentadora; segunda de ceremonia, sin burlarte personalmente del eliminado.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Quedan {supervivientes} en pie. El resto, a la grada.»

**Texto hablado / subtítulo futuro:** Los que seguís en pie, a por la siguiente. El resto, a la grada.

**Pegar en ElevenLabs:**

```text
Los que seguís en pie, a por la siguiente. El resto, a la grada.
```

### eliminated-05

**Archivo:** `eliminated-05.wav` · **Evento:** `eliminated`

**Interpretación:** Empieza una despedida respetuosa. «Bueno» corrige el elogio con naturalidad, como si te obligaras a ser preciso; el final cae seco.

**Texto hablado / subtítulo futuro:** Despedimos a un valiente. Bueno, a un valiente equivocado.

**Pegar en ElevenLabs:**

```text
[sarcastic] Despedimos a un valiente. Bueno, a un valiente equivocado.
```

### eliminated-06

**Archivo:** `eliminated-06.wav` · **Evento:** `eliminated`

**Interpretación:** Comenta el tubo satisfecho sin efectos vocales de monstruo. La segunda frase devuelve la atención al concurso y mantiene el ritmo de la partida.

**Texto hablado / subtítulo futuro:** El tubo ha comido. Y aún queda concurso.

**Pegar en ElevenLabs:**

```text
[mischievously] El tubo ha comido. Y aún queda concurso.
```

<a id="situacion-15"></a>

## 15. Abandono o desconexión — 4 clips

**Cuándo:** Cuando un jugador abandona la partida.
**Dirección del bloque:** Energía 2/5; continuidad del espectáculo. Nunca insinuar que sabemos por qué se desconectó.

### left-01

**Archivo:** `left-01.wav` · **Evento:** `player_left`

**Interpretación:** Cuenta la salida como una noticia del plató, sin enfado real. El tubo recibe una nostalgia fingida breve y simpática.

**Texto hablado / subtítulo futuro:** Un concursante ha huido del plató. El tubo lo echará de menos.

**Pegar en ElevenLabs:**

```text
[mischievously] Un concursante ha huido del plató. El tubo lo echará de menos.
```

### left-02

**Archivo:** `left-02.wav` · **Evento:** `player_left`

**Interpretación:** Anuncia la ausencia con sobriedad. Después anima a quienes quedan; la energía sube ligeramente en «protagonismo» sin festejar la desconexión.

**Texto hablado / subtítulo futuro:** Uno menos. Más protagonismo para los valientes que quedan.

**Pegar en ElevenLabs:**

```text
Uno menos. Más protagonismo para los valientes que quedan.
```

### left-03

**Archivo:** `left-03.wav` · **Evento:** `player_left`

**Interpretación:** Información serena y profesional. Da continuidad con «el espectáculo continúa», firme y cordial, sin reproches ni dramatismo de despedida.

**Texto hablado / subtítulo futuro:** Alguien ha abandonado el plató. El espectáculo continúa.

**Pegar en ElevenLabs:**

```text
Alguien ha abandonado el plató. El espectáculo continúa.
```

### left-04

**Archivo:** `left-04.wav` · **Evento:** `player_left`

**Interpretación:** Finge una ofensa de etiqueta, no una acusación seria. «Sin despedirse» prepara el remate, que debe sonar a anfitrión presumido y juguetón.

**Texto hablado / subtítulo futuro:** Un concursante se ha marchado sin despedirse. Qué poca educación.

**Pegar en ElevenLabs:**

```text
[sarcastic] Un concursante se ha marchado sin despedirse. Qué poca educación.
```

<a id="situacion-16"></a>

## 16. Victoria — 6 clips

**Cuándo:** Al superar las quince preguntas y terminar con victoria.
**Dirección del bloque:** Energía 5/5; alegría merecida, sin saturar ni tapar el resultado.

### victory-01

**Archivo:** `victory-01.wav` · **Evento:** `victory`

**Interpretación:** Celebra la victoria de verdad y reconoce el premio con amplitud. Después baja a una confidencia alegre sobre el cheque de cartón, sin perder la sonrisa.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «¡Victoria! Os lleváis {premio}. Contad bien el cheque, que es de cartón.»

**Texto hablado / subtítulo futuro:** ¡Victoria! Os lleváis el premio. Contad bien el cheque, que es de cartón.

**Pegar en ElevenLabs:**

```text
[excited] ¡Victoria! Os lleváis el premio. Contad bien el cheque, que es de cartón.
```

### victory-02

**Archivo:** `victory-02.wav` · **Evento:** `victory`

**Interpretación:** Suspiro mínimo de rendición cómica, no agotamiento. Da valor a las quince preguntas y admite el elogio final como algo que te cuesta conceder.

**Texto hablado / subtítulo futuro:** Quince preguntas superadas. Me duele decirlo: lo habéis hecho bien.

**Pegar en ElevenLabs:**

```text
[sighs] Quince preguntas superadas. Me duele decirlo: lo habéis hecho bien.
```

### victory-03

**Archivo:** `victory-03.wav` · **Evento:** `victory`

**Interpretación:** Triunfo genuino en el arranque. Ofrece premio y respeto con ceremonia; la autocrítica final baja de tono y convierte al presentador en el blanco del chiste.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «¡Lo habéis conseguido! {premio} y mi respeto, que vale bastante menos.»

**Texto hablado / subtítulo futuro:** ¡Lo habéis conseguido! El premio y mi respeto, que vale bastante menos.

**Pegar en ElevenLabs:**

```text
[excited] ¡Lo habéis conseguido! El premio y mi respeto, que vale bastante menos.
```

### victory-04

**Archivo:** `victory-04.wav` · **Evento:** `victory`

**Interpretación:** Orgullo y asombro por el recorrido completo. La llamada al banco es una celebración traviesa; termina alegre, sin imitar una llamada telefónica.

**Texto hablado / subtítulo futuro:** Quince de quince. Avisad al banco, que se va a llevar un susto.

**Pegar en ElevenLabs:**

```text
[excited] Quince de quince. Avisad al banco, que se va a llevar un susto.
```

### victory-05

**Archivo:** `victory-05.wav` · **Evento:** `victory`

**Interpretación:** «Campeones» debe sonar merecido, aunque te resistas. La queja posterior admite que el grupo es listo y cierra con afecto bajo la ironía.

**Texto hablado / subtítulo futuro:** Campeones. El decorado tendrá que esperar a un grupo menos listo.

**Pegar en ElevenLabs:**

```text
[sarcastic] Campeones. El decorado tendrá que esperar a un grupo menos listo.
```

### victory-06

**Archivo:** `victory-06.wav` · **Evento:** `victory`

**Interpretación:** Felicita al grupo superviviente sin contar personas. La broma de la cena es un epílogo corto y sonriente después de celebrar, no el centro de la victoria.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Victoria con {supervivientes} supervivientes. El tubo se va a dormir sin cenar.»

**Texto hablado / subtítulo futuro:** ¡Victoria para los supervivientes! El tubo se va a dormir sin cenar.

**Pegar en ElevenLabs:**

```text
[excited] ¡Victoria para los supervivientes! El tubo se va a dormir sin cenar.
```

<a id="situacion-17"></a>

## 17. Derrota — 6 clips

**Cuándo:** Al terminar una partida con derrota.
**Dirección del bloque:** Energía 2/5; cierre teatral y humor, dejando ganas de revancha.

### defeat-01

**Archivo:** `defeat-01.wav` · **Evento:** `defeat`

**Interpretación:** Cierra el concurso con aplomo y una pena impostada muy ligera. «Que es lo justo» es una insolencia de personaje, nunca una condena agresiva.

**Texto hablado / subtítulo futuro:** Fin del concurso. Os habéis quedado sin nada, que es lo justo.

**Pegar en ElevenLabs:**

```text
Fin del concurso. Os habéis quedado sin nada, que es lo justo.
```

### defeat-02

**Archivo:** `defeat-02.wav` · **Evento:** `defeat`

**Interpretación:** Da el resultado con claridad. El presupuesto de la temporada siguiente produce una satisfacción culpable y educada, sin carcajada larga.

**Texto hablado / subtítulo futuro:** Derrota. El decorado ya tiene presupuesto para la próxima temporada.

**Pegar en ElevenLabs:**

```text
[mischievously] Derrota. El decorado ya tiene presupuesto para la próxima temporada.
```

### defeat-03

**Archivo:** `defeat-03.wav` · **Evento:** `defeat`

**Interpretación:** Primera frase breve, de cierre. Luego aparenta una explicación institucional y confiesa «a mí» más bajo, como si se te escapara el verdadero negocio.

**Texto hablado / subtítulo futuro:** Se acabó. El dinero vuelve a casa, es decir, a mí.

**Pegar en ElevenLabs:**

```text
[sarcastic] Se acabó. El dinero vuelve a casa, es decir, a mí.
```

### defeat-04

**Archivo:** `defeat-04.wav` · **Evento:** `defeat`

**Interpretación:** Reconoce el final con calidez contenida. La invitación a la revancha es sincera y abierta; suena como alguien que quiere volver a ver al grupo.

**Adaptación:** Sustitución de variables por una versión fija.
**Original del catálogo:** «Derrota en la pregunta {ronda}. Os esperamos en la revancha.»

**Texto hablado / subtítulo futuro:** La partida termina aquí. Os esperamos en la revancha.

**Pegar en ElevenLabs:**

```text
La partida termina aquí. Os esperamos en la revancha.
```

### defeat-05

**Archivo:** `defeat-05.wav` · **Evento:** `defeat`

**Interpretación:** «Cero» cae corto y seco. La referencia al autobús es una pausa cómica, no una burla cruel; el agradecimiento final recupera la sonrisa de anfitrión.

**Texto hablado / subtítulo futuro:** Cero. Ni para el autobús. Gracias por financiar el plató.

**Pegar en ElevenLabs:**

```text
[sarcastic] Cero. Ni para el autobús. Gracias por financiar el plató.
```

### defeat-06

**Archivo:** `defeat-06.wav` · **Evento:** `defeat`

**Interpretación:** Suspiro breve de final de espectáculo. Empieza sentimental sin exageración y pincha esa solemnidad al rematar «que fue poco» en voz más cotidiana.

**Texto hablado / subtítulo futuro:** Fin del sueño. Ha sido bonito mientras duró, que fue poco.

**Pegar en ElevenLabs:**

```text
[sighs] Fin del sueño. Ha sido bonito mientras duró, que fue poco.
```

## Control antes de dar una toma por aprobada

- [ ] ID y evento correctos; nombre base exacto, sin renumerar.
- [ ] Se oye la misma persona, acento y distancia al micrófono que en las tomas de prueba.
- [ ] Texto completo, sin cifras inventadas, sin variables ni etiquetas leídas en voz alta.
- [ ] Interpretación con intención y remate, sin gritos, risas largas ni tono plano.
- [ ] Sin música, efectos, clipping, cortes de sílabas ni silencios excesivos.
- [ ] Duración compatible con su evento; especial atención a los tres segundos del cierre.
- [ ] Al integrar: subtítulo idéntico al texto hablado, clip vinculado por ID y volumen de voz equilibrado con música/efectos.

Estado de este documento: guion preparado; generación de audio, selección de tomas, importación, subtítulos y reproducción en Unity pendientes.
