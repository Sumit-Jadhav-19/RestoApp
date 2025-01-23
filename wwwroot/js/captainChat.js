"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (user, message) {
    console.log(message);
    if (message.toUpperCase() === 'OREDERACCEPT') {
        if (typeof GetPreviousOrders === 'function') {
            GetPreviousOrders();
        }
    }
    else {
        console.log('checked');
        alertToast(message);
        GetPreviousOrders();
    }
});

connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});

$(function () {

});