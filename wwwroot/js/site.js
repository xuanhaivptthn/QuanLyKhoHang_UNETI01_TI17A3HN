// Sidebar toggle (off-canvas on small/medium screens)
document.addEventListener("DOMContentLoaded", function () {
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
});
