let loadedScripts = {};

export default function( src ) {
    var tag = document.createElement( 'script' );
    var firstScript = document.getElementsByTagName( 'script' )[ 0 ];

    return new Promise( ( resolve, reject ) => {
    	if ( loadedScripts[ src ] ) {
    		resolve();
    		return;
    	}

        tag.addEventListener( 'load', function( e ) {
        	loadedScripts[ src ] = true;
        	resolve();
        }, false );

        tag.addEventListener( 'error', function( e ) {
            reject();
        }, false );
        tag.async = true;
        tag.src = /^(https?:)?\/\//.test( src ) ? src : ( ( document.body.dataset.baseAssetsUrl || '/' ) + src.replace( /^\//, '' ) );
        firstScript.parentNode.insertBefore( tag, firstScript );
    } );
}
