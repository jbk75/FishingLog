var APIBaseUrl = 'http://localhost:81/api/';

//var APIBaseUrl = 'http://localhost:15749/api/';


function GetVeidistadir() {
    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            jQuery.each(data, function (i, val) {
                $('#selectVeidiStadir').append($('<option value="' + val.id + '">' + val.name + '</option>'));
            }
            );
            //   console.log(data);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.status);
            console.log(thrownError);
        }
    });
}

function GetVeidiferd(id) {
    if (!id) {
        return;
    }

    $.ajax({
        url: APIBaseUrl + 'veidiferd/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            $("#textVeidiferd").val(data.description);
            var vsid = Number(data.vsId);
            $('#event-modal input[name="event-location"]').val(vsid);
            $("#selectVeidiStadir").val(vsid);
        }
    });
}

function GetVeidiferdir()
{
    console.log('Getting veidiferdir...')
    $.ajax({
        url: APIBaseUrl + 'veidiferd',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            console.log('Done getting veidiferdir!')
            var dataSource = $('#calendar').data('calendar').getDataSource();
            dataSource = [];

            //calendarData = data;
            jQuery.each(data, function (i, val) {
                
                var dFrom = new Date(val.dagsFra);
                var dFromYear = dFrom.getFullYear();
                if (dFromYear === 2017) {
                    console.log('asdfasdf');
                }
                var dFromMonth = dFrom.getMonth();
                var dFromDay = dFrom.getDate();

                var dTo = new Date(val.dagsTil);
                var dToYear = dTo.getFullYear();

                var dToMonth = dTo.getMonth();
                var dToDay = dTo.getDate();

                var event = {
                    id: val.id - 0,
                    name: val.description,
                    location: val.vsId,
                    startDate: new Date(dFromYear, dFromMonth, dFromDay),
                    endDate: new Date(dToYear, dToMonth, dToDay)
                }

                dataSource.push(event);
            });
            $('#calendar').data('calendar').setDataSource(dataSource);
            //   console.log(data);
        }
    });
}

function GetVeidiferdNextId() {
    $.ajax({
        url: APIBaseUrl + 'veidiferdID',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            return data;
        }
    });
}