// Entry bundle loaded on every manager page. Auto-registers every .vue file under
// manager/components as a global Vue component named after its filename.
import * as components from "./manager/components/components";

for (const name of Object.keys(components.default)) {
    Vue.component(name, components.default[name]);
}
