var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblCategory').DataTable({
        "ajax": { url: '/admin/category/GetCategories' },
        "columns": [
            { data: 'name', "width": "40%" },
            { data: 'displayOrder', "width": "20%" },
            {
                data: 'id',
                "render": function (data) {
                    return `  <div class="w-75 btn-group" role="group">
                                <a href="/admin/category/upsert?id=${data}" class="btn btn-primary mx-2" >
                                    <i class="bi bi-pencil"></i> Edit
                                </a>
                                <a OnClick=Delete('/admin/category/delete/${data}')  class="btn btn-danger mx-2">
                                    <i class="bi bi-trash3"></i> Delete
                                </a>
                            </div > `
                },
                "width": "40%"
            }
        ]
    });
}
//data format - {"id":1,"name":"Action","displayOrder":1}

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
            //Swal.fire(
            //    'Deleted!',
            //    'Your file has been deleted.',
            //    'success'
            //)
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    //alert(data.message);
                    toastr.success(data.message);
                }
            })
        }
    })
}

