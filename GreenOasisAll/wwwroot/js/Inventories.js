var adminUser = $("#ifAdmin").val();

dataTable = $('#myInventoryTable').DataTable({
    colReorder: true,
    "ajax": {
        "url": "/Inventory/getInventoryList",
    },
    "columns": [
        { "data": "name", "width": "25%" },
        { "data": "category", "width": "25%" },
        {
            "data": "purchase_Price",
            "render": function (data) {
                return data + "₫";
            },
            "width": "10%"
        },
        {
            "data": "quantity",
            "render": function (data) {
                if (data < 5) {
                    return '<p class="badge badge-danger">' + data + '</p>';
                } else {
                    return data;
                }
            },
            "width": "10%"
        },
        {
            "data": "id",
            "render": function (data) {
                if (adminUser === "1") {
                    return `
                        <div class="row text-center">
                            <div class="col-4">
                                <a href="/Inventory/Edit/${data}" class="btn btn-success text-white form-control" style="cursor: pointer">
                                    <i class="fas fa-edit"></i>
                                </a>
                            </div>
                            <div class="col-4">
                                <a href="/Inventory/Delete/${data}" class="btn btn-success text-white form-control" style="cursor: pointer">
                                    <i class="fas fa-trash"></i>
                                </a>
                            </div>
                            <div class="col-4">
                                <a href="/Inventory/Details/${data}" class="btn btn-success text-white form-control" style="cursor: pointer">
                                    <i class="fas fa-info-circle"></i>
                                </a>
                            </div>
                        </div>`;
                }
            },
            "width": "30%"
        }
    ]
});
