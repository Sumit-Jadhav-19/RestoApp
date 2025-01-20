$(function () {
    getUsers();
    $('#register-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        let formData = {
            UserName: form.find('#UserName').val(),
            FirstName: form.find('#FirstName').val(),
            LastName: form.find('#LastName').val(),
            DateOfBirth: form.find('#DateOfBirth').val(),
            Email: form.find('#Email').val(),
            Password: form.find('#Password').val(),
            ConfirmPassword: form.find('#ConfirmPassword').val(),
        };
        callPostApi('Admin/Register', formData)
            .then(data => {
                $('#register-modal').modal('hide');
                getUsers();
                form.find('#UserName').val(''),
                    form.find('#FirstName').val(''),
                    form.find('#LastName').val(''),
                    form.find('#DateOfBirth').val(''),
                    form.find('#Email').val(''),
                    form.find('#Password').val(''),
                    form.find('#ConfirmPassword').val('')
            });
    });
});
function getUsers() {
    callGetApi('Admin/GetAllRegisteredUsers')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                let date = new Date(v.dateOfBirth);
                let dob = ("0" + date.getDate()).slice(-2) + "-" + ("0" + (date.getMonth() + 1)).slice(-2) + "-" + date.getFullYear();
                let status = v.isActive ? `<div class="status-badge">
                                <span class="icon">✔</span>
                                <span class="text">Active</span>
                            </div>`: `<div class="status-badge inactive">
                                <span class="icon">✖</span>
                                <span class="text">Inactive</span>
                            </div>`;
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${v.firstName}</td>
                                   <td>${v.lastName}</td>
                                   <td>${v.userName}</td>
                                   <td>${v.email}</td>
                                   <td>${v.phoneNumber}</td>
                                   <td>${dob}</td>
                                   <td>${status}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getUser('${v.userId}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteUser('${v.userId}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-user tbody').html(tableHtml);
        });
}
function getUser(id) {
    $('#register-modal').modal('show');
}
function deleteUser(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}