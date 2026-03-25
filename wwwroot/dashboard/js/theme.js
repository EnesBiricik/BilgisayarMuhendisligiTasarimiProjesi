"use strict";

// Theme flags
let isDarkTheme = false;

// Initialize theme from localStorage on window load
window.addEventListener('load', function() {
    initializeThemeFromStorage();
});

function initializeThemeFromStorage() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        applyTheme(savedTheme);
    } else {
        // Set default theme based on system preference
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        const defaultTheme = prefersDark ? 'dark' : 'light';
        applyTheme(defaultTheme);
    }

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
            const newTheme = e.matches ? 'dark' : 'light';
            applyTheme(newTheme);
        }
    });
}

function applyTheme(theme) {
    // Save theme to localStorage
    localStorage.setItem('theme', theme);

    // Apply theme to body
    const body = document.getElementsByTagName('body')[0];
    body.setAttribute('data-pc-theme', theme);
    isDarkTheme = theme === 'dark';

    // Update logos based on theme
    if (isDarkTheme) {
        updateLogo('.pc-sidebar .m-header .logo-lg', '/dashboard/images/logo-white.png');
        updateLogo('.navbar-brand .logo-lg', '/dashboard/images/logo-white.png');
        updateLogo('.auth-main.v1 .auth-sidefooter img', '/dashboard/images/logo-white.png');
        updateLogo('.footer-top .footer-logo', '/dashboard/images/logo-white.png');
    } else {
        updateLogo('.pc-sidebar .m-header .logo-lg', '/dashboard/images/logo-dark.png');
        updateLogo('.navbar-brand .logo-lg', '/dashboard/images/logo-dark.png');
        updateLogo('.auth-main.v1 .auth-sidefooter img', '/dashboard/images/logo-dark.png');
        updateLogo('.footer-top .footer-logo', '/dashboard/images/logo-dark.png');
    }

    // Update theme buttons if they exist
    updateThemeButtons(theme);
}

function updateLogo(selector, src) {
    const logo = document.querySelector(selector);
    if (logo) {
        logo.setAttribute('src', src);
    }
}

function updateThemeButtons(theme) {
    const activeButton = document.querySelector('.theme-layout .btn.active');
    if (activeButton) {
        activeButton.classList.remove('active');
    }

    const newActiveButton = document.querySelector(`.theme-layout .btn[data-value="${theme === 'light'}"]`);
    if (newActiveButton) {
        newActiveButton.classList.add('active');
    }
}

// Theme button click handlers
document.addEventListener('DOMContentLoaded', function() {
    const themeButtons = document.querySelectorAll('.theme-layout .btn');
    themeButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.stopPropagation();
            const target = e.target.closest('.btn');
            if (!target) return;

            const isLight = target.getAttribute('data-value') === 'true';
            const newTheme = isLight ? 'light' : 'dark';
            applyTheme(newTheme);
        });
    });
});

function layout_theme_contrast_change(e) {
  var t = document.getElementsByTagName("body")[0],
    a = document.querySelector(".theme-contrast .btn.active"),
    t =
      (t.setAttribute("data-pc-theme_contrast", e),
      a && a.classList.remove("active"),
      document.querySelector(`.theme-contrast .btn[data-value='${e}']`));
  t && t.classList.add("active");
}
function layout_caption_change(e) {
  "true" == e
    ? (document
        .getElementsByTagName("body")[0]
        .setAttribute("data-pc-sidebar-caption", "true"),
      document.querySelector(".theme-nav-caption .btn.active") &&
        (document
          .querySelector(".theme-nav-caption .btn.active")
          .classList.remove("active"),
        document
          .querySelector(".theme-nav-caption .btn[data-value='true']")
          .classList.add("active")))
    : (document
        .getElementsByTagName("body")[0]
        .setAttribute("data-pc-sidebar-caption", "false"),
      document.querySelector(".theme-nav-caption .btn.active") &&
        (document
          .querySelector(".theme-nav-caption .btn.active")
          .classList.remove("active"),
        document
          .querySelector(".theme-nav-caption .btn[data-value='false']")
          .classList.add("active")));
}
function preset_change(e) {
  document.getElementsByTagName("body")[0].setAttribute("data-pc-preset", e),
    document.querySelector(".pct-offcanvas") &&
      (document
        .querySelector(".preset-color > a.active")
        .classList.remove("active"),
      document
        .querySelector(".preset-color > a[data-value='" + e + "']")
        .classList.add("active"));
}
function layout_rtl_change(e) {
  var t = document.getElementsByTagName("body")[0],
    a = document.getElementsByTagName("html")[0],
    o = document.querySelector(".theme-direction .btn.active");
  "true" === e
    ? ((rtl_flag = !0),
      t.setAttribute("data-pc-direction", "rtl"),
      a.setAttribute("dir", "rtl"),
      a.setAttribute("lang", "ar"),
      o &&
        (o.classList.remove("active"),
        document
          .querySelector(".theme-direction .btn[data-value='true']")
          .classList.add("acxtive")))
    : ((rtl_flag = !1),
      t.setAttribute("data-pc-direction", "ltr"),
      a.removeAttribute("dir"),
      a.removeAttribute("lang"),
      o &&
        (o.classList.remove("active"),
        document
          .querySelector(".theme-direction .btn[data-value='false']")
          .classList.add("active")));
}
function change_box_container(e) {
  var t,
    a,
    o = document.querySelector(".pc-content"),
    c = document.querySelector(".footer-wrapper");
  o &&
    c &&
    ("true" === e
      ? (o.classList.add("container"),
        c.classList.add("container"),
        c.classList.remove("container-fluid"),
        (t = document.querySelector(".theme-container .btn.active")) &&
          t.classList.remove("active"),
        (a = document.querySelector(
          '.theme-container .btn[data-value="true"]'
        )) && a.classList.add("active"))
      : (o.classList.remove("container"),
        c.classList.remove("container"),
        c.classList.add("container-fluid"),
        (t = document.querySelector(".theme-container .btn.active")) &&
          t.classList.remove("active"),
        (a = document.querySelector(
          '.theme-container .btn[data-value="false"]'
        )) && a.classList.add("active")));
}
document.addEventListener("DOMContentLoaded", function () {
  if (document.querySelectorAll(".preset-color"))
    for (
      var e = document.querySelectorAll(".preset-color > a"), t = 0;
      t < e.length;
      t++
    )
      e[t].addEventListener("click", function (e) {
        e = e.target;
        preset_change(
          (e = "SPAN" == e.tagName ? e.parentNode : e).getAttribute(
            "data-value"
          )
        );
      });
  document.querySelector(".pct-body") &&
    new SimpleBar(document.querySelector(".pct-body"));
  var a = document.querySelector("#layoutreset");
  a &&
    a.addEventListener("click", function (e) {
      localStorage.clear(),
        location.reload(),
        localStorage.setItem("layout", "vertical");
    });
});
