# Documentación del juego

Documentos derivados del [GDD](GDD.md), referencia principal de reglas y alcance. Estos documentos concretan cada área sin dar por confirmadas las propuestas de balance.

| Documento | Contenido |
| --- | --- |
| [LEVEL.md](LEVEL.md) | Distribución del plató, cabinas, mesa, presentación y resultados |
| [PLAYER.md](PLAYER.md) | Personaje base, acciones, estados, personalización y espectadores |
| [RECURSOS.md](RECURSOS.md) | Inventario de arte, animación, audio, interfaz y contenido |
| [SISTEMAS.md](SISTEMAS.md) | Partida, dinero, comodines, fichas, ranking y multijugador |
| [CONTENIDO.md](CONTENIDO.md) | Preguntas, frases del presentador y entrada opcional de IA |
| [UI_UX.md](UI_UX.md) | Pantallas, información visible y respuestas a las acciones |
| [DESARROLLO.md](DESARROLLO.md) | Orden de construcción, verificaciones y decisiones pendientes |
| [Bocetos visuales](bocetos/README.md) | Concepto del plató y láminas para modelado de personaje, presentador y recursos |
| [JUGAR_CON_AMIGOS.md](JUGAR_CON_AMIGOS.md) | Cómo probar el juego con otras personas gratis (build + VPN gratuita) |

## Referencias visuales

Los bocetos se organizan por [player](bocetos/player/README.md), [presentador](bocetos/presentador/README.md), [plató](bocetos/plato/README.md) y [recursos](bocetos/recursos/README.md). Los ZIP están reunidos en [paquetes](bocetos/paquetes/README.md).

La referencia actual para futuras piezas del jugador es la [base completa en T-pose y A-pose](bocetos/player/archivo/diseno-modular/base/README.md), sin ropa, pelo ni zapatos. El antiguo despiece se retiró.

## Cómo mantenerlos

- Una regla nueva se registra primero en el GDD y después en los documentos afectados.
- **Confirmado** significa acordado en el GDD; **propuesta** significa por validar; **pendiente** significa sin resolver.
- Motor (Unity 6 + URP), cámara en tercera persona, red (Netcode for GameObjects + Steam) y voz (Steam, enrutada por el host) están confirmados en el GDD. Otros detalles nuevos de producción son propuestas.
- El alcance de salida conserva cultura general, tiempo límite por pregunta (configurable) y eliminación hasta la siguiente partida.
- No ampliar el prototipo con funciones futuras para completar una lista.

## Pendientes que afectan a varias áreas

Prueba temprana del transporte de Steam y de la voz, reparto de comodines, capital y denominaciones, desconexiones y validación del ranking. Consultar [DESARROLLO.md](DESARROLLO.md).
