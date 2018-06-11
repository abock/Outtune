window.onload = function() {
    let elem = document.getElementsByClassName('ms-bgc-tp')[0]
    let style = window.getComputedStyle(elem)
    window.webkit.messageHandlers.styleUpdated.postMessage({
        topBarBackgroundColor: style.backgroundColor
    })
}

// Of course OWA does not use any proper SPA navigation at all, which
// is half the problem using OWA mobile in a browser.
//
// So this is nicely convoluted since I cannot figure out how to synthesize
// a valid tap event (e.g. new Event('tap', {...})), so listen for the
// first one and save it; it will be used to "tap" the chevron left
// button, should one exist, when the screen-edge "back" gesture is
// recognized (iOS will call _showa_goBack).
window.addEventListener('tap', _showa_tapListener)
function _showa_tapListener(evnt) {
    window._showa_tapEvent = evnt
    window.removeEventListener('tap', _showa_tapListener)
}

function _showa_goBack() {
    let icon = document.getElementsByClassName("ms-Icon--chevronThickLeft")[0]
    if (icon && icon.parentElement)
        icon.parentElement.dispatchEvent(window._showa_tapEvent)
}

// the login UI explicitly allows scaling, which is really awful looking on
// iOS; always force mobile viewport even though the main OWA app does this
let meta = document.createElement('meta')
meta.setAttribute('name', 'viewport')
meta.setAttribute('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=0');
document.getElementsByTagName('head')[0].appendChild(meta);