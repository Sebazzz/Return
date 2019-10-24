// Polyfills
if (window.NodeList && !NodeList.prototype.forEach) {
    NodeList.prototype.forEach = Array.prototype.forEach;
}

// Drag-and-drop and Firefox compatibility, see: https://github.com/chrissainty/SimpleDragAndDropWithBlazor/issues/1
document.addEventListener('dragstart', function(ev) {
    if (ev.target.classList.contains('draggable')) {
        ev.dataTransfer.setData('text', null);
    } else {
        console.log('ignoring draggable');
    }
});

document.addEventListener('drop', function(ev) {
    ev.preventDefault();
});

// Key press handlers
// Essentially this code handles key shortcuts
// Shortcuts are handled by elements which have the data-keypress-handler attribute defined (for instance: data-keypress-handler="CTRL+I")
// To scope a shortcut to a specific handler element, define both with data-keypress-container id.
(function() {
    document.addEventListener('keydown', function(ev) {
        // Ignore IME events
        if (event.isComposing || event.keyCode === 229) {
            return;
        }

        // Create a handler search string
        var eventStr = '';
        function appendToEventStr(str) {
            if (eventStr !== '') {
                eventStr += '+';
            }

            eventStr += str;
        }

        function appendModifier(shouldAppend, name) {
            if (shouldAppend) {
                appendToEventStr(name);
            }
        }

        appendModifier(ev.ctrlKey, 'CTRL');
        appendModifier(ev.altKey, 'ALT');
        appendModifier(ev.shiftKey, 'SHIFT');

        // If this is a single key event, ignore it if it comes from a input field
        if (eventStr === '') {
            switch (ev.target.localName) {
                case 'SELECT':
                case 'TEXTAREA':
                case 'INPUT':
                    return;
            }
        }

        // Append non-modifier keys (Left, and any char keys)
        var modifiers = ['Control', 'Shift', 'Alt'];
        appendModifier(modifiers.indexOf(ev.key) === -1 && ev.key, ev.key.length > 1 ? ev.key : ev.key.toUpperCase());

        // Example strings now:
        //  CTRL+Left
        //  CTRL+I
        //  CTRL+1

        if (!eventStr) {
            return;
        }

        // Find an associated handler
        var escapedEventStr = eventStr.replace('"', '\\"'),
            selector = '[data-keypress-handler="' + escapedEventStr + '"]',
            keyPressHandlerId = null;

        if (document.activeElement) {
            keyPressHandlerId = document.activeElement.dataset['keypressHandlerId'] || null;
        }

        // Find the handler to handle the event
        var restrictedSelector,
            globalHandlerSelector = '*' + selector + ':not([data-keypress-handler-id])';
        if (keyPressHandlerId) {
            restrictedSelector = '*' + selector + '[data-keypress-handler-id="' + keyPressHandlerId + '"]';
        } else {
            restrictedSelector = globalHandlerSelector;
        }

        var handlingElement = document.querySelector(restrictedSelector);
        if (keyPressHandlerId && !handlingElement) {
            // If this event occurred in a scope, but no handler was found this is probably a global event
            handlingElement = document.querySelector(globalHandlerSelector);
        }

        if (handlingElement && typeof handlingElement.click === 'function') {
            ev.preventDefault();
            ev.stopImmediatePropagation();

            // Ignore repeating presses but do handle the event to prevent the browser from reacting
            if (!ev.isRepeating) {
                handlingElement.click();
            } else {
                console.info('Found handler for %s - ignoring repeating key press', eventStr);
            }
        }
    });
})();

// Auto-expanding text areas
(function() {
    function setTextAreaHeight(textArea) {
        var initialHeight = +textArea.dataset['initialHeight'];
        if (!initialHeight) {
            textArea.dataset['initialHeight'] = initialHeight = textArea.clientHeight;
        }

        var height = Math.max(textArea.scrollHeight, initialHeight);
        textArea.style.minHeight = height + 'px';
        if (height === initialHeight) {
            textArea.style.removeProperty('min-height');
        }
        textArea.style.overflowY = 'hidden';
    }

    document.addEventListener('keyup', function(ev) {
        if (ev.target.tagName !== 'TEXTAREA') {
            return;
        }

        var textArea = ev.target;
        setTextAreaHeight(textArea);
    });

    var observer = new MutationObserver(function(mutationList) {
        mutationList.forEach(function(record) {
            record.addedNodes.forEach(function(node) {
                if (node.tagName !== 'TEXTAREA') {
                    return;
                }

                setTextAreaHeight(node);
            });
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
})();

// JSInterop functions
(function(retro) {
    retro.focusNoteElement = function(element) {
        if (element && typeof element.focus === 'function') {
            console.log('Focusing text input');
            element.focus();
        }
    };
})((window.retro = window.retro || {}));
