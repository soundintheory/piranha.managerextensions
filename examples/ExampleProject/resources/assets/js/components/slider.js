import Swiper from 'swiper';

class Slider {
    constructor( element, additionalOptions = {}, mount = true, sync = null ) {
        if ( !element ) {
            throw new Error( 'Slider element null or undefined' );
        }

        this.root = element;
        this.mount = mount;
        this.sync = sync;

        const elementOptions = JSON.parse( this.root.dataset.splide || '{}' );

        this.options = Object.assign( {}, Slider.defaultOptions, elementOptions, additionalOptions );
        this.slider = new Swiper( this.root, this.options );


        this.root.classList.add( 'splide__initialised' );
    }

    init() {
        if ( this.options.events && Object.keys( this.options.events ).length > 0 ) {
            Object.keys( this.options.events ).forEach( event => {
                this.slider.on( event, this.options.events[ event ] );
            } );
        }
    }

    static defaultOptions = {

    }
}

export default Slider;