$(function () {
    getCategory();
    $('#category-form').submit(function (event) {
        event.preventDefault();
        var form = $(this);
        console.log(form.find('#category-id').val());
        let formData = {
            Id: form.find('#category-id').val() == '' ? 0 : form.find('#category-id').val(),
            Name: form.find('#Name').val(),
            Description: form.find('#Description').val()
        };
        console.log(formData);
        callPostApi('Category/AddCategory', formData)
            .then(data => {
                $('#category-modal').modal('hide');
                getCategory();
                form.find('#Name').val('');
                $('#category-form button[type="submit"]').text('Save');
                form.find('#Description').val('')
            });
    });
});

function getCategory() {
    callGetApi('Category/GetCategories')
        .then(data => {
            let tableHtml = '';
            console.log(data);
            $.each(data, function (i, v) {
                var s = checkIsNullObj(v);
                tableHtml += `  <tr>
                                   <td>${i + 1}</td>
                                   <td>${s.name}</td>
                                   <td>${s.description}</td>
                                   <td>${s.dataEnteredBy}</td>
                                   <td>${fromatDate(s.dataEnteredOn)}</td>
                                   <td>${s.dataModifiedBy}</td>
                                   <td>${fromatDate(s.dataModifiedOn)}</td>
                                   <td>${s.isActive}</td>
                                   <td>
                                       <div class='d-flex'><a class="btn-i" onclick="getCategoryById('${v.id}')"><i class="bi bi-pencil-fill"></i></a>
                                       <a class="btn-i"><i class="bi bi-trash-fill" onclick="deleteCategory('${v.id}')"></i></a></div>
                                   </td>
                                </tr>`;
            });
            $('#tbl-category tbody').html(tableHtml);
        });
}
function getCategoryById(id) {
    $('#category-modal').modal('show');
    $('#category-form button[type="submit"]').text('Update');
    callGetApi('Category/GetCategoryById', { id: id })
        .then(res => {
            console.log(res);
            var form = $('#category-form');
            form.find('#Name').val(res.name);
            form.find('#Description').val(res.description);
            form.find('#category-id').val(res.id);
        });
}
function deleteCategory(id) {
    confirmation()
        .then(res => {
            console.log(res);
        });
}