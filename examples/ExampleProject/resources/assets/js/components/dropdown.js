class Dropdown
{
    constructor(elem)
    {
        this.el = elem;
        this.button = this.el.querySelector('.btn');

        this.button.addEventListener('click', () => {
            if ( this.el.classList.contains( 'open' ) ) {
                this.el.dispatchEvent( new Event( 'close-dropdown' ) );
            } else {
                this.el.dispatchEvent( new Event( 'open-dropdown' ) );
            }
        } );

        this.el.addEventListener('open-dropdown',() => {
            this.el.classList.add('open');

            document.addEventListener('click', this.closeClickHandler.bind( this ) );
            this.el.addEventListener('click', this.blockElemCloseClickHandler.bind( this ) );
        } );

        this.el.addEventListener('close-dropdown',() => {
            this.el.classList.remove( 'open' );

            document.removeEventListener('click', this.closeClickHandler.bind( this ) );
            this.el.removeEventListener('click', this.blockElemCloseClickHandler.bind( this ) );
        } );
    }

    closeClickHandler() {
        let event = new Event( 'close-dropdown' );
        this.el.dispatchEvent( event );
    }

    blockElemCloseClickHandler( event ) {
        event.stopPropagation();
    }
}

const dropdowns = document.getElementsByClassName( 'dropdown-group' );
for ( let i = 0; i < dropdowns.length; ++i ) {
    var dropdown = dropdowns[ i ];

    if ( !dropdown.Dropdown ) {
        dropdown.Dropdown = new Dropdown( dropdown );
    }
}

var actions = document.querySelectorAll('[data-action="dropdown"]');

for (var i = 0; i < actions.length; i++) {
    actions[i].addEventListener('click', (e) => {
        if (e.target.dataset.target) {
            var dropdown = document.querySelector(e.target.dataset.target);
            if (dropdown != null) {
                dropdown.dispatchEvent(new Event('open-dropdown'));
                if (dropdown.scrollIntoView) {
                    dropdown.scrollIntoView();
                }
            }
        }
        e.preventDefault();
        e.stopPropagation();
    }, true);
}


