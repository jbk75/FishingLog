$(document).ready(function () {
    loadTides();
});

function loadTides() {
    var year = new Date().getFullYear();
    $("#tideYear").text(year);
    $("#tideError").hide().text("");

    $.ajax({
        url: APIBaseUrl + 'tides/' + year,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            if (!data || !data.events) {
                showTideError('No tide data returned from the API.');
                return;
            }

            $("#tideModel").text(data.model || 'N/A');
            renderTides(data.events);
        },
        error: function (xhr) {
            var message = 'Failed to load tide information.';
            if (xhr && xhr.responseText) {
                message += ' ' + xhr.responseText;
            }
            showTideError(message);
        }
    });
}

function renderTides(events) {
    var days = {};
    var order = [];

    jQuery.each(events, function (_, item) {
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
            '<td>' + (entry.highs.length ? entry.highs.join(', ') : '-') + '</td>' +
            '<td>' + (entry.lows.length ? entry.lows.join(', ') : '-') + '</td>' +
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

function showTideError(message) {
    $("#tideTableBody").html('<tr><td colspan="3">Unable to load tides.</td></tr>');
    $("#tideError").text(message).show();
}
