var dataTable;
var id;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({        
        "ajax": { url: '/admin/user/GetUsers' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },        
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "10%" },
            {
                data: { id: 'id', lockoutEnd:'lockoutEnd'},
                "render": function (data) {  
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `  
                        <div class="text-center" >
                             <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;" >
                               <i class="bi bi-lock" ></i> Lock
                            </a>    
                           <a href="/admin/user/rolemanagement?userId=${data.id}" class="btn btn-primary text-white" style="cursor:pointer; ">
                                <i class="bi bi-pencil"></i> Permission
                           </a> 
                        </div > `
                    }
                    else {
                        return `  
                        <div class="text-center" >
                             <a onclick=LockUnlock('${data.id}')  class="btn btn-success text-white" style="cursor:pointer; width:100px;" >
                                <i class="bi bi-unlock" ></i> Unlock
                            </a>                           
                            <a href="/admin/user/rolemanagement?userId=${data.id}" class="btn btn-primary text-white" style="cursor:pointer; ">
                                <i class="bi bi-pencil"></i> Permission
                           </a> 
                        </div > `
                    }                   
                },
                "width": "25%"
            }
        ]
    });
}

function LockUnlock(id) {             
    $.ajax({
        type: "POST",
        url: "/Admin/User/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json",

        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload();
                toastr.success(data.message);
            }
        }
    });       
}

//data format 
//{
//    "name": "company",
//    "streetAddress": "test",
//    "city": "test",
//    "country": "test",
//    "postalCode": "44444",
//    "companyId": 3,
//    "company": {
//        "id": 3,
//        "name": "Acer",
//        "streetAddress": "test",
//        "city": "test",
//        "state": "test",
//        "country": "test",
//        "postalCode": "44444",
//        "phoneNumber": "4444444"
//    },
//    "role": "Company",
//    "id": "016e8b10-93d4-4cc9-894a-70c05d996549",
//    "userName": "testCompany@test.com",
//    "normalizedUserName": "TESTCOMPANY@TEST.COM",
//    "email": "testCompany@test.com",
//    "normalizedEmail": "TESTCOMPANY@TEST.COM",
//    "emailConfirmed": false,
//    "passwordHash": "AQAAAAIAAYagAAAAENhM6BuEr+AjPgacplaGAvGyjNNNqV7z9ix1NwfRNkWxcd5kEpEwXcFaBpaCOaqn7g==",
//    "securityStamp": "BMJF2VYKCZKHNXTO2BTXKXIUONOB2BQA",
//    "concurrencyStamp": "3fffdf50-f0f2-47df-b964-c361a9039193",
//    "phoneNumber": "77777777",
//    "phoneNumberConfirmed": false,
//    "twoFactorEnabled": false,
//    "lockoutEnd": null,
//    "lockoutEnabled": true,
//    "accessFailedCount": 0
//},