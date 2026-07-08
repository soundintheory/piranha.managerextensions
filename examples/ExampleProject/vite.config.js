import { extname, relative, resolve } from 'path'
import { defineConfig, normalizePath } from 'vite'
import * as glob from 'glob';
import { fileURLToPath } from 'node:url';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import queryStringHash from './resources/assets/js/build/vite-plugin-query-string-hash';

var jsEntries = Object.fromEntries(
    glob.sync('./resources/assets/js/modules/*.js').map(file => [
        // This remove `src/` as well as the file extension from each file, so e.g.
        // src/nested/foo.js becomes nested/foo
        normalizePath(relative('./resources', file.slice(0, file.length - extname(file).length))),
        // This expands the relative paths to absolute paths, so e.g.
        // src/nested/foo becomes /project/src/nested/foo.js
        fileURLToPath(new URL(file, import.meta.url))
    ])
);

jsEntries['assets/js/app'] = resolve('./resources/assets/js/app.js');

var cssEntries = Object.fromEntries(
    glob.sync('./resources/assets/scss/modules/*.scss').map(file => [
        // This remove `src/` as well as the file extension from each file, so e.g.
        // src/nested/foo.js becomes nested/foo
        normalizePath(relative('./resources', file.slice(0, file.length - extname(file).length)).replace("scss", "css")),
        // This expands the relative paths to absolute paths, so e.g.
        // src/nested/foo becomes /project/src/nested/foo.js
        fileURLToPath(new URL(file, import.meta.url))
    ])
);

cssEntries['assets/css/app.critical.css'] = resolve('./resources/assets/scss/app.critical.scss');
cssEntries['assets/css/app.non-critical.css'] = resolve('./resources/assets/scss/app.non-critical.scss');

export default defineConfig({
    publicDir: false,
    root: "resources",
    build: {
        outDir: "../wwwroot",
        manifest: 'assets/manifest.json',
        write: true,
        target: "es2015",
        emptyOutDir: false,
        rollupOptions: {
            input: { ...jsEntries, ...cssEntries },
            output: {
                entryFileNames: '[name].js',
                assetFileNames: '[name].[ext]',
                chunkFileNames: 'assets/js/_chunks/[name]-[hash].js',
                sourcemapFileNames: (chunk) => {
                    if (!chunk.isEntry && chunk.name.indexOf('assets') !== 0) {
                        return 'assets/js/_chunks/[name].js.map';
                    }
                    return '[name].js.map';
                }
            }
        },
        cssCodeSplit: true
    },
    plugins: [
        queryStringHash(),
        viteStaticCopy({
            targets: [
                {
                    src: normalizePath(resolve(__dirname, './resources/assets/fonts')),
                    dest: 'assets'
                },
                {
                    src: normalizePath(resolve(__dirname, './resources/assets/images')),
                    dest: 'assets'
                },
                {
                    src: normalizePath(resolve(__dirname, './resources/assets/svg')),
                    dest: 'assets'
                },
                {
                    src: normalizePath(resolve(__dirname, './resources/assets/favicons')),
                    dest: 'assets'
                }
            ]
        }),
    ]
});
