$(document).ready(function () {
    $(document).on('click', '.inc.qtybutton', function (e) {
        e.preventDefault();
        var productId = $(this).attr('asp-route-productId');
        if (!productId) {
            console.error('Thiếu productId');
            return;
        }
        $.ajax({
            url: '/Cart/AddToCart',
            type: 'GET',
            data: {
                productId: productId,
                returnUrl: window.location.pathname
            },
            success: function () {
                location.reload();
            },
            error: function () {
                alert("Có lỗi khi thêm sản phẩm.");
            }
        });
    });

    $(document).on('click', '.dec.qtybutton', function (e) {
        e.preventDefault();
        var productId = $(this).attr('asp-route-productId');
        if (!productId) {
            console.error('Thiếu productId');
            return;
        }
        $.ajax({
            url: '/Cart/MinusAnItem',
            type: 'GET',
            data: {
                productId: productId
            },
            success: function () {
                location.reload();
            },
            error: function () {
                alert("Có lỗi khi giảm số lượng.");
            }
        });
    });
});
