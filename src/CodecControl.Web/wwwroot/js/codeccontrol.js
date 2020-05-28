"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/audioStatusHub").build();

connection.on("AudioStatus", (sipAddress, audioStatus) => {
    console.log("AudioStatus ", sipAddress, audioStatus);
    var codecs = app.codecs.filter(c => c.sipAddress === sipAddress);
    console.log("Codecs ", codecs);
    var codec = codecs[0];
    codec.audioStatus = audioStatus;
    codec.updated = Date.now();
});

connection.on("CodecControlSystemStatus", (systemInfo) => {
    console.log("CodecControlSystemStatus ", systemInfo);
    app.systemInformation = systemInfo;
});

connection.start().then(() => {
    connection.invoke("subscribeSystemStatus")
        .then(() => {
            console.log("Subscribed to SystemStatus");
        })
        .catch((err) => {
            return console.error(err.toString());
        });
}).catch((err) => {
    return console.error(err.toString());
});

var subscribe = (sipAddress) => {
    console.log("Subscribe to " + sipAddress);
    connection.invoke("subscribe", sipAddress)
        .then(() => {
            
        })
        .catch((err) => {
            return console.error(err.toString());
        });
};

var unsubscribe = (sipAddress) => {
    console.log("Unsubscribe to " + sipAddress);
    connection.invoke("unsubscribe", sipAddress)
        .then(() => {
            
        })
        .catch((err) => {
            return console.error(err.toString());
        });
};

const getCodecInformationBySipAddress = (sipAddress) => {
    if (!sipAddress) return;

    axios.get("/debug/codecinformation", { params: { sipAddress: sipAddress } })
        .then((response) => {
            let codecInformation = response.data;

            if (!codecInformation || !codecInformation.sipAddress) {
                console.warn('sipAddress not found');
                app.userFeedback = "SIP-address is not found";
                return;
            }

            if (app.codecs.some((ci) => { return ci.sipAddress === codecInformation.sipAddress; })) {
                console.warn('sipAddress already added');
                app.userFeedback = "SIP-address is already added";
                return;
            }

            app.codecs.push({
                sipAddress: codecInformation.sipAddress,
                ip: codecInformation.ip,
                api: codecInformation.api,
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
        })
        .catch((error) => {
            console.log(error);
        });
};

const setLogLevel = (level) => {
    console.log('set log level to ' + level);
    axios.post("/setloglevel", { logLevel: level })
        .then((response) => {
            var newLogLevel = response.data;
            app.currentLogLevel = newLogLevel;
            console.log('response', newLogLevel);
        });
};

var app = new Vue({
    el: '#app',
    data: {
        codecs: [],
        sipAddress: null,
        systemInformation: {},
        currentLogLevel: '',
        logLevels: ['Trace', 'Debug', 'Info', 'Warn', 'Error'],
        userFeedback: '',
        selectedLogLevel: '',
    },
    methods: {
        subscribe: subscribe,
        unsubscribe: unsubscribe,
        getCodecInformationBySipAddress: getCodecInformationBySipAddress,
        setLogLevel: setLogLevel,
    },
    mounted() {
        console.log("Mounted app");
    }
});
