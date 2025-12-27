$(function () {
    var currentYear = new Date().getFullYear();
    var yearSelect = $('#weatherYearSelect');

    for (var year = currentYear; year >= 1970; year--) {
        yearSelect.append($('<option>', { value: year, text: year }));
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
                row.append($('<td></td>').text(day.condition));
                row.append($('<td></td>').text(windText));
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
        $('#weatherHistoryContainer').html('<span class="fa fa-spinner fa-spin"></span>');

        $.ajax({
            url: APIBaseUrl + 'weather-history',
            type: 'GET',
            dataType: 'json',
            data: { year: selectedYear },
            success: function (data) {
                renderWeatherHistory(data);
            },
            error: function () {
                $('#weatherHistoryContainer').html('<p class="text-danger">Failed to load weather history.</p>');
            }
        });
    }

    yearSelect.on('change', loadWeatherHistory);
    yearSelect.val(currentYear);
    loadWeatherHistory();
});
