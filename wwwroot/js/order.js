
var queryString = window.location.search;
var urlParams = new URLSearchParams(queryString);
let userOrders = [];
$(function () {
    GetHallTables();
    GetPreviousOrders();
    $('.mob-order').click(function () {
        orderCount = 0;
        $('.order-count').addClass('d-none');
        $('.order-count').text(orderCount);
        $('.order-container').toggleClass('active');
    });
    $('.btnUserOrder').click(function () {
        AddUserOrders();
    });
    $('.btnUserBill').click(function () {
        OrderBill();
    });
});
function GetHallTables() {
    const tableId = urlParams.get('table');
    if (tableId != null || tableId != '' || tableId != undefined) {
        callGetApi('Captain/GetHallTablesById', { tableId: tableId })
            .then(data => {
                $('.order-hall').text(data.hallName);
                $('.order-table').text(data.name);
            });
    }
}
function GetPreviousOrders() {
    callGetApi('Captain/GetPreviousOrders', { tableId: urlParams.get('table') })
        .then(data => {
            console.log(data);
            let strOrders = '';
            $('.accordion').html('');
            $.each(data, function (i, v) {
                $('.preOrderId').text(v.orderId);
                $('#orderId').val(v.orderId);
                let isAccept = v.isAccept ? '' : `<span class="accordion-close" onclick="deletePrevOrderItem('${v.detailsId}')"><i class="bi bi-trash-fill text-danger"></i></span>`
                let strSize = '';

                let orderStatus = v.isAccept ? v.orderStatus == "C" ? `<div class="isAccept text-success">Completed</div>` : `<div class="isAccept text-primary">Preparing</div>` : `<div class="isAccept text-danger">Not Accept</div>`;
                if (data.isHalfAvailable) {
                    strSize = `<div>
                            <span>Size</span>
                            <div class="size-box d-flex align-items-center fs-3 gap-2">
                                <div>
                                    <input type="radio" class="form-check-inline d-none" id="rd${v.detailsId}_f" name="g_${v.detailsId}" value="h"  onchange="handleRadioChange(this)"/>
                                    <label for="rd${v.detailsId}_f"><i class="bi bi-heart-half"></i></label>
                                </div>
                                <div>
                                    <input type="radio" class="form-check-inline d-none" id="rd${v.detailsId}_s" name="g_${v.detailsId}" value="f" checked  onchange="handleRadioChange(this)"/>
                                    <label for="rd${v.detailsId}_s"><i class="bi bi-suit-heart-fill"></i></label>
                                </div>
                            </div>
                        </div>`;
                }
                strOrders = `<div class="accordion__item position-relative" id="${v.detailsId}">
                <div class="d-flex align-items-center justify-content-between pe-3">
                    <div class="accordion__header d-flex align-items-center gap-2" id="faq${v.detailsId}" onclick=toggleAccordionItem('faq${v.detailsId}') data-menuId='${v.detailsId}'>
                        <span><i class="bi bi-chevron-right"></i></span>
                        <div class="d-flex align-items-center gap-3">
                            <span>${v.name}</span> <span>-</span>
                            <div class="d-flex align-items-center">
                                <h5 class="m-0 quant">${v.quantity}</h5>
                                <i class="bi bi-dot fs-3"></i>
                               ${v.size.toLowerCase() == "f" ? '<span class="size">Full</span>' : '<span class="size">Half</span>'} 
                            </div>
                        </div>
                    </div>
                    ${isAccept}
                </div>
                <div class="accordion__content" id="faq${v.detailsId}_content">
                ${orderStatus}
                    <div class="p-3 d-flex justify-content-between">
                        <div>
                            <span>Quantity</span>
                            <div class="quantity-box">
                                <button class="btn btn-primary" onclick="decreaseQuantity(this)">-</button>
                                <input type="number" id="quantity" min="1" value="${v.quantity}">
                                <button class="btn btn-primary" onclick="increaseQuantity(this)">+</button>
                            </div>
                        </div>
                ${strSize}
                    </div>
                    ${v.isAccept ? '' : '<div class="d-flex justify-content-center mb-2" onclick="updatePrevItem(this)"><button class="btn btn-primary">Update</button></div>'} 
                </div>
            </div>`;
                $('.accordion').append(strOrders);
            });
            if (data.length > 0) {
                $('.btnUserBill').attr('disabled', false);
                $('.btnUserOrder').attr('disabled', true);
            }
        });
}
function toggleAccordionItem(toggle) {
    const togglers = document.querySelectorAll('.accordion__header');
    const toggler = document.getElementById(toggle);
    const blocks = document.querySelectorAll(`.accordion__content`);
    const block = document.querySelector(`#${toggle}_content`);

    togglers.forEach((t) => {
        if (toggler != t) {
            t.classList.remove('active');
        }
    });
    blocks.forEach((b) => {
        if (block != b) {
            b.style.maxHeight = '';
            b.previousElementSibling.style.backgroundColor = '';
            b.parentElement.classList.remove('accordion__item__active');
        }
    });
    if (toggler.classList.contains('active')) {
        block.style.maxHeight = '';
        block.previousElementSibling.style.backgroundColor = '';
        block.parentElement.classList.remove('accordion__item__active');
    } else {
        block.style.maxHeight = block.scrollHeight + 'px';
        block.previousElementSibling.style.backgroundColor = '#c0c0c042';
        block.parentElement.classList.add('accordion__item__active');
    }
    toggler.classList.toggle('active')
}
function deleteOrderItem(event) {
    const deleteButton = event.target;
    const accordionItem = deleteButton.closest('.accordion__item');
    const accordinHeader = accordionItem.querySelector('.accordion__header')
    const MenuId = accordinHeader.getAttribute('data-menuId');
    userOrders = userOrders.filter(item => item.MenuId !== MenuId.toString());
    if (accordionItem) {
        accordionItem.remove();
        if (userOrders.length > 0) {
            $('.btnUserOrder').attr('disabled', false);
        } else {
            $('.btnUserOrder').attr('disabled', true);
        }
    }
}
function increaseQuantity(quantityInput) {
    const accordianConent = quantityInput.closest('.accordion__content');
    const accordinHeader = accordianConent.previousElementSibling;
    var currentQuantity = parseInt(quantityInput.previousElementSibling.value);
    quantityInput.previousElementSibling.value = currentQuantity + 1;
    const quant = accordinHeader.querySelector('.quant');
    quant.textContent = quantityInput.previousElementSibling.value;
    const MenuId = accordinHeader.firstElementChild.getAttribute('data-menuId');
    let item = userOrders.find(item => item.MenuId === MenuId.toString());
    if (item) {
        item.Quantity = quantityInput.previousElementSibling.value;
    }
}
function decreaseQuantity(quantityInput) {
    const accordianConent = quantityInput.closest('.accordion__content');
    const accordinHeader = accordianConent.previousElementSibling;
    var currentQuantity = parseInt(quantityInput.nextElementSibling.value);
    if (currentQuantity > 1) {
        quantityInput.nextElementSibling.value = currentQuantity - 1;
        const quant = accordinHeader.querySelector('.quant');
        quant.textContent = quantityInput.nextElementSibling.value;
        const MenuId = accordinHeader.firstElementChild.getAttribute('data-menuId');
        let item = userOrders.find(item => item.MenuId === MenuId.toString());
        if (item) {
            item.Quantity = quantityInput.nextElementSibling.value;
        }
        console.log(userOrders);
    }
}
function handleRadioChange(selectedRadio) {
    const accordianConent = selectedRadio.closest('.accordion__content');
    const accordinHeader = accordianConent.previousElementSibling;
    const groupName = selectedRadio.name;
    const selectedValue = selectedRadio.value;
    const size = accordinHeader.querySelector('.size');
    size.textContent = selectedValue == 'h' ? 'Half' : 'Full';
    const MenuId = accordinHeader.firstElementChild.getAttribute('data-menuId');
    let item = userOrders.find(item => item.MenuId === MenuId.toString());
    if (item) {
        item.Size = selectedValue;
    }
}
let orderCount = 0;
function addOrders(menuId, menu) {
    let strOrder = '';
    if (menuId != null && menu != null || menuId != '' && menu != '' || menuId != undefined && menu != undefined) {
        let objOrder = { MenuId: menuId, Quantity: 1, Size: 'F' };
        if (!userOrders.some(item => item.MenuId === objOrder.MenuId)) {
            userOrders.push(objOrder);
        }
        else {
            return;
        }

        callGetApi('Menu/GetMenuById', { Id: menuId })
            .then(data => {
                console.log(data);
                let strSize = '';
                if (data.isHalfAvailable) {
                    strSize = `<div>
                            <span>Size</span>
                            <div class="size-box d-flex align-items-center fs-3 gap-2">
                                <div>
                                    <input type="radio" class="form-check-inline d-none" id="rd${menuId}_f" name="g_${menuId}" value="h"  onchange="handleRadioChange(this)"/>
                                    <label for="rd${menuId}_f"><i class="bi bi-heart-half"></i></label>
                                </div>
                                <div>
                                    <input type="radio" class="form-check-inline d-none" id="rd${menuId}_s" name="g_${menuId}" value="f" checked  onchange="handleRadioChange(this)"/>
                                    <label for="rd${menuId}_s"><i class="bi bi-suit-heart-fill"></i></label>
                                </div>
                            </div>
                        </div>`;
                }
                strOrder = `<div class="accordion__item">
                <div class="d-flex align-items-center justify-content-between pe-3">
                    <div class="accordion__header d-flex align-items-center gap-2" id="faq${menuId}" onclick=toggleAccordionItem('faq${menuId}') data-menuId='${menuId}'>
                        <span><i class="bi bi-chevron-right"></i></span>
                        <div class="d-flex align-items-center gap-3">
                            <span>${menu}</span> <span>-</span>
                            <div class="d-flex align-items-center">
                                <h5 class="m-0 quant">1</h5>
                               ${data.isHalfAvailable ? '<i class="bi bi-dot fs-3"></i> <span class="size">Full</span>' : ''} 
                            </div>
                        </div>
                    </div>
                    <span class="accordion-close" onclick="deleteOrderItem(event)"><i class="bi bi-trash-fill text-danger"></i></span>
                </div>
                <div class="accordion__content" id="faq${menuId}_content">
                    <div class="p-3 d-flex justify-content-between">
                        <div>
                            <span>Quantity</span>
                            <div class="quantity-box">
                                <button class="btn btn-primary" onclick="decreaseQuantity(this)">-</button>
                                <input type="number" id="quantity" value="1" min="1">
                                <button class="btn btn-primary" onclick="increaseQuantity(this)">+</button>
                            </div>
                        </div>
                ${strSize}
                    </div>
                </div>
            </div>`;
                $('.accordion').append(strOrder);

                console.log(userOrders);
                orderCount++;
                $('.order-count').removeClass('d-none');
                $('.order-count').text(orderCount);
                if (userOrders.length > 0) {
                    $('.btnUserOrder').attr('disabled', false);
                }
            });
    }
}
function AddUserOrders() {
    var order = {
        OrderId: $('#orderId').val() == '' ? 0 : $('#orderId').val(),
        TableId: urlParams.get('table'),
        OrderDetails: userOrders
    };
    callPostApi('Captain/AddOrders', order)
        .then(data => {
            $('.order-container').removeClass('active');
            userOrders = [];
            $('.accordion').html('');
            $('.btnUserOrder').attr('disabled', true);
            GetPreviousOrders();
        });
}
function OrderBill() {
    console.log('sdsdg');
    callPostApi('Captain/OrderBill', { tableId: urlParams.get('table') })
        .then(data => {

        });
}

function deletePrevOrderItem(Id) {
    console.log(Id);
    confirmation()
        .then(res => {
            if (res) {
                callPostApi('Captain/DeletePrevOrderItem', { detailsId: Id })
                    .then(data => {
                        GetPreviousOrders();
                    });
            }
        });
}
function updatePrevItem(quantityInput) {
    const quant = quantityInput.previousElementSibling;
    callPostApi('Captain/UpdatePrevOrderItem', { detailsId: quantityInput.closest('.accordion__item').id, quantity: quant.querySelector('#quantity').value,size:'F' })
        .then(data => {
            GetPreviousOrders();
        });

}