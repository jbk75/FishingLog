//var APIBaseUrl = 'http://localhost/fishinglog/api/';


//var APIBaseUrl = 'https://log20170517012354.azurewebsites.net/api/';

//var APIBaseUrl = 'http://localhost:15749/api/';
var APIBaseUrl = 'https://fishingloggerapi20190710081131.azurewebsites.net/api/';

function GetVeidistadir() {
    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            jQuery.each(data, function (i, val) {
                $('#selectVeidiStadir').append($('<option value="' + val.VsId + '">' + val.Heiti + '</option>'));
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
            $("#textVeidiferd").val(data.Lysing);
            var vsid = Number(data.VsId);
            $('#event-modal input[name="event-location"]').val(Vsid);
            $("#selectVeidiStadir").val(data.VsId);
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
                //$('#selectVeidiStadir').append($('<option value="' + val.vsId + '">' + val.lysing + '</option>'));
                var dFrom = new Date(val.DagsFra);
                var dFromYear = dFrom.getFullYear();
                if (dFromYear === 2017) {
                    console.log('asdfasdf');
                }
                var dFromMonth = dFrom.getMonth();
                var dFromDay = dFrom.getDate();

                var dTo = new Date(val.DagsTil);
                var dToYear = dTo.getFullYear();

                var dToMonth = dTo.getMonth();
                var dToDay = dTo.getDate();

                var event = {
                    id: val.Id - 0,
                    name: val.Lysing,
                    location: val.VsId,
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