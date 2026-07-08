import Slider from '../components/slider';

class GenericSlider {

    constructor(element) {
        this.sliderWrapper = new Slider(element, {
            loop: 'true',
            speed: 400,
            interval: {
                delay: 3000,
            },
            autoplay: true,
            pagination: false,
            slidesPerView: 1,
        });
    }
}

document.addEventListener('sliders-init', () => {
    let promotionSliders = document.querySelectorAll('.swiper');

    for (let i = 0; i < promotionSliders.length; ++i) {
        var promotionSlider = promotionSliders[i];

        if (!promotionSlider.PromotionSlider) {
            promotionSlider.PromotionSlider = new GenericSlider(promotionSlider);
        }
    }
});

let ev = new Event('sliders-init', { 'bubbles': true });
document.dispatchEvent(ev);

export default GenericSlider;
