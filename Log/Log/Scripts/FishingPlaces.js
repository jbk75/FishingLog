let placeData = [];
let placeNewsCounts = {};
let selectedPlace = null;

$(document).ready(function () {
    loadFishingPlaces();

    $('#savePlaceBtn').on('click', function () {
        saveFishingPlace();
    });

    $('#refreshPlacesBtn').on('click', function () {
        loadFishingPlaces();
    });

    $('#placeList').on('contextmenu', 'tbody tr', function (event) {
        event.preventDefault();
        const row = $(this);
        const placeId = parseInt(row.data('place-id'), 10);
        if (Number.isNaN(placeId)) {
            return;
        }
        selectedPlace = {
            id: placeId,
            name: row.data('place-name') || 'Fishing place'
        };
        showContextMenu(event.pageX, event.pageY);
    });

    $('#placeContextMenu').on('click', '.place-context-delete', function (event) {
        event.preventDefault();
        hideContextMenu();
        if (!selectedPlace) {
            return;
        }
        openDeleteModal(selectedPlace);
    });

    $('#confirmDeletePlaceBtn').on('click', function () {
        if (!selectedPlace) {
            return;
        }
        deleteFishingPlace(selectedPlace.id);
    });

    $(document).on('click', function () {
        hideContextMenu();
    });

    $(window).on('resize scroll', function () {
        hideContextMenu();
    });
});

function loadFishingPlaces() {
    const listContainer = $('#placeList');
    const placeCount = $('#placeCount');
    listContainer.html('<span class="fa fa-spinner fa-spin"></span>');
    placeCount.text('0');
    placeData = [];
    placeNewsCounts = {};
    selectedPlace = null;

    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            placeData = data || [];

            if (!placeData || placeData.length === 0) {
                listContainer.html('<p>No fishing places found.</p>');
                return;
            }

            placeCount.text(placeData.length);
            loadNewsCounts();
        },
        error: function () {
            listContainer.html('<p class="text-danger">Unable to load fishing places.</p>');
            placeCount.text('0');
        }
    });
}

function loadNewsCounts() {
    $.ajax({
        url: APIBaseUrl + 'fishingnews',
        type: 'GET',
        dataType: 'json'
    }).done(function (newsItems) {
        placeNewsCounts = buildNewsCountMap(newsItems || []);
        renderPlaceTable();
    }).fail(function () {
        placeNewsCounts = {};
        renderPlaceTable();
    });
}

function renderPlaceTable() {
    const listContainer = $('#placeList');

    if (!placeData || placeData.length === 0) {
        listContainer.html('<p>No fishing places found.</p>');
        return;
    }

    const table = $('<table class="table table-striped table-bordered"></table>');
    const thead = $('<thead><tr><th>Name</th><th>Type</th><th>Description</th><th>News count</th></tr></thead>');
    table.append(thead);

    const tbody = $('<tbody></tbody>');
    $.each(placeData, function (i, place) {
        const placeId = place.id || place.Id;
        const placeName = place.name || place.Name || '';
        const row = $('<tr></tr>');
        row.attr('data-place-id', placeId);
        row.attr('data-place-name', placeName);
        row.append('<td>' + placeName + '</td>');
        row.append('<td>' + formatType(place.fishingPlaceTypeID || place.fishingPlaceTypeId || place.FishingPlaceTypeID) + '</td>');
        row.append('<td>' + (place.description || place.Description || '') + '</td>');
        row.append('<td class="text-center">' + getNewsCount(placeId) + '</td>');
        tbody.append(row);
    });

    table.append(tbody);
    listContainer.html(table);
}

function saveFishingPlace() {
    const name = $('#placeName').val().trim();
    const type = $('#placeType').val();
    const description = $('#placeDescription').val().trim();

    $('#placeErrorMessage').hide();
    $('#placeSaveMessage').hide();

    if (!name) {
        $('#placeErrorMessage').text('Please enter a fishing place name.').show();
        return;
    }

    if (!type) {
        $('#placeErrorMessage').text('Please select a fishing place type.').show();
        return;
    }

    const payload = {
        name: name,
        fishingPlaceTypeID: parseInt(type, 10),
        description: description,
        longitude: '',
        latitude: '',
        numberOfSpots: 0
    };

    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function () {
            $('#placeSaveMessage').show();
            $('#placeName').val('');
            $('#placeType').val('');
            $('#placeDescription').val('');
            loadFishingPlaces();
        },
        error: function (xhr) {
            const message = xhr.responseText || 'Unable to save fishing place.';
            $('#placeErrorMessage').text(message).show();
        }
    });
}

function formatType(typeId) {
    if (typeId === 2) return 'River';
    if (typeId === 1) return 'Lake';
    return 'Unknown';
}

function buildNewsCountMap(newsItems) {
    const map = {};
    $.each(newsItems, function (i, news) {
        const placeId = news.fishingPlaceId || news.FishingPlaceId;
        if (!placeId) {
            return;
        }
        if (!map[placeId]) {
            map[placeId] = 0;
        }
        map[placeId] += 1;
    });
    return map;
}

function getNewsCount(placeId) {
    if (!placeId) {
        return 0;
    }
    return placeNewsCounts[placeId] || 0;
}

function showContextMenu(left, top) {
    $('#placeContextMenu')
        .css({ left: left, top: top })
        .show();
}

function hideContextMenu() {
    $('#placeContextMenu').hide();
}

function openDeleteModal(place) {
    $('#placeDeleteModalLabel').text('Delete ' + place.name);
    $('#placeDeleteMessage').text('Checking related records...');
    $('#placeDeleteRelations').empty();
    $('#placeDeleteError').hide().text('');
    $('#confirmDeletePlaceBtn').hide().prop('disabled', true);
    $('#placeDeleteModal').modal('show');

    $.ajax({
        url: APIBaseUrl + 'veidistadur/' + place.id + '/relations',
        type: 'GET',
        dataType: 'json'
    }).done(function (counts) {
        updateDeleteModal(place, counts || {});
    }).fail(function () {
        $('#placeDeleteMessage').text('Unable to check related records for this fishing place.');
        $('#placeDeleteRelations').empty();
        $('#confirmDeletePlaceBtn').hide();
    });
}

function updateDeleteModal(place, counts) {
    const fishingNewsCount = counts.fishingNewsCount || counts.FishingNewsCount || 0;
    const fishingPlaceSpotCount = counts.fishingPlaceSpotCount || counts.FishingPlaceSpotCount || 0;
    const tripCount = counts.tripCount || counts.TripCount || 0;
    const hasRelations = fishingNewsCount > 0 || fishingPlaceSpotCount > 0 || tripCount > 0;

    const list = [
        '<li><strong>Fishing news:</strong> ' + fishingNewsCount + '</li>',
        '<li><strong>Fishing place spots:</strong> ' + fishingPlaceSpotCount + '</li>',
        '<li><strong>Trips:</strong> ' + tripCount + '</li>'
    ];

    $('#placeDeleteRelations').html(list.join(''));

    if (hasRelations) {
        $('#placeDeleteMessage').text('This fishing place is related to other records and cannot be deleted.');
        $('#confirmDeletePlaceBtn').hide();
        return;
    }

    $('#placeDeleteMessage').text('This will permanently delete the fishing place.');
    $('#confirmDeletePlaceBtn').show().prop('disabled', false);
}

function deleteFishingPlace(placeId) {
    $('#confirmDeletePlaceBtn').prop('disabled', true);
    $('#placeDeleteError').hide().text('');

    $.ajax({
        url: APIBaseUrl + 'veidistadur/' + placeId,
        type: 'DELETE'
    }).done(function () {
        $('#placeDeleteModal').modal('hide');
        loadFishingPlaces();
    }).fail(function (xhr) {
        if (xhr.status === 409 && xhr.responseJSON) {
            updateDeleteModal(selectedPlace || { name: 'Fishing place' }, xhr.responseJSON);
            return;
        }
        const message = xhr.responseText || 'Unable to delete fishing place.';
        $('#placeDeleteError').text(message).show();
        $('#confirmDeletePlaceBtn').prop('disabled', false);
    });
}
