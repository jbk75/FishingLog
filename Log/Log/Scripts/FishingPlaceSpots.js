let spotData = [];
let spotSort = { key: 'fishingPlaceName', direction: 'asc' };

$(document).ready(function () {
    loadFishingPlaces();
    loadSpotList();

    $('#spotPlaceSelect').on('change', function () {
        checkSpotExists();
    });

    $('#filterPlaceSelect').on('change', function () {
        const selectedPlaceId = $(this).val();
        loadSpotList(selectedPlaceId);
    });

    $('#spotName').on('blur', function () {
        checkSpotExists();
    });

    $('#saveSpotBtn').on('click', function () {
        saveSpot();
    });

    $('#spotList').on('click', 'th[data-sort]', function () {
        const sortKey = $(this).data('sort');
        if (spotSort.key === sortKey) {
            spotSort.direction = spotSort.direction === 'asc' ? 'desc' : 'asc';
        } else {
            spotSort.key = sortKey;
            spotSort.direction = 'asc';
        }
        renderSpotTable();
    });

    $('#spotList').on('focus', '.spot-description-input', function () {
        $(this).data('original', $(this).val());
    });

    $('#spotList').on('blur', '.spot-description-input', function () {
        const input = $(this);
        const original = input.data('original');
        const current = input.val();
        const spotId = parseInt(input.data('spot-id'), 10);

        if (original === current || Number.isNaN(spotId)) {
            return;
        }

        updateSpotDescription(spotId, current, input);
    });
});

function loadFishingPlaces() {
    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const placeSelect = $('#spotPlaceSelect');
            const filterSelect = $('#filterPlaceSelect');

            placeSelect.empty();
            filterSelect.empty();

            filterSelect.append('<option value="">All fishing places</option>');

            $.each(data, function (i, place) {
                const option = '<option value="' + place.id + '">' + place.name + '</option>';
                placeSelect.append(option);
                filterSelect.append(option);
            });

            checkSpotExists();
        },
        error: function () {
            $('#spotErrorMessage').text('Unable to load fishing places.').show();
        }
    });
}

function checkSpotExists() {
    const fishingPlaceId = $('#spotPlaceSelect').val();
    const name = $('#spotName').val();

    if (!fishingPlaceId || !name) {
        toggleExistsIndicator(false);
        return;
    }

    $.ajax({
        url: APIBaseUrl + 'FishingPlaceSpot/exists',
        type: 'GET',
        data: { fishingPlaceId: fishingPlaceId, name: name },
        success: function (exists) {
            toggleExistsIndicator(exists === true || exists === 'true');
        },
        error: function () {
            toggleExistsIndicator(false);
        }
    });
}

function toggleExistsIndicator(exists) {
    if (exists) {
        $('#spotExistsIndicator').show();
        $('#saveSpotBtn').prop('disabled', true);
    } else {
        $('#spotExistsIndicator').hide();
        $('#saveSpotBtn').prop('disabled', false);
    }
}

function saveSpot() {
    const fishingPlaceId = $('#spotPlaceSelect').val();
    const name = $('#spotName').val();
    const description = $('#spotDescription').val();

    $('#spotErrorMessage').hide();
    $('#spotSaveMessage').hide();

    if (!fishingPlaceId || !name) {
        $('#spotErrorMessage').text('Please select a fishing place and enter a spot name.').show();
        return;
    }

    const payload = {
        fishingPlaceId: parseInt(fishingPlaceId, 10),
        name: name,
        description: description
    };

    $.ajax({
        url: APIBaseUrl + 'FishingPlaceSpot',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function () {
            $('#spotSaveMessage').show();
            $('#spotName').val('');
            $('#spotDescription').val('');
            toggleExistsIndicator(false);
            loadSpotList($('#filterPlaceSelect').val());
        },
        error: function (xhr) {
            const message = xhr.responseText || 'Unable to save fishing spot.';
            $('#spotErrorMessage').text(message).show();
        }
    });
}

function loadSpotList(fishingPlaceId) {
    const listContainer = $('#spotList');
    listContainer.html('<span class="fa fa-spinner fa-spin"></span>');

    const url = fishingPlaceId ? APIBaseUrl + 'FishingPlaceSpot?fishingPlaceId=' + fishingPlaceId : APIBaseUrl + 'FishingPlaceSpot';

    $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            spotData = data || [];
            renderSpotTable();
        },
        error: function () {
            listContainer.html('<p class="text-danger">Unable to load fishing spots.</p>');
        }
    });
}

function renderSpotTable() {
    const listContainer = $('#spotList');

    if (!spotData || spotData.length === 0) {
        listContainer.html('<p>No fishing spots found.</p>');
        return;
    }

    const sortedSpots = getSortedSpots(spotData, spotSort);
    const table = $('<table class="table table-striped table-bordered"></table>');
    const thead = $('<thead></thead>');
    const headerRow = $('<tr></tr>');
    const headers = [
        { key: 'fishingPlaceName', label: 'Fishing place' },
        { key: 'name', label: 'Fishing spot' },
        { key: 'description', label: 'Information' },
        { key: 'lastModified', label: 'Last modified (UTC)' }
    ];

    $.each(headers, function (i, header) {
        const indicator = getSortIndicator(header.key);
        headerRow.append('<th data-sort="' + header.key + '" role="button">' + header.label + indicator + '</th>');
    });

    thead.append(headerRow);
    table.append(thead);

    const tbody = $('<tbody></tbody>');
    $.each(sortedSpots, function (i, spot) {
        const row = $('<tr></tr>');
        row.append('<td>' + spot.fishingPlaceName + '</td>');
        row.append('<td>' + spot.name + '</td>');

        const descriptionCell = $('<td></td>');
        const descriptionInput = $('<textarea class="form-control spot-description-input" rows="2"></textarea>');
        descriptionInput.val(spot.description || '');
        descriptionInput.attr('data-spot-id', spot.id);
        descriptionCell.append(descriptionInput);
        descriptionCell.append('<div class="text-muted small spot-description-status" style="margin-top:4px;"></div>');
        row.append(descriptionCell);

        const lastModifiedText = spot.lastModified ? new Date(spot.lastModified).toISOString() : '';
        row.append('<td class="spot-last-modified">' + lastModifiedText + '</td>');
        tbody.append(row);
    });

    table.append(tbody);
    listContainer.html(table);
}

function getSortIndicator(key) {
    if (spotSort.key !== key) {
        return '';
    }
    return spotSort.direction === 'asc' ? ' <span class="fa fa-sort-asc"></span>' : ' <span class="fa fa-sort-desc"></span>';
}

function getSortedSpots(data, sort) {
    const direction = sort.direction === 'asc' ? 1 : -1;
    return data.slice().sort(function (a, b) {
        const aValue = getSortValue(a, sort.key);
        const bValue = getSortValue(b, sort.key);

        if (aValue < bValue) {
            return -1 * direction;
        }
        if (aValue > bValue) {
            return 1 * direction;
        }
        return 0;
    });
}

function getSortValue(spot, key) {
    if (key === 'lastModified') {
        return spot.lastModified ? new Date(spot.lastModified).getTime() : 0;
    }
    if (key === 'description') {
        return (spot.description || '').toLowerCase();
    }
    if (key === 'name') {
        return spot.name.toLowerCase();
    }
    return spot.fishingPlaceName.toLowerCase();
}

function updateSpotDescription(spotId, description, input) {
    const row = input.closest('tr');
    const status = row.find('.spot-description-status');

    status.removeClass('text-danger').text('Saving...');

    $.ajax({
        url: APIBaseUrl + 'FishingPlaceSpot/' + spotId + '/description',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ description: description }),
        success: function (updatedSpot) {
            const updated = spotData.find(function (spot) {
                return spot.id === spotId;
            });
            if (updated) {
                updated.description = updatedSpot.description;
                updated.lastModified = updatedSpot.lastModified;
            }

            row.find('.spot-last-modified').text(updatedSpot.lastModified ? new Date(updatedSpot.lastModified).toISOString() : '');
            input.data('original', updatedSpot.description || '');
            status.text('Saved');
            setTimeout(function () {
                status.text('');
            }, 1500);
        },
        error: function (xhr) {
            const message = xhr.responseText || 'Unable to update description.';
            status.addClass('text-danger').text(message);
        }
    });
}
