var tideLocations = [
    { id: 'reykjavik', name: 'Reykjavík' },
    { id: 'seydisfjordur', name: 'Seyðisfjörður' },
    { id: 'raufarhofn', name: 'Raufarhöfn' },
    { id: 'borgarnes', name: 'Borgarnes' },
    { id: 'isafjordur', name: 'Ísafjörður' },
    { id: 'vik-i-myrdal', name: 'Vík í mýrdal' },
    { id: 'klaustur', name: 'Klaustur' },
    { id: 'egilsstadir', name: 'Egilsstaðir' },
    { id: 'blondous', name: 'Blönduós' },
    { id: 'akureyri', name: 'Akureyri' },
    { id: 'budardalur', name: 'Búðardalur' }
];

var monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
];

var weekdayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
var currentTideYear = new Date().getFullYear();
var currentTideMonths = null;

$(document).ready(function () {
    initializeTideYearSelect();
    initializeTideLocationSelect();
    initializeFishingMonthToggle();
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

function initializeFishingMonthToggle() {
    $("#tideFishingMonthsOnly").on('change', function () {
        if (currentTideMonths) {
            renderSpringTideYear(currentTideYear, currentTideMonths);
        }
    });
}

function loadTides(year, location) {
    var selectedYear = year || new Date().getFullYear();
    var selectedLocation = location || tideLocations[0];

    currentTideYear = selectedYear;
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
            currentTideMonths = months;
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
        if (!shouldShowMonth(month)) {
            continue;
        }
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
    var dayRows = [];
    for (var day = 1; day <= daysInMonth; day++) {
        var date = new Date(Date.UTC(year, month - 1, day));
        var weekday = weekdayNames[date.getUTCDay()];
        var springInfo = springDays[day];
        var isSpring = !!springInfo && springInfo.isSpringTide;
        var reason = springInfo && springInfo.reason ? springInfo.reason : '';
        var phase = springInfo ? springInfo.phase : null;
        var phaseIcon = getPhaseIcon(phase);
        var springLabel = isSpring ? 'Stórstraumur' : '';
        dayRows.push(
            '<tr class="tide-day' + (isSpring ? ' tide-day--spring' : '') + '" title="' + escapeHtml(reason) + '">' +
            '<td class="tide-day__date">' + monthNames[month - 1] + ' ' + day + '</td>' +
            '<td class="tide-day__weekday">' + weekday + '</td>' +
            '<td class="tide-day__icon">' + phaseIcon + '</td>' +
            '<td class="tide-day__spring">' + springLabel + '</td>' +
            '</tr>'
        );
    }

    return (
        '<div class="tide-month">' +
        '<div class="tide-month__header">' + monthNames[month - 1] + '</div>' +
        '<table class="tide-month__table">' +
        '<tbody>' + dayRows.join('') + '</tbody>' +
        '</table>' +
        '</div>'
    );
}

function getPhaseIcon(phase) {
    if (phase === null || phase === undefined) {
        return '';
    }
    if (phase === 0 || phase === 'NewMoon') {
        return '<span class="tide-day__icon-marker" aria-label="New moon"><span class="fa fa-circle" aria-hidden="true"></span></span>';
    }
    if (phase === 2 || phase === 'FullMoon') {
        return '<span class="tide-day__icon-marker" aria-label="Full moon"><span class="fa fa-moon-o" aria-hidden="true"></span></span>';
    }
    return '';
}

function shouldShowMonth(month) {
    var fishingOnly = $("#tideFishingMonthsOnly").is(':checked');
    if (!fishingOnly) {
        return true;
    }
    return month >= 4 && month <= 10;
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
