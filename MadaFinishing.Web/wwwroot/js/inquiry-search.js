(() => {
    const form = document.querySelector("#inquiry-search");
    const filters = document.querySelector("#inquiry-status-filters");
    const notice = document.querySelector("#inquiry-search-notice");
    const reset = document.querySelector("#inquiry-search-reset");
    const search = form.elements.namedItem("Search");
    const status = form.elements.namedItem("Status");
    let pendingSearch;

    function updateControls(results) {
        search.value = results.dataset.search ?? "";
        status.value = results.dataset.status ?? "";
        reset.classList.toggle("d-none", !search.value.trim() && !status.value);

        for (const filter of filters.querySelectorAll("a")) {
            const filterUrl = new URL(filter.href);
            const isSelected = (filterUrl.searchParams.get("status") ?? "") === status.value;
            filter.classList.toggle("active", isSelected);

            if (isSelected) {
                filter.setAttribute("aria-current", "page");
            } else {
                filter.removeAttribute("aria-current");
            }

            filterUrl.searchParams.delete("search");
            if (search.value.trim()) {
                filterUrl.searchParams.set("search", search.value);
            }
            filter.href = filterUrl.href;
        }
    }

    function displayResults(results, updatedResults, requestSearch, url) {
        const searchWasEdited = search.value !== requestSearch;
        const typedSearch = search.value;
        const moveFocus = document.activeElement === reset;
        const paging = document.activeElement?.closest("#inquiry-pagination");
        results.replaceWith(updatedResults);
        updateControls(updatedResults);
        if (searchWasEdited) search.value = typedSearch;
        if (moveFocus && reset.classList.contains("d-none")) search.focus();
        if (paging) updatedResults.querySelector("#results-title").focus();
        if (url.href !== window.location.href) history.pushState(null, "", url);
        notice.classList.add("visually-hidden");
        const count = updatedResults.querySelector("#inquiry-result-count").textContent.trim();
        const page = updatedResults.querySelector("#inquiry-page-description")?.textContent.trim();
        notice.textContent = page ? `${count}، ${page}` : count;
    }

    async function loadResults(url, restoringHistory = false) {
        pendingSearch?.abort();
        const request = new AbortController();
        const requestSearch = search.value;
        pendingSearch = request;
        const results = document.querySelector("#inquiry-results");
        results.setAttribute("aria-busy", "true");
        notice.classList.remove("text-danger", "visually-hidden");
        notice.textContent = "جارٍ تحديث النتائج…";

        try {
            const response = await fetch(url, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                signal: request.signal
            });
            if (request.signal.aborted) return;

            if (response.redirected) {
                window.location.assign(response.url);
                return;
            }
            if (response.status === 401 || response.status === 403) {
                window.location.assign(response.headers.get("Location") ?? url.href);
                return;
            }
            if (!response.ok) throw new Error("Search request failed");

            const html = await response.text();
            if (request.signal.aborted) return;
            const updatedResults = new DOMParser().parseFromString(html, "text/html")
                .querySelector("#inquiry-results");
            if (!updatedResults) throw new Error("Search response has no results section");

            displayResults(results, updatedResults, requestSearch, url);
        } catch {
            if (request.signal.aborted) return;
            if (restoringHistory) {
                window.location.reload();
                return;
            }
            notice.classList.add("text-danger");
            notice.textContent = "تعذّر تحديث النتائج. حاول البحث مرة أخرى.";
        } finally {
            if (pendingSearch === request) results.removeAttribute("aria-busy");
        }
    }

    form.addEventListener("submit", event => {
        event.preventDefault();
        const url = new URL(form.action);
        if (search.value.trim()) url.searchParams.set("search", search.value.trim());
        if (status.value) url.searchParams.set("status", status.value);
        loadResults(url);
    });

    function followSearchLink(event) {
        const link = event.target.closest("#inquiry-status-filters a, #inquiry-search-reset, #inquiry-pagination a");
        if (!link || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

        event.preventDefault();
        loadResults(new URL(link.href));
    }

    document.addEventListener("click", followSearchLink);
    window.addEventListener("popstate", () => loadResults(new URL(window.location.href), true));
})();
