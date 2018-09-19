"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/audioStatusHub").build();

connection.on("AudioStatus", function (sipAddress, audioStatus) {
    console.log(sipAddress, audioStatus);
    var codecs = app.codecs.filter(c => c.sipAddress === sipAddress);
    console.log("codecs", codecs);
    var codec = codecs[0];
    codec.audioStatus = audioStatus;
    codec.updated = Date.now();
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

var subscribe = function (sipAddress) {
    console.log("subscribe to " + sipAddress);
    connection.invoke("subscribe", sipAddress).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};

var unsubscribe = function (sipAddress) {
    console.log("unsubscribe to " + sipAddress);
    connection.invoke("unsubscribe", sipAddress).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};

var app = new Vue({
    el: '#app',
    data: {
        codecs: []
    },
    methods: {
        subscribe: subscribe,
        unsubscribe: unsubscribe
    }
});

// Call CCM and fill codecs list.
axios.get("/codecinformation")
    .then(function (response) {
        let codecInformation = response.data;
        var codecs = codecInformation
            .filter(s => { return s.api === "IkusNet"; })
            .map(s => {
                return {
                    sipAddress: s.sipAddress,
                    updated: Date.now(),
                    audioStatus: {
                        vuValues: {
                            txLeft: 0,
                            txRight: 0,
                            rxLeft: 0,
                            rxRight: 0
                        }
                    }
                };
            });
        console.log(codecs);
        app.codecs = codecs;
    })
    .catch(function (error) {
        console.log(error);
    });