var APIBaseUrl = 'http://localhost/fishinglog/api/';

function GetVeidistadir()
{
    $.ajax({
        url: APIBaseUrl + 'veidistadur',
        type: 'GET',
        dataType: 'json',
        success: function (data)
        {
            jQuery.each(data, function (i, val)
            {
                $('#selectVeidiStadir').append($('<option value="' + val.vsId + '">' + val.heiti + '</option>'));
            });
            //   console.log(data);
        }
    });
}

function GetVeidiferd(id)
{
    $.ajax({
        url: APIBaseUrl + 'veidiferd/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (data)
        {
            $("#textVeidiferd").val(data.lysingLong)
            var vsid = Number(data.vsId);
            $('#event-modal input[name="event-location"]').val(vsid);
            $("#selectVeidiStadir").val(vsid);
        }
    });
}

function GetVeidiferdir()
{
    $.ajax({
        url: APIBaseUrl + 'veidiferd',
        type: 'GET',
        dataType: 'json',
        success: function (data)
        {
            var dataSource = $('#calendar').data('calendar').getDataSource();
            dataSource = [];
            
            //calendarData = data;
            jQuery.each(data, function (i, val)
            {
                //$('#selectVeidiStadir').append($('<option value="' + val.vsId + '">' + val.lysing + '</option>'));
                var dFrom = new Date(val.dagsFra);
                var dFromYear = dFrom.getFullYear();
                if (dFromYear === 2017)
                {
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
                    name: val.lysing,
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

function GetVeidiferdNextId()
{
    $.ajax({
        url: APIBaseUrl + 'veidiferdID',
        type: 'GET',
        dataType: 'json',
        success: function (data)
        {
            return data;
        }
    });
}