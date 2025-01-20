$(function () {
    GetRolePermissions();
});
function GetRolePermissions() {
    callGetApi('Admin/GetRolesAndPermissions')
        .then(data => {
            console.log(data);
            let strHtml = '';
            $.each(data, function (i, v) {
                strHtml += ` <tr>
                        <td>${i + 1}</td>
                        <td><strong>${v.roleName}</strong></td>
                        <td>
                            <input type="checkbox" class="form-check-input" ${v.table == 1 ? 'checked' : ''} data-role='${v.roleId}' data-permission='1'/>
                        </td>
                        <td><input type="checkbox" class="form-check-input" ${v.user == 1 ? 'checked' : ''} data-role='${v.roleId}' data-permission='2'/></td>
                        <td><input type="checkbox" class="form-check-input" ${v.menu == 1 ? 'checked' : ''} data-role='${v.roleId}' data-permission='3'/></td>
                        <td><input type="checkbox" class="form-check-input" ${v.captain == 1 ? 'checked' : ''} data-role='${v.roleId}' data-permission='4'/></td>
                        <td><input type="checkbox" class="form-check-input" ${v.chef == 1 ? 'checked' : ''} data-role='${v.roleId}' data-permission='5'/></td>
                    </tr>`;
            });
            $('#tbl-user-role tbody').html(strHtml);
        })
}
$('#tbl-user-role tbody').on('change', '.form-check-input', function () {
    const role = $(this).attr('data-role');
    const permission = $(this).attr('data-permission');
    addRolePermission(role, permission);
});

function addRolePermission(r, p) {
    callPostApi('Admin/AddRolePermission', { role: r, permission: p })
        .then(data => {

        });
}