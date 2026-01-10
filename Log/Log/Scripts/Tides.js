var tideLocations = [
    { id: 'reykjavik', name: 'Reykjavik' }
];

var monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
];

var weekdayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

$(document).ready(function () {
    initializeTideYearSelect();
    initializeTideLocationSelect();
    loadTides(getSelectedTideYear(), getSelectedLocation());
});

function initializeTideYearSelect() {
    var currentYear = new Date().getFullYear();
    var startYear = currentYear - 5;
    var endYear = currentYear + 1;
    var options = [];

    for (var year = startYear; year <= endYear; year++) {
        options.push('<option value="' + year + '">' + year + '</option>');
    }

    var select = $("#tideYearSelect");
    select.html(options.join(''));
    select.val(currentYear);
    select.on('change', function () {
        loadTides(getSelectedTideYear(), getSelectedLocation());
    });
}

function getSelectedTideYear() {
    var selected = parseInt($("#tideYearSelect").val(), 10);
    if (isNaN(selected)) {
        return new Date().getFullYear();
    }
    return selected;
}

function initializeTideLocationSelect() {
    var select = $("#tideLocationSelect");
    var options = tideLocations.map(function (location) {
        return '<option value="' + location.id + '">' + location.name + '</option>';
    });
    select.html(options.join(''));
    select.val(tideLocations[0].id);
    select.on('change', function () {
        loadTides(getSelectedTideYear(), getSelectedLocation());
    });
}

function getSelectedLocation() {
    var selectedId = $("#tideLocationSelect").val();
    for (var i = 0; i < tideLocations.length; i++) {
        if (tideLocations[i].id === selectedId) {
            return tideLocations[i];
        }
    }
    return tideLocations[0];
}

function loadTides(year, location) {
    var selectedYear = year || new Date().getFullYear();
    var selectedLocation = location || tideLocations[0];

    $("#tideYear").text(selectedYear);
    $("#tideLocation").text(selectedLocation.name);
    $("#tideYearSelect").val(selectedYear);
    $("#tideLocationSelect").val(selectedLocation.id);
    $("#tideError").hide().text("");
    $("#tideModel").text("SpringTides");
    $("#tideMonthGrid").html('<div class="tide-loading"><span class="fa fa-spinner fa-spin"></span> Loading tides...</div>');

    fetchSpringTideYear(selectedYear, selectedLocation)
        .done(function () {
            var months = {};
            for (var i = 0; i < arguments.length; i++) {
                months[i + 1] = arguments[i][0];
            }
            renderSpringTideYear(selectedYear, months);
        })
        .fail(function (xhr) {
            var message = 'Failed to load tide information.';
            if (xhr && xhr.responseText) {
                message += ' ' + xhr.responseText;
            }
            showTideError(message);
        });
}

function fetchSpringTideYear(year, location) {
    var requests = [];
    for (var month = 1; month <= 12; month++) {
        requests.push(fetchSpringTideMonth(year, month, location));
    }
    return $.when.apply($, requests);
}

function fetchSpringTideMonth(year, month, location) {
    return $.ajax({
        url: APIBaseUrl + 'spring-tides/' + year + '/' + month + '?location=' + encodeURIComponent(location.name),
        type: 'GET',
        dataType: 'json'
    });
}

function renderSpringTideYear(year, months) {
    var monthCards = [];
    for (var month = 1; month <= 12; month++) {
        var monthData = months[month] || [];
        var springDays = buildSpringDayMap(monthData);
        monthCards.push(renderMonthCard(year, month, springDays));
    }
    $("#tideMonthGrid").html(monthCards.join(''));
}

function buildSpringDayMap(monthData) {
    var map = {};
    jQuery.each(monthData, function (_, item) {
        if (!item || !item.date) {
            return;
        }
        var day = parseInt(item.date.substring(8, 10), 10);
        if (!isNaN(day)) {
            map[day] = item;
        }
    });
    return map;
}

function renderMonthCard(year, month, springDays) {
    var daysInMonth = new Date(year, month, 0).getDate();
    var dayCells = [];
    for (var day = 1; day <= daysInMonth; day++) {
        var date = new Date(Date.UTC(year, month - 1, day));
        var weekday = weekdayNames[date.getUTCDay()];
        var springInfo = springDays[day];
        var isSpring = !!springInfo && springInfo.isSpringTide;
        var reason = springInfo && springInfo.reason ? springInfo.reason : '';
        dayCells.push(
            '<div class="tide-day' + (isSpring ? ' tide-day--spring' : '') + '" title="' + escapeHtml(reason) + '">' +
            '<span class="tide-day__number">' + day + '</span>' +
            '<span class="tide-day__weekday">' + weekday + '</span>' +
            '<span class="tide-day__marker" aria-hidden="true"></span>' +
            '</div>'
        );
    }

    return (
        '<div class="tide-month" style="--month-days:' + daysInMonth + ';">' +
        '<div class="tide-month__header">' + monthNames[month - 1] + '</div>' +
        '<div class="tide-month__days">' + dayCells.join('') + '</div>' +
        '<div class="tide-month__chart">' +
        '<div class="tide-wave">' + buildWaveSvg(daysInMonth) + '</div>' +
        '</div>' +
        '</div>'
    );
}

function buildWaveSvg(daysInMonth) {
    var dayWidth = 12;
    var viewWidth = Math.max(1, daysInMonth) * dayWidth;
    var midY = 42;
    var peakY = 12;
    var troughY = 72;
    var path = 'M0 ' + midY;
    var gridLines = [];

    for (var day = 0; day < daysInMonth; day++) {
        var x0 = day * dayWidth;
        var x1 = x0 + dayWidth / 2;
        var x2 = x0 + dayWidth;
        var controlY = day % 2 === 0 ? peakY : troughY;
        path += ' Q ' + x1 + ' ' + controlY + ' ' + x2 + ' ' + midY;
        gridLines.push('<line x1="' + x2 + '" y1="0" x2="' + x2 + '" y2="84" />');
    }

    return '<svg viewBox="0 0 ' + viewWidth + ' 84" preserveAspectRatio="none" aria-hidden="true">' +
        '<g class="tide-wave__grid">' + gridLines.join('') + '</g>' +
        '<path class="tide-wave__path" d="' + path + '" />' +
        '</svg>';
}

function escapeHtml(value) {
    return String(value || '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function showTideError(message) {
    $("#tideMonthGrid").html('<div class="tide-loading">Unable to load tides.</div>');
    $("#tideError").text(message).show();
}
