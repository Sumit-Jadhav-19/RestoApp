$(function () {
    getMenu();
    $('#menu-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        var isHalfAvailable = $('#IsHalfAvailable').is(':checked');
        var halfPrice = parseFloat($('#HalfPrice').val()) || 0;
        var fileInput = $("#ImageFile")[0].files[0];
        console.log(fileInput);
        if (isHalfAvailable && halfPrice <= 0) {
            $('#HalfPrice').next().text('Please Enter Half menu price');
            return;
        }
        if (fileInput && !["image/jpeg", "image/png"].includes(fileInput.type)) {
            toast('w', "Only JPG and PNG images are allowed.");
            return;
        }
        let formData = new FormData(); // Use FormData for file uploads

        formData.append("MenuId", form.find('#menu-id').val() === '' ? 0 : form.find('#menu-id').val());
        formData.append("Name", form.find('#Name').val());
        formData.append("Description", form.find('#Description').val());
        formData.append("CategoryId", form.find('#CategoryId').val());
        formData.append("Price", form.find('#Price').val());
        formData.append("IsHalfAvailable", form.find('#IsHalfAvailable').is(':checked'));
        formData.append("HalfPrice", form.find('#HalfPrice').val());
        formData.append("Discount", form.find('#Discount').val());
        formData.append("IsAvailable", form.find('#IsAvailable').is(':checked'));
        formData.append("PreparationTime", form.find('#PreparationTime').val());
        formData.append("IsSpecial", form.find('#IsSpecial').is(':checked'));

        // Get the file input
        if (fileInput) {
            formData.append("ImageFile", fileInput); // Append the file
        }
        console.log(formData);
        callPostApiProcessData('Menu/AddMenu', formData)
            .then(data => {
                $('#menu-modal').modal('hide');
                getMenu();
                form.find('#Name').val('');
                $('#menu-form button[type="submit"]').text('Save');
                form.find('#Description').val('')
            });
    });
    $('#ImageFile').on('change', function (e) {
        var file = e.target.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function (event) {
                $('#filePreview').attr('src', event.target.result).show();
            };
            reader.readAsDataURL(file);
        } else {
            $('#filePreview').hide();
        }
    });
});
function showMenuModal() {
    showModal('menu-modal');
    $('#filePreview').hide();
    $("#ImageFile").val('');
}
function getMenu() {
    callGetApi('Menu/GetMenus')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                let IsHalf = v.isHalfAvailable ? 'Yes' : 'No';
                let IsAvailable = v.isAvailable ? 'Yes' : 'No';
                let IsSpecial = v.isSpecial ? 'Yes' : 'No';
                var s = checkIsNullObj(v);
                let ImagePath = s.imagePath == '-' ? '-' : `<a onclick='showMenuImage("${s.imagePath}")' href='#'>${s.imagePath}</a>`;
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${s.name}</td>
                                   <td>${s.description}</td>
                                   <td>${s.categoryName}</td>
                                   <td>${s.price}</td>
                                   <td>${IsHalf}</td>
                                   <td>${s.halfPrice}</td>
                                   <td>${s.discount}</td>
                                   <td>${ImagePath}</td>
                                   <td>${IsAvailable}</td>
                                   <td>${s.preparationTime}</td>
                                   <td>${IsSpecial}</td>
                                   <td>${s.createdUser}</td>
                                   <td>${fromatDate(s.createdOn)}</td>
                                   <td>${s.modifiedUser}</td>
                                   <td>${fromatDate(s.modifiedOn)}</td>
                                   <td>${s.isActive}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getMenuById('${v.menuId}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteMenu('${v.menuId}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-menu tbody').html(tableHtml);
        });
}

function showMenuImage(image) {
    if (image != null || image != undefined || image != '') {
        $('#menu-image-modal').modal('show');
        $('#menu-Image-Preview').attr('src', `/menuImages/${image}`).show()
    }
}
function getMenuById(id) {
    $("#ImageFile").val('');
    $(`#menu-form .field-validation-error`).text('');
    $('#menu-modal').modal('show');
    $('#menu-form button[type="submit"]').text('Update');
    callGetApi('Menu/GetMenuById', { id: id })
        .then(res => {
            console.log(res);
            var form = $('#menu-form');
            form.find('#Name').val(res.name);
            form.find('#Description').val(res.description);
            form.find('#menu-id').val(res.menuId);
            form.find('#CategoryId').val(res.categoryId);
            form.find('#Price').val(res.price);
            form.find('#IsHalfAvailable').prop('checked', res.isHalfAvailable);
            res.halfPrice == 0 ? form.find('#HalfPrice').val('') : form.find('#HalfPrice').val(res.halfPrice);
            form.find('#Discount').val(res.halfPrice);
            form.find('#PreparationTime').val(res.preparationTime);
            form.find('#IsAvailable').prop('checked', res.isAvailable);
            form.find('#IsSpecial').prop('checked', res.isSpecial);
            res.imagePath != null ? $('#filePreview').attr('src', `/menuImages/${res.imagePath}`).show() : $('#filePreview').attr('src', ``).hide();
        });
}
function deleteMenu(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}