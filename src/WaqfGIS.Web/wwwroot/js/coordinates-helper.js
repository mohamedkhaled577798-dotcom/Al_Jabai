/**
 * coordinates-helper.js
 * أداة مساعدة لإدارة الإحداثيات الجغرافية في نظام WaqfGIS
 * تدعم: استخراج الإحداثيات من روابط Google Maps / Apple Maps / Plus Codes
 *        وإدخال يدوي مع تحديث الخريطة تلقائياً
 */

// ===== استخراج الإحداثيات من رابط خرائط =====
function extractCoordsFromLink(uniqueId, latId, lngId) {
    var input = document.getElementById('mapsLink_' + uniqueId);
    var resultDiv = document.getElementById('linkResult_' + uniqueId);
    if (!input) return;
    
    var url = input.value.trim();
    if (!url) {
        resultDiv.style.display = 'none';
        return;
    }

    var result = parseMapLink(url);
    
    if (result) {
        // تحديث حقول الإحداثيات
        setCoordinateFields(latId, lngId, result.lat, result.lng);
        
        // تحديث الخريطة إن وُجدت
        updateMapMarker(uniqueId, result.lat, result.lng);
        
        // عرض النتيجة
        resultDiv.style.display = 'block';
        resultDiv.innerHTML = `
            <div class="alert alert-success alert-sm p-2 mb-0">
                <i class="fas fa-check-circle me-1"></i>
                <strong>تم استخراج الإحداثيات بنجاح:</strong>
                خط العرض: <span class="text-primary">${result.lat.toFixed(6)}</span>،
                خط الطول: <span class="text-primary">${result.lng.toFixed(6)}</span>
                ${result.source ? `<span class="badge bg-secondary ms-1">${result.source}</span>` : ''}
            </div>`;
    } else if (url.length > 10) {
        resultDiv.style.display = 'block';
        resultDiv.innerHTML = `
            <div class="alert alert-warning alert-sm p-2 mb-0">
                <i class="fas fa-exclamation-triangle me-1"></i>
                تعذّر استخراج الإحداثيات من هذا الرابط. تأكد أنه رابط Google Maps أو Apple Maps صحيح.
            </div>`;
    }
}

/**
 * تحليل رابط الخريطة واستخراج الإحداثيات
 * يدعم أنواع متعددة من الروابط
 */
function parseMapLink(url) {
    if (!url) return null;
    
    var lat, lng;
    
    // ===== Google Maps =====
    
    // 1. رابط عادي مع @lat,lng  مثال: https://www.google.com/maps/@33.315,44.361,15z
    var match = url.match(/@(-?\d+\.?\d*),(-?\d+\.?\d*)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 2. رابط مشاركة مع !3d!4d  مثال: !3d33.315!4d44.361
    match = url.match(/!3d(-?\d+\.?\d+)!4d(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 3. رابط مع ?q=lat,lng  مثال: https://maps.google.com/?q=33.315,44.361
    match = url.match(/[?&]q=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 4. رابط مع ll=lat,lng  مثال: https://maps.google.com/?ll=33.315,44.361
    match = url.match(/[?&]ll=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 5. رابط Google Maps مختصر goo.gl أو maps.app.goo.gl (يحتوي على إحداثيات في المسار)
    match = url.match(/\/place\/[^/]+\/@(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 6. رابط Google Maps مع /maps/place/.../@lat,lng
    match = url.match(/maps\/place\/.*?\/@@?(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // 7. إحداثيات مضمنة في URL مع sll=
    match = url.match(/sll=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Google Maps' };
    }
    
    // ===== Apple Maps =====
    
    // 8. رابط Apple Maps  مثال: https://maps.apple.com/?ll=33.315,44.361
    match = url.match(/maps\.apple\.com.*?[?&]ll=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Apple Maps' };
    }
    
    // 9. Apple Maps مع q=lat,lng
    match = url.match(/maps\.apple\.com.*?[?&]q=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Apple Maps' };
    }
    
    // ===== OpenStreetMap =====
    
    // 10. رابط OSM  مثال: https://www.openstreetmap.org/?mlat=33.315&mlon=44.361
    match = url.match(/mlat=(-?\d+\.?\d+).*?mlon=(-?\d+\.?\d+)/);
    if (!match) match = url.match(/mlon=(-?\d+\.?\d+).*?mlat=(-?\d+\.?\d+)/);
    if (match) {
        // قد يكون الترتيب مختلفاً
        var m1 = parseFloat(match[1]), m2 = parseFloat(match[2]);
        if (url.indexOf('mlat') < url.indexOf('mlon')) {
            lat = m1; lng = m2;
        } else {
            lat = m2; lng = m1;
        }
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'OpenStreetMap' };
    }
    
    // 11. إحداثيات مباشرة بصيغة  lat,lng  أو  lat, lng
    match = url.match(/^(-?\d{1,3}\.\d+)\s*,\s*(-?\d{1,3}\.\d+)$/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'إحداثيات مباشرة' };
    }
    
    // 12. Waze  مثال: https://waze.com/ul?ll=33.315,44.361
    match = url.match(/waze\.com.*?ll=(-?\d+\.?\d+),(-?\d+\.?\d+)/);
    if (match) {
        lat = parseFloat(match[1]);
        lng = parseFloat(match[2]);
        if (isValidCoords(lat, lng)) return { lat, lng, source: 'Waze' };
    }
    
    return null;
}

/**
 * التحقق من صحة الإحداثيات
 */
function isValidCoords(lat, lng) {
    return !isNaN(lat) && !isNaN(lng) &&
           lat >= -90 && lat <= 90 &&
           lng >= -180 && lng <= 180 &&
           !(lat === 0 && lng === 0); // رفض الإحداثيات الصفرية
}

/**
 * تعيين قيم حقول الإحداثيات
 */
function setCoordinateFields(latId, lngId, lat, lng) {
    var latField = document.getElementById(latId);
    var lngField = document.getElementById(lngId);
    
    if (latField) {
        latField.value = lat.toFixed(6);
        // إزالة readonly مؤقتاً إن كان موجوداً
        latField.removeAttribute('readonly');
    }
    if (lngField) {
        lngField.value = lng.toFixed(6);
        lngField.removeAttribute('readonly');
    }
}

/**
 * تحديث علامة الخريطة عند إدخال إحداثيات يدوية
 */
function onManualCoordInput(uniqueId, latId, lngId) {
    var latField = document.getElementById(latId);
    var lngField = document.getElementById(lngId);
    if (!latField || !lngField) return;
    
    var lat = parseFloat(latField.value);
    var lng = parseFloat(lngField.value);
    
    if (isValidCoords(lat, lng)) {
        updateMapMarker(uniqueId, lat, lng);
    }
}

/**
 * الانتقال إلى الإحداثيات المدخلة يدوياً
 */
function goToManualCoords(uniqueId, latId, lngId) {
    var latField = document.getElementById(latId);
    var lngField = document.getElementById(lngId);
    if (!latField || !lngField) return;
    
    var lat = parseFloat(latField.value);
    var lng = parseFloat(lngField.value);
    
    if (isValidCoords(lat, lng)) {
        updateMapMarker(uniqueId, lat, lng, true);
        
        // عرض رسالة تأكيد
        var resultDiv = document.getElementById('linkResult_' + uniqueId);
        if (resultDiv) {
            resultDiv.style.display = 'block';
            resultDiv.innerHTML = `
                <div class="alert alert-info alert-sm p-2 mb-0">
                    <i class="fas fa-map-marker-alt me-1"></i>
                    تم تحديد الموقع: ${lat.toFixed(6)}, ${lng.toFixed(6)}
                </div>`;
        }
    } else {
        alert('يرجى إدخال إحداثيات صحيحة\nمثال - خط العرض: 33.315000، خط الطول: 44.361000');
    }
}

/**
 * تحديث علامة الخريطة - يجب تعريف mapInstance في كل صفحة
 * uniqueId يُستخدم للتمييز بين الخرائط المتعددة
 */
function updateMapMarker(uniqueId, lat, lng, flyTo) {
    // يحاول الوصول إلى كائن الخريطة العام
    // كل صفحة يجب أن تعرّف window['mapMarker_' + uniqueId] أو تتعامل مع الحدث
    
    var mapObj = window['locationMap_' + uniqueId] || window.locationMap || window.map;
    var markerObj = window['locationMarker_' + uniqueId] || window.locationMarker || window.marker;
    
    if (mapObj) {
        if (markerObj) {
            markerObj.setLatLng([lat, lng]);
        } else {
            // إنشاء علامة جديدة
            var newMarker = L.marker([lat, lng], { draggable: true }).addTo(mapObj);
            newMarker.on('dragend', function() {
                var ll = newMarker.getLatLng();
                setCoordinateFields(
                    document.getElementById(window.currentLatId || 'latitude'),
                    document.getElementById(window.currentLngId || 'longitude'),
                    ll.lat, ll.lng
                );
            });
            window['locationMarker_' + uniqueId] = newMarker;
            window.marker = newMarker;
        }
        
        if (flyTo !== false) {
            mapObj.flyTo([lat, lng], Math.max(mapObj.getZoom(), 16), { duration: 0.8 });
        }
    }
}

/**
 * مسح حقل الرابط
 */
function clearMapsLink(uniqueId) {
    var input = document.getElementById('mapsLink_' + uniqueId);
    var resultDiv = document.getElementById('linkResult_' + uniqueId);
    if (input) input.value = '';
    if (resultDiv) resultDiv.style.display = 'none';
}

/**
 * تهيئة مساعد الإحداثيات لصفحة معينة
 * يجب استدعاء هذه الدالة بعد تهيئة الخريطة
 */
function initCoordinatesHelper(uniqueId, mapInstance, markerInstance, latId, lngId) {
    window['locationMap_' + uniqueId] = mapInstance;
    window['locationMarker_' + uniqueId] = markerInstance;
    window.currentLatId = latId;
    window.currentLngId = lngId;
    
    // ربط حدث النقر على الخريطة لتحديث الحقول
    if (mapInstance) {
        mapInstance.on('click', function(e) {
            setCoordinateFields(latId, lngId, e.latlng.lat, e.latlng.lng);
            // مسح نتيجة الرابط إن وُجدت
            var resultDiv = document.getElementById('linkResult_' + uniqueId);
            if (resultDiv) resultDiv.style.display = 'none';
        });
    }
    
    // ربط حدث سحب العلامة
    if (markerInstance) {
        markerInstance.on('dragend', function() {
            var ll = markerInstance.getLatLng();
            setCoordinateFields(latId, lngId, ll.lat, ll.lng);
        });
    }
}
