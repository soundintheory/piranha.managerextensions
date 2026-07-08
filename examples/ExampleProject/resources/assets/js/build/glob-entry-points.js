import * as glob from 'glob';

export default function globEntryPoints(pattern, relativeTo) {
    return Object.fromEntries(
        glob.sync(pattern).map(file => [
            // This remove `src/` as well as the file extension from each file, so e.g.
            // src/nested/foo.js becomes nested/foo
            normalizePath(relative(relativeTo, file.slice(0, file.length - extname(file).length))),
            // This expands the relative paths to absolute paths, so e.g.
            // src/nested/foo becomes /project/src/nested/foo.js
            fileURLToPath(new URL(file, import.meta.url))
        ])
    );
}
