// Zoom keyboard shortcuts handler
export function initializeZoomKeyboardShortcuts(dotNetHelper) {
    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey || e.metaKey) {
            if (e.key === '=' || e.key === '+') {
                e.preventDefault();
                dotNetHelper.invokeMethodAsync('ZoomIn');
            } else if (e.key === '-') {
                e.preventDefault();
                dotNetHelper.invokeMethodAsync('ZoomOut');
            } else if (e.key === '0') {
                e.preventDefault();
                dotNetHelper.invokeMethodAsync('ZoomReset');
            }
        }
    });
}
