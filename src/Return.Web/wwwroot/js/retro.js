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
