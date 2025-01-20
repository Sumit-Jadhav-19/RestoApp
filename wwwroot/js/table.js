
$(function () {
    getTables();
    $('#table-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        let formData = {
            Id: form.find('#table-id').val() == '' ? 0 : form.find('#table-id').val(),
            Name: form.find('#Name').val(),
            HallId: form.find('#HallId').val()
        };
        console.log('form data');
        console.log(formData);
        callPostApi('Admin/AddTable', formData)
            .then(data => {
                $('#table-modal').modal('hide');
                getTables();
                form.find('#Name').val('');
                $('#table-form button[type="submit"]').text('Save');
            });
    });
});

function getTables() {
    callGetApi('Admin/GetTables')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                let s = checkIsNullObj(v);
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${s.name}</td>
                                   <td>${s.hall.hallName}</td>
                                   <td>${s.dataEnteredBy}</td>
                                   <td>${fromatDate(s.dataEnteredOn)}</td>
                                   <td>${s.dataModifiedBy}</td>
                                   <td>${fromatDate(s.dataModifiedOn)}</td>
                                   <td>${s.isActive}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getTable('${v.id}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteTable('${v.id}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-table tbody').html(tableHtml);
        });
}
function getTable(id) {
    $('#table-modal').modal('show');
    $('#table-form button[type="submit"]').text('Update');
    callGetApi('Admin/GetTableById', { id: id })
        .then(res => {
            console.log(res);
            var form = $('#table-form');
            form.find('#Name').val(res.name);
            form.find('#HallId').val(res.hallId);
            form.find('#table-id').val(res.id);
        });
}
function deleteTable(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}