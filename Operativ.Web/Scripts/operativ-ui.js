(function () {
    "use strict";

    function alternarMenuUsuario(evento) {
        evento.stopPropagation();

        var boton = evento.currentTarget;
        var dropdown = document.getElementById("menuUsuarioDropdown");

        if (!dropdown) {
            return;
        }

        var abierto = dropdown.classList.toggle("activo");
        boton.setAttribute("aria-expanded", abierto ? "true" : "false");
    }

    function cerrarMenuUsuario() {
        var dropdown = document.getElementById("menuUsuarioDropdown");
        var boton = document.getElementById("btnMenuUsuario");

        if (dropdown) {
            dropdown.classList.remove("activo");
        }

        if (boton) {
            boton.setAttribute("aria-expanded", "false");
        }
    }

    function abrirModalCambiarClave(evento) {
        if (evento) {
            evento.preventDefault();
        }

        cerrarMenuUsuario();

        var modal = document.getElementById("modalCambiarClave");

        if (modal) {
            modal.classList.add("activo");
        }
    }

    function cerrarModalCambiarClave(evento) {
        if (evento) {
            evento.preventDefault();
        }

        var modal = document.getElementById("modalCambiarClave");

        if (modal) {
            modal.classList.remove("activo");
        }
    }

    function clicOverlayModal(evento) {
        if (evento.target === evento.currentTarget) {
            cerrarModalCambiarClave(evento);
        }
    }

    function alternarVisibilidadContrasena(boton) {
        var campo = boton.parentElement.querySelector(".campo-contrasena");

        if (!campo) {
            return;
        }

        campo.type = campo.type === "password" ? "text" : "password";
    }

    document.addEventListener("click", function (evento) {
        var menu = document.getElementById("menuUsuario");
        var dropdown = document.getElementById("menuUsuarioDropdown");

        if (dropdown && dropdown.classList.contains("activo") && menu && !menu.contains(evento.target)) {
            cerrarMenuUsuario();
        }
    });

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Escape") {
            cerrarMenuUsuario();
            cerrarModalCambiarClave();
        }
    });

    function crearEncabezadoCategoriaPermiso(categoria, titulo, expandidoPorDefecto) {
        var div = document.createElement("div");
        div.className = "grupo-permiso-encabezado" + (expandidoPorDefecto ? " expandido" : "");
        div.setAttribute("data-categoria", categoria);
        div.setAttribute("onclick", "Operativ.alternarGrupoPermiso(this)");

        var izquierda = document.createElement("div");
        izquierda.className = "grupo-permiso-encabezado-izquierda";
        izquierda.innerHTML = '<svg class="grupo-permiso-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"></polyline></svg>'
            + '<span class="grupo-permiso-titulo"></span><span class="grupo-permiso-contador"></span>';
        izquierda.querySelector(".grupo-permiso-titulo").textContent = titulo;

        var derecha = document.createElement("div");
        derecha.className = "grupo-permiso-encabezado-derecha";
        derecha.innerHTML = '<span class="grupo-permiso-seleccionados"></span>'
            + '<button type="button" class="grupo-permiso-alternar" onclick="Operativ.alternarSeleccionGrupo(this, event)">'
            + '<svg class="icono-grupo-mas" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg>'
            + '<svg class="icono-grupo-menos" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"></line></svg>'
            + '</button>';

        div.appendChild(izquierda);
        div.appendChild(derecha);
        return div;
    }

    function crearFilaPermiso(elemento) {
        var textos = window.OperativTextosPermisos || {};

        elemento.input.classList.add("chk-permiso");

        var fila = document.createElement("div");
        fila.className = "fila-permiso";
        fila.setAttribute("data-categoria", elemento.categoria);
        fila.setAttribute("data-nombre", elemento.texto.toLowerCase());
        fila.setAttribute("data-descripcion", elemento.descripcion.toLowerCase());
        fila.setAttribute("data-familias", elemento.familias);

        var envoltorioCheckbox = document.createElement("span");
        envoltorioCheckbox.className = "fila-permiso-checkbox";
        envoltorioCheckbox.appendChild(elemento.input);

        var textoDiv = document.createElement("div");
        textoDiv.className = "fila-permiso-texto";

        var spanNombre = document.createElement("span");
        spanNombre.className = "fila-permiso-nombre";
        spanNombre.textContent = elemento.texto;

        var spanDescripcion = document.createElement("span");
        spanDescripcion.className = "fila-permiso-descripcion";
        spanDescripcion.textContent = elemento.descripcion;

        textoDiv.appendChild(spanNombre);
        textoDiv.appendChild(spanDescripcion);

        var spanBadge = document.createElement("span");
        spanBadge.className = "badge-permiso " + (elemento.heredada ? "badge-permiso-heredado" : "badge-permiso-no-heredado");
        spanBadge.textContent = elemento.heredada ? textos.heredada : textos.noHeredada;

        fila.appendChild(envoltorioCheckbox);
        fila.appendChild(textoDiv);
        fila.appendChild(spanBadge);

        return fila;
    }

    function inicializarPermisos() {
        var contenedor = document.getElementById("listaPermisos");

        if (!contenedor) {
            return;
        }

        var envoltoriosOriginales = contenedor.querySelectorAll("span.chk-permiso");

        if (envoltoriosOriginales.length === 0) {
            return;
        }

        var elementos = [];

        for (var i = 0; i < envoltoriosOriginales.length; i++) {
            var envoltorio = envoltoriosOriginales[i];
            var input = envoltorio.querySelector("input[type=checkbox]");
            var etiqueta = envoltorio.querySelector("label");

            if (!input) {
                continue;
            }

            elementos.push({
                input: input,
                texto: etiqueta ? etiqueta.textContent : "",
                categoria: envoltorio.getAttribute("data-categoria"),
                categoriaTitulo: envoltorio.getAttribute("data-categoria-titulo"),
                descripcion: envoltorio.getAttribute("data-descripcion") || "",
                heredada: envoltorio.getAttribute("data-heredada") === "1",
                familias: envoltorio.getAttribute("data-familias") || ""
            });
        }

        contenedor.innerHTML = "";

        var categoriaActual = null;
        var primerEncabezado = true;

        for (var j = 0; j < elementos.length; j++) {
            var elemento = elementos[j];

            if (elemento.categoria !== categoriaActual) {
                categoriaActual = elemento.categoria;
                contenedor.appendChild(crearEncabezadoCategoriaPermiso(elemento.categoria, elemento.categoriaTitulo, primerEncabezado));
                primerEncabezado = false;
            }

            contenedor.appendChild(crearFilaPermiso(elemento));
        }

        actualizarContadoresPermisos();
    }

    function actualizarContadoresPermisos() {
        var contenedor = document.getElementById("listaPermisos");

        if (!contenedor) {
            return;
        }

        var textos = window.OperativTextosPermisos || {};
        var encabezados = contenedor.querySelectorAll(".grupo-permiso-encabezado");

        for (var i = 0; i < encabezados.length; i++) {
            var encabezado = encabezados[i];
            var categoria = encabezado.getAttribute("data-categoria");
            var filas = contenedor.querySelectorAll('.fila-permiso[data-categoria="' + categoria + '"]');

            var seleccionados = 0;

            for (var j = 0; j < filas.length; j++) {
                var chk = filas[j].querySelector("input.chk-permiso");

                if (chk && chk.checked) {
                    seleccionados++;
                }
            }

            var spanContador = encabezado.querySelector(".grupo-permiso-contador");

            if (spanContador && textos.formatoCantidad) {
                spanContador.textContent = textos.formatoCantidad.replace("{0}", filas.length);
            }

            var spanSeleccionados = encabezado.querySelector(".grupo-permiso-seleccionados");

            if (spanSeleccionados && textos.formatoSeleccionados) {
                spanSeleccionados.textContent = textos.formatoSeleccionados.replace("{0}", seleccionados);
            }

            var botonAlternar = encabezado.querySelector(".grupo-permiso-alternar");

            if (botonAlternar) {
                botonAlternar.classList.toggle("grupo-permiso-alternar-activo", seleccionados > 0);
            }
        }
    }

    function alternarGrupoPermiso(encabezado) {
        var categoria = encabezado.getAttribute("data-categoria");
        var expandir = !encabezado.classList.contains("expandido");
        encabezado.classList.toggle("expandido", expandir);

        var filas = document.querySelectorAll('.fila-permiso[data-categoria="' + categoria + '"]');

        for (var i = 0; i < filas.length; i++) {
            filas[i].classList.toggle("oculto-categoria", !expandir);
        }
    }

    function alternarSeleccionGrupo(boton, evento) {
        evento.stopPropagation();

        var encabezado = boton.closest(".grupo-permiso-encabezado");
        var categoria = encabezado ? encabezado.getAttribute("data-categoria") : null;

        if (!categoria) {
            return;
        }

        var checkboxes = document.querySelectorAll('.fila-permiso[data-categoria="' + categoria + '"] input.chk-permiso');
        var hayAlgunoSeleccionado = false;

        for (var i = 0; i < checkboxes.length; i++) {
            if (checkboxes[i].checked) {
                hayAlgunoSeleccionado = true;
                break;
            }
        }

        var nuevoEstado = !hayAlgunoSeleccionado;

        for (var j = 0; j < checkboxes.length; j++) {
            if (!checkboxes[j].disabled) {
                checkboxes[j].checked = nuevoEstado;
            }
        }

        actualizarContadoresPermisos();
    }

    function aplicarSeleccionMasivaPermisos(marcar) {
        var contenedor = document.getElementById("listaPermisos");

        if (!contenedor) {
            return;
        }

        var filas = contenedor.querySelectorAll(".fila-permiso");

        for (var i = 0; i < filas.length; i++) {
            var fila = filas[i];

            if (fila.classList.contains("oculto-filtro")) {
                continue;
            }

            var chk = fila.querySelector("input.chk-permiso");

            if (chk && !chk.disabled) {
                chk.checked = marcar;
            }
        }

        actualizarContadoresPermisos();
    }

    function seleccionarTodosPermisos() {
        aplicarSeleccionMasivaPermisos(true);
    }

    function quitarTodosPermisos() {
        aplicarSeleccionMasivaPermisos(false);
    }

    function filtrarPermisos() {
        var contenedor = document.getElementById("listaPermisos");

        if (!contenedor) {
            return;
        }

        var campoBusqueda = document.querySelector(".campo-busqueda-permisos-input");
        var texto = campoBusqueda ? campoBusqueda.value.toLowerCase().trim() : "";

        var campoFamilia = document.querySelector(".select-filtro-familia-permisos");
        var idFamilia = campoFamilia ? campoFamilia.value : "";

        var filas = contenedor.querySelectorAll(".fila-permiso");
        var visiblesPorCategoria = {};

        for (var i = 0; i < filas.length; i++) {
            var fila = filas[i];

            var coincideTexto = texto === ""
                || fila.getAttribute("data-nombre").indexOf(texto) !== -1
                || fila.getAttribute("data-descripcion").indexOf(texto) !== -1;

            var familias = fila.getAttribute("data-familias") || "";
            var coincideFamilia = idFamilia === ""
                || (" " + familias.split(",").join(" ") + " ").indexOf(" " + idFamilia + " ") !== -1;

            var visible = coincideTexto && coincideFamilia;
            fila.classList.toggle("oculto-filtro", !visible);

            var categoria = fila.getAttribute("data-categoria");
            visiblesPorCategoria[categoria] = (visiblesPorCategoria[categoria] || 0) + (visible ? 1 : 0);
        }

        var encabezados = contenedor.querySelectorAll(".grupo-permiso-encabezado");

        for (var j = 0; j < encabezados.length; j++) {
            var encabezado = encabezados[j];
            var cat = encabezado.getAttribute("data-categoria");
            var hayVisibles = (visiblesPorCategoria[cat] || 0) > 0;
            encabezado.classList.toggle("oculto-filtro", !hayVisibles);
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        inicializarPermisos();
    });

    document.addEventListener("input", function (evento) {
        if (evento.target && evento.target.classList && evento.target.classList.contains("campo-busqueda-permisos-input")) {
            filtrarPermisos();
        }
    });

    document.addEventListener("change", function (evento) {
        if (!evento.target || !evento.target.classList) {
            return;
        }

        if (evento.target.classList.contains("select-filtro-familia-permisos")) {
            filtrarPermisos();
        } else if (evento.target.classList.contains("chk-permiso")) {
            actualizarContadoresPermisos();
        }
    });

    window.Operativ = window.Operativ || {};
    window.Operativ.alternarMenuUsuario = alternarMenuUsuario;
    window.Operativ.abrirModalCambiarClave = abrirModalCambiarClave;
    window.Operativ.cerrarModalCambiarClave = cerrarModalCambiarClave;
    window.Operativ.clicOverlayModal = clicOverlayModal;
    window.Operativ.alternarVisibilidadContrasena = alternarVisibilidadContrasena;
    window.Operativ.alternarGrupoPermiso = alternarGrupoPermiso;
    window.Operativ.alternarSeleccionGrupo = alternarSeleccionGrupo;
    window.Operativ.seleccionarTodosPermisos = seleccionarTodosPermisos;
    window.Operativ.quitarTodosPermisos = quitarTodosPermisos;
})();
