/* global L */

let _markers = [];
let _markerClusterGroup;
let _markerStyles = {};

export function createMarkerForExistingLifePoint(leafletMap, id, createdBy, latitude, longitude) {
    if (!_markerClusterGroup) {
        _markerClusterGroup = L.markerClusterGroup();
        leafletMap.addLayer(_markerClusterGroup);
    }

    let myIcon = L.icon({
        iconUrl: 'leaflet/images/marker-icon.png',
        iconRetinaUrl: 'leaflet/images/marker-icon-2x.png',
        shadowUrl: 'leaflet/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        tooltipAnchor: [16, -28],
        shadowSize: [41, 41],
        className: _markerStyles[createdBy]
    });
    let marker = L.marker([latitude, longitude], {icon: myIcon});
    marker._id = id;
    marker.bindPopup("<life-point-detail id='" + id + "'></life-point-detail>", {maxWidth: _calculateMaxWidth()});
    _markerClusterGroup.addLayer(marker);
    _markers.push(marker);
}

export function setupMarkerStylesForCreators(creatorIds) {
    const numberOfMarkerStyles = 360 / creatorIds.length;
    for (let i = 0; i < creatorIds.length; i++) {
        let currentAngle = Math.round(numberOfMarkerStyles * i);
        let currentCssLabel = "huechange" + currentAngle;
        let style = document.createElement("style");
        style.innerHTML = "img." + currentCssLabel + " { filter: hue-rotate(" + currentAngle + "deg); }";
        document.head.appendChild(style);
        _markerStyles[creatorIds[i]] = currentCssLabel;
    }
}

export function removeMarkerOfLifePoint(id) {
    // https://stackoverflow.com/questions/45931963/leaflet-remove-specific-marker
    let newMarkers = [];
    _markers.forEach(function (marker) {
        if (marker._id === id) {
            _markerClusterGroup.removeLayer(marker);
        } else {
            newMarkers.push(marker);
        }
    });
    _markers = newMarkers;
}

export function updatePopup(id) {
    _markers.forEach(function (marker) {
        if (marker._id === id) {
            let popup = marker.getPopup();
            // the following code is borrowed from the original Leaflet sources
            // /src/layer/DivOverlay.js (base class of popup), update().
            // Make sure that this._updateContent() doesn't get called, because otherwise there'll be an infinite loop
            // between updating the popup and recreating the Blazor component. 
            popup._updateLayout();
            popup._updatePosition();
            popup._adjustPan();
        }
    })
}

function _calculateMaxWidth() {
    return (window.devicePixelRatio > 1 ? 300 : 500);
}
