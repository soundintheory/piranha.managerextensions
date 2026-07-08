
let config = {
    baseModulePath: '/assets/js/modules',
    loadedClass: 'lazy-loaded',
    observedClass: 'lazy-observed',
    selector: '[data-src],[data-style],[data-module-lazy]',
    rootMargin: '50px 0px', // If the element gets within 50px in the Y axis, start the download.
    threshold: 0.01
};

/**
 * Import all the modules that can be loaded at runtime via [data-module] and [data-module-lazy].
 * Add to this glob pattern if files from other paths are needed
 */
const modules = import.meta.glob('/assets/js/modules/**/*.js', { eager: false });

let elements = null;
let elementCount = null;
let observer = null;
let initd = false;
let modulesLoaded = [];

function initLazy() {
    elements = document.querySelectorAll(config.selector);
    elementCount = elements.length;

    if (!initd) {
        loadLazyStyles();
    }

    // If we don't have support for intersection observer, loads the elements immediately
    if (!('IntersectionObserver' in window)) {
        loadImmediately(elements);
    } else {
        // If there is an existing observer, disconnect it before creating a new one
        disconnect();

        // It is supported, load the elements
        observer = new IntersectionObserver((entries) => onIntersection(entries), config);

        for (let i = 0; i < elements.length; i++) {
            let element = elements[i];

            element.classList.add(config.observedClass);

            if (element.classList.contains(config.loadedClass)) {
                elementCount--;
                continue;
            }

            observer.observe(element);
        }
    }

    initd = true;
}

function loadLazyStyles() {
    let styles = window.lazyStyles || [];
    let head = document.getElementsByTagName('head')[0];

    for (let i = 0; i < styles.length; ++i) {
        var link = document.createElement('link');
        link.href = styles[i];
        link.rel = 'stylesheet';
        head.appendChild(link);
    }
}

/**
 * Loads all of the elements immediately
 */
function loadImmediately(elementsToLoad) {
    for (let i = 0; i < elementsToLoad.length; i++) {
        loadElement(elementsToLoad[i]);
    }
}

/**
 * Loads a single element
 */
function loadElement(el) {
    if (el.dataset.style) {
        el.style = el.dataset.style;
    } else if (el.dataset.src) {
        el.src = el.dataset.src;
    } else if (el.dataset.moduleLazy) {
        loadModule(el.dataset.moduleLazy);
    }

    // Add the "loaded" class
    el.classList.add(config.loadedClass);
}

/**
 * Fires when observed elements come into view
 */
function onIntersection(entries) {
    // Disconnect if we've already loaded all of the elements
    if (elementCount === 0) {
        disconnect();
        return;
    }

    // Loop through the entries
    for (let i = 0; i < entries.length; i++) {
        let entry = entries[i];

        //  Stop watching and load the element if we are in viewport
        if (entry.intersectionRatio > 0) {
            elementCount--;
            observer.unobserve(entry.target);
            loadElement(entry.target);
        }
    }
}

function disconnect() {
    if (observer && observer.disconnect) {
        observer.disconnect();
    }
}

initLazy();

window.addEventListener('lazyload:init', initLazy);

const modulesToLoad = document.querySelectorAll('[data-module]');

modulesToLoad.forEach((e, i) => {
    loadModule(e.dataset.module);
});

function resolveModulePath(path) {

    if (!path || modules[path]) {
        return path;
    }

    // Try to use constructed module path, eg. 'my-module' -> '/assets/js/modules/my-module.js'
    if (path.charAt(0) !== '/' && path.indexOf('.') === -1) {
        const modulePath = config.baseModulePath + '/' + path + '.js';
        if (modules[modulePath]) {
            return modulePath;
        }
    }

    return path;
}

async function loadModule(path) {

    path = resolveModulePath(path);

    if (path && !modulesLoaded.includes(path)) {
        try {
            if (modules[path]) {
                await modules[path]();
            } else {
                await import(path);
            }
        } catch (e) {
            console.error(e);
        }
        modulesLoaded.push(path);
    }
}
