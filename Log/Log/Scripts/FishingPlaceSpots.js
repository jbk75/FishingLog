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
            if (!data || data.length === 0) {
                listContainer.html('<p>No fishing spots found.</p>');
                return;
            }

            const table = $('<table class="table table-striped table-bordered"></table>');
            const thead = $('<thead><tr><th>Fishing place</th><th>Fishing spot</th><th>Information</th><th>Last modified (UTC)</th></tr></thead>');
            table.append(thead);

            const tbody = $('<tbody></tbody>');
            $.each(data, function (i, spot) {
                const row = $('<tr></tr>');
                row.append('<td>' + spot.fishingPlaceName + '</td>');
                row.append('<td>' + spot.name + '</td>');
                row.append('<td>' + (spot.description || '') + '</td>');
                row.append('<td>' + (spot.lastModified ? new Date(spot.lastModified).toISOString() : '') + '</td>');
                tbody.append(row);
            });

            table.append(tbody);
            listContainer.html(table);
        },
        error: function () {
            listContainer.html('<p class="text-danger">Unable to load fishing spots.</p>');
        }
    });
}
