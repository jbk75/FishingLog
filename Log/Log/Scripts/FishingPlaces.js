$(document).ready(function () {
    loadFishingPlaces();

    $('#savePlaceBtn').on('click', function () {
        saveFishingPlace();
    });
});

function loadFishingPlaces() {
    const listContainer = $('#placeList');
    listContainer.html('<span class="fa fa-spinner fa-spin"></span>');

    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            if (!data || data.length === 0) {
                listContainer.html('<p>No fishing places found.</p>');
                return;
            }

            const table = $('<table class="table table-striped table-bordered"></table>');
            const thead = $('<thead><tr><th>Name</th><th>Type</th><th>Description</th></tr></thead>');
            table.append(thead);

            const tbody = $('<tbody></tbody>');
            $.each(data, function (i, place) {
                const row = $('<tr></tr>');
                row.append('<td>' + place.name + '</td>');
                row.append('<td>' + formatType(place.fishingPlaceTypeID || place.fishingPlaceTypeId) + '</td>');
                row.append('<td>' + (place.description || '') + '</td>');
                tbody.append(row);
            });

            table.append(tbody);
            listContainer.html(table);
        },
        error: function () {
            listContainer.html('<p class="text-danger">Unable to load fishing places.</p>');
        }
    });
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
