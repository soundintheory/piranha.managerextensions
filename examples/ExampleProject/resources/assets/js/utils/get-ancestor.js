
if ( !Element.prototype.matches ) {
    Element.prototype.matches = Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
}

function getAncestor( element, selector ) {
    if ( element && element.closest ) {
        return element.closest( selector );
    }
    if ( element && element.matches && element.matches( selector ) ) {
        return element;
    }
    return element.parentNode ? getAncestor( element.parentNode, selector ) : null;
}

export default getAncestor;
