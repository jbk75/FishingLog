var tideLocations = [
    { id: 'reykjavik', name: 'Reykjavík', lat: 64.1466, lon: -21.9426 },
    { id: 'akureyri', name: 'Akureyri', lat: 65.6885, lon: -18.1262 }
];

var tideApiConfig = {
    baseUrl: 'https://api.stormglass.io/v2/tide/extremes/point',
    keyStorage: '1a08bf46-ec0d-11f0-a0d3-0242ac130003-1a08bff0-ec0d-11f0-a0d3-0242ac130003'
};

$(document).ready(function () {
    initializeTideYearSelect();
    initializeTideLocationSelect();
    initializeTideApiKeyInput();
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

function initializeTideApiKeyInput() {
    var storedKey = getStoredApiKey();
    var input = $("#tideApiKeyInput");
    if (storedKey) {
        input.val(storedKey);
    }
    input.on('change', function () {
        var value = input.val().trim();
        if (value) {
            localStorage.setItem(tideApiConfig.keyStorage, value);
        } else {
            localStorage.removeItem(tideApiConfig.keyStorage);
        }
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
    var apiKey = tideApiConfig.keyStorage;

    $("#tideYear").text(selectedYear);
    $("#tideLocation").text(selectedLocation.name);
    $("#tideYearSelect").val(selectedYear);
    $("#tideLocationSelect").val(selectedLocation.id);
    $("#tideError").hide().text("");
    $("#tideModel").text("Stormglass");

    if (!apiKey) {
        showTideError('Enter a Stormglass API key to load tide predictions.');
        return;
    }

    var startEpoch = Date.UTC(selectedYear, 0, 1) / 1000;
    var endEpoch = Date.UTC(selectedYear + 1, 0, 1) / 1000 - 1;

    fetchTideEvents(startEpoch, endEpoch, selectedLocation, apiKey)
        .done(function (events) {
            if (!events.length) {
                showTideError('No tide data returned from the API.');
                return;
            }
            renderTides(events);
        })
        .fail(function (xhr) {
            var message = 'Failed to load tide information.';
            if (xhr && xhr.responseText) {
                message += ' ' + xhr.responseText;
            }
            showTideError(message);
        });
}

function fetchTideEvents(startEpoch, endEpoch, location, apiKey) {
    var deferred = $.Deferred();
    var chunkSeconds = 7 * 24 * 60 * 60;
    var currentStart = startEpoch;
    var events = [];
    var seen = {};

    function fetchNext() {
        if (currentStart > endEpoch) {
            deferred.resolve(events);
            return;
        }

        var currentEnd = Math.min(currentStart + chunkSeconds - 1, endEpoch);
        var url = tideApiConfig.baseUrl +
            '?lat=' + location.lat +
            '&lng=' + location.lon +
            '&start=' + currentStart +
            '&end=' + currentEnd;

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            headers: {
                Authorization: apiKey
            },
            success: function (data) {
                if (data && data.data && data.data.length) {
                    data.data.forEach(function (entry) {
                        var key = entry.time + '-' + entry.type;
                        if (!seen[key]) {
                            seen[key] = true;
                            events.push({
                                timestamp: entry.time,
                                level: entry.type === 'high' ? 'High' : 'Low'
                            });
                        }
                    });
                }
                currentStart = currentEnd + 1;
                fetchNext();
            },
            error: function (xhr) {
                deferred.reject(xhr);
            }
        });
    }

    fetchNext();
    return deferred.promise();
}

function renderTides(events) {
    var sortedEvents = events.slice().sort(function (a, b) {
        return new Date(a.timestamp) - new Date(b.timestamp);
    });
    var days = {};
    var order = [];

    jQuery.each(sortedEvents, function (_, item) {
        if (!item || !item.timestamp) {
            return;
        }

        var dateKey = item.timestamp.substring(0, 10);
        if (!days[dateKey]) {
            days[dateKey] = { highs: [], lows: [] };
            order.push(dateKey);
        }

        var timeLabel = formatTideTime(item.timestamp);
        if (item.level === 'High') {
            days[dateKey].highs.push(timeLabel);
        } else {
            days[dateKey].lows.push(timeLabel);
        }
    });

    var rows = [];
    order.forEach(function (dateKey) {
        var entry = days[dateKey];
        rows.push('<tr>' +
            '<td>' + dateKey + '</td>' +
            '<td>' + renderTideTimes(entry.highs, 'high') + '</td>' +
            '<td>' + renderTideTimes(entry.lows, 'low') + '</td>' +
            '</tr>');
    });

    if (!rows.length) {
        rows.push('<tr><td colspan="3">No tide entries found.</td></tr>');
    }

    $("#tideTableBody").html(rows.join(''));
}

function formatTideTime(timestamp) {
    var date = new Date(timestamp);
    return date.toLocaleTimeString('en-GB', {
        hour: '2-digit',
        minute: '2-digit',
        timeZone: 'UTC'
    });
}

function renderTideTimes(times, type) {
    if (!times.length) {
        return '-';
    }

    var iconClass = type === 'high' ? 'fa-arrow-up text-primary' : 'fa-arrow-down text-info';
    var label = type === 'high' ? 'High tide' : 'Low tide';
    var iconHtml = '<span class="fa ' + iconClass + '" aria-hidden="true"></span>' +
        '<span class="sr-only">' + label + '</span>';

    return times.map(function (time) {
        return iconHtml + ' ' + time;
    }).join(', ');
}

function showTideError(message) {
    $("#tideTableBody").html('<tr><td colspan="3">Unable to load tides.</td></tr>');
    $("#tideError").text(message).show();
}
