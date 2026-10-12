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

    function imprimirDocumento() {
        if (document.fonts && document.fonts.ready) {
            document.fonts.ready.then(function () {
                window.print();
            });
            return;
        }

        window.print();
    }

    function imprimirDocumentoAlCargar() {
        window.addEventListener("load", imprimirDocumento);
    }

    var PATRON_VISA = /^4/;
    var PATRON_AMEX = /^3[47]/;
    var PATRON_MASTERCARD = /^(5[1-5]|2(2[2-9][1-9]|2[3-9]\d|[3-6]\d\d|7[01]\d|720))/;
    var PATRON_TITULAR = /^[A-Za-zÀ-ɏ][A-Za-zÀ-ɏ\s'.\-]*$/;
    var MARCA_VISA = "visa";
    var MARCA_MASTERCARD = "mastercard";
    var MARCA_AMEX = "amex";

    function soloDigitos(texto) {
        return (texto || "").replace(/\D/g, "");
    }

    function detectarMarcaTarjeta(numero) {
        var digitos = soloDigitos(numero);

        if (PATRON_VISA.test(digitos)) {
            return MARCA_VISA;
        }

        if (PATRON_AMEX.test(digitos)) {
            return MARCA_AMEX;
        }

        if (PATRON_MASTERCARD.test(digitos)) {
            return MARCA_MASTERCARD;
        }

        return "";
    }

    function cumpleLuhn(digitos) {
        var suma = 0;
        var duplicar = false;

        for (var indice = digitos.length - 1; indice >= 0; indice--) {
            var digito = parseInt(digitos.charAt(indice), 10);

            if (duplicar) {
                digito *= 2;

                if (digito > 9) {
                    digito -= 9;
                }
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return suma % 10 === 0;
    }

    function esNumeroTarjetaValido(numero) {
        var digitos = soloDigitos(numero);
        var marca = detectarMarcaTarjeta(digitos);

        if (!marca) {
            return false;
        }

        var longitudValida = marca === MARCA_AMEX ? digitos.length === 15 : (digitos.length === 16 || (marca === MARCA_VISA && digitos.length === 13));

        return longitudValida && cumpleLuhn(digitos);
    }

    function esTitularTarjetaValido(titular) {
        var normalizado = (titular || "").trim();

        return normalizado.length >= 3 && normalizado.length <= 60 && PATRON_TITULAR.test(normalizado);
    }

    function esVencimientoTarjetaValido(vencimiento) {
        var coincidencia = /^(0[1-9]|1[0-2])\/(\d{2})$/.exec((vencimiento || "").trim());

        if (!coincidencia) {
            return false;
        }

        var hoy = new Date();
        var mes = parseInt(coincidencia[1], 10);
        var anio = 2000 + parseInt(coincidencia[2], 10);
        var ultimoDia = new Date(anio, mes, 0, 23, 59, 59);

        return ultimoDia >= hoy && anio <= hoy.getFullYear() + 20;
    }

    function esCodigoSeguridadValido(codigo, numero) {
        var longitud = detectarMarcaTarjeta(numero) === MARCA_AMEX ? 4 : 3;

        return new RegExp("^\\d{" + longitud + "}$").test((codigo || "").trim());
    }

    function esPagoConTarjeta() {
        var medioPago = document.querySelector("[data-medio-pago]");

        return !medioPago || medioPago.value === "Tarjeta";
    }

    function valorCampoTarjeta(nombre) {
        var campo = document.querySelector("[data-campo='" + nombre + "']");

        return campo ? campo.value : "";
    }

    function validarNumeroTarjeta(origen, argumentos) {
        argumentos.IsValid = !esPagoConTarjeta() || esNumeroTarjetaValido(argumentos.Value);
    }

    function validarTitularTarjeta(origen, argumentos) {
        argumentos.IsValid = !esPagoConTarjeta() || esTitularTarjetaValido(argumentos.Value);
    }

    function validarVencimientoTarjeta(origen, argumentos) {
        argumentos.IsValid = !esPagoConTarjeta() || esVencimientoTarjetaValido(argumentos.Value);
    }

    function validarCodigoSeguridad(origen, argumentos) {
        argumentos.IsValid = !esPagoConTarjeta() || esCodigoSeguridadValido(argumentos.Value, valorCampoTarjeta("numero"));
    }

    function formatearNumeroTarjeta(numero) {
        var digitos = soloDigitos(numero);

        if (detectarMarcaTarjeta(digitos) === MARCA_AMEX) {
            digitos = digitos.substring(0, 15);

            return [digitos.substring(0, 4), digitos.substring(4, 10), digitos.substring(10, 15)].filter(Boolean).join(" ");
        }

        digitos = digitos.substring(0, 16);

        return digitos.replace(/(\d{4})(?=\d)/g, "$1 ");
    }

    function formatearVencimientoTarjeta(vencimiento) {
        var digitos = soloDigitos(vencimiento).substring(0, 4);

        if (digitos.length > 0 && parseInt(digitos.charAt(0), 10) > 1) {
            digitos = "0" + digitos;
        }

        return digitos.length > 2 ? digitos.substring(0, 2) + "/" + digitos.substring(2, 4) : digitos;
    }

    function inicializarPagoTarjeta() {
        var panel = document.querySelector("[data-pago-tarjeta]");
        var medioPago = document.querySelector("[data-medio-pago]");

        if (!panel || !medioPago) {
            return;
        }

        var campoNumero = panel.querySelector("[data-campo='numero']");
        var campoTitular = panel.querySelector("[data-campo='titular']");
        var campoVencimiento = panel.querySelector("[data-campo='vencimiento']");
        var campoCodigo = panel.querySelector("[data-campo='codigo']");
        var vistaNumero = panel.querySelector("[data-vista='numero']");
        var vistaTitular = panel.querySelector("[data-vista='titular']");
        var vistaVencimiento = panel.querySelector("[data-vista='vencimiento']");
        var textoTitularInicial = vistaTitular.textContent;
        var textoVencimientoInicial = vistaVencimiento.textContent;
        var textoNumeroInicial = vistaNumero.textContent;

        function actualizarVisibilidad() {
            panel.style.display = medioPago.value === "Tarjeta" ? "" : "none";
        }

        function actualizarNumero() {
            var marca = detectarMarcaTarjeta(campoNumero.value);
            var formateado = formatearNumeroTarjeta(campoNumero.value);

            if (formateado !== campoNumero.value) {
                campoNumero.value = formateado;
            }

            panel.setAttribute("data-marca", marca);
            campoCodigo.maxLength = marca === MARCA_AMEX ? 4 : 3;
            campoCodigo.value = campoCodigo.value.substring(0, campoCodigo.maxLength);
            vistaNumero.textContent = formateado || textoNumeroInicial;
        }

        function actualizarTitular() {
            vistaTitular.textContent = campoTitular.value.trim().toUpperCase() || textoTitularInicial;
        }

        function actualizarVencimiento() {
            var formateado = formatearVencimientoTarjeta(campoVencimiento.value);

            if (formateado !== campoVencimiento.value) {
                campoVencimiento.value = formateado;
            }

            vistaVencimiento.textContent = formateado || textoVencimientoInicial;
        }

        function actualizarCodigo() {
            var soloNumeros = soloDigitos(campoCodigo.value);

            if (soloNumeros !== campoCodigo.value) {
                campoCodigo.value = soloNumeros;
            }
        }

        medioPago.addEventListener("change", actualizarVisibilidad);
        campoNumero.addEventListener("input", actualizarNumero);
        campoTitular.addEventListener("input", actualizarTitular);
        campoVencimiento.addEventListener("input", actualizarVencimiento);
        campoCodigo.addEventListener("input", actualizarCodigo);

        actualizarVisibilidad();
        actualizarNumero();
        actualizarTitular();
        actualizarVencimiento();
    }

    document.addEventListener("DOMContentLoaded", actualizarContadoresCaracteres);
    document.addEventListener("DOMContentLoaded", inicializarPagoTarjeta);
    window.Operativ = window.Operativ || {};
    window.Operativ.imprimirDocumento = imprimirDocumento;
    window.Operativ.imprimirDocumentoAlCargar = imprimirDocumentoAlCargar;
    window.Operativ.alternarMenuUsuario = alternarMenuUsuario;
    window.Operativ.abrirModalCambiarClave = abrirModalCambiarClave;
    window.Operativ.cerrarModalCambiarClave = cerrarModalCambiarClave;
    window.Operativ.clicOverlayModal = clicOverlayModal;
    window.Operativ.alternarVisibilidadContrasena = alternarVisibilidadContrasena;
    window.Operativ.alternarGrupoPermiso = alternarGrupoPermiso;
    window.Operativ.alternarSeleccionGrupo = alternarSeleccionGrupo;
    window.Operativ.seleccionarTodosPermisos = seleccionarTodosPermisos;
    window.Operativ.quitarTodosPermisos = quitarTodosPermisos;
    window.Operativ.validarNumeroTarjeta = validarNumeroTarjeta;
    window.Operativ.validarTitularTarjeta = validarTitularTarjeta;
    window.Operativ.validarVencimientoTarjeta = validarVencimientoTarjeta;
    window.Operativ.validarCodigoSeguridad = validarCodigoSeguridad;
})();
