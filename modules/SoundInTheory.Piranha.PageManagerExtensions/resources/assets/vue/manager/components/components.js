// Collects every .vue file in this folder, keyed by filename (without extension), so app.js can
// register each one as a global Vue component. e.g. `example-component.vue` -> `<example-component>`.
const components = import.meta.glob('./*.vue', { eager: true, import: 'default' });

export default Object.keys(components).reduce((acc, path) => {
    const name = path.split('/').pop().replace(/\.vue$/, '');
    acc[name] = components[path];
    return acc;
}, {});
