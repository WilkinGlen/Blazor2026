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

function getEdgePoint(boxX, boxY, boxWidth, boxHeight, centerX, centerY, targetX, targetY) {
    // Calculate direction from center to target
    let dx = targetX - centerX;
    let dy = targetY - centerY;
    
    // Normalize
    const distance = Math.sqrt(dx * dx + dy * dy);
    if (distance === 0) {
        return { x: centerX, y: centerY };
    }
    
    dx /= distance;
    dy /= distance;
    
    // Calculate intersection with box edges
    const halfWidth = boxWidth / 2;
    const halfHeight = boxHeight / 2;
    const margin = 2;
    
    // Calculate where the ray intersects each edge
    const tRight = dx !== 0 ? (halfWidth - margin) / dx : Infinity;
    const tLeft = dx !== 0 ? -(halfWidth - margin) / dx : Infinity;
    const tBottom = dy !== 0 ? (halfHeight - margin) / dy : Infinity;
    const tTop = dy !== 0 ? -(halfHeight - margin) / dy : Infinity;
    
    // Find the smallest positive t value (closest intersection in the direction we're going)
    let t = Infinity;
    if (tRight > 0 && tRight < t && dx > 0) t = tRight;
    if (tLeft > 0 && tLeft < t && dx < 0) t = tLeft;
    if (tBottom > 0 && tBottom < t && dy > 0) t = tBottom;
    if (tTop > 0 && tTop < t && dy < 0) t = tTop;
    
    // Calculate the intersection point
    const x = Math.max(boxX + margin, Math.min(boxX + boxWidth - margin, centerX + t * dx));
    const y = Math.max(boxY + margin, Math.min(boxY + boxHeight - margin, centerY + t * dy));
    
    return { x, y };
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

function updateLinesBetweenTables(line, container, zoomScale) {
    const fromTable = line.getAttribute('data-from-table');
    const toTable = line.getAttribute('data-to-table');
    
    const fromTableElement = container.querySelector(`[data-table-id="${fromTable}"]`);
    const toTableElement = container.querySelector(`[data-table-id="${toTable}"]`);
    
    if (!fromTableElement || !toTableElement) return;

    const fromPos = getTablePositionAndDimensions(fromTableElement, zoomScale);
    const toPos = getTablePositionAndDimensions(toTableElement, zoomScale);
    
    const fromEdge = getEdgePoint(fromPos.x, fromPos.y, fromPos.width, fromPos.height, 
                                   fromPos.centerX, fromPos.centerY, toPos.centerX, toPos.centerY);
    const toEdge = getEdgePoint(toPos.x, toPos.y, toPos.width, toPos.height, 
                                 toPos.centerX, toPos.centerY, fromPos.centerX, fromPos.centerY);
    
    line.setAttribute('x1', fromEdge.x);
    line.setAttribute('y1', fromEdge.y);
    line.setAttribute('x2', toEdge.x);
    line.setAttribute('y2', toEdge.y);
    line.style.opacity = '1';
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
        const lines = svg.querySelectorAll('line');
        
        lines.forEach(line => {
            const fromTable = line.getAttribute('data-from-table');
            const toTable = line.getAttribute('data-to-table');

            if (fromTable === tableId || toTable === tableId) {
                updateLinesBetweenTables(line, container, zoomScale);
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
    const lines = svg.querySelectorAll('line');
    
    lines.forEach(line => {
        updateLinesBetweenTables(line, container, zoomScale);
        line.style.transition = 'opacity 0.3s ease';
    });
}
