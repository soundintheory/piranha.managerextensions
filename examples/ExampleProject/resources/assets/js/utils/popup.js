import MicroModal from 'micromodal'; 

import recaptcha from '../helpers/recaptcha'; 
/*import Pristine from 'pristinejs';
import loaderContent from '../templates/form-loader.html';*/

export default  class Popup
{
    constructor(config) {
        this.config = Object.assign( {}, Popup.configDefaults, config );
    }
    
    inlinePopup(content, config) {  
        if (content === null) return;
        
        document.body.appendChild(this.htmlToElement(this.getModalTemplate(content, config)));
        this.displayPopup(config.callbacks);
    }

    getModalTemplate(content, config) {
        return `
            <div class="modal micromodal-slide fill" id="content-modal" aria-hidden="true">
                <div class="modal__wrap fill ${config.wrapClass || ''}" data-micromodal-close>
                    ${this.config.preContainer}
                    <div class="modal__container ${config.class || ''}" role="dialog" aria-modal="true" aria-labelledby="ajax-modal-title">
                        <button type="button" class="btn modal__close" aria-label="Close modal" data-micromodal-close></button>
                        ${content} 
                    </div>
                </div>
            </div>`;
    }

    htmlToElement(html) {
        let template = document.createElement('div');
        html = html.trim(); // Never return a text node of whitespace as the result
        template.innerHTML = html;
        return template.firstChild;
    }

    displayPopup(callbacks) {
        document.body.classList.add("modal-open");   
        MicroModal.show("content-modal",{ 
            onClose: (modal, activeElement, event) => {
                if (event && event.preventDefault) {
                    event.preventDefault();
                }
                if (callbacks && callbacks.close) { callbacks.close(); } 
                document.body.classList.remove("modal-open");
                if (!!modal.parentNode) {
                    modal.parentNode.removeChild(modal);
                }
            },
            onShow: (modal, activeElement, event) => {
                let form = modal.querySelector('form[data-ajax]');
                if (callbacks && callbacks.open) { callbacks.open(); } 
                if(!!form)
                {
                    /*new FormAjax(form);*/
                }
            },
        });
        setTimeout(() => {
            (document.querySelector('.modal__wrap') || {}).scrollTop = 0;
        }, 1);
    }

    static configDefaults = {
        preContainer: ''
    }
}