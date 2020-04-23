The IRIS Codec Control - Codec Control
=========================================================

* Web Site: https://www.irisbroadcast.org
* Github: https://github.com/irisbroadcast

Gateway for controlling audio-codecs and devices. Exposes one unified restful and websocket interface for multiple proprietary codec-api's. 
This application uses CCM for device discovery.

It's written in .NET CORE MVC  and Windows/Linux) *Add this if you need codec control*
- Proxy for Codec Control requests, protocol translation and unification
- Query's CCM with SIP-address for getting IP-address to control
- REST API + Web Socket (SignalR) interface for codec control

License
=======

IRIS Codec Control is (C) Sveriges Radio AB, Stockholm, Sweden 2017
The code is licensed under the BSD 3-clause license.

The license for Codec Control is in the LICENSE.txt file

## API Documentation

The RESTful API documentation is automatically generated using Swagger. See your local Codec Control instance:

`https://<codeccontrol-url>/swagger/index.html`

There is a SignalR (WebSocket) hub available with some realtime data / topics.

`
'Online': // TO BE implemented
    Tick
    UnitName
    OnlineSince/Time
`

`
'AudioStatus':
    VuValues
    InputStatus
    Gpos
`
 