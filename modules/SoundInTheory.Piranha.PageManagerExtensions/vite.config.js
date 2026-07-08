import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue2';

// Builds the Vue 2 manager assets. Entries in resources/assets/vue are emitted to ./assets/vue,
// which the .csproj embeds into the assembly. `npm run build` for a one-off build,
// `npm run watch` (mode=watch) to rebuild on change while developing against a DEBUG host.
export default defineConfig(({ mode }) => ({
    build: {
        outDir: './',
        emptyOutDir: false,
        manifest: false,
        minify: true,
        target: 'es2015',
        cssCodeSplit: false,
        watch: mode === 'watch' ? {} : null,
        rollupOptions: {
            // Add an entry per manager screen/bundle. app.js auto-registers every .vue component.
            input: [
                './resources/assets/vue/app.js',
                './resources/assets/vue/manager/js/pagemanager.js'
            ],
            output: {
                entryFileNames: 'assets/vue/[name].js',
                assetFileNames: (info) => {
                    let extType = info.name.split('.').at(-1);
                    if (/png|jpe?g|svg|gif|tiff|bmp|ico|webp|avif/i.test(extType)) {
                        extType = 'images';
                    } else if (/ttf|otf|eot|woff|woff2/i.test(extType)) {
                        extType = 'fonts';
                    }
                    return `assets/vue/${extType}/[name].[ext]`;
                }
            },
            // Vue is provided globally by the Piranha manager, so don't bundle it.
            external: ['vue']
        }
    },
    plugins: [vue()]
}));
