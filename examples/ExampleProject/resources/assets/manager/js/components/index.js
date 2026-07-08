const components = import.meta.glob('./*.vue', { import: 'default' });

export default Object.keys(components).reduce((fields, path) => {
    var filename = path.split('\\').pop().split('/').pop().replace(/\.[^/.]+$/, "");
    fields[filename] = components[path];
    return fields;
}, {});
