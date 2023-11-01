var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblCompany').DataTable({
        "ajax": { url: '/admin/company/getcompanys' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'city', "width": "15%" },
            { data: 'country', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `  <div class="w-75 btn-group" role="group">
                                <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2" >
                                    <i class="bi bi-pencil"></i> Edit
                                </a>
                                <a OnClick=Delete('/admin/company/delete/${data}')  class="btn btn-danger mx-2">
                                    <i class="bi bi-trash3"></i> Delete
                                </a>
                            </div > `
                },
                "width": "25%"
            }
        ]
    });
}

//dataformat -- { "id": 1, "name": "Lenovo", "streetAddress": "test", "city": "test", "state": "test", "country": "test", "postalCode": "22222", "phoneNumber": "22222222" },

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    })
}

