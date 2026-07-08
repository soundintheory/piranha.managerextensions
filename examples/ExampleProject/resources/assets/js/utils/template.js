export default function( template, context ) {
    if ( typeof ( template ) === 'function' ) {
        return template( context || {} );
    }

    return null;
}
