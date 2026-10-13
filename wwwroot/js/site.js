// ── Sidebar toggle + theme toggle ───────────────────────────────────────────
document.addEventListener("DOMContentLoaded", function () {
    // --- Sidebar (original logic unchanged) ---
    const sidebar = document.getElementById("appSidebar");
    const backdrop = document.getElementById("sidebarBackdrop");
    const openBtn = document.getElementById("sidebarToggle");
    const closeBtn = document.getElementById("sidebarClose");

    function openSidebar() {
        if (sidebar) sidebar.classList.add("show");
        if (backdrop) backdrop.classList.add("show");
    }

    function closeSidebar() {
        if (sidebar) sidebar.classList.remove("show");
        if (backdrop) backdrop.classList.remove("show");
    }

    if (openBtn) openBtn.addEventListener("click", openSidebar);
    if (closeBtn) closeBtn.addEventListener("click", closeSidebar);
    if (backdrop) backdrop.addEventListener("click", closeSidebar);

    if (sidebar) {
        sidebar.querySelectorAll(".nav-link:not(.nav-toggle), .nav-sublink").forEach(function (link) {
            link.addEventListener("click", closeSidebar);
        });
    }

    // --- Theme toggle ---
    const toggleBtn = document.getElementById("themeToggle");
    if (toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            const isDark = document.documentElement.getAttribute("data-theme") === "dark";
            const next = isDark ? "light" : "dark";
            document.documentElement.setAttribute("data-theme", next);
            try {
                localStorage.setItem("theme", next);
                document.cookie = "theme=" + encodeURIComponent(next) + ";path=/;max-age=31536000;SameSite=Lax";
            } catch (e) {}
            // Set cookie via ASP.NET Core controller (Response.Cookies.Append)
            fetch("/Home/SetTheme", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ theme: next })
            }).catch(function () {});
        });
    }

    // Follow OS preference changes only if user hasn't manually set a theme
    window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", function (e) {
        if (!localStorage.getItem("theme")) {
            const next = e.matches ? "dark" : "light";
            document.documentElement.setAttribute("data-theme", next);
            try {
                document.cookie = "theme=" + encodeURIComponent(next) + ";path=/;max-age=31536000;SameSite=Lax";
            } catch (e) {}
            fetch("/Home/SetTheme", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ theme: next })
            }).catch(function () {});
        }
    });
});
