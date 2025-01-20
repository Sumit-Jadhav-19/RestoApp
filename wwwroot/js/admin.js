$(function () {
    GetOrderBills();
    GetTop100Ordsers();
    $('#user-payment').click(function () {
        PaymentConfirmed();
    });

    const ctx = document.getElementById('salesChart').getContext('2d');
    const salesChart = new Chart(ctx, {
        type: 'line', 
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June'], 
            datasets: [{
                label: 'Monthly Sales', 
                data: [5000, 7000, 8000, 6000, 9000, 10000], 
                borderColor: 'rgba(153, 117, 251, 1)', 
                backgroundColor: 'rgba(75, 192, 192, 0.2)', 
                tension: 0.4, 
                borderWidth: 2 
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: true, 
                    position: 'top'
                },
                tooltip: {
                    enabled: true 
                }
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Months' 
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Sales (in INR)' 
                    },
                    beginAtZero: true 
                }
            }
        }
    });
});

function GetOrderBills() {
    callGetApi('Admin/GetOrderdBills')
        .then(data => {
            console.log(data);
            let strHtml = '';
            if (data.length > 0) {
                $.each(data, function (i, v) {
                    strHtml += `<tr class="cursor-pointer" onclick='GetOrderDetails("${v.order}")'>
                        <td>${v.order}</td>
                        <td>${v.hallName}</td>
                        <td>${v.tableName}</td>
                        <td>${fromatDate(v.orderDate)}</td>
                        <td>₹ ${v.total}</td>
                    </tr>`;
                });
            }
            else {
                console.log(data.length);
                strHtml += `<tr class='text-secondary text-center'><td colspan=5>No billing orders !<td></tr>`;
            }
            console.log(strHtml);
            $('#tbl-Order-bills tbody').html(strHtml);
        });
}
function GetOrderDetails(orderId) {
    console.log(orderId);
    callGetApi(`Admin/GetOrderdBillsById/`, { OrderId: orderId })
        .then(data => {
            if (data.length > 0) {
                let strHtml = '';
                $.each(data[0].menus, function (i, v) {
                    let size = v.size.toUpperCase() == "F" ? '' : ' <span>Half</span>';
                    strHtml += `<div class="d-flex align-items-center gap-1 pt-2 pb-2 ps-2 pe-2 border-bottom">
                    <div class="d-flex align-items-center justify-content-between w-100 ps-2 pe-2">
                        <div class="d-flex align-items-center gap-3">
                            <h5 class="m-0 quant">${v.quantity}</h5>
                            <div>
                                <span>${v.menuName}</span>
                                ${size}
                            </div>
                        </div>
                        <h6 class="m-0">₹ ${v.totalPrice}</h6>
                    </div>
                            </div > `;
                });
                $('.order-menus').html(strHtml);
                $('#orderId').val(data[0].orderId);
                $('.preOrderId').text(data[0].orderId);
                $('.hall').text(data[0].hall);
                $('.order-table').text(data[0].table);
                $('#subtotal').text(`₹ ${data[0].subtotal} `);
                $('#tax').text(`₹ ${data[0].tax} `);
                $('#discount').text(`₹ 0.00`);
                $('#total').text(`₹ ${data[0].total} `);
                $('#offcanvas-billing-orders').offcanvas('show');
            }
        });
}
function PaymentConfirmed() {
    let orderId = $('#orderId').val();
    let paymentType = $('#paymentType').val();
    console.log(paymentType);
    if (paymentType == 0) {
        toast('w', 'Please select payment type');
        return;
    }
    console.log(orderId);
    if (orderId != null) {
        callPostApi('Admin/PaymentConfirmed', { OrderId: orderId, paymentType: paymentType })
            .then(data => {
                GetOrderBills();
                $('#offcanvas-billing-orders').offcanvas('hide');
                GetTop100Ordsers();
            });
    }
}

function GetTop100Ordsers() {
    callGetApi('Admin/GetTop100Orders')
        .then(data => {
            console.log(data);
            let strHtml = '';
            if (data.length > 0) {
                $.each(data, function (i, v) {
                    v = checkIsNullObj(v);
                    let paymentStatus = v.isBillPayed ? '<span class="paid">Paid</span>' : '<span class="unpaid">Unpaid</span>';
                    let orderStatus = v.orderStatus ? '<span class="text-success">Completed</span>' : '<span class="text-warning">Pending</span>';
                    strHtml += `<tr>
                                   <td>${i < 10 ? '00' + (i + 1) : i < 100 ? '0' + (i + 1) : i + 1}</td>
                                   <td>${v.orderId}</td>
                                   <td>${getCurrentDate(v.orderDateTime)} - ${getCurrentTime(v.orderDateTime)}</td>
                                   <td>${v.hallName}</td>
                                   <td>${v.name}</td>
                                   <td>${v.captainName}</td>
                                   <td>${orderStatus}</td>
                                   <td>₹ ${v.totalPayment}</td>
                                   <td>${paymentStatus}</td>
                                   <td>${v.paymentType}</td>
                                   <td><a  onclick="GetOrderDetailById('${v.orderId}')" class="text-decoration-none cursor-pointer">Details</a></td>
                               </tr>`;
                });
            } else {
                strHtml = '<tr><td colspan=8>No Data found !</td></tr>';
            }
            $('#tbl-top-100 tbody').html(strHtml);
        });
}
function GetOrderDetailById(orderId) {
    callGetApi('Admin/GetOrderDetailsById', { OrderId: orderId })
        .then(data => {
            let strHtml = '';
            if (data.length > 0) {
                let total = 0;
                $.each(data, function (i, v) {
                    let size = v.size.toUpperCase() == "F" ? 'Full' : 'Half';
                    let orderStatus = v.orderStatus == 'C' ? '<span class="text-success">Completed</span>' : '<span class="text-warning">Preparing</span>';
                    strHtml += `<tr>
                                  <td>${i + 1}</td>
                                  <td>${v.name}</td>
                                  <td>${v.category}</td>
                                  <td>${v.quantity}</td>
                                  <td>${size}</td>
                                  <td>${orderStatus}</td>
                                  <td>${v.price}</td>
                                  <td>${v.subTotal}</td>
                                </tr>`;
                    total += v.subTotal;
                    $('#details-order-id').text(v.orderId);
                });
                strHtml += `<tr>
                            <td colspan=6></td>
                            <td><h6 class="m-0">Total :</h6></td>
                            <td><h6 class="m-0">${total}</h6></td>
                        </tr>`;
            }
            else {
                strHtml = '<tr><td colspan=8>No Data found !</td></tr>';
            }
            $('#tbl-order-details tbody').html(strHtml);
            $('#order-details-modal').modal('show');
        });
}