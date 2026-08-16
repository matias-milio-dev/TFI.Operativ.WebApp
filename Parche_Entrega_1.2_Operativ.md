# Parche Entrega 1.2 — Ajustes visuales de notificaciones — Plataforma Operativ

Parche corto sobre lo ya implementado (Entrega 1 + Parche 1.1). Corrige dos detalles vistos al probar la app en el login. No cambia funcionalidad ni reglas de negocio: solo presentación del control `Notificaciones.ascx`.

---

## 1. Mostrar solo el mensaje, sin el código `ERRXX -`

- En pantalla, `Notificaciones.ascx` debe mostrar **únicamente el mensaje** (ej.: `La contraseña ingresada es incorrecta (Quedan 2 intentos)`), sin el prefijo `ERRXX -`.
- Esto es solo un cambio de **presentación**: el código de error (`ERR02`, etc.) se sigue manteniendo internamente en `ErroresHandler`/`OperativException` (va a hacer falta más adelante para Bitácora y trazabilidad), simplemente ya no se concatena al string que se muestra en el control.
- `ErroresHandler` debe seguir exponiendo el código y el mensaje por separado (dos propiedades/métodos distintos); es `Notificaciones.ascx` quien decide renderizar solo el mensaje. No borrar la lógica del código, solo dejar de imprimirlo en pantalla.

## 2. Tamaño y ancho del cartel de notificación

Según la captura: el cartel de error queda con letra grande y más ancho que la tarjeta de login, desbordando hacia los costados.

- Reducir el tamaño de fuente del mensaje (de aprox. `1.1rem` a algo como `0.9rem`).
- El contenedor de `Notificaciones.ascx` debe tener el **mismo ancho máximo que la tarjeta del formulario** que está debajo (la card blanca de Login, Recuperar Contraseña, etc.), no un ancho fijo mayor. Usar el mismo `max-width` (o la misma variable/clase CSS) que usa el contenedor de la tarjeta del formulario, y centrarlo igual que esa tarjeta.
- El texto debe hacer wrap dentro del cartel (`word-wrap`/`overflow-wrap: break-word`) para mensajes largos, en vez de forzar el ancho del contenedor hacia afuera.
- Este ajuste aplica a `Notificaciones.ascx` en general (no solo en Login), para que se vea consistente en todas las pantallas donde se use.

## 3. Criterio de "terminado"

- Provocar el error de contraseña incorrecta en Login: el cartel muestra solo `La contraseña ingresada es incorrecta (Quedan 2 intentos)`, sin `ERR02 -`.
- El ancho del cartel de notificación coincide visualmente con el ancho de la tarjeta de Login (no se sale de los bordes de la tarjeta en ningún lado).
- La fuente del mensaje es visiblemente más chica que la del título "Operativ", consistente con el resto de los textos de la tarjeta (labels, botón).

---

*Este parche se aplica sobre `Notificaciones.ascx` y su hoja de estilos. No modifica `ErroresHandler`, `OperativException` ni ninguna regla de negocio o de acceso a datos ya definida en los documentos anteriores.*
