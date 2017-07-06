$('.datetime').datetimepicker({
    locale: 'vi',
    format: 'MM/DD/YYYY',
    defaultDate: new Date()
});




function removeProduct(eventId, productId) {

    $.ajax({
        type: "GET",
        url: "/eventmanage/RemoveEventProduct",
        data: { eventId: eventId, productId: productId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var id = '#rowproduct' + productId;
                $(id).remove();
            }
        }
    });
}

function removeAward(awardId) {

    $.ajax({
        type: "GET",
        url: "/eventmanage/removeEventAward",
        data: { awardId: awardId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var id = '#rowaward' + awardId;
                $(id).remove();
            }
        }
    });
}

function addArea(eventId) {

    var areaId = $('#choosearea').val();
    console.log(eventId);
    $.ajax({
        type: "GET",
        url: "/eventmanage/addeventarea",
        data: { eventId: eventId, areaId: areaId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var tr = $("<tr/>");
                tr.attr({ id: 'rowarea' + response.id });
                $("<td/>").html(response.name).appendTo(tr);
                $("<td/>").html(response.code).appendTo(tr);
                $("<td/>").html(response.notes).appendTo(tr);
                var eId = "'" + eventId + "'";
                var aId = response.id;
                $("<td/>").html('<a class="btn btn-xs btn-secondary" href="javascript:removeArea(' + eId + ', ' + aId + ')"><i class="fa fa-trash"></i></a> &nbsp;&nbsp;<a class="btn btn-xs btn-secondary" href="/eventmanage/eventareamodify?eventId=' + eventId + '&areaId=' + aId + '" target="_blank"><i class="fa fa-plus"></i></a>').appendTo(tr);
                tr.appendTo("tbody#resultarea");
            }
        }
    });
}

function removeArea(eventId, areaId) {

    $.ajax({
        type: "GET",
        url: "/eventmanage/removeeventarea",
        data: { eventId: eventId, areaId: areaId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var id = '#rowarea' + areaId;
                $(id).remove();
            }
        }
    });
}


// award
function addAward(eventId) {

    var awardId = $('#chooseaward').val();

    $.ajax({
        type: "GET",
        url: "/eventmanage/AddEventAward",
        data: { eventId: eventId, awardId: awardId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var tr = $("<tr/>");
                tr.attr({ id: 'rowaward' + response.id });
                $("<td/>").html(response.name).appendTo(tr);
                $("<td/>").html(response.point).appendTo(tr);
                $("<td/>").html(' <div class="thumbnail"><div class="thumbnail-view"><a href="' + response.image + '" class="image-link" title="' + response.name + '"> <img src="' + response.image + '" alt="' + response.name + '" class="img-responsive" width="125" /></a></div></div> ').appendTo(tr);
                var eId = "'" + eventId + "'";
                var aId = "'" + response.id + "'";
                $("<td/>").html('<a class="btn btn-xs btn-secondary" href="javascript:removeAward(' + eId + ', ' + aId + ')"><i class="fa fa-trash"></i></a>').appendTo(tr);
                tr.appendTo("tbody#resultaward");
            }
        }
    });
}

function removeAward(eventId, awardId) {

    $.ajax({
        type: "GET",
        url: "/eventmanage/removeeventaward",
        data: { eventId: eventId, awardId: awardId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var id = '#rowaward' + awardId;
                $(id).remove();
            }
        }
    });
}

//
function addAreaFarmer(eventId) {

    var areaId = $('#chooseareafarmer').val();
    console.log(eventId);
    $.ajax({
        type: "GET",
        url: "/eventmanage/addeventareafarmer",
        data: { eventId: eventId, areaId: areaId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var tr = $("<tr/>");
                tr.attr({ id: 'rowareafarmer' + response.id });
                $("<td/>").html(response.name).appendTo(tr);
                $("<td/>").html(response.code).appendTo(tr);
                $("<td/>").html(response.notes).appendTo(tr);
                var eId = "'" + eventId + "'";
                var aId = response.id;
                $("<td/>").html('<a class="btn btn-xs btn-secondary" href="javascript:removeAreaFarmer(' + eId + ', ' + aId + ')"><i class="fa fa-trash"></i></a> &nbsp;&nbsp;<a class="btn btn-xs btn-secondary" href="/eventmanage/EventAreaFarmerModify?eventId=' + eventId + '&areaId=' + aId + '" target="_blank"><i class="fa fa-plus"></i></a>').appendTo(tr);
                tr.appendTo("tbody#resultareafarmer");
            }
        }
    });
}

function removeAreaFarmer(eventId, areaId) {

    $.ajax({
        type: "GET",
        url: "/eventmanage/removeeventareafarmer",
        data: { eventId: eventId, areaId: areaId },
        success: function (response) {
            alert(response.msg);
            if (response.error === 1) {
                var id = '#rowareafarmer' + areaId;
                $(id).remove();
            }
        }
    });
}