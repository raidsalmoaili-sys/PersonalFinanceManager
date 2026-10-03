(function () {
  "use strict";

  function initThemeToggle() {
    var btn = document.getElementById("themeToggle");
    if (!btn) return;

    var label = document.getElementById("themeToggleLabel");

    function sync() {
      var isDark = document.documentElement.classList.contains("dark");
      if (label) label.textContent = isDark ? "الوضع النهاري" : "الوضع الليلي";
    }

    sync();

    btn.addEventListener("click", function () {
      var isDark = document.documentElement.classList.toggle("dark");
      try { localStorage.setItem("pfm-theme", isDark ? "dark" : "light"); } catch (e) { }
      sync();
    });
  }

  function initFontSelect() {
    var select = document.getElementById("fontSelect");
    if (!select) return;

    var current = "plex";
    try {
      var saved = localStorage.getItem("pfm-font");
      if (saved) current = saved;
    } catch (e) { }
    select.value = current;

    select.addEventListener("change", function () {
      document.documentElement.classList.remove("font-cairo", "font-tajawal");
      if (select.value === "cairo") document.documentElement.classList.add("font-cairo");
      else if (select.value === "tajawal") document.documentElement.classList.add("font-tajawal");
      try { localStorage.setItem("pfm-font", select.value); } catch (e) { }
    });
  }

  function initSidebarToggle() {
    var btn = document.getElementById("sidebarToggle");
    var sidebar = document.getElementById("sidebar");
    var icon = document.getElementById("sidebarToggleIcon");
    if (!btn || !sidebar) return;

    function applyCollapsed(collapsed) {
      sidebar.style.width = collapsed ? "66px" : "238px";
      if (icon) icon.textContent = collapsed ? "»" : "«";
      document.querySelectorAll(".sidebar-label").forEach(function (el) {
        el.classList.toggle("hidden", collapsed);
      });
    }

    var collapsed = false;
    try { collapsed = localStorage.getItem("pfm-sidebar") === "collapsed"; } catch (e) { }
    applyCollapsed(collapsed);

    btn.addEventListener("click", function () {
      collapsed = !collapsed;
      applyCollapsed(collapsed);
      try { localStorage.setItem("pfm-sidebar", collapsed ? "collapsed" : "expanded"); } catch (e) { }
    });
  }

  function initAuthTabs() {
    var tabButtons = document.querySelectorAll("[data-auth-tab]");
    if (!tabButtons.length) return;

    var panels = {
      login: document.getElementById("authPanelLogin"),
      register: document.getElementById("authPanelRegister")
    };

    function activate(tab) {
      tabButtons.forEach(function (b) {
        b.classList.toggle("auth-tab-active", b.dataset.authTab === tab);
      });
      if (panels.login) panels.login.classList.toggle("hidden", tab !== "login");
      if (panels.register) panels.register.classList.toggle("hidden", tab !== "register");
    }

    tabButtons.forEach(function (b) {
      b.addEventListener("click", function () { activate(b.dataset.authTab); });
    });

    if (window.pfmAuthActiveTab) activate(window.pfmAuthActiveTab);
  }

  function initModal(modalId, openIds, closeIds) {
    var modal = document.getElementById(modalId);
    if (!modal) return;

    function open() {
      modal.classList.remove("hidden");
      modal.classList.add("flex");
    }
    function close() {
      modal.classList.add("hidden");
      modal.classList.remove("flex");
    }

    openIds.forEach(function (id) {
      var btn = document.getElementById(id);
      if (btn) btn.addEventListener("click", open);
    });
    closeIds.forEach(function (id) {
      var btn = document.getElementById(id);
      if (btn) btn.addEventListener("click", close);
    });

    modal.addEventListener("click", function (e) {
      if (e.target === modal) close();
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && !modal.classList.contains("hidden")) close();
    });
  }

  function initCreatePlanModal() {
    initModal("createPlanModal", ["openCreatePlanBtn", "openCreatePlanBtnSidebar"], ["closeCreatePlanBtn", "cancelCreatePlanBtn"]);
    initModal("editPlanModal", ["openEditPlanBtn"], ["closeEditPlanBtn", "cancelEditPlanBtn"]);
  }

  // كل نموذج فيه [data-plan-form] يدير أزرار نوع الخطة ومنتقي اللون بشكل مستقل
  // عن أي نموذج آخر بنفس الصفحة (مثلاً نافذة "إضافة خطة" و"تعديل خطة" معاً).
  function initPlanTypeAndColorPickers() {
    document.querySelectorAll("[data-plan-form]").forEach(function (form) {
      var options = form.querySelectorAll(".plan-type-option");
      var swatches = form.querySelectorAll(".color-swatch");
      var hiddenColor = form.querySelector(".color-hex-input");

      function refreshTypeOptions() {
        options.forEach(function (span) {
          var input = span.previousElementSibling;
          var checked = input && input.checked;
          span.style.backgroundColor = checked ? span.dataset.color : "";
          span.style.color = checked ? "#fff" : "var(--ink2)";
        });
      }

      function refreshSwatches() {
        if (!hiddenColor) return;
        swatches.forEach(function (s) {
          s.classList.toggle("color-swatch-selected", s.dataset.color.toLowerCase() === hiddenColor.value.toLowerCase());
        });
      }

      options.forEach(function (span) {
        var input = span.previousElementSibling;
        if (!input) return;
        input.addEventListener("change", function () {
          refreshTypeOptions();
          if (hiddenColor && span.dataset.color) {
            hiddenColor.value = span.dataset.color;
            refreshSwatches();
          }
        });
      });

      swatches.forEach(function (s) {
        s.addEventListener("click", function () {
          if (!hiddenColor) return;
          hiddenColor.value = s.dataset.color;
          refreshSwatches();
        });
      });

      refreshTypeOptions();
      refreshSwatches();
    });
  }

  function initChartToggle() {
    var barBtn = document.getElementById("chartTypeBar");
    var areaBtn = document.getElementById("chartTypeArea");
    var barChart = document.getElementById("chartBar");
    var areaChart = document.getElementById("chartArea");
    if (!barBtn || !areaBtn || !barChart || !areaChart) return;

    barBtn.addEventListener("click", function () {
      barChart.classList.remove("hidden");
      areaChart.classList.add("hidden");
      barBtn.classList.add("chart-toggle-active");
      areaBtn.classList.remove("chart-toggle-active");
    });

    areaBtn.addEventListener("click", function () {
      areaChart.classList.remove("hidden");
      barChart.classList.add("hidden");
      areaBtn.classList.add("chart-toggle-active");
      barBtn.classList.remove("chart-toggle-active");
    });
  }

  function initDoubleConfirmDelete() {
    var buttons = document.querySelectorAll("[data-confirm-delete]");
    buttons.forEach(function (btn) {
      var armed = false;
      var timer = null;
      var originalText = btn.textContent;
      var confirmText = btn.getAttribute("data-confirm-delete") || "تأكيد الحذف؟";

      btn.addEventListener("click", function (e) {
        if (!armed) {
          e.preventDefault();
          armed = true;
          btn.textContent = confirmText;
          btn.classList.add("ring-2");
          timer = setTimeout(function () {
            armed = false;
            btn.textContent = originalText;
            btn.classList.remove("ring-2");
          }, 3000);
        } else {
          clearTimeout(timer);
        }
      });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initThemeToggle();
    initFontSelect();
    initSidebarToggle();
    initAuthTabs();
    initCreatePlanModal();
    initPlanTypeAndColorPickers();
    initChartToggle();
    initDoubleConfirmDelete();
  });
})();
