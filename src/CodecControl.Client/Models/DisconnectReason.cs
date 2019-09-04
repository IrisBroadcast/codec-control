#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

using System.ComponentModel;
using CodecControl.Client.Attributes;

namespace CodecControl.Client.Models
{
    public enum DisconnectReason
    {
        [MapAsResource("SipResponse_None")] None,
        [MapAsResource("SipResponse_UnableToResolveName")] UnableToResolveName,
        [MapAsResource("SipResponse_ErrorConnecting")] ErrorConnecting,
        [MapAsResource("SipResponse_DifferentProtocol")] DifferentProtocol,
        [MapAsResource("SipResponse_ConnectionRejected")] ConnectionRejected,
        [MapAsResource("SipResponse_RemoteDisconnected")] RemoteDisconnected,
        [MapAsResource("SipResponse_HangUp")] HangUp,
        [MapAsResource("SipResponse_ConnectionDropped")] ConnectionDropped,
        [MapAsResource("SipResponse_NotReady")] NotReady,

        // SIP Response Messages
        [MapAsResource("SipResponse_SipTrying")] SipTrying = 100,
        [MapAsResource("SipResponse_SipRinging")] SipRinging = 180,
        [MapAsResource("SipResponse_SipCallBeingForwarded")] SipCallBeingForwarded = 181,
        [MapAsResource("SipResponse_SipQueued")] SipQueued = 182,
        [MapAsResource("SipResponse_SipProgress")] SipProgress = 183,
        [MapAsResource("SipResponse_SipOk")] SipOk = 200,
        [MapAsResource("SipResponse_SipAccepted")] SipAccepted = 202,
        [MapAsResource("SipResponse_SipMultipleChoices")] SipMultipleChoices = 300,
        [MapAsResource("SipResponse_SipMovedPermanently")] SipMovedPermanently = 301,
        [MapAsResource("SipResponse_SipMovedTemporarily")] SipMovedTemporarily = 302,
        [MapAsResource("SipResponse_SipUseProxy")] SipUseProxy = 305,
        [MapAsResource("SipResponse_SipAlternativeService")] SipAlternativeService = 380,
        [MapAsResource("SipResponse_SipBadRequest")] SipBadRequest = 400,
        [MapAsResource("SipResponse_SipUnauthorized")] SipUnauthorized = 401,
        [MapAsResource("SipResponse_SipPaymentRequired")] SipPaymentRequired = 402,
        [MapAsResource("SipResponse_SipForbidden")] SipForbidden = 403,
        [MapAsResource("SipResponse_SipNotFound")] SipNotFound = 404,
        [MapAsResource("SipResponse_SipMethodNotAllowed")] SipMethodNotAllowed = 405,
        [MapAsResource("SipResponse_SipNotAcceptable")] SipNotAcceptable = 406,
        [MapAsResource("SipResponse_SipProxyAuthenticationRequired")] SipProxyAuthenticationRequired = 407,
        [MapAsResource("SipResponse_SipRequestTimeout")] SipRequestTimeout = 408,
        [MapAsResource("SipResponse_SipGone")] SipGone = 410,
        [MapAsResource("SipResponse_SipRequestEntityTooLarge")] SipRequestEntityTooLarge = 413,
        [MapAsResource("SipResponse_SipRequestUriTooLong")] SipRequestUriTooLong = 414,
        [MapAsResource("SipResponse_SipUnsupportedMediaType")] SipUnsupportedMediaType = 415,
        [MapAsResource("SipResponse_SipUnsupportedUriScheme")] SipUnsupportedUriScheme = 416,
        [MapAsResource("SipResponse_SipBadExtension")] SipBadExtension = 420,
        [MapAsResource("SipResponse_SipExtensionRequired")] SipExtensionRequired = 421,
        [MapAsResource("SipResponse_SipSessionTimerTooSmall")] SipSessionTimerTooSmall = 422,
        [MapAsResource("SipResponse_SipIntervalTooBrief")] SipIntervalTooBrief = 423,
        [MapAsResource("SipResponse_SipTemporarilyUnavailable")] SipTemporarilyUnavailable = 480,
        [MapAsResource("SipResponse_SipCallTsxDoesNotExist")] SipCallTsxDoesNotExist = 481,
        [MapAsResource("SipResponse_SipLoopDetected")] SipLoopDetected = 482,
        [MapAsResource("SipResponse_SipTooManyHops")] SipTooManyHops = 483,
        [MapAsResource("SipResponse_SipAddressIncomplete")] SipAddressIncomplete = 484,
        [MapAsResource("SipResponse_SipAmbiguous")] SipAmbiguous = 485,
        [MapAsResource("SipResponse_SipBusyHere")] SipBusyHere = 486,
        [MapAsResource("SipResponse_SipRequestTerminated")] SipRequestTerminated = 487,
        [MapAsResource("SipResponse_SipNotAcceptableHere")] SipNotAcceptableHere = 488,
        [MapAsResource("SipResponse_SipBadEvent")] SipBadEvent = 489,
        [MapAsResource("SipResponse_SipRequestUpdated")] SipRequestUpdated = 490,
        [MapAsResource("SipResponse_SipRequestPending")] SipRequestPending = 491,
        [MapAsResource("SipResponse_SipUndecipherable")] SipUndecipherable = 493,
        [MapAsResource("SipResponse_SipInternalServerError")] SipInternalServerError = 500,
        [MapAsResource("SipResponse_SipNotImplemented")] SipNotImplemented = 501,
        [MapAsResource("SipResponse_SipBadGateway")] SipBadGateway = 502,
        [MapAsResource("SipResponse_SipServiceUnavailable")] SipServiceUnavailable = 503,
        [MapAsResource("SipResponse_SipServerTimeout")] SipServerTimeout = 504,
        [MapAsResource("SipResponse_SipVersionNotSupported")] SipVersionNotSupported = 505,
        [MapAsResource("SipResponse_SipMessageTooLarge")] SipMessageTooLarge = 513,
        [MapAsResource("SipResponse_SipPreconditionFailure")] SipPreconditionFailure = 580,
        [MapAsResource("SipResponse_SipBusyEverywhere")] SipBusyEverywhere = 600,
        [MapAsResource("SipResponse_SipDecline")] SipDecline = 603,
        [MapAsResource("SipResponse_SipDoesNotExistAnywhere")] SipDoesNotExistAnywhere = 604,
        [MapAsResource("SipResponse_SipNotAcceptableAnywhere")] SipNotAcceptableAnywhere = 606
    }
}