function validateShippingInput() {
    if (document.getElementById("trackingNumber").value == "") {
        swal({
            icon: 'error',
            title: 'Oops...',
            text: 'Please enter the trackingNumber',
        })
        return false;
    }
    if (document.getElementById("carrier").value == "") {
        swal({
            icon: 'error',
            title: 'Oops...',
            text: 'Please enter the carrier name',
        })
        return false;
    }
}

function EnterDataCarrier() {
    if (document.getElementById("carrier").value == "") {
        document.getElementById("carrier").style.border = "thin solid red";
    } else {
        document.getElementById("carrier").style.border = "thin solid #0000FF";
    }
}
function EnterTrackingNumber() {
    if (document.getElementById("trackingNumber").value == "") {
        document.getElementById("trackingNumber").style.border = "thin solid red";
    } else {
        document.getElementById("trackingNumber").style.border = "thin solid #0000FF";
    }
}

function RefundIssue(url) {
    swal({
        title: "Click OK if you want to refund the order",
        text: "If you click OK, it will be not revert back again",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willRefund) => {
        if (willRefund) {
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

