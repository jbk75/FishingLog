$(document).ready(function () {
    loadSuggestedTrips();
});

function loadSuggestedTrips() {
    var currentYear = new Date().getFullYear();
    var tripsRequest = $.getJSON(APIBaseUrl + 'veidiferd');
    var newsRequest = $.getJSON(APIBaseUrl + 'fishingnews');
    var placesRequest = $.getJSON(APIBaseUrl + 'veidistadur');

    $.when(tripsRequest, newsRequest, placesRequest)
        .done(function (tripsResponse, newsResponse, placesResponse) {
            var trips = tripsResponse[0] || [];
            var newsItems = newsResponse[0] || [];
            var places = placesResponse[0] || [];

            var placeMap = buildPlaceMap(places);
            var grouped = buildDateGroups(trips, newsItems);
            var suggestions = buildSuggestions(grouped, placeMap, currentYear);

            renderSuggestions(suggestions, currentYear);
        })
        .fail(function () {
            $('#suggestedTripsList').html('<div class="alert alert-danger">Unable to load suggested trips right now.</div>');
        });
}

function buildPlaceMap(places) {
    var map = {};
    $.each(places, function (i, place) {
        var id = place.id || place.Id;
        if (id !== undefined && id !== null) {
            map[id] = place.name || place.Name || 'Unknown place';
        }
    });
    return map;
}

function buildDateGroups(trips, newsItems) {
    var grouped = {};

    $.each(trips, function (i, trip) {
        var placeId = trip.vsId || trip.VsId;
        var dateValue = trip.dagsFra || trip.DagsFra;
        var description = trip.description || trip.Description || '';
        addEntry(grouped, placeId, dateValue, description, 'trip');
    });

    $.each(newsItems, function (i, news) {
        var placeId = news.fishingPlaceId || news.FishingPlaceId;
        var dateValue = news.date || news.Date;
        var description = news.description || news.Description || '';
        addEntry(grouped, placeId, dateValue, description, 'news');
    });

    return grouped;
}

function addEntry(grouped, placeId, dateValue, description, source) {
    if (!placeId || !dateValue) {
        return;
    }

    var date = new Date(dateValue);
    if (isNaN(date.getTime())) {
        return;
    }

    var month = date.getMonth() + 1;
    var day = date.getDate();
    var key = month + '-' + day;

    if (!grouped[placeId]) {
        grouped[placeId] = {};
    }

    if (!grouped[placeId][key]) {
        grouped[placeId][key] = {
            month: month,
            day: day,
            score: 0,
            tripCount: 0,
            newsCount: 0,
            descriptions: []
        };
    }

    var entry = grouped[placeId][key];
    entry.score += 1 + getDescriptionBoost(description);

    if (source === 'trip') {
        entry.tripCount += 1;
    } else {
        entry.newsCount += 1;
    }

    if (description) {
        entry.descriptions.push(description);
    }
}

function getDescriptionBoost(description) {
    if (!description) {
        return 0;
    }

    var text = description.toLowerCase();
    var keywords = ['good', 'great', 'best', 'many', 'lots', 'plenty', 'caught', 'catch', 'fish', 'fishing', 'lax', 'silung', 'bleik', 'tide', 'high'];
    var boost = 0;

    $.each(keywords, function (i, keyword) {
        if (text.indexOf(keyword) !== -1) {
            boost += 1;
        }
    });

    if (text.match(/\d+/)) {
        boost += 1;
    }

    return boost;
}

function buildSuggestions(grouped, placeMap, year) {
    var suggestions = [];

    $.each(grouped, function (placeId, entries) {
        var bestEntry = null;

        $.each(entries, function (key, entry) {
            if (!bestEntry || entry.score > bestEntry.score || (entry.score === bestEntry.score && entry.tripCount + entry.newsCount > bestEntry.tripCount + bestEntry.newsCount)) {
                bestEntry = entry;
            }
        });

        if (!bestEntry) {
            return;
        }

        var suggestedDate = createDateInYear(bestEntry.month, bestEntry.day, year);
        var snippet = getDescriptionSnippet(bestEntry.descriptions);
        var reason = buildReason(bestEntry, snippet);

        suggestions.push({
            placeId: placeId,
            placeName: placeMap[placeId] || 'Unknown place',
            date: suggestedDate,
            reason: reason
        });
    });

    suggestions.sort(function (a, b) {
        return a.date - b.date;
    });

    return suggestions;
}

function createDateInYear(month, day, year) {
    var date = new Date(year, month - 1, day);
    if (date.getMonth() !== month - 1) {
        return new Date(year, month, 0);
    }
    return date;
}

function getDescriptionSnippet(descriptions) {
    if (!descriptions || descriptions.length === 0) {
        return '';
    }

    var snippet = $.grep(descriptions, function (description) {
        return description && description.trim().length > 0;
    })[0];

    if (!snippet) {
        return '';
    }

    snippet = snippet.replace(/\s+/g, ' ').trim();
    if (snippet.length > 140) {
        snippet = snippet.substring(0, 137) + '...';
    }

    return snippet;
}

function buildReason(entry, snippet) {
    var tripLabel = entry.tripCount === 1 ? 'trip log entry' : 'trip log entries';
    var newsLabel = entry.newsCount === 1 ? 'fishing news report' : 'fishing news reports';

    var reason = 'Based on ' + entry.tripCount + ' ' + tripLabel + ' and ' + entry.newsCount + ' ' + newsLabel + ' around this time.';

    if (snippet) {
        reason += ' Notes mention: "' + snippet + '".';
    }

    return reason;
}

function renderSuggestions(suggestions, year) {
    if (!suggestions || suggestions.length === 0) {
        $('#suggestedTripsList').html('<div class="alert alert-warning">No suggestions available for ' + year + ' yet.</div>');
        return;
    }

    var rows = '';
    $.each(suggestions, function (i, suggestion) {
        rows += '<tr>' +
            '<td>' + suggestion.placeName + '</td>' +
            '<td>' + formatSuggestedDate(suggestion.date) + '</td>' +
            '<td>' + suggestion.reason + '</td>' +
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
