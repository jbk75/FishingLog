$(document).ready(function () {
    loadSuggestedTrips();
});

function loadSuggestedTrips() {
    $.getJSON(APIBaseUrl + 'suggestedtrips')
        .done(function (response) {
            var suggestions = response.suggestions || response.Suggestions || [];
            var seasonYear = response.seasonYear || response.SeasonYear || new Date().getFullYear();
            renderSuggestions(suggestions, seasonYear);
        })
        .fail(function () {
            $('#suggestedTripsList').html('<div class="alert alert-danger">Unable to load suggested trips right now.</div>');
        });
}

function renderSuggestions(suggestions, year) {
    if (!suggestions || suggestions.length === 0) {
        $('#suggestedTripsList').html('<div class="alert alert-warning">No suggestions available for ' + year + ' yet.</div>');
        return;
    }

    var rows = '';
    $.each(suggestions, function (i, suggestion) {
        var dateValue = parseSuggestedDate(suggestion.date || suggestion.Date);
        if (!dateValue) {
            return;
        }

        rows += '<tr>' +
            '<td>' + (suggestion.placeName || suggestion.PlaceName || 'Unknown place') + '</td>' +
            '<td>' + formatSuggestedDate(dateValue) + '</td>' +
            '<td>' + (suggestion.reason || suggestion.Reason || '') + '</td>' +
            '</tr>';
    });

    var table = '<table class="table table-striped">' +
        '<thead><tr><th>Fishing place</th><th>Suggested date</th><th>Why this day</th></tr></thead>' +
        '<tbody>' + rows + '</tbody>' +
        '</table>';

    $('#suggestedTripsList').html(table);
}

function formatSuggestedDate(date) {
    var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var day = date.getDate();
    var monthName = months[date.getMonth()];
    var year = date.getFullYear();

    return day + '. ' + monthName + ' ' + year;
}

function parseSuggestedDate(dateValue) {
    if (!dateValue) {
        return null;
    }

    var parsed = new Date(dateValue);
    if (isNaN(parsed.getTime())) {
        return null;
    }

    return parsed;
}
