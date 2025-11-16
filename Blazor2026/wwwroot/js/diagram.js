export function initializeDraggable(dotNetHelper) {
    const container = document.querySelector('.diagram-zoom-container');
    if (!container) return;

    const zoomWrapper = container.querySelector('div[style*="transform: scale"]');
    if (!zoomWrapper) return;

    let activeElement = null;
    let startX = 0;
    let startY = 0;
    let initialLeft = 0;
    let initialTop = 0;

    container.addEventListener('mousedown', dragStart, false);
    document.addEventListener('mouseup', dragEnd, false);
    document.addEventListener('mousemove', drag, false);
    container.addEventListener('touchstart', dragStart, false);
    document.addEventListener('touchend', dragEnd, false);
    document.addEventListener('touchmove', drag, false);

    function getZoomScale() {
        const zoomWrapper = container.querySelector('div[style*="transform: scale"]');
        if (!zoomWrapper) return 1.0;
        
        const transform = zoomWrapper.style.transform;
        const match = transform.match(/scale\(([0-9.]+)\)/);
        return match ? parseFloat(match[1]) : 1.0;
    }

    function dragStart(e) {
        const target = e.target.closest('.table-box');
        if (!target) return;

        activeElement = target;
        
        // Get the actual rendered position
        const rect = target.getBoundingClientRect();
        
        // Store where element currently is (in container content coordinates)
        initialLeft = parseInt(target.style.left) || 0;
        initialTop = parseInt(target.style.top) || 0;

        // Store mouse position relative to the element's top-left corner
        if (e.type === 'touchstart') {
            const touch = e.touches[0];
            startX = touch.clientX - rect.left;
            startY = touch.clientY - rect.top;
        } else {
            startX = e.clientX - rect.left;
            startY = e.clientY - rect.top;
        }

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
        
        // After drag ends, check if we can shrink the scrollable area
        adjustContainerSize();
    }

    function drag(e) {
        if (!activeElement) return;

        e.preventDefault();

        const containerRect = container.getBoundingClientRect();
        const zoomScale = getZoomScale();
        let clientX, clientY;
        
        if (e.type === 'touchmove') {
            const touch = e.touches[0];
            clientX = touch.clientX;
            clientY = touch.clientY;
        } else {
            clientX = e.clientX;
            clientY = e.clientY;
        }

        // Mouse position in container content coordinates
        // Account for zoom scale when calculating positions
        const newX = (clientX - containerRect.left + container.scrollLeft - startX) / zoomScale;
        const newY = (clientY - containerRect.top + container.scrollTop - startY) / zoomScale;

        // Clamp position to keep table within reasonable bounds (allow some dragging outside)
        // But prevent negative positions
        const clampedX = Math.max(0, newX);
        const clampedY = Math.max(0, newY);

        activeElement.style.left = clampedX + 'px';
        activeElement.style.top = clampedY + 'px';

        updateConnectedLines(activeElement);
    }

    function adjustContainerSize() {
        // Find all table boxes
        const tables = container.querySelectorAll('.table-box');
        if (tables.length === 0) return;
        
        let maxRight = 0;
        let maxBottom = 0;
        
        // Calculate the maximum extent of all tables
        tables.forEach(table => {
            const rect = table.getBoundingClientRect();
            const zoomScale = getZoomScale();
            
            const left = parseInt(table.style.left) || 0;
            const top = parseInt(table.style.top) || 0;
            const width = rect.width / zoomScale;
            const height = rect.height / zoomScale;
            
            maxRight = Math.max(maxRight, left + width);
            maxBottom = Math.max(maxBottom, top + height);
        });
        
        // Add some padding
        maxRight += 50;
        maxBottom += 50;
        
        // Get the container's natural size
        const containerRect = container.getBoundingClientRect();
        const zoomScale = getZoomScale();
        const naturalWidth = containerRect.width / zoomScale;
        const naturalHeight = containerRect.height / zoomScale;
        
        // Set SVG size to the larger of: natural size or content size
        const svg = container.querySelector('svg');
        if (svg) {
            const newWidth = Math.max(naturalWidth, maxRight);
            const newHeight = Math.max(naturalHeight, maxBottom);
            
            svg.style.minWidth = newWidth + 'px';
            svg.style.minHeight = newHeight + 'px';
        }
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
        
        // Add small margin to prevent lines from touching the border
        const margin = 2;
        
        // Calculate potential intersection points for all four edges
        let x, y;
        
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
        x = centerX + t * dx;
        y = centerY + t * dy;
        
        // Clamp to ensure we stay within box bounds
        x = Math.max(boxX + margin, Math.min(boxX + boxWidth - margin, x));
        y = Math.max(boxY + margin, Math.min(boxY + boxHeight - margin, y));
        
        return { x, y };
    }

    function getTableDimensions(tableElement) {
        const rect = tableElement.getBoundingClientRect();
        const zoomScale = getZoomScale();
        return {
            width: rect.width / zoomScale,
            height: rect.height / zoomScale
        };
    }

    function updateConnectedLines(tableElement) {
        const tableId = tableElement.getAttribute('data-table-id');
        if (!tableId) return;

        // Find all lines connected to this table
        const svg = container.querySelector('svg');
        if (!svg) return;

        const lines = svg.querySelectorAll('line');
        lines.forEach(line => {
            const fromTable = line.getAttribute('data-from-table');
            const toTable = line.getAttribute('data-to-table');

            if (fromTable === tableId || toTable === tableId) {
                // Get both table elements
                const fromTableElement = container.querySelector(`[data-table-id="${fromTable}"]`);
                const toTableElement = container.querySelector(`[data-table-id="${toTable}"]`);
                
                if (!fromTableElement || !toTableElement) return;

                // Get positions and dimensions
                const fromX = parseInt(fromTableElement.style.left) || 0;
                const fromY = parseInt(fromTableElement.style.top) || 0;
                const fromDims = getTableDimensions(fromTableElement);
                const fromWidth = fromDims.width;
                const fromHeight = fromDims.height;
                const fromCenterX = fromX + fromWidth / 2;
                const fromCenterY = fromY + fromHeight / 2;

                const toX = parseInt(toTableElement.style.left) || 0;
                const toY = parseInt(toTableElement.style.top) || 0;
                const toDims = getTableDimensions(toTableElement);
                const toWidth = toDims.width;
                const toHeight = toDims.height;
                const toCenterX = toX + toWidth / 2;
                const toCenterY = toY + toHeight / 2;
                
                // Calculate edge points for both tables
                const fromEdge = getEdgePoint(fromX, fromY, fromWidth, fromHeight, fromCenterX, fromCenterY, toCenterX, toCenterY);
                const toEdge = getEdgePoint(toX, toY, toWidth, toHeight, toCenterX, toCenterY, fromCenterX, fromCenterY);
                
                line.setAttribute('x1', fromEdge.x);
                line.setAttribute('y1', fromEdge.y);
                line.setAttribute('x2', toEdge.x);
                line.setAttribute('y2', toEdge.y);
                
                // Ensure line is visible
                line.style.opacity = '1';
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

    function getZoomScale() {
        const zoomWrapper = container.querySelector('div[style*="transform: scale"]');
        if (!zoomWrapper) return 1.0;
        
        const transform = zoomWrapper.style.transform;
        const match = transform.match(/scale\(([0-9.]+)\)/);
        return match ? parseFloat(match[1]) : 1.0;
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
        
        // Add small margin to prevent lines from touching the border
        const margin = 2;
        
        // Calculate potential intersection points for all four edges
        let x, y;
        
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
        x = centerX + t * dx;
        y = centerY + t * dy;
        
        // Clamp to ensure we stay within box bounds
        x = Math.max(boxX + margin, Math.min(boxX + boxWidth - margin, x));
        y = Math.max(boxY + margin, Math.min(boxY + boxHeight - margin, y));
        
        return { x, y };
    }

    function getTableDimensions(tableElement) {
        const rect = tableElement.getBoundingClientRect();
        const zoomScale = getZoomScale();
        return {
            width: rect.width / zoomScale,
            height: rect.height / zoomScale
        };
    }

    const lines = svg.querySelectorAll('line');
    lines.forEach(line => {
        const fromTable = line.getAttribute('data-from-table');
        const toTable = line.getAttribute('data-to-table');
        
        const fromTableElement = container.querySelector(`[data-table-id="${fromTable}"]`);
        const toTableElement = container.querySelector(`[data-table-id="${toTable}"]`);
        
        if (!fromTableElement || !toTableElement) return;

        const fromX = parseInt(fromTableElement.style.left) || 0;
        const fromY = parseInt(fromTableElement.style.top) || 0;
        const fromDims = getTableDimensions(fromTableElement);
        const fromWidth = fromDims.width;
        const fromHeight = fromDims.height;
        const fromCenterX = fromX + fromWidth / 2;
        const fromCenterY = fromY + fromHeight / 2;

        const toX = parseInt(toTableElement.style.left) || 0;
        const toY = parseInt(toTableElement.style.top) || 0;
        const toDims = getTableDimensions(toTableElement);
        const toWidth = toDims.width;
        const toHeight = toDims.height;
        const toCenterX = toX + toWidth / 2;
        const toCenterY = toY + toHeight / 2;
        
        const fromEdge = getEdgePoint(fromX, fromY, fromWidth, fromHeight, fromCenterX, fromCenterY, toCenterX, toCenterY);
        const toEdge = getEdgePoint(toX, toY, toWidth, toHeight, toCenterX, toCenterY, fromCenterX, fromCenterY);
        
        line.setAttribute('x1', fromEdge.x);
        line.setAttribute('y1', fromEdge.y);
        line.setAttribute('x2', toEdge.x);
        line.setAttribute('y2', toEdge.y);
        
        // Add transition and make line visible
        line.style.transition = 'opacity 0.3s ease';
        line.style.opacity = '1';
    });
}
