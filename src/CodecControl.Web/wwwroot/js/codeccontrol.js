"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/audioStatusHub").build();

connection.on("AudioStatus", function (audioStatus) {
    console.log(audioStatus);
    //var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    //var encodedMsg = user + " says " + msg;
    //var li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

var subscribe = function(sipAddress) {
    connection.invoke("subscribe", sipAddress).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};

var unsubscribe = function (sipAddress) {
    connection.invoke("unsubscribe", sipAddress).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});