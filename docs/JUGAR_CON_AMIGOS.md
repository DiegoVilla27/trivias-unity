# Jugar con amigos gratis (por internet)

Guía para probar el juego con otras personas sin pagar nada, mientras no esté integrado Steam. Se usa una **red virtual (VPN) gratuita**: los jugadores quedan en la «misma red» y se unen escribiendo la dirección del host en el menú.

> Guía preparada sin una prueba real entre dos ordenadores todavía. Si algún paso no coincide con lo que veis, actualizarla.

## Paso 1 — Crear la build (el host)

Los demás no necesitan Unity, solo el juego compilado.

1. En Unity: **Tools → Trivias → Crear build → macOS** o **→ Windows** (según el ordenador del amigo).
   - Para Windows hace falta una vez el módulo gratuito: **Unity Hub → Installs → ⚙ de Unity 6000.6 → Add modules → «Windows Build Support (Mono)»**. Si falta, Unity avisa.
2. Al terminar se abre la carpeta con el zip listo: `Builds/Trivias-Mac.zip` o `Builds/Trivias-Windows.zip`.
3. Compartir el zip (Google Drive, WeTransfer…). En Mac se comprime con `ditto` para conservar los permisos de la app.

Alternativa manual: **File → Build Profiles** → plataforma → *Switch Platform* → **Build** (solo la escena `Stage`).

En las builds normales hacen falta **2 jugadores** para empezar (GDD). El modo «jugar solo» solo existe en el editor.

## Paso 2 — Red virtual gratuita (todos)

Recomendado: **ZeroTier** (gratis hasta 10 dispositivos; Mac y Windows).

1. **Host:** crear cuenta en **my.zerotier.com** → *Create A Network*. Copiar el **Network ID** (16 caracteres).
2. **Todos:** instalar **ZeroTier One** (zerotier.com/download).
3. **Todos:** ZeroTier One → **Join Network** → pegar el Network ID.
4. **Host:** en my.zerotier.com → la red → *Members* → marcar **«Auth»** en cada dispositivo.
5. Ahí aparece la **IP asignada** de cada uno (tipo `10.147.x.x`). **Apuntar la del host.**

Si todos usan Windows, **Radmin VPN** es más simple: uno crea una red y los demás se unen con nombre y contraseña.

## Paso 3 — Jugar

1. **Voz:** usar **Discord** (la voz dentro del juego aún no existe).
2. **Host:** abrir el juego → **Crear sala**. Si el sistema pregunta por **conexiones entrantes**, pulsar **Permitir**.
3. En la sala de espera, el recuadro **«Invita a tus amigos»** muestra la dirección que hay que compartir (el juego detecta la red virtual —ZeroTier, Radmin, Tailscale, Hamachi— y la pone primero; si no hay, la de la red local). Pulsar **Copiar** y pasarla por WhatsApp o Discord.
4. **Los demás:** **Unirme a una sala** → pegar la dirección → **Conectar**. El juego recuerda la última dirección usada.
5. En la sala de espera se ve «JUGADORES n/6». Mientras esperáis, **Cómo se juega** explica las reglas. El host pulsa **Empezar partida**.

## Problemas frecuentes

| Síntoma | Qué revisar |
| --- | --- |
| «No hay ninguna sala en …» | Que la IP sea la de ZeroTier (no la del wifi), que todos tengan **Auth** marcado y que el host aceptara el aviso del firewall. Si no salió, permitir el juego en el firewall (**puerto UDP 7777**). |
| Mac: «no se puede abrir porque no está verificada» | Clic derecho sobre la app → **Abrir** → Abrir. Si persiste: `xattr -cr Trivias.app` en la Terminal. |
| Retraso | Normal con VPN; que haga de host quien tenga mejor conexión. |
| «La sala está llena» / «La partida ya empezó» | Máximo 6 jugadores; no se puede entrar a mitad de partida (GDD). |

## Más adelante: Steam

Para invitar desde la lista de amigos de Steam sin escribir IP, se integrará Steam con el **AppID 480 «Spacewar»** (gratuito, para desarrollo). Publicar con AppID propio cuesta 100 $ (Steam Direct), solo al lanzar.
