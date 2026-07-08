import anime from 'animejs/lib/anime.es.js';

class Expander {
    static defaults = {
        duration: 150,
        easing: 'linear'
    };

    constructor( el, config = {} ) {
        if ( !el ) {
            throw new Error( 'element null or undefined' );
        }
    
        this.config = Object.assign( {}, Expander.defaults, config );
        this.el = el;
        this.expanded = !el.classList.contains( 'closed' );
        this.btn = this.el.querySelector( '[data-expander=true]' );

        if ( this.btn ) {
            this.btn.addEventListener( 'click', event => {
                event.preventDefault();
                event.stopPropagation();
                this.toggle( event );
            } );
        }

        this.el.addEventListener( 'expander:expand', this.expand.bind( this ) );
        this.el.addEventListener( 'expander:collapse', this.collapse.bind( this ) );
    }

    toggle( event ) {
        if ( this.expanded ) {
            this.collapse( event );
        } else {
            this.expand( event );
        }
    }

    expand( event ) {
        if ( this.expanded ) {
            return;
        }

        this.expanded = true;

        const animation = {
            targets: this.el,
            duration: this.config.duration,
            height: this.el.scrollHeight,
            easing: this.config.easing,
            complete: () => {
                this.el.style.height = '';
            }
        };

        anime( animation );

        this.el.classList.remove( 'closed' );
        this.el.dispatchEvent( new Event( 'expander:expanded', { 'bubbles': true } ) );
    }

    collapse( event ) {
        if ( !this.expanded ) {
            return;
        }

        this.expanded = false;

        const collapsedHeight = this.getCollapsedHeight();

        const animation = {
            targets: this.el,
            duration: this.config.duration,
            height: collapsedHeight,
            easing: this.config.easing,
            complete: () => {
                this.el.style.height = '';
            }
        };

        anime( animation );

        this.el.classList.add( 'closed' );
        this.el.dispatchEvent( new Event( 'expander:collapsed', { 'bubbles': true } ) );
    }

    getCollapsedHeight() {
        const style = this.el.style.cssText;
        this.el.classList.add( 'closed' );
        this.el.style.cssText = 'transition: none; -o-transition: none; -webkit-transition: none; -moz-transition: none;';

        let collapsedHeight = parseInt( this.el.offsetHeight );

        if ( isNaN( collapsedHeight ) ) {
            collapsedHeight = 0;
        }

        if ( this.expanded ) {
            this.el.classList.remove( 'closed' );
        }

        if ( style ) {
            this.el.style.cssText = style;
        }

        return collapsedHeight;
    }
}

const expanders = document.getElementsByClassName( 'expander' );

for ( let i = 0; i < expanders.length; i++ ) {
    if ( expanders[ i ].classList.contains( 'expander__initialised' ) ) {
        continue;
    }

    new Expander( expanders[ i ] );
}

window.Expander = Expander;
