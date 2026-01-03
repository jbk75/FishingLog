$(document).ready(function () {
    initializeFishingNewsForm();
    loadFishingPlaces();
    loadFishingNews();

    $('#saveFishingNewsBtn').on('click', function () {
        saveFishingNews();
    });

    $('#refreshFishingNewsBtn').on('click', function () {
        loadFishingNews();
    });

    $('#fishingNewsList').on('click', '#fishingNewsTable thead th.sortable', function () {
        var newKey = $(this).data('sort');
        if (fishingNewsSortState.key === newKey) {
            fishingNewsSortState.direction = fishingNewsSortState.direction === 'asc' ? 'desc' : 'asc';
        } else {
            fishingNewsSortState.key = newKey;
            fishingNewsSortState.direction = 'asc';
        }
        renderFishingNews();
    });

    $('#fishingNewsList').on('click', '#fishingNewsTable tbody .delete-news', function () {
        var id = Number($(this).data('id'));
        if (!id) {
            return;
        }
        if (!confirm('Delete this fishing news entry?')) {
            return;
        }
        deleteFishingNews(id);
    });
});

var fishingNewsData = [];
var fishingNewsSortState = {
    key: 'date',
    direction: 'desc'
};

function initializeFishingNewsForm() {
    var today = new Date();
    $('#newsDate').val(formatDateForInput(today));
}

function loadFishingPlaces() {
    var select = $('#newsFishingPlace');
    select.empty();
    select.append('<option value="">-- Select fishing place --</option>');

    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            jQuery.each(data, function (i, val) {
                var option = $('<option>')
                    .attr('value', val.id)
                    .text(val.name);
                select.append(option);
            });
        },
        error: function () {
            showFishingNewsError('Failed to load fishing places.');
        }
    });
}

function loadFishingNews() {
    $('#fishingNewsList').html('<span class="fa fa-spinner fa-spin"></span>');
    $.ajax({
        url: APIBaseUrl + 'fishingnews',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            fishingNewsData = data || [];
            renderFishingNews();
        },
        error: function () {
            $('#fishingNewsList').html('<div class="alert alert-danger">Unable to load fishing news.</div>');
        }
    });
}

function renderFishingNews() {
    var container = $('#fishingNewsList');

    if (!fishingNewsData || fishingNewsData.length === 0) {
        container.html('<div class="alert alert-warning">No fishing news entries found.</div>');
        updateFishingNewsCount(0);
        updateSortIndicators();
        return;
    }

    var sortedData = fishingNewsData.slice().sort(function (a, b) {
        var key = fishingNewsSortState.key;
        var direction = fishingNewsSortState.direction === 'asc' ? 1 : -1;
        var valueA = getSortValue(a, key);
        var valueB = getSortValue(b, key);

        if (valueA < valueB) return -1 * direction;
        if (valueA > valueB) return 1 * direction;
        return 0;
    });

    var rows = '';
    jQuery.each(sortedData, function (i, val) {
        var dateValue = parseDateValue(val.date || val.Date);
        rows += '<tr>' +
            '<td>' + (val.id || val.Id || '') + '</td>' +
            '<td>' + (val.fishingPlaceName || val.FishingPlaceName || 'Unknown') + '</td>' +
            '<td>' + (dateValue ? formatDateForDisplay(dateValue) : '') + '</td>' +
            '<td>' + escapeHtml(val.description || val.Description || '') + '</td>' +
            '<td><button class="btn btn-xs btn-danger delete-news" data-id="' + (val.id || val.Id) + '">Delete</button></td>' +
            '</tr>';
    });

    var table = '<table class="table table-striped" id="fishingNewsTable">' +
        '<thead><tr>' +
        '<th class="sortable" data-sort="id">Id<span class="sort-indicator"></span></th>' +
        '<th class="sortable" data-sort="fishingPlaceName">Fishing place<span class="sort-indicator"></span></th>' +
        '<th class="sortable" data-sort="date">Date<span class="sort-indicator"></span></th>' +
        '<th class="sortable" data-sort="description">Description<span class="sort-indicator"></span></th>' +
        '<th>Actions</th>' +
        '</tr></thead>' +
        '<tbody>' + rows + '</tbody>' +
        '</table>';

    container.html(table);
    updateFishingNewsCount(sortedData.length);
    updateSortIndicators();
}

function getSortValue(item, key) {
    if (key === 'id') {
        return Number(item.id || item.Id || 0);
    }
    if (key === 'fishingPlaceName') {
        return (item.fishingPlaceName || item.FishingPlaceName || '').toLowerCase();
    }
    if (key === 'date') {
        var dateValue = parseDateValue(item.date || item.Date);
        return dateValue ? dateValue.getTime() : 0;
    }
    if (key === 'description') {
        return (item.description || item.Description || '').toLowerCase();
    }
    return '';
}

function updateSortIndicators() {
    $('#fishingNewsTable thead th.sortable').each(function () {
        var indicator = $(this).find('.sort-indicator');
        if ($(this).data('sort') === fishingNewsSortState.key) {
            indicator.text(fishingNewsSortState.direction === 'asc' ? '▲' : '▼');
        } else {
            indicator.text('');
        }
    });
}

function saveFishingNews() {
    clearFishingNewsMessages();
    var fishingPlaceId = Number($('#newsFishingPlace').val());
    var dateValue = $('#newsDate').val();
    var description = $('#newsDescription').val().trim();

    if (!fishingPlaceId) {
        showFishingNewsError('Please select a fishing place.');
        return;
    }

    if (!dateValue) {
        showFishingNewsError('Please choose a date.');
        return;
    }

    var payload = {
        fishingPlaceId: fishingPlaceId,
        date: dateValue,
        description: description
    };

    $.ajax({
        url: APIBaseUrl + 'fishingnews',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function () {
            $('#newsDescription').val('');
            $('#newsDate').val(formatDateForInput(new Date()));
            $('#newsFishingPlace').val('');
            showFishingNewsSuccess('Saved fishing news entry.');
            loadFishingNews();
        },
        error: function () {
            showFishingNewsError('Unable to save fishing news entry.');
        }
    });
}

function deleteFishingNews(id) {
    $.ajax({
        url: APIBaseUrl + 'fishingnews/' + id,
        type: 'DELETE',
        success: function () {
            showFishingNewsSuccess('Deleted fishing news entry.');
            fishingNewsData = fishingNewsData.filter(function (entry) {
                return (entry.id || entry.Id) !== id;
            });
            renderFishingNews();
        },
        error: function () {
            showFishingNewsError('Unable to delete fishing news entry.');
        }
    });
}

function updateFishingNewsCount(count) {
    $('#fishingNewsCount').text(count);
}

function showFishingNewsSuccess(message) {
    $('#fishingNewsSaveMessage').text(message).show();
    $('#fishingNewsErrorMessage').hide();
}

function showFishingNewsError(message) {
    $('#fishingNewsErrorMessage').text(message).show();
    $('#fishingNewsSaveMessage').hide();
}

function clearFishingNewsMessages() {
    $('#fishingNewsSaveMessage').hide();
    $('#fishingNewsErrorMessage').hide();
}

function parseDateValue(value) {
    if (!value) {
        return null;
    }
    var parsed = new Date(value);
    if (isNaN(parsed.getTime())) {
        return null;
    }
    return parsed;
}

function formatDateForDisplay(date) {
    return date.toISOString().split('T')[0];
}

function formatDateForInput(date) {
    return date.toISOString().split('T')[0];
}

function escapeHtml(text) {
    return $('<div>').text(text).html();
}
