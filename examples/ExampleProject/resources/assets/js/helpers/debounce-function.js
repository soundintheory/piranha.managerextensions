/**
 * Returns a debounced version of a function
 */
 export default function(duration, callback) {
    var timeout = null;
    return function() {
        clearTimeout(timeout);
        timeout = setTimeout(() => callback.apply(this, arguments), duration);
    }
}