/*global piranha, Vue, fetch, $ */

// Recursive tree node — a copy of the core "sitemap-item" component, reading our PageTreeNode model and
// calling into piranha.pagemanager (we ship our own copy because the core component is bundled in
// piranha.pagelist.min.js, which this screen replaces, and hard-references piranha.pagelist).
Vue.component("pm-sitemap-item", {
    props: ["item"],
    template:
        '<li class="dd-item" v-bind:class="{ expanded: item.isExpanded || item.items.length === 0 }" v-bind:data-id="item.id" v-bind:data-can-sort="item.canSort ? \'true\' : \'false\'" v-bind:data-can-receive="item.canReceive ? \'true\' : \'false\'">' +
        '  <div class="sitemap-item" v-bind:class="{ dimmed: item.isUnpublished || item.isScheduled }">' +
        '    <div v-if="item.canSort" class="handle dd-handle"><i class="fas fa-ellipsis-v"></i></div>' +
        '    <div class="link">' +
        '      <span class="actions">' +
        '        <a v-if="item.items.length > 0 && item.isExpanded" href="#" v-on:click.prevent="toggle" class="expand"><i class="fas fa-minus"></i></a>' +
        '        <a v-if="item.items.length > 0 && !item.isExpanded" href="#" v-on:click.prevent="toggle" class="expand"><i class="fas fa-plus"></i></a>' +
        '      </span>' +
        '      <a v-if="piranha.permissions.pages.edit" v-bind:href="piranha.baseUrl + item.editUrl + item.id">' +
        '        <span>{{ item.title }}</span>' +
        '        <span v-if="item.isRestricted" class="icon-restricted text-secondary small"><i class="fas fa-lock"></i></span>' +
        '        <span v-if="item.status" class="badge badge-info">{{ item.status }}</span>' +
        '        <span v-if="item.isScheduled" class="badge badge-info">{{ piranha.resources.texts.scheduled }}</span>' +
        '        <span v-if="item.isCopy" class="badge badge-warning">{{ piranha.resources.texts.copy }}</span>' +
        '      </a>' +
        '      <span v-else class="title">' +
        '        <span>{{ item.title }}</span>' +
        '        <span v-if="item.isRestricted" class="icon-restricted text-secondary small"><i class="fas fa-lock"></i></span>' +
        '        <span v-if="item.status" class="badge badge-info">{{ item.status }}</span>' +
        '        <span v-if="item.isScheduled" class="badge badge-info">{{ piranha.resources.texts.scheduled }}</span>' +
        '        <span v-if="item.isCopy" class="badge badge-warning">{{ piranha.resources.texts.copy }}</span>' +
        '      </span>' +
        '    </div>' +
        '    <div class="type d-none d-md-block">{{ item.typeName }}</div>' +
        '    <div class="date d-none d-lg-block">{{ item.published }}</div>' +
        '    <div class="actions">' +
        '      <a v-if="piranha.permissions.pages.add" href="#" v-on:click.prevent="piranha.pagemanager.add(item.siteId, item.id, true)"><i class="fas fa-angle-down"></i></a>' +
        '      <a v-if="piranha.permissions.pages.add" href="#" v-on:click.prevent="piranha.pagemanager.add(item.siteId, item.id, false)"><i class="fas fa-angle-right"></i></a>' +
        '      <a v-if="piranha.permissions.pages.delete && item.items.length === 0" href="#" v-on:click.prevent="piranha.pagemanager.remove(item.id)" class="danger"><i class="fas fa-trash"></i></a>' +
        '    </div>' +
        '  </div>' +
        '  <ol v-if="item.items.length > 0" class="dd-list">' +
        '    <pm-sitemap-item v-for="child in item.items" v-bind:key="child.id" v-bind:item="child"></pm-sitemap-item>' +
        '  </ol>' +
        '</li>',
    methods: {
        toggle: function () {
            this.item.isExpanded = !this.item.isExpanded;
        }
    }
});

// Copy-source tree node — a copy of the core "pagecopy-item", used in the add-page modal's Copy tab.
Vue.component("pm-pagecopy-item", {
    props: ["item"],
    template:
        '<li class="dd-item" v-bind:class="{ expanded: item.isExpanded || item.items.length === 0 }">' +
        '  <div class="sitemap-item expanded">' +
        '    <div class="link" v-bind:class="{ readonly: item.isCopy }">' +
        '      <a v-if="!item.isCopy && piranha.pagemanager.addPageId !== null" v-bind:href="piranha.baseUrl + \'manager/page/copyrelative/\' + item.id + \'/\' + piranha.pagemanager.addPageId + \'/\' + piranha.pagemanager.addAfter">{{ item.title }}</a>' +
        '      <a v-else-if="!item.isCopy && piranha.pagemanager.addPageId === null" v-bind:href="piranha.baseUrl + \'manager/page/copy/\' + item.id + \'/\' + piranha.pagemanager.addToSiteId">{{ item.title }}</a>' +
        '      <a href="#" v-else>{{ item.title }} <span v-if="item.isCopy" class="badge badge-warning">{{ piranha.resources.texts.copy }}</span></a>' +
        '      <div class="content-blocker"></div>' +
        '    </div>' +
        '    <div class="type d-none d-md-block">{{ item.typeName }}</div>' +
        '  </div>' +
        '  <ol class="dd-list" v-if="item.items.length > 0">' +
        '    <pm-pagecopy-item v-for="child in item.items" v-bind:key="child.id" v-bind:item="child"></pm-pagecopy-item>' +
        '  </ol>' +
        '</li>'
});

// The replacement Pages screen. Mirrors piranha.pagelist (multi-site) but reads the rooted/filtered tree
// from our API; page mutations (delete, add, copy, move) reuse the core manager/api/page endpoints.
piranha.pagemanager = new Vue({
    el: "#pagemanager",
    data: {
        loading: true,
        updateBindings: false,
        canReorder: false,
        rootId: null,
        sites: [],
        pageTypes: [],
        addSiteId: null,
        addSiteTitle: null,
        addToSiteId: null,
        addPageId: null,
        addAfter: true
    },
    computed: {
        // True when the tree is re-rooted (a site carries a rootId) — used to hide site-level actions.
        isRooted: function () {
            return this.sites.some(function (site) { return !!site.rootId; });
        }
    },
    methods: {
        load: function (rootId) {
            var self = this;
            self.rootId = rootId || null;

            piranha.permissions.load(function () {
                var url = piranha.baseUrl + "manager/api/pagemanager/list" + (self.rootId ? "/" + self.rootId : "");
                fetch(url)
                    .then(function (response) { return response.json(); })
                    .then(function (result) {
                        self.sites = result.sites;
                        self.pageTypes = result.pageTypes;
                        self.canReorder = result.canReorder;
                        self.updateBindings = result.canReorder;
                    })
                    .catch(function (error) { console.log("error:", error); });
            });
        },
        remove: function (id) {
            var self = this;
            piranha.alert.open({
                title: piranha.resources.texts.delete,
                body: piranha.resources.texts.deletePageConfirm,
                confirmCss: "btn-danger",
                confirmIcon: "fas fa-trash",
                confirmText: piranha.resources.texts.delete,
                onConfirm: function () {
                    fetch(piranha.baseUrl + "manager/api/page/delete", {
                        method: "delete",
                        headers: piranha.utils.antiForgeryHeaders(),
                        body: JSON.stringify(id)
                    })
                    .then(function (response) { return response.json(); })
                    .then(function (result) {
                        piranha.notifications.push(result);
                        self.load(self.rootId);
                    })
                    .catch(function (error) { console.log("error:", error); });
                }
            });
        },
        // Drag-to-reorder with per-sibling-group granularity. A node is draggable only if its sibling
        // group is complete (data-can-sort), and a drop is accepted only into a complete group
        // (data-can-receive on the new parent, or data-can-reorder for a container's top level). Because
        // both ends are then complete, the visible order equals the real order, so we can persist the
        // move as just { id, parentId, after } via our own endpoint — no whole-tree serialisation, and
        // re-rooted containers work too (their top-level parent is the container's data-root).
        bind: function () {
            var self = this;
            $("#pagemanager .sitemap-container.dd").each(function (i, container) {
                $(container).nestable({
                    maxDepth: 100,
                    group: i,
                    onDragStart: function (list, item) {
                        return $(item).attr("data-can-sort") === "true";
                    },
                    beforeDragStop: function (list, item, parentList) {
                        var parentItem = $(parentList).closest(".dd-item");
                        if (parentItem.length) {
                            return parentItem.attr("data-can-receive") === "true";
                        }
                        return $(container).attr("data-can-reorder") === "true";
                    },
                    callback: function (list, moved) {
                        var el = $(moved);
                        var parentItem = el.parent(".dd-list").closest(".dd-item");
                        var parentId = parentItem.length
                            ? parentItem.attr("data-id")
                            : ($(container).attr("data-root") || null);
                        var prev = el.prev(".dd-item");
                        var after = prev.length ? prev.attr("data-id") : null;

                        fetch(piranha.baseUrl + "manager/api/pagemanager/move", {
                            method: "post",
                            headers: piranha.utils.antiForgeryHeaders(),
                            body: JSON.stringify({ id: el.attr("data-id"), parentId: parentId, after: after })
                        })
                        .then(function (response) { return response.json(); })
                        .then(function (result) {
                            piranha.notifications.push(result.status);
                            // The DOM already reflects a successful move; only resync on rejection/error.
                            if (result.status.type !== "success") {
                                self.load(self.rootId);
                            }
                        })
                        .catch(function (error) {
                            console.log("error:", error);
                            self.load(self.rootId);
                        });
                    }
                });
            });
        },
        add: function (siteId, pageId, after) {
            var self = this;
            self.addSiteId = siteId;
            self.addToSiteId = siteId;
            self.addPageId = pageId || null;
            self.addAfter = after;

            self.sites.forEach(function (site) {
                if (site.id === siteId) {
                    self.addSiteTitle = site.title;
                }
            });

            $("#pmPageAddModal").modal("show");
        },
        selectSite: function (siteId) {
            var self = this;
            self.addSiteId = siteId;
            self.sites.forEach(function (site) {
                if (site.id === siteId) {
                    self.addSiteTitle = site.title;
                }
            });
        },
        collapse: function () {
            for (var n = 0; n < this.sites.length; n++) {
                for (var i = 0; i < this.sites[n].pages.length; i++) {
                    this.changeVisibility(this.sites[n].pages[i], false);
                }
            }
        },
        expand: function () {
            for (var n = 0; n < this.sites.length; n++) {
                for (var i = 0; i < this.sites[n].pages.length; i++) {
                    this.changeVisibility(this.sites[n].pages[i], true);
                }
            }
        },
        changeVisibility: function (page, expanded) {
            page.isExpanded = expanded;
            for (var i = 0; i < page.items.length; i++) {
                this.changeVisibility(page.items[i], expanded);
            }
        }
    },
    updated: function () {
        if (this.updateBindings) {
            this.bind();
            this.updateBindings = false;
        }
        this.loading = false;
    }
});
