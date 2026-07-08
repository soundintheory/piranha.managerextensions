/*global piranha, Vue, fetch */

// Single-region edit screen. Loads one region of a page (reusing the manager's page edit model) and
// renders it with the core "region" component; saving posts just this region back — the server merges it
// into a fresh copy of the page so other regions are never clobbered.
piranha.scoperegionedit = new Vue({
    el: "#scoperegionedit",
    data: {
        loading: true,
        pageId: null,
        regionId: null,
        typeId: null,
        title: null,
        region: null
    },
    computed: {
        regionName: function () {
            return this.region && this.region.meta ? this.region.meta.name : "";
        }
    },
    methods: {
        load: function (pageId, regionId) {
            var self = this;
            self.pageId = pageId;
            self.regionId = regionId;

            fetch(piranha.baseUrl + "manager/api/scoperegion/" + pageId + "/" + regionId)
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    self.typeId = result.typeId;
                    self.title = result.title;
                    self.region = result.region;
                    self.loading = false;
                })
                .catch(function (error) { console.log("error:", error); });
        },
        save: function () {
            var self = this;
            fetch(piranha.baseUrl + "manager/api/scoperegion/" + self.pageId + "/" + self.regionId, {
                method: "post",
                headers: piranha.utils.antiForgeryHeaders(),
                body: JSON.stringify(self.region)
            })
            .then(function (response) { return response.json(); })
            .then(function (result) { piranha.notifications.push(result); })
            .catch(function (error) { console.log("error:", error); });
        }
    }
});
