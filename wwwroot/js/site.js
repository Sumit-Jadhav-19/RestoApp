const toggleButton = document.getElementById("toggle-btn");
const sidebar = document.getElementById("sidebar");
function toggleSidebar() {
    toggleButton.classList.toggle("rotate");

    const container = document.querySelector("body");

    if (sidebar.classList.contains("close")) {
        sidebar.classList.remove("close");
        document.body.classList.add('overflow-y-hidden');
        container.classList.add('show');
    } else {
        sidebar.classList.add("close");
        document.body.classList.remove('overflow-y-hidden');
        container.classList.remove('show');
    }

    closeAllSubMenus();
}
function toggleSubMenu(button) {
    if (!button.nextElementSibling.classList.contains("show")) {
        closeAllSubMenus();
    }
    button.nextElementSibling.classList.toggle("show");

    const container = document.querySelector("body");

    if (sidebar.classList.contains("close")) {
        toggleButton.classList.toggle("rotate");
        sidebar.classList.remove("close");
        container.classList.add('show');
    }

    // if (sidebar.classList.contains("close")) {
    //   sidebar.classList.toggle("close");
    //   toggleButton.classList.toggle("rotate");
    // }
}
function closeAllSubMenus() {
    Array.from(sidebar.getElementsByClassName("show")).forEach((ul) => {
        ul.classList.remove("show");
        ul.previousElementSibling.classList.remove("rotate");
    });
}
const checkbox = document.getElementById('clicker');
const panel = document.querySelector('.info-panel');
const close = document.querySelector('.modal-close');
document.body.addEventListener('click', function (event) {
    if (panel) {
        if (!panel.contains(event.target) && !checkbox.contains(event.target) || close.contains(event.target)) {
            checkbox.checked = false;
        }
    }
});
const procheckbox = document.getElementById('profileClicker');
const proPanel = document.querySelector('.profile-panel');
const proSpan = document.querySelector('.profileClicker span ');
const proSVG = document.querySelector('.profileClicker svg');
function togglePanel() {
    if (procheckbox.checked) {
        procheckbox.checked = true;
    } else {
        procheckbox.checked = false;
    }
}
if (procheckbox) {
    procheckbox.addEventListener('change', togglePanel);
}
document.body.addEventListener('click', function (event) {
    if (proPanel && procheckbox && !proPanel.contains(event.target) && event.target != procheckbox && event.target != proSpan && event.target != proSVG) {
        procheckbox.checked = false;
        procheckbox.checked = false;
    }
});
var baseUrl = "/";
$(function () {
    getHostName();
    let currentTime = getCurrentTime();
    $('#current-time').text(currentTime);
    setInterval(function () {
        let currentDate = getCurrentDate();
        let currentTime = getCurrentTime();
        $('#current-date').text(currentDate);
        $('#current-time').text(currentTime);
    }, 1000);
});
function getCurrentDate(dt) {
    const currentDate = dt == undefined ? new Date() : new Date(dt);
    const options = { weekday: 'short', day: '2-digit', month: 'short', year: 'numeric' };
    const formattedDate = currentDate.toLocaleDateString('en-US', options);
    return formattedDate;
}
function getCurrentTime(dt) {
    var date = dt == undefined ? new Date() : new Date(dt);
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var seconds = date.getSeconds();
    var ampm = hours >= 12 ? "PM" : "AM";
    hours = hours % 12;
    hours = hours ? hours : 12;
    minutes = minutes < 10 ? "0" + minutes : minutes;
    //seconds = seconds < 10 ? "0" + seconds : seconds;
    var formattedTime = `${hours}:${minutes}:${seconds} ${ampm}`;
    return formattedTime;
}
function getHostName() {
    var hostname = window.location.host;
    var pathname = window.location.pathname;
    if (hostname.indexOf(':') >= 0) {
        baseUrl = "/";
    } else {
        var readupto = pathname.indexOf('/', 1) == -1 ? pathname.length : pathname.indexOf('/', 1);
        baseUrl = "/" + pathname.substring(1, readupto) + "/";
    }
}
function showModal(m) {
    $(`#${m}`).modal('show');
    $(`#${m} form input[type="text"]`).val('');
    $(`#${m} form input[type="hidden"]:not([name="__RequestVerificationToken"])`).val('');
    $(`#${m} .field-validation-error`).text('');
}
function callGetApi(e, d) {
    return new Promise((resolve, reject) => {
        $.ajax({
            type: "GET",
            url: baseUrl + e,
            data: d,
            success: function (res) {
                if (res.statusCode === 1) {
                    resolve(res.data);
                }
            },
            error: function (xhr, e, msg) {
                reject(new Error(e + ' ' + msg));
            }
        });
    });
}
function callPostApi(e, d) {
    return new Promise((resolve, reject) => {
        // Get anti-forgery token value from the form
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            type: "POST",
            url: baseUrl + e,
            data: d,
            headers: {
                'RequestVerificationToken': token
            },
            success: function (res) {
                console.log(res);
                if (res.status != null) {
                    if (res.statusCode === 1) {
                        resolve(res.data);
                        toast('s', res.message);
                    }
                    else {
                        toast('e', res.message);
                    }
                }
            },
            error: function (xhr, e, msg) {
                reject(new Error(e + ' ' + msg));
            }
        });
    });
}
function callPostApiProcessData(e, d) {
    return new Promise((resolve, reject) => {
        // Get anti-forgery token value from the form
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            type: "POST",
            url: baseUrl + e,
            data: d,
            headers: {
                'RequestVerificationToken': token
            },
            processData: false,
            contentType: false,
            success: function (res) {
                console.log(res);
                if (res.status != null) {
                    if (res.statusCode === 1) {
                        resolve(res.data);
                        toast('s', res.message);
                    }
                    else {
                        toast('e', res.message);
                    }
                }
            },
            error: function (xhr, e, msg) {
                reject(new Error(e + ' ' + msg));
            }
        });
    });
}
function confirmation() {
    console.log('called');
    const popup = $('#confirmationPopup');
    const confirmBtn = $('#confirmBtn');
    const cancelBtn = $('#cancelBtn');
    const closeBtn = $('#confirmationPopupClose');
    popup.removeClass('d-none');
    return new Promise((resolve, reject) => {
        confirmBtn.click(function () {
            setTimeout(function () {
                popup.addClass('d-none');
            }, 100);
            resolve(true);
        });
        cancelBtn.click(function () {
            setTimeout(function () {
                popup.addClass('d-none');
            }, 100);
            resolve(false);
        });
        closeBtn.click(function () {
            setTimeout(function () {
                popup.addClass('d-none');
            }, 100);
            resolve(false);
        });
    });

}
function toast(t, m) {
    var toastElement = $('<div>', {
        class: 'toast',
        role: 'alert',
        'aria-live': 'assertive',
        'aria-atomic': 'true'
    });
    var toastHeader = $('<div>', { class: 'toast-header' });
    var svgIcon = '';
    var strong = '';
    if (t.toLowerCase() === 's') {
        svgIcon = $('<div>').html(`
            <svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 50 50" xml:space="preserve" width="800px" height="800px" fill="#000000">
               <g stroke-width="0"/>
               <g stroke-linecap="round" stroke-linejoin="round"/>
               <g> <circle style="fill:#25ad4d;" cx="25" cy="25" r="25"/> <polyline style="fill:none;stroke:#FFFFFF;stroke-width:2;stroke-linecap:round;stroke-linejoin:round;stroke-miterlimit:10;" points=" 38,15 22,33 12,25 "/> </g>
            </svg>
        `);
        strong = $('<strong>', { class: 'me-auto ms-2 text-success', text: 'Sucess' });
    } else if (t.toLowerCase() === 'e') {
        svgIcon = $('<div>').html(`
            <svg version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 50 50" xml:space="preserve" width="20" height="20" fill="#000000">
                <g stroke-width="0"/>
                <g stroke-linecap="round" stroke-linejoin="round"/>
                <g>
                    <circle style="fill:#ee4f3a;" cx="25" cy="25" r="25"/>
                    <polyline style="fill:none;stroke:#FFFFFF;stroke-width:2;stroke-linecap:round;stroke-miterlimit:10;" points="16,34 25,25 34,16"/>
                    <polyline style="fill:none;stroke:#FFFFFF;stroke-width:2;stroke-linecap:round;stroke-miterlimit:10;" points="16,16 25,25 34,34"/>
                </g>
            </svg>
        `);
        strong = $('<strong>', { class: 'me-auto ms-2 text-danger', text: 'Failed' });
    }
    else {
        svgIcon = $('<div>').html(`
             <svg width="800px" height="800px" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" aria-hidden="true" role="img" class="iconify iconify--emojione" preserveAspectRatio="xMidYMid meet" fill="#000000">
                 <g stroke-width="0"/>
                 <g stroke-linecap="round" stroke-linejoin="round"/>
                 <g > <path d="M5.9 62c-3.3 0-4.8-2.4-3.3-5.3L29.3 4.2c1.5-2.9 3.9-2.9 5.4 0l26.7 52.5c1.5 2.9 0 5.3-3.3 5.3H5.9z" fill="#ffce31"> </path> <g fill="#ffffff"> <path d="M27.8 23.6l2.8 18.5c.3 1.8 2.6 1.8 2.9 0l2.7-18.5c.5-7.2-8.9-7.2-8.4 0"> </path> <circle cx="32" cy="49.6" r="4.2"> </circle> </g> </g>
             </svg>
        `);
        strong = $('<strong>', { class: 'me-auto ms-2 text-warning', text: 'Warning' });
    }
    var small = $('<small>', { text: 'Just now' });
    var closeButton = $('<button>', {
        type: 'button',
        class: 'btn-close',
        'data-bs-dismiss': 'toast',
        'aria-label': 'Close'
    });
    toastHeader.append(svgIcon, strong, small, closeButton);
    var toastBody = $('<div>', { class: 'toast-body', text: m });
    toastElement.append(toastHeader, toastBody);
    $('#toastContainer').append(toastElement);
    var toastBootstrap = new bootstrap.Toast(toastElement[0]);
    toastBootstrap.show();
    setTimeout(function () { toastElement.remove(); }, 6000);
}

function fromatDate(dt) {
    if (dt == "-") {
        return "-";
    }
    var date = new Date(dt);
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var seconds = date.getSeconds();
    var ampm = hours >= 12 ? "PM" : "AM";
    hours = hours % 12;
    hours = hours ? hours : 12;
    minutes = minutes < 10 ? "0" + minutes : minutes;
    //seconds = seconds < 10 ? "0" + seconds : seconds;
    var formattedDate = `${date.getDate()}-${date.getMonth() + 1}-${date.getFullYear()} ${hours}:${minutes} ${ampm}`;
    return formattedDate;
}
function checkIsNullObj(inputObj) {
    $.each(inputObj, function (key, value) {
        if (value === null || value === "" || value === undefined) {
            inputObj[key] = "-";
        }
    });
    return inputObj;
}