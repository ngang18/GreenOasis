$(document).ready(function () {
    var status = window.location.search.substring(8);
    switch (status) {
        case "Pending":
            Pending = "active text-white bg-success";
            getDataTable(status)
            break;
        case "Inprocess":
            Inprocess = "active text-white bg-success";
            getDataTable(status)
            break;
        case "Shipped":
            Shipped = "active text-white bg-success";
            getDataTable(status)
            break;
        case "Completed":
            Completed = "active text-white bg-success";
            getDataTable(status)
            break;
        case "Cancelled":
            Cancelled = "active text-white bg-success";
            getDataTable(status)
            break;
        default:
            All = "active text-white bg-success";
            getDataTable(status)
            break;
    }
});

function getDataTable(status) {
    var adminUser = $('#ifAdmin').val();
    dataTable = $('#myOrderTable').DataTable({
        "ajax": {
            "url": "/Order/OrderListAll?status=" + status,
        },
        "columns": [
            { "data": "dateOfOrder", "render": function (data) { return moment(data).format('MM/DD/YYYY'); }, "width": "35%" },
            { "data": "name", "width": "15%" },
            { "data": "totalOrderAmount", "width": "15%" },
            {
                "data": "orderStatus", "render": function (data) {
                    if (data === 'Canceled') {
                        return '<p class ="text-danger">' + data + '</p>'
                    }
                    else {
                        return '<p class ="text-success">' + data + '</p>'
                    }
                }, "width": "15%"
            },
            {
                "data": "id",
                "render": function (data) {
                    if (adminUser === "1") {
                        return `
                        <div class="row">
	                        <div class="col-12">
                                <a href="/Order/OrderDetails/${data}" class="btn btn-success text-white form-control" style="cursor: pointer">
                                    <i class=" fas fa-edit"></i>
                                    </a>
	                        </div>
                        </div>
                        `;
                    } else {
                        return `
                        <div class="row">
	                        <div class="col-12">
                                <a href="/Order/OrderDetails/${data}" class="btn btn-success text-white form-control" style="cursor: pointer">
                                    <i class=" fa fa-credit-card"></i>
                                    </a>
	                        </div>
                        </div>
                        `;
                    }
                }, "width": "20%"
            }
        ],
        dom: "lBfrtip",
        buttons: [
            'pageLength', 'copy', 'csv', 'excel', 'pdf', 'print'
        ],
    });
}