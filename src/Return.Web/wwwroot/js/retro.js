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
