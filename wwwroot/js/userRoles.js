
$(function () {
    getRoles();
    $('#role-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        console.log(form.find('#role-id').val());
        let formData = {
            Id: form.find('#role-id').val() == '' ? 0 : form.find('#role-id').val(),
            Name: form.find('#Name').val()
        };
        console.log(formData);
        callPostApi('Admin/CreateRole', formData)
            .then(data => {
                $('#role-modal').modal('hide');
                getRoles();
                form.find('#Name').val('');
                $('#role-form button[type="submit"]').text('Save');
            });
    });
});

function getRoles() {
    callGetApi('Admin/GetAllRoles')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${v.name}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getRole('${v.id}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteRole('${v.id}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-user-role tbody').html(tableHtml);
        });
}
function getRole(id) {
    $('#role-modal').modal('show');
    $('#role-form button[type="submit"]').text('Update');
    callGetApi('Admin/GetRoleById', { id: id })
        .then(res => {
            console.log(res);
            var form = $('#role-form');
            form.find('#Name').val(res.name);
            form.find('#role-id').val(res.id);
        });
}
function deleteRole(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}