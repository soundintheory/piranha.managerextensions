import Popup from '../utils/popup';
import contactPopup from '../templates/general-contact-popup.html';
import loginPopup from '../templates/login-popup.html';
import searchPopup from '../templates/search-popup.html';
import forgottenPasswordPopup from '../templates/forgotten-password-popup.html';
import userAreaEnquiry from '../templates/user-area-enquiry.html';

class GeneralContactPopup {
    get configs() {
        return {
            template: contactPopup
        };
    }

    constructor() {
        this.PopupManager = new Popup();

        document.addEventListener('open-general-contact-popup', (e) => {   
            this.openPopup(e.currentTarget.dataset);
            e.preventDefault();
        });

        document.addEventListener('open-exit-contact-popup', (e) => {   
            this.openPopup(e.currentTarget.dataset);
            e.preventDefault();
        });

        document.addEventListener( 'situ:init-general-contact-popup', this.initButtons.bind( this ) );

        this.initButtons();
    }

    initButtons() {
        let actions = document.querySelectorAll( '[data-action="general-contact-popup"]' );

        for ( let i = 0; i < actions.length; ++i ) {
            if ( actions[ i ].dataset.generalContactPopup === 'true' ) {
                continue;
            }

            actions[ i ].addEventListener( 'click', event => {
                this.openPopup( Object.assign({firstname : "", lastname: "", company: "" }, event.currentTarget.dataset) );
                event.preventDefault();
            } );

            actions[ i ].dataset.generalContactPopup = 'true';
        }
    }

    openPopup( config ) {
        this.PopupManager.inlinePopup( this.configs.template, config, {
            closeOnBgClick: true
        } );

        let ev = new Event( 'select-group-init', { 'bubbles': true } );
        document.dispatchEvent( ev );

        document.querySelector('#general-contact-popup form').addEventListener("change", event => {

            let value = document.querySelector('#general-contact-popup [data-action="toggle-company"]').value;

            if(value != "Business Stay")
            {
                document.querySelector('#general-contact-popup .company-toggle').classList.add('hidden');
            }
            else{
                document.querySelector('#general-contact-popup .company-toggle').classList.remove('hidden');
            }
            
        });
    }
}

class LoginPopup {
    get configs() {
        return {
            template: loginPopup
        };
    }

    constructor() {
        this.PopupManager = new Popup();

        let actions = document.querySelectorAll( '[data-action="login-popup"]' );

        for ( let i = 0; i < actions.length; i++ ) {
            actions[ i ].addEventListener( 'click', ( e ) => {
                this.openPopup( e.currentTarget.dataset );
                e.preventDefault();
            } );
        }
    }

    openPopup( config ) {
        this.PopupManager.inlinePopup( this.configs.template, config, {
            closeOnBgClick: true
        } );

        document.querySelector( '[data-action="forgotten-password-popup"]' ).addEventListener( 'click', ( e )=>{
            e.preventDefault();
            let ev = new Event( 'open-reset-password-popup', { 'bubbles': true } );
            document.dispatchEvent( ev );
        } );
    }
}

class ForgottenPasswordPopup {
    get configs() {
        return {
            template: forgottenPasswordPopup
        };
    }

    constructor() {
        this.PopupManager = new Popup( {
            preContainer: '<span class="close-link-top" data-micromodal-close>×</span>'
        } );

        const actions = document.querySelectorAll( '[data-action="forgotten-password-popup"]' );

        for ( let i = 0; i < actions.length; i++ ) {
            actions[ i ].addEventListener( 'click', ( event ) => {
                event.preventDefault();
                this.openPopup( event.currentTarget.dataset );
            } );
        }

        document.addEventListener( 'open-reset-password-popup', ( event  ) => {
            event.preventDefault();
            MicroModal.close( 'content-modal' );
            this.openPopup( event.currentTarget.dataset );
        } );
    }

    openPopup( config ) {
        this.PopupManager.inlinePopup( this.configs.template, config, {
            closeOnBgClick: true
        } );
    }
}

class SearchPopup {
    get configs() {
        return {
            template: searchPopup
        };
    }

    constructor() {
        this.PopupManager = new Popup();

        let actions = document.querySelectorAll( '[data-action="search-popup"]' ); let i;

        for ( i = 0; i < actions.length; ++i ) {
            actions[ i ].addEventListener( 'click', ( e ) => {
                this.openPopup( e.currentTarget.dataset );
                e.preventDefault();
            } );
        }
        
        document.addEventListener('open-search-popup', (e) => {   
            this.openPopup(e.currentTarget.dataset);
            e.preventDefault();
        });
    }

    openPopup( config ) {
        this.PopupManager.inlinePopup( this.configs.template, config, {
            closeOnBgClick: true
        } );
        let ev = new Event( 'search-bar-init', { 'bubbles': true } );
        document.dispatchEvent( ev );
        ev = new Event( 'daterange-picker-init', { 'bubbles': true } );
        document.dispatchEvent( ev );
        ev = new Event( 'select-group-init', { 'bubbles': true } );
        document.dispatchEvent( ev );
        ev = new Event( 'typehaead-init', { 'bubbles': true } );
        document.dispatchEvent( ev );
    }
}

class UserAreaEnquiryPopup
{
    get configs(){
        return {
            template: userAreaEnquiry,      
        }
    };

    constructor()
    {
        this.PopupManager = new Popup();

        let actions = document.querySelectorAll('[data-action="user-area-enquiry-popup"]'),i;
        
        for (i = 0; i < actions.length; ++i) {
            
            actions[i].addEventListener('click', (e) => {   
                this.openPopup(e.currentTarget.dataset);
                e.preventDefault();
            });
        }

        
        document.addEventListener('user-area-enquiry-popup', (e) => {   
            this.openPopup(e.currentTarget.dataset);
            e.preventDefault();
        });
    }

    openPopup(config)
    {
        this.PopupManager.inlinePopup(this.configs.template, config, {
            closeOnBgClick: true
        });

        setTimeout(() => {
            document.querySelector('.micromodal-slide [data-action="search-popup"]').addEventListener('click',(e)=>{
                MicroModal.close( 'content-modal' );
                e.preventDefault();
                let ev = new Event('open-search-popup',{"bubbles":true});
                document.dispatchEvent(ev);
            });
            window.EnquiryFormWidget.init('.enquiry-form-widget-nonform',true);
        }, 0);
    }
}

new LoginPopup();

new ForgottenPasswordPopup();

new SearchPopup();

new UserAreaEnquiryPopup();

window.GeneralContactPopup = new GeneralContactPopup();

window.Popup = Popup;

document.dispatchEvent( new CustomEvent( 'situ:popups-ready', {
    bubbles: true
} ) );
