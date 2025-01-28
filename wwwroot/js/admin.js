$(function () {
    GetOrderBills();
    GetTop100Ordsers();
    GetDashboardData("");
    GetChartData();
    $('#user-payment').click(function () {
        PaymentConfirmed();
    });
    $('#date-periode-filter').change(function () {
        let datePeriode = $(this).val();
        GetDashboardData(datePeriode);
    });
    $('#date-periode-chart-filter').change(function () {
        let datePeriode = $(this).val();
        GetChartData(datePeriode);
    });
});

function GetOrderBills() {
    callGetApi('Admin/GetOrderdBills')
        .then(data => {
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
                strHtml += `<tr class='text-secondary text-center'><td colspan=5>No billing orders !<td></tr>`;
            }
            $('#tbl-Order-bills tbody').html(strHtml);
        });
}
function GetOrderDetails(orderId) {
    callGetApi(`Admin/GetOrderdBillsById/`, { OrderId: orderId })
        .then(data => {
            if (data.length > 0) {
                let strHtml = '';
                console.log(data);
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

                let printData = JSON.stringify(data);
                console.log(printData);
                $('#print-invoice').attr('href', `print://${data[0].orderId}`);
            }
        });
}
function PaymentConfirmed() {
    let orderId = $('#orderId').val();
    let paymentType = $('#paymentType').val();
    if (paymentType == 0) {
        toast('w', 'Please select payment type');
        return;
    }
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

function GetDashboardData(t) {
    callGetApi('Admin/GetDashboardData', { type: t })
        .then(data => {
            $('#total-sale-amt').text(`₹ ${data.totalSale}`);
            $('#total-sale-detail').text(`${data.isSaleIncreased ? '+' : '-'} ₹ ${data.totalSaleDetail}`);
            $('#total-sale-per').html(`<span>${data.isSaleIncreased ? '+' : '-'} ${data.totalSalePer}%</span> <span class="icon-up ms-1"><i class="bi bi-arrow-up"></i></span>`);
            $('#total-sale-detail').parent().removeClass('green').removeClass('red').addClass(data.isSaleIncreased ? 'green' : 'red');

            $('#total-item-sale').text(`${data.totalItemSale}`);
            $('#totalItemSaleDetail').text(`${data.isItemSaleIncreased ? '+' : '-'} ${data.totalItemSaleDetail}`);
            $('#totalItemSalePer').html(`<span>${data.isItemSaleIncreased ? '+' : '-'} ${data.totalItemSalePer}%</span> <span class="icon-up ms-1"><i class="bi bi-arrow-up"></i></span>`);
            $('#totalItemSaleDetail').parent().removeClass('green').removeClass('red').addClass(data.isItemSaleIncreased ? 'green' : 'red');

            $('#total-net-profit').text(`₹ ${data.totalNetProfit}`);
            $('#totalNetDetail').text(`${data.isNetProfitIncreased ? '+' : '-'} ₹ ${data.totalNetDetail}`);
            $('#totalNetPer').html(`<span>${data.isNetProfitIncreased ? '+' : '-'} ${data.totalNetPer}%</span> <span class="icon-up ms-1"><i class="bi bi-arrow-up"></i></span>`);
            $('#totalNetDetail').parent().removeClass('green').removeClass('red').addClass(data.isNetProfitIncreased ? 'green' : 'red');

            $('#total-customer').text(`${data.totalCustomer}`);
            $('#totalCustomerDetail').text(`${data.isCustomerIncreased ? '+' : '-'} ₹ ${data.totalCustomerDetail}`);
            $('#totalCustomerPer').html(`<span>${data.isCustomerIncreased ? '+' : '-'} ${data.totalCustomerPer}%</span> <span class="icon-up ms-1"><i class="bi bi-arrow-up"></i></span>`);
            $('#totalCustomerDetail').parent().removeClass('green').removeClass('red').addClass(data.isCustomerIncreased ? 'green' : 'red');
        });

}
function GetChartData(t) {
    callGetApi('Admin/GetChartData', { type: t })
        .then(data => {
            t = t == undefined ? 'Year' : t;
            // Check if a chart already exists and destroy it
            if (window.myChart) {
                window.myChart.destroy();
            }
            const ctx = document.getElementById('salesChart').getContext('2d');
            const config = {
                type: 'line',
                data: {
                    labels: data.months,
                    datasets: [{
                        label: `${t}ly Sales`,
                        data: data.sales,
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
                                text: t
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
            };

            const salesChart = new Chart(ctx, config);
            window.myChart = salesChart;
        });
}