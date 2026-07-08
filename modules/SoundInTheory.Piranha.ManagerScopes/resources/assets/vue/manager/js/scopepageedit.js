/*global piranha, Vue, fetch, sortable */

// Bespoke scope-page editor. Loads the page's title, blocks (main content), and the regions that are NOT
// surfaced in the scoped menu; renders them with the core block/region components. Saving posts them back
// and the server merges into a fresh copy of the page, so the menu regions (edited elsewhere) are kept.
// Block handling mirrors piranha.pageedit (block picker, add/remove/collapse/reorder).
piranha.scopepageedit = new Vue({
    el: "#scopepageedit",
    data: {
        loading: true,
        blocksBound: false,
        id: null,
        typeId: null,
        title: null,
        useBlocks: false,
        blocks: [],
        regions: []
    },
    methods: {
        load: function (id) {
            var self = this;
            self.id = id;
            fetch(piranha.baseUrl + "manager/api/scopepage/" + id)
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    self.typeId = result.typeId;
                    self.title = result.title;
                    self.useBlocks = result.useBlocks;
                    self.blocks = result.blocks || [];
                    self.regions = result.regions || [];
                    self.loading = false;
                })
                .catch(function (error) { console.log("error:", error); });
        },
        save: function () {
            var self = this;
            fetch(piranha.baseUrl + "manager/api/scopepage/" + self.id, {
                method: "post",
                headers: piranha.utils.antiForgeryHeaders(),
                body: JSON.stringify({
                    title: self.title,
                    blocks: JSON.parse(JSON.stringify(self.blocks)),
                    regions: self.regions
                })
            })
            .then(function (response) { return response.json(); })
            .then(function (result) { piranha.notifications.push(result); })
            .catch(function (error) { console.log("error:", error); });
        },
        // Block editing — same shape as piranha.pageedit.
        addBlock: function (type, pos) {
            var self = this;
            fetch(piranha.baseUrl + "manager/api/content/block/" + type)
                .then(function (response) { return response.json(); })
                .then(function (result) { self.blocks.splice(pos, 0, result.body); })
                .catch(function (error) { console.log("error:", error); });
        },
        moveBlock: function (from, to) {
            this.blocks.splice(to, 0, this.blocks.splice(from, 1)[0]);
        },
        collapseBlock: function (block) {
            block.meta.isCollapsed = !block.meta.isCollapsed;
        },
        removeBlock: function (block) {
            var index = this.blocks.indexOf(block);
            if (index !== -1) {
                this.blocks.splice(index, 1);
            }
        },
        updateBlockTitle: function (e) {
            for (var n = 0; n < this.blocks.length; n++) {
                if (this.blocks[n].meta.uid === e.uid) {
                    this.blocks[n].meta.title = e.title;
                    break;
                }
            }
        }
    },
    mounted: function () {
        // (load is kicked off by the page's inline script, after permissions load)
    },
    updated: function () {
        // Wire up block drag-reorder once the blocks container exists; refresh it after block changes.
        if (!document.getElementById("content-blocks")) {
            return;
        }
        if (!this.blocksBound) {
            sortable("#content-blocks", {
                handle: ".handle",
                items: ":not(.unsortable)"
            })[0].addEventListener("sortupdate", function (e) {
                piranha.scopepageedit.moveBlock(e.detail.origin.index, e.detail.destination.index);
            });
            this.blocksBound = true;
        } else {
            sortable("#content-blocks", "disable");
            sortable("#content-blocks", "enable");
        }
    }
});
