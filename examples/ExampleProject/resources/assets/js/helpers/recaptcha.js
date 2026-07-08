export default {
    getToken: function( context, callback ) {
        if ( typeof ( grecaptcha ) === 'undefined' ) {
            populateContext( context, 'error' );
            return callback( 'error' );
        }
        grecaptcha.ready( function() {
            console.log( window.appConfig);
            grecaptcha.execute( window.appConfig.recaptcha.site_key, { action: 'submit' } ).then( function( token ) {
                populateContext( context, token );
                callback( token );
            }.bind( this ), function() {
                populateContext( context, 'error' );
                callback( 'error' );
            } );
        }.bind( this ) );
    }
};

function populateContext( context, token ) {
    if ( context && ( ( context instanceof Element || context instanceof HTMLDocument ) && !!context.querySelectorAll ) ) {
        // It's a form. Use a hidden input
        var input = context.querySelector( 'input[name="g-recaptcha-response"]' );
        if ( !input ) {
            input = document.createElement( 'input' );
            input.setAttribute( 'type', 'hidden' );
            input.setAttribute( 'name', 'g-recaptcha-response' );
            context.appendChild( input );
        }
        input.setAttribute( 'value', token );
    } else if ( context ) {
        // Just put the token in the object
        context[ 'g-recaptcha-response' ] = token;
    }
}
