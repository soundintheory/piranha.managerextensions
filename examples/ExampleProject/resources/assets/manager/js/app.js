import fields from "./fields";
import components from "./components";

for (const item of Object.keys(fields)) {
    Vue.component(item, fields[item])
}

for (const item of Object.keys(components)) {
    Vue.component(item, components[item])
}
