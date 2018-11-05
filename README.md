# Codec Control



## CallController

------
#### `POST` `/api/codeccontrol/call`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Callee : [string] }`
* `{ ProfileName : [string] }`

**Response**
```
[boolean]
```

------
#### `POST` `/api/codeccontrol/hangup`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`

**Response**
```
[boolean]
```

------
#### `POST` `/api/codeccontrol/reboot`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`

**Response**
```
[boolean]
```


## CodecControlController

------
#### `GET` `/api/codeccontrol/isavailable`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
Available
```

------
#### `GET` `/api/codeccontrol/getavailablegpos`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
Active
Name
Number
```

------
#### `GET` `/api/codeccontrol/getinputgainandenabled`

**URL Params**

* `sipAddress = [string]`
* `input = [int]`

**Data Params**

* `None`

**Response**
```
Input
Enabled
GainLevel
```

------
#### `GET` `/api/codeccontrol/getinputenabled`

**URL Params**

* `sipAddress = [string]`
* `input = [int]`

**Data Params**

* `None`

**Response**
```
Input
Enabled
```

------
#### `GET` `/api/codeccontrol/getlinestatus`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
LineStatus
DisconnectReasonCode
DisconnectReasonDescription
RemoteAddress
```

------
#### `GET` `/api/codeccontrol/getvuvalues`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
RxLeft
RxRight
TxLeft
TxRight
```
                                                   
------
#### `GET` `/api/codeccontrol/getaudiostatus`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
Gpos
InputStatus
VuValues
```

------
#### `GET` `/api/codeccontrol/getaudiomode`

**URL Params**

* `sipAddress = [string]`

**Data Params**

* `None`

**Response**
```
EncoderAudioMode
DecoderAudioMode
```


## CodecControlController

------
#### `POST` `/api/codeccontrol/setgpo`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Number : [integer] }`
* `{ Active : [boolean] }`

**Response**
```
Number
Active
```

------
#### `POST` `/api/codeccontrol/setinputenabled`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Input : [integer] }`
* `{ Enabled : [boolean] }`

**Response**
```
Input
Enabled
```

------
#### `POST` `/api/codeccontrol/increaseinputgain`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Input : [integer] }`

**Response**
```
Input
GainLevel
```

------
#### `POST` `/api/codeccontrol/decreaseinputgain`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Input : [integer] }`

**Response**
```
Input
GainLevel
```

------
#### `POST` `/api/codeccontrol/changeinputgain`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Input : [integer] }`
* `{ Level : [integer] }`

**Response**
```
Input
GainLevel
```

------
#### `POST` `/api/codeccontrol/setinputgain`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ Input : [integer] }`
* `{ Level : [integer] }`

**Response**
```
Input
GainLevel
```

------
#### `POST` `/api/codeccontrol/batchsetinputenabled`

**URL Params**

* `None`

**Data Params**

* `{ SipAddress : [string] }`
* `{ InputEnableRequests : [ { Input : [integer], Enabled : [boolean] } ] }`

**Response**
```
Input
Enabled
```