import autoComplete  from '@tarekraafat/autocomplete.js';
import "regenerator-runtime/runtime";

class Typeahead{
    
    constructor(el)
    {
        const pinsvg = '<svg xmlns="http://www.w3.org/2000/svg" width="18.777" height="23.318" viewBox="0 0 18.777 23.318"><path id="icon_pin" d="M821.642,178.076a8.388,8.388,0,0,0-8.388,8.388c0,.023,0,.046,0,.069s0,.03,0,.045c0,6.348,6.853,8.512,8.344,12.816,1.039-2.861,4.391-4.777,6.524-7.6a8.388,8.388,0,0,0-6.479-13.715m0,3.827a4.216,4.216,0,1,1-4.216,4.186,4.177,4.177,0,0,1,4.216-4.186" transform="translate(-812.254 -177.076)" fill="none" stroke="#e2d4db" stroke-linejoin="round" stroke-width="2"/></svg>';

        var config = {
            selector: () => { return el },
            data: {
              src: async (query) => { 
                try{     
                  const source = await fetch('/data/autocomplete.json?input=' + query);
                  const data = await source.json();
                  return data;
                }
                catch(error)
                {
                  return error;
                }
              },
              cache: false,
              keys: ["description"],
              filter: (list) => {
                return list;
              }
            },
            placeHolder: "Enter any location",
            threshold: 2,
            debounce: 100,
            resultItem: {
              element: (item, data) => {
                  item.innerHTML = pinsvg + item.innerHTML;
              },
            },
            resultsList: {
                tabSelect: true
            },
            searchEngine : (query, record) => {
                return record;
            }
        };
        this.autoComplete = new autoComplete( config );

        // Clear the input when focused so the user can start typing
        el.addEventListener('focus', () => {
          this.originalValue = el.value;
          el.value = '';
        });

        // When done, make sure the value in the input reflects what is selected
        el.addEventListener('focusout', () => {
          setTimeout(() => {
            let value = this.getValue();
            if (value) {
                el.value = value.description;
            } else if (this.hasResults()) {
                this.autoComplete.select(this.autoComplete.cursor > -1 ? this.autoComplete.cursor : 0);
            } else {
                el.value = this.originalValue;
            }
          }, 5);
        });

        // Select the first result when you hit enter
        el.addEventListener('keydown', (e) => {
            if (e.keyCode === 13 && this.hasResults() && this.autoComplete.cursor === -1) {
                this.autoComplete.select(0);
            }
        });

       el.addEventListener("selection", function (event) {
          // "event.detail" carries the autoComplete.js "feedback" object
          var value = event.detail.selection.value;
          el.value = value.description;
          if(!!el.closest('.input'))
          {
            el.closest('.input').querySelector('.situ_location_input').value = !!value.situ_location ? value.situ_location : null;
            el.closest('.input').querySelector('.placeid_input').value = !!value.place_id ? value.place_id : null;
          }
          
          let ev = new CustomEvent('location-selected',{'bubbles': true, 'detail': event.detail});
          el.dispatchEvent(ev);
      });
    }

    getValue()
    {
        var selection = (this.autoComplete.feedback || {}).selection;
        return selection && selection.value ? selection.value : null;
    }

    hasResults()
    {
        return this.autoComplete.feedback && this.autoComplete.feedback.results && this.autoComplete.feedback.results.length > 0;
    }
}



document.addEventListener('typehaead-init',() => {
  const typeaheads = document.getElementsByClassName('typeahead-vanilla');

  for(let i = 0; i < typeaheads.length; i++)
  {
      let typeahead = typeaheads[i];
      
      if(!typeahead.typeahead)
      {
        typeahead.typeahead = new Typeahead(typeahead);
      }
  }
});

let ev = new Event('typehaead-init',{"bubbles":true});
document.dispatchEvent(ev);