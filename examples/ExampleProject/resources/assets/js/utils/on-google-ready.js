export default function(callback) {
	if (window.google && window.google.maps) {
	    callback();
	} else {
	    window.addEventListener('googleReady', callback);
	}
}