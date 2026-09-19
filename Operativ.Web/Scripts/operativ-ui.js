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

    function obtenerCheckboxesDeGrupo(grupo) {
        return grupo.querySelectorAll(".fila-permiso input[type=checkbox]");
    }

    function actualizarContadoresPermisos() {
        var grupos = document.querySelectorAll(".grupo-permiso");

        for (var i = 0; i < grupos.length; i++) {
            var grupo = grupos[i];
            var checkboxes = obtenerCheckboxesDeGrupo(grupo);
            var seleccionados = 0;

            for (var j = 0; j < checkboxes.length; j++) {
                if (checkboxes[j].checked) {
                    seleccionados++;
                }
            }

            var spanSeleccionados = grupo.querySelector(".grupo-permiso-seleccionados");

            if (spanSeleccionados) {
                var formato = spanSeleccionados.getAttribute("data-formato") || "{0}";
                var formatoSingular = spanSeleccionados.getAttribute("data-formato-singular");

                if (seleccionados === 1 && formatoSingular) {
                    formato = formatoSingular;
                }

                spanSeleccionados.textContent = formato.replace("{0}", seleccionados);
            }

            var botonAlternar = grupo.querySelector(".grupo-permiso-alternar");

            if (botonAlternar) {
                botonAlternar.classList.toggle("grupo-permiso-alternar-activo", seleccionados > 0);
            }
        }
    }

    function alternarGrupoPermiso(encabezado) {
        var grupo = encabezado.closest(".grupo-permiso");

        if (grupo) {
            grupo.classList.toggle("expandido");
        }
    }

    function alternarSeleccionGrupo(boton, evento) {
        evento.stopPropagation();

        var grupo = boton.closest(".grupo-permiso");

        if (!grupo) {
            return;
        }

        var checkboxes = obtenerCheckboxesDeGrupo(grupo);
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
        var filas = document.querySelectorAll(".fila-permiso");

        for (var i = 0; i < filas.length; i++) {
            var fila = filas[i];

            if (fila.classList.contains("oculto-filtro")) {
                continue;
            }

            var chk = fila.querySelector("input[type=checkbox]");

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

        var grupos = contenedor.querySelectorAll(".grupo-permiso");

        for (var i = 0; i < grupos.length; i++) {
            var grupo = grupos[i];
            var filas = grupo.querySelectorAll(".fila-permiso");
            var visiblesEnGrupo = 0;

            for (var j = 0; j < filas.length; j++) {
                var fila = filas[j];

                var coincideTexto = texto === ""
                    || fila.getAttribute("data-nombre").indexOf(texto) !== -1
                    || fila.getAttribute("data-descripcion").indexOf(texto) !== -1;

                var familias = fila.getAttribute("data-familias") || "";
                var coincideFamilia = idFamilia === ""
                    || (" " + familias.split(",").join(" ") + " ").indexOf(" " + idFamilia + " ") !== -1;

                var visible = coincideTexto && coincideFamilia;
                fila.classList.toggle("oculto-filtro", !visible);

                if (visible) {
                    visiblesEnGrupo++;
                }
            }

            grupo.classList.toggle("oculto-filtro", visiblesEnGrupo === 0);

            if (visiblesEnGrupo > 0 && (texto !== "" || idFamilia !== "")) {
                grupo.classList.add("expandido");
            }
        }
    }

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
        } else if (evento.target.type === "checkbox" && evento.target.closest(".fila-permiso")) {
            actualizarContadoresPermisos();
        }
    });

    function filtrarProgramas() {
        var campo = document.querySelector(".campo-busqueda-programas-input");
        var lista = document.querySelector(".lista-programas");

        if (!campo || !lista) {
            return;
        }

        var texto = campo.value.toLowerCase().trim();
        var items = lista.querySelectorAll("li");
        var visibles = 0;

        for (var i = 0; i < items.length; i++) {
            var visible = texto === "" || items[i].textContent.toLowerCase().indexOf(texto) !== -1;
            items[i].classList.toggle("oculto-filtro", !visible);

            if (visible) {
                visibles++;
            }
        }

        var sinResultados = document.querySelector(".sin-resultados-programas");

        if (sinResultados) {
            sinResultados.classList.toggle("oculto-filtro", visibles > 0);
        }
    }

    function actualizarContadoresCaracteres() {
        var contadores = document.querySelectorAll(".contador-caracteres");

        for (var i = 0; i < contadores.length; i++) {
            var contador = contadores[i];
            var campo = document.getElementById(contador.getAttribute("data-contador-para"));
            var maximo = parseInt(contador.getAttribute("data-maximo"), 10);

            if (campo) {
                campo.maxLength = maximo;
                contador.textContent = campo.value.length + "/" + maximo;
            }
        }
    }

    document.addEventListener("input", function (evento) {
        var destino = evento.target;

        if (!destino || !destino.classList) {
            return;
        }

        if (destino.classList.contains("campo-busqueda-programas-input")) {
            filtrarProgramas();
        }

        if (destino.tagName === "TEXTAREA") {
            actualizarContadoresCaracteres();
        }
    });

    document.addEventListener("keydown", function (evento) {
        if (evento.key === "Enter" && evento.target && evento.target.classList && evento.target.classList.contains("campo-busqueda-programas-input")) {
            evento.preventDefault();
        }
    });

    document.addEventListener("DOMContentLoaded", actualizarContadoresCaracteres);
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
