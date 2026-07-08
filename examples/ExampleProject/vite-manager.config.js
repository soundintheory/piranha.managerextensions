import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue2';
import queryStringHash from './resources/assets/js/build/vite-plugin-query-string-hash';

// https://vitejs.dev/config/
export default defineConfig({
    publicDir: false,
    root: "resources",
    build: {
        outDir: "../wwwroot",
        write: true,
        emptyOutDir: false,
        manifest: 'assets/manager/manifest.json',
        target: "es2015",
        cssCodeSplit: true,
        rollupOptions: {
            input: {
                'assets/manager/js/app': './resources/assets/manager/js/app.js',
                'assets/manager/css/app.css': './resources/assets/manager/scss/app.scss'
            },
            output: {
                entryFileNames: '[name].js',
                assetFileNames: '[name].[ext]',
                chunkFileNames: 'assets/manager/js/_chunks/[name]-[hash].js',
                sourcemapFileNames: (chunk) => {
                    if (!chunk.isEntry && chunk.name.indexOf('assets') !== 0) {
                        return 'assets/manager/js/_chunks/[name].js.map';
                    }
                    return '[name].js.map';
                }
            },
            external: ['vue']
        }
    },
    plugins: [vue(), queryStringHash()],
})
