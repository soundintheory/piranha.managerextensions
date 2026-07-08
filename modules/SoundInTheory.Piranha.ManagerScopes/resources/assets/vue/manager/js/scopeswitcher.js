/*global piranha, Vue, fetch, document, window, $ */

// The scope switcher. Loads the accessible scopes into a select2 typeahead (select2 is bundled with
// Piranha). On change it sets the current scope (server-side, in session) and redirects to the
// equivalent of the currently-active menu item in the new scope — or the pages view if there's none.
// Markup lives in _ScopeSwitcher.cshtml.
(function () {
    var el = document.getElementById("scope-switcher");
    if (!el || !window.piranha) {
        return;
    }

    piranha.scopeswitcher = new Vue({
        el: "#scope-switcher",
        data: {
            loading: true,
            scopes: [],
            currentScopeId: null,
            canUnscoped: true
        },
        methods: {
            load: function () {
                var self = this;
                fetch(piranha.baseUrl + "manager/api/managerscopes/list")
                    .then(function (response) { return response.json(); })
                    .then(function (result) {
                        self.scopes = result.scopes || [];
                        self.currentScopeId = result.currentScopeId || null;
                        self.canUnscoped = result.canUnscoped;
                        self.loading = false;
                        // Options are rendered by Vue — init select2 once they're in the DOM.
                        Vue.nextTick(function () { self.initSelect2(); });
                    })
                    .catch(function (error) { console.log("error:", error); });
            },
            initSelect2: function () {
                var self = this;
                var select = self.$refs.select;
                if (!select) {
                    return;
                }
                var $s = $(select);
                $s.select2({ width: "100%" });
                // Reflect the current scope without firing our change handler (change.select2 only
                // updates the widget); the handler is attached after, for user selections. "__all__" is
                // the unscoped ("Main Website") sentinel.
                $s.val(self.currentScopeId || "__all__").trigger("change.select2");
                $s.on("select2:select", function (e) {
                    self.onChange(e.params.data.id);
                });
            },
            onChange: function (value) {
                var self = this;
                var scopeId = (value && value !== "__all__") ? value : null;
                if (scopeId === self.currentScopeId) {
                    return;
                }

                // Tell the server which menu item we're on; it returns where to land in the new scope.
                var activeEl = document.querySelector("#scoped-navbar .nav-item.active");
                var activeId = activeEl ? activeEl.getAttribute("data-internal-id") : null;

                var url = piranha.baseUrl + "manager/api/managerscopes/set" + (scopeId ? "/" + scopeId : "");
                if (activeId) {
                    url += "?active=" + encodeURIComponent(activeId);
                }
                fetch(url, { method: "post", headers: piranha.utils.antiForgeryHeaders() })
                    .then(function (response) {
                        if (!response.ok) { throw new Error("Scope switch rejected"); }
                        return response.json();
                    })
                    .then(function (result) {
                        window.location.href = result.redirect;
                    })
                    .catch(function () {
                        piranha.notifications.push({ body: "Unable to switch scope.", type: "danger", hide: true });
                    });
            }
        },
        mounted: function () {
            this.load();
        }
    });
})();
