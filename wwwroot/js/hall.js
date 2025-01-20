
$(function () {
    getHalls();
    $('#hall-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        let formData = {
            HallId: form.find('#hall-id').val() == '' ? 0 : form.find('#hall-id').val(),
            HallName: form.find('#HallName').val(),
            HallDescription: form.find('#HallDescription').val()
        };
        console.log('form data');
        console.log(formData);
        callPostApi('Admin/AddHall', formData)
            .then(data => {
                $('#hall-modal').modal('hide');
                getHalls();
                form.find('#Name').val('');
                $('#hall-form button[type="submit"]').text('Save');
            });
    });
});

function getHalls() {
    callGetApi('Admin/GetHalls')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                let s = checkIsNullObj(v);
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${s.hallName}</td>
                                   <td>${s.hallDescription}</td>
                                   <td>${s.dataEnteredBy}</td>
                                   <td>${fromatDate(s.dataEnteredOn)}</td>
                                   <td>${s.dataModifiedBy}</td>
                                   <td>${fromatDate(s.dataModifiedOn)}</td>
                                   <td>${s.isActive}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getHall('${v.hallId}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteHall('${v.hallId}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-hall tbody').html(tableHtml);
        });
}
function getHall(id) {
    $('#hall-modal').modal('show');
    $('#hall-form button[type="submit"]').text('Update');
    callGetApi('Admin/GetHallById', { id: id })
        .then(res => {
            console.log(res);
            var form = $('#hall-form');
            form.find('#HallName').val(res.hallName);
            form.find('#HallDescription').val(res.hallDescription);
            form.find('#hall-id').val(res.hallId);
        });
}
function deleteHall(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}