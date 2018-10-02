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
    connection.invoke("subscribe", sipAddress)
        .then(function() {
            getSubscriptions();
        })
        .catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
};

var unsubscribe = function (sipAddress) {
    console.log("unsubscribe to " + sipAddress);
    connection.invoke("unsubscribe", sipAddress)
        .then(function () {
            getSubscriptions();
        })
        .catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
    getSubscriptions();
};

var getSubscriptions = function() {
    axios.get("/debug/subscriptions")
        .then(function (response) {
            let subscriptions = response.data;
            console.log("Prenumerationer", subscriptions);
            app.subscriptions = subscriptions;
        })
        .catch(function (error) {
            console.log(error);
        });
};

const getCodecInformationBySipAddress = function (sipAddress) {
    if (!sipAddress) return;

    axios.get("/debug/codecinformation", { params: { sipAddress: sipAddress } })
        .then(function (response) {
            let codecInformation = response.data;

            if (!codecInformation || !codecInformation.sipAddress) {
                console.warn('sipAddress not found');
                return;
            }
            if (app.codecInformation.some(function (ci) { return ci.sipAddress === codecInformation.sipAddress; })) {
                console.warn('sipAddress already added');
                return;
            }

            app.codecInformation.push(codecInformation);

            if (codecInformation.api === 'IkusNet') {
                app.codecs.push({
                    sipAddress: codecInformation.sipAddress,
                    updated: Date.now(),
                    audioStatus: {
                        vuValues: {
                            txLeft: 0,
                            txRight: 0,
                            rxLeft: 0,
                            rxRight: 0
                        }
                    }
                });
            }
        })
        .catch(function (error) {
            console.log(error);
        });
};

var app = new Vue({
    el: '#app',
    data: {
        codecs: [],
        codecInformation: [],
        subscriptions: [],
        sipAddress: null
    },
    methods: {
        subscribe: subscribe,
        unsubscribe: unsubscribe,
        getSubscriptions: getSubscriptions,
        getCodecInformationBySipAddress: getCodecInformationBySipAddress
    }
});

//getCodecInformationBySipAddress('mtu-25@contrib.sr.se');
//getSubscriptions();