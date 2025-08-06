function Delete(url) {
    swal({
        title: "Click OK if you want to delete the item",
        text: "If you click OK, it will be deleted permanently",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.info(data.message);

                        setTimeout(reloadPage, 3000);
                    }
                    else {
                        toastr.error(data.message);
                    }
                },
                error: function () {
                    toastr.error(data.message);
                }
            })
        }
    });
}

function reloadPage() {
    window.location.reload();
}
