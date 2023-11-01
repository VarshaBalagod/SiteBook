var dataTable;
var id;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else {
        if (url.includes("inprocess")) {
            loadDataTable("inprocess");
        }
        else {
            if (url.includes("completed")) {
                loadDataTable("completed");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/GetOrders?status=' + status },
        "columns": [
            { data: 'orderId', "width": "10%" },
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'orderId',
                "render": function (data) {
                    return `  <div class="w-75 btn-group" role="group">
                                <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2" >
                                    <i class="bi bi-pencil"></i>
                                </a>                              
                            </div > `
                },
                "width": "20%"
            }
        ]
    });
}

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
                    toastr.success(data.message);
                }
            })
        }
    })
}

//data format
//{
//    "orderId": 4,
//    "applicationUserId": "016e8b10-93d4-4cc9-894a-70c05d996549",
//    "applicationUser": {
//        "name": "company",
//        "streetAddress": "test",
//        "city": "test",
//        "country": "test",
//        "postalCode": "44444",
//        "companyId": null,
//        "company": null,
//        "id": "016e8b10-93d4-4cc9-894a-70c05d996549",
//        "userName": "testCompany@test.com",
//        "normalizedUserName": "TESTCOMPANY@TEST.COM",
//        "email": "testCompany@test.com",
//        "normalizedEmail": "TESTCOMPANY@TEST.COM",
//        "emailConfirmed": false,
//        "passwordHash": "AQAAAAIAAYagAAAAENhM6BuEr+AjPgacplaGAvGyjNNNqV7z9ix1NwfRNkWxcd5kEpEwXcFaBpaCOaqn7g==",
//        "securityStamp": "BMJF2VYKCZKHNXTO2BTXKXIUONOB2BQA",
//        "concurrencyStamp": "3fffdf50-f0f2-47df-b964-c361a9039193",
//        "phoneNumber": "77777777",
//        "phoneNumberConfirmed": false,
//        "twoFactorEnabled": false,
//        "lockoutEnd": null,
//        "lockoutEnabled": true,
//        "accessFailedCount": 0
//    },
//    "orderDate": "2023-10-03T10:48:48.2759485",
//    "shippingDate": "0001-01-01T00:00:00",
//    "orderTotal": 510,
//    "orderStatus": "Pending",
//    "paymentStatus": "Pending",
//    "trackingNumber": null,
//    "carrier": null,
//    "paymentDate": "0001-01-01T00:00:00",
//    "paymentDueDate": "0001-01-01T00:00:00",
//    "sessionId": null,
//    "paymentIntentId": null,
//    "phoneNumber": "7777777734",
//    "streetAddress": "test34",
//    "city": "test34",
//    "country": "test34",
//    "postalCode": "4444434",
//    "name": "company"
// },