$(function () {
    var currentYear = new Date().getFullYear();
    var yearSelect = $('#weatherYearSelect');
    var stationSelect = $('#weatherStationSelect');

    var weatherStations = [
        { name: 'Reykjavík', latitude: 64.1466, longitude: -21.9426 },
        { name: 'Akureyri', latitude: 65.6885, longitude: -18.1262 },
        { name: 'Keflavík', latitude: 63.9850, longitude: -22.6056 },
        { name: 'Ísafjörður', latitude: 66.0749, longitude: -23.1350 },
        { name: 'Egilsstaðir', latitude: 65.2669, longitude: -14.3948 },
        { name: 'Höfn', latitude: 64.2539, longitude: -15.2121 },
        { name: 'Vestmannaeyjar', latitude: 63.4420, longitude: -20.2730 }
    ];

    for (var year = currentYear; year >= 1970; year--) {
        yearSelect.append($('<option>', { value: year, text: year }));
    }

    $.each(weatherStations, function (index, station) {
        stationSelect.append($('<option>', {
            value: index,
            text: station.name,
            'data-latitude': station.latitude,
            'data-longitude': station.longitude
        }));
    });

    function getWindDirectionDegrees(direction) {
        if (!direction) {
            return null;
        }

        var normalized = direction.toUpperCase();
        var directionMap = {
            N: 0,
            NNE: 22.5,
            NE: 45,
            ENE: 67.5,
            E: 90,
            ESE: 112.5,
            SE: 135,
            SSE: 157.5,
            S: 180,
            SSW: 202.5,
            SW: 225,
            WSW: 247.5,
            W: 270,
            WNW: 292.5,
            NW: 315,
            NNW: 337.5
        };

        return directionMap.hasOwnProperty(normalized) ? directionMap[normalized] : null;
    }

    function getConditionIcon(condition) {
        if (!condition) {
            return null;
        }

        var normalized = condition.toLowerCase();
        if (normalized === 'sunny') {
            return $('<span></span>')
                .addClass('fa fa-sun-o weather-condition-icon weather-condition-icon--sunny')
                .attr('aria-hidden', 'true')
                .attr('title', 'Sunny');
        }

        if (normalized === 'rain') {
            return $('<span></span>')
                .addClass('fa fa-umbrella weather-condition-icon weather-condition-icon--rain')
                .attr('aria-hidden', 'true')
                .attr('title', 'Rain');
        }

        return null;
    }

    function renderWeatherHistory(data) {
        var container = $('#weatherHistoryContainer');
        container.empty();

        if (!data || !data.months || data.months.length === 0) {
            container.append('<p class="text-muted">No weather history found for this year.</p>');
            return;
        }

        $.each(data.months, function (index, month) {
            var panel = $('<div class="panel panel-default weather-month"></div>');
            var heading = $('<div class="panel-heading"></div>').text(month.monthName);
            var body = $('<div class="panel-body"></div>');

            var table = $('<table class="table table-striped table-condensed"></table>');
            var thead = $('<thead><tr><th>Date</th><th>Condition</th><th>Wind</th><th>Humidity</th><th>Pressure</th></tr></thead>');
            var tbody = $('<tbody></tbody>');

            $.each(month.days, function (dayIndex, day) {
                var windText = 'N/A';
                var windDirectionDegrees = getWindDirectionDegrees(day.windDirection);
                if (day.windSpeedMax !== null && day.windSpeedMax !== undefined) {
                    windText = day.windSpeedMax.toFixed(1) + ' km/h';
                    if (day.windDirection) {
                        windText += ' ' + day.windDirection;
                    }
                }

                var humidityText = day.relativeHumidityMean !== null && day.relativeHumidityMean !== undefined
                    ? day.relativeHumidityMean.toFixed(0) + '%'
                    : 'N/A';

                var pressureText = day.surfacePressureMean !== null && day.surfacePressureMean !== undefined
                    ? day.surfacePressureMean.toFixed(0) + ' hPa'
                    : 'N/A';

                var row = $('<tr></tr>');
                row.append($('<td></td>').text(day.date));
                var conditionCell = $('<td></td>');
                var conditionIcon = getConditionIcon(day.condition);
                if (conditionIcon) {
                    conditionCell.append(conditionIcon);
                }
                conditionCell.append($('<span></span>').text(day.condition));
                row.append(conditionCell);
                var windCell = $('<td></td>').text(windText);
                if (windDirectionDegrees !== null) {
                    var windIcon = $('<span></span>')
                        .addClass('fa fa-long-arrow-up wind-direction-icon')
                        .attr('aria-hidden', 'true')
                        .css('transform', 'rotate(' + windDirectionDegrees + 'deg)')
                        .attr('title', 'Wind direction: ' + day.windDirection);
                    windCell.append(windIcon);
                }
                row.append(windCell);
                row.append($('<td></td>').text(humidityText));
                row.append($('<td></td>').text(pressureText));
                tbody.append(row);
            });

            table.append(thead);
            table.append(tbody);
            body.append(table);
            panel.append(heading);
            panel.append(body);
            container.append(panel);
        });
    }

    function loadWeatherHistory() {
        var selectedYear = yearSelect.val();
        var selectedStation = stationSelect.find('option:selected');
        var latitude = selectedStation.data('latitude');
        var longitude = selectedStation.data('longitude');
        $('#weatherHistoryContainer').html('<span class="fa fa-spinner fa-spin"></span>');

        $.ajax({
            url: APIBaseUrl + 'weather-history',
            type: 'GET',
            dataType: 'json',
            data: { year: selectedYear, latitude: latitude, longitude: longitude },
            success: function (data) {
                renderWeatherHistory(data);
            },
            error: function () {
                $('#weatherHistoryContainer').html('<p class="text-danger">Failed to load weather history.</p>');
            }
        });
    }

    yearSelect.on('change', loadWeatherHistory);
    stationSelect.on('change', loadWeatherHistory);
    yearSelect.val(currentYear);
    stationSelect.val('0');
    loadWeatherHistory();
});
