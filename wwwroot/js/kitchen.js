$(function () {
    GetAcceptedOrders();
    GetCompletedOrders();
});
function GetAcceptedOrders() {
    callGetApi('Kitchen/GetAcceptedOrders')
        .then(data => {
            console.log(data);
            let strOrders = '';
            $('.kitchen-order-processing-content').html('');
            $('.procssing-order-count').text(data.length);
            $.each(data, function (i, v) {
                let strSubOrder = '';
                $.each(v.orders, function (j, d) {
                    let size = d.size.toUpperCase() === 'F' ? 'Full' : 'Half';
                    strSubOrder += `${d.quantity}x ${d.name},`;
                });
                strSubOrder = strSubOrder.slice(0, -1);
                if (v.orders.length > 5) {
                    strSubOrder += `<a href="#" class="more-link">${v.orders.length - 5}+</a>`;
                }
                let acceptBtn = 'btn-success';
                const diffInMinutes = (new Date() - new Date(v.acceptedDate)) / 60000;
                if (diffInMinutes > 5) {
                    acceptBtn = 'btn-warning';
                }
                if (diffInMinutes > 10) {
                    acceptBtn = 'btn-danger';
                }
                strOrders = `<div class="kitchen-order-card" id="${v.orderId}${v.orderCount}">
                                <div class="d-flex justify-content-between">
                                    <span class="fw-600">#${v.orderId}</span>
                                    <p class="m-0 f-s0-8rem order-time" data-order-date="${v.acceptedDate}">${timeAgo(v.acceptedDate)}</p>
                                </div>
                                <div class="d-flex align-items-center justify-content-between">
                                    <span class="d-flex align-items-center">
                                        <i class="bi bi-dot fs-3"></i>
                                        <h6 class="m-0">H1</h6>
                                    </span>
                                    <span class="d-flex align-items-center">
                                        <i class="bi bi-dot fs-3"></i>
                                        <h6 class="m-0">Table1</h6>
                                    </span>
                                </div>
                                <hr  class="mt-0"/>
                                <div class="d-flex justify-content-between align-items-center gap-2">
                                    <div class="text-container">
                                        <div>
                                            <p class=" m-0">
                                                ${strSubOrder}
                                            </p>
                                        </div>
                                    </div>
                                    <div class="actions">
                                        <button class="accept btn ${acceptBtn}" id="btn-${v.orderId}${v.orderCount}" onclick="ReadyOrder('${v.orderId}','${v.orderCount}')">
                                            Proccessed
                                        </button>
                                    </div>
                                </div>
                            </div>`;
                $('.kitchen-order-container .kitchen-order-processing-content').append(strOrders);
            });
            if (data.length == 0) {
                $('.kitchen-order-container .kitchen-order-processing-content').append('<p class="text-center text-secondary">No proccessing orders !</p>');
            }
            else {
                setInterval(updateTimeAgo, 12000);
            }
        });
}
function GetCompletedOrders() {
    callGetApi('Kitchen/GetCompletedOrders')
        .then(data => {
            console.log(data);
            let strOrders = '';
            $('.kitchen-order-completed-content').html('');
            $('.completed-order-count').text(data.length);
            $.each(data, function (i, v) {
                let strSubOrder = '';
                $.each(v.orders, function (j, d) {
                    let size = d.size.toUpperCase() === 'F' ? 'Full' : 'Half';
                    strSubOrder += `${d.quantity}x ${d.name},`;
                });
                strSubOrder = strSubOrder.slice(0, -1);
                if (v.orders.length > 5) {
                    strSubOrder += `<a href="#" class="more-link">${v.orders.length - 5}+</a>`;
                }
                strOrders = `<div class="kitchen-order-card" id="${v.orderId}${v.orderCount}">
                                <div class="d-flex justify-content-between">
                                    <span class="fw-600">#${v.orderId}</span>
                                    <p class="m-0 f-s0-8rem order-time" data-order-date="${v.acceptedDate}">${timeAgo(v.acceptedDate)}</p>
                                </div>
                                <div class="d-flex align-items-center justify-content-between">
                                    <span class="d-flex align-items-center">
                                        <i class="bi bi-dot fs-3"></i>
                                        <h6 class="m-0">H1</h6>
                                    </span>
                                    <span class="d-flex align-items-center">
                                        <i class="bi bi-dot fs-3"></i>
                                        <h6 class="m-0">Table1</h6>
                                    </span>
                                </div>
                                <hr  class="mt-0"/>
                                <div class="d-flex justify-content-between align-items-center gap-2">
                                    <div class="text-container">
                                        <div>
                                            <p class=" m-0">
                                                ${strSubOrder}
                                            </p>
                                        </div>
                                    </div>
                                    <div class="actions">
                                        <span class="text-success">
                                            Completed
                                        </span>
                                    </div>
                                </div>
                            </div>`;
                $('.kitchen-order-container .kitchen-order-completed-content').append(strOrders);
            });
            if (data.length == 0) {
                $('.kitchen-order-container .kitchen-order-completed-content').append('<p class="text-center text-secondary">No completed orders !</p>');
            }
            else {
                setInterval(updateTimeAgo, 12000);
            }
        });
}
function ReadyOrder(orderId, orderCount) {
    callPostApi('Kitchen/ReadyOrder', { OrderId: orderId, OrderCount: orderCount })
        .then(data => {
            GetAcceptedOrders();
            GetCompletedOrders();
        });
}
function timeAgo(date) {
    const now = new Date();
    const past = new Date(date);
    const diffInSeconds = Math.floor((now - past) / 1000);

    const minutes = Math.floor(diffInSeconds / 60);
    const hours = Math.floor(diffInSeconds / 3600);
    const days = Math.floor(diffInSeconds / 86400);
    const months = Math.floor(diffInSeconds / 2592000);
    const years = Math.floor(diffInSeconds / 31536000);

    if (years > 0) {
        return years === 1 ? "1 year ago" : years + " years ago";
    } else if (months > 0) {
        return months === 1 ? "1 month ago" : months + " months ago";
    } else if (days > 0) {
        return days === 1 ? "1 day ago" : days + " days ago";
    } else if (hours > 0) {
        return hours === 1 ? "1 hour ago" : hours + " hours ago";
    } else if (minutes > 0) {
        return minutes === 1 ? "1 minute ago" : minutes + " minutes ago";
    } else {
        return "Just now";
    }
}