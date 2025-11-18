function getZoomScale(container) {
    const zoomWrapper = container.querySelector('div[style*="transform: scale"]');
    if (!zoomWrapper) return 1.0;
    
    const transform = zoomWrapper.style.transform;
    const match = transform.match(/scale\(([0-9.]+)\)/);
    return match ? parseFloat(match[1]) : 1.0;
}

function getTableDimensions(tableElement, zoomScale) {
    const rect = tableElement.getBoundingClientRect();
    return {
        width: rect.width / zoomScale,
        height: rect.height / zoomScale
    };
}

function getTablePositionAndDimensions(tableElement, zoomScale) {
    const x = parseInt(tableElement.style.left) || 0;
    const y = parseInt(tableElement.style.top) || 0;
    const dims = getTableDimensions(tableElement, zoomScale);
    
    return {
        x,
        y,
        width: dims.width,
        height: dims.height,
        centerX: x + dims.width / 2,
        centerY: y + dims.height / 2
    };
}

function getConnectionPoint(box, side) {
    const margin = 2;
    switch (side) {
        case 'top':
            return { x: box.centerX, y: box.y + margin };
        case 'bottom':
            return { x: box.centerX, y: box.y + box.height - margin };
        case 'left':
            return { x: box.x + margin, y: box.centerY };
        case 'right':
            return { x: box.x + box.width - margin, y: box.centerY };
        default:
            return { x: box.centerX, y: box.centerY };
    }
}

function getBestSides(fromBox, toBox) {
    // Determine which sides of the boxes should be connected
    // based on their relative positions
    
    const dx = toBox.centerX - fromBox.centerX;
    const dy = toBox.centerY - fromBox.centerY;
    
    let fromSide, toSide;
    
    // Determine horizontal relationship
    if (Math.abs(dx) > Math.abs(dy)) {
        // Primarily horizontal relationship
        if (dx > 0) {
            fromSide = 'right';
            toSide = 'left';
        } else {
            fromSide = 'left';
            toSide = 'right';
        }
    } else {
        // Primarily vertical relationship
        if (dy > 0) {
            fromSide = 'bottom';
            toSide = 'top';
        } else {
            fromSide = 'top';
            toSide = 'bottom';
        }
    }
    
    return { fromSide, toSide };
}

function createOrthogonalPath(fromBox, toBox) {
    const { fromSide, toSide } = getBestSides(fromBox, toBox);
    const start = getConnectionPoint(fromBox, fromSide);
    const end = getConnectionPoint(toBox, toSide);
    
    const points = [start];
    const routeOffset = 30; // Distance to route away from boxes
    
    // Create orthogonal path based on which sides we're connecting
    if ((fromSide === 'right' && toSide === 'left') || (fromSide === 'left' && toSide === 'right')) {
        // Horizontal connection
        const midX = (start.x + end.x) / 2;
        points.push({ x: midX, y: start.y });
        points.push({ x: midX, y: end.y });
    } else if ((fromSide === 'bottom' && toSide === 'top') || (fromSide === 'top' && toSide === 'bottom')) {
        // Vertical connection
        const midY = (start.y + end.y) / 2;
        points.push({ x: start.x, y: midY });
        points.push({ x: end.x, y: midY });
    } else {
        // Mixed connection (e.g., right to top, bottom to left, etc.)
        if (fromSide === 'right' || fromSide === 'left') {
            // Start horizontally
            const offsetX = fromSide === 'right' ? start.x + routeOffset : start.x - routeOffset;
            points.push({ x: offsetX, y: start.y });
            points.push({ x: offsetX, y: end.y });
        } else {
            // Start vertically
            const offsetY = fromSide === 'bottom' ? start.y + routeOffset : start.y - routeOffset;
            points.push({ x: start.x, y: offsetY });
            points.push({ x: end.x, y: offsetY });
        }
    }
    
    points.push(end);
    
    // Convert points to SVG polyline format
    return points.map(p => `${p.x},${p.y}`).join(' ');
}

function updateLinesBetweenTables(polyline, container, zoomScale) {
    const fromTable = polyline.getAttribute('data-from-table');
    const toTable = polyline.getAttribute('data-to-table');
    
    const fromTableElement = container.querySelector(`[data-table-id="${fromTable}"]`);
    const toTableElement = container.querySelector(`[data-table-id="${toTable}"]`);
    
    if (!fromTableElement || !toTableElement) return;

    const fromPos = getTablePositionAndDimensions(fromTableElement, zoomScale);
    const toPos = getTablePositionAndDimensions(toTableElement, zoomScale);
    
    const pathPoints = createOrthogonalPath(fromPos, toPos);
    
    polyline.setAttribute('points', pathPoints);
    polyline.style.opacity = '1';
}

export function initializeDraggable(dotNetHelper) {
    const container = document.querySelector('.diagram-zoom-container');
    if (!container) return;

    const zoomWrapper = container.querySelector('div[style*="transform: scale"]');
    if (!zoomWrapper) return;

    let activeElement = null;
    let startX = 0;
    let startY = 0;

    container.addEventListener('mousedown', dragStart, false);
    document.addEventListener('mouseup', dragEnd, false);
    document.addEventListener('mousemove', drag, false);
    container.addEventListener('touchstart', dragStart, false);
    document.addEventListener('touchend', dragEnd, false);
    document.addEventListener('touchmove', drag, false);

    function dragStart(e) {
        const target = e.target.closest('.table-box');
        if (!target) return;

        activeElement = target;
        const rect = target.getBoundingClientRect();

        // Store mouse position relative to the element's top-left corner
        const touch = e.type === 'touchstart' ? e.touches[0] : e;
        startX = touch.clientX - rect.left;
        startY = touch.clientY - rect.top;

        target.style.cursor = 'grabbing';
        e.preventDefault();
    }

    function dragEnd(e) {
        if (!activeElement) return;

        const target = activeElement;
        const tableId = target.getAttribute('data-table-id');
        
        target.style.cursor = 'grab';

        const finalX = parseInt(target.style.left) || 0;
        const finalY = parseInt(target.style.top) || 0;

        if (dotNetHelper && tableId) {
            dotNetHelper.invokeMethodAsync('UpdateTablePosition', tableId, finalX, finalY);
        }

        activeElement = null;
        adjustContainerSize();
    }

    function drag(e) {
        if (!activeElement) return;

        e.preventDefault();

        const containerRect = container.getBoundingClientRect();
        const zoomScale = getZoomScale(container);
        
        const touch = e.type === 'touchmove' ? e.touches[0] : e;
        const clientX = touch.clientX;
        const clientY = touch.clientY;

        // Mouse position in container content coordinates, accounting for zoom scale
        const newX = (clientX - containerRect.left + container.scrollLeft - startX) / zoomScale;
        const newY = (clientY - containerRect.top + container.scrollTop - startY) / zoomScale;

        // Prevent negative positions
        const clampedX = Math.max(0, newX);
        const clampedY = Math.max(0, newY);

        activeElement.style.left = clampedX + 'px';
        activeElement.style.top = clampedY + 'px';

        updateConnectedLines(activeElement);
    }

    function adjustContainerSize() {
        const tables = container.querySelectorAll('.table-box');
        if (tables.length === 0) return;
        
        const zoomScale = getZoomScale(container);
        let maxRight = 0;
        let maxBottom = 0;
        
        tables.forEach(table => {
            const pos = getTablePositionAndDimensions(table, zoomScale);
            maxRight = Math.max(maxRight, pos.x + pos.width);
            maxBottom = Math.max(maxBottom, pos.y + pos.height);
        });
        
        // Add padding
        maxRight += 50;
        maxBottom += 50;
        
        // Get the container's natural size
        const containerRect = container.getBoundingClientRect();
        const naturalWidth = containerRect.width / zoomScale;
        const naturalHeight = containerRect.height / zoomScale;
        
        // Set SVG size to the larger of: natural size or content size
        const svg = container.querySelector('svg');
        if (svg) {
            svg.style.minWidth = Math.max(naturalWidth, maxRight) + 'px';
            svg.style.minHeight = Math.max(naturalHeight, maxBottom) + 'px';
        }
    }

    function updateConnectedLines(tableElement) {
        const tableId = tableElement.getAttribute('data-table-id');
        if (!tableId) return;

        const svg = container.querySelector('svg');
        if (!svg) return;

        const zoomScale = getZoomScale(container);
        const polylines = svg.querySelectorAll('polyline');
        
        polylines.forEach(polyline => {
            const fromTable = polyline.getAttribute('data-from-table');
            const toTable = polyline.getAttribute('data-to-table');

            if (fromTable === tableId || toTable === tableId) {
                updateLinesBetweenTables(polyline, container, zoomScale);
            }
        });
    }

    return {
        dispose: () => {
            container.removeEventListener('mousedown', dragStart);
            document.removeEventListener('mouseup', dragEnd);
            document.removeEventListener('mousemove', drag);
            container.removeEventListener('touchstart', dragStart);
            document.removeEventListener('touchend', dragEnd);
            document.removeEventListener('touchmove', drag);
        }
    };
}

export function updateAllLines() {
    const container = document.querySelector('.diagram-zoom-container');
    if (!container) return;

    const svg = container.querySelector('svg');
    if (!svg) return;

    const zoomScale = getZoomScale(container);
    const polylines = svg.querySelectorAll('polyline');
    
    polylines.forEach(polyline => {
        updateLinesBetweenTables(polyline, container, zoomScale);
        polyline.style.transition = 'opacity 0.3s ease';
    });
}
