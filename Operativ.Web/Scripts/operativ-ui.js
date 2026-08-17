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

    window.Operativ = window.Operativ || {};
    window.Operativ.alternarMenuUsuario = alternarMenuUsuario;
    window.Operativ.abrirModalCambiarClave = abrirModalCambiarClave;
    window.Operativ.cerrarModalCambiarClave = cerrarModalCambiarClave;
    window.Operativ.clicOverlayModal = clicOverlayModal;
    window.Operativ.alternarVisibilidadContrasena = alternarVisibilidadContrasena;
})();
