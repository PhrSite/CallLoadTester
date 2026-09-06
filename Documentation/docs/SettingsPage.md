# Settings Page
This page allows you to set all of the configuration settings for the Call Load Tester application.

The settings are saved in a file when you click on the Submit button.

## SIP Address Settings

### <a name="ToSipUri">To SIP URI</a>
This text field specifies the address of who to call in the form of a SIP URI.

The following table shows some examples of valid SIP URIs.

| SIP URI | Description |
|---------|-------------|
| sip:911@192.168.64:5060 | Send an INVITE request to IPv4 address = 192.168.1.64 on port 5060 using UDP |
| sip:911@192.168.64:5060;transport=tcp | Send an INVITE request to IPv4 address = 192.168.1.64 on port 5060 using TCP |
| sip:911@[2600:1700:7A40:4740::525];transport=tcp | Send an INVITE to IPv6 address = 2600:1700:7A40:4740::525 on port 5060 using TCP |
| sips:911@psapsimulator.net;transport=tcp | Perform a DNS query for psapsimulator.net. Send an INVITE to the resolved IP address on port 5061 using SIPS (SIP over TLS) |

The host portion of the SIP URI can either be host name/domain or an IP endpoint consisting of an IP address and a port number.

If the host portion of the SIP URI is a host name then the application will lookup the host name via a DNS request to resolve it to an IP address. If the computer has an IPv4 address then the application will perform a A record DNS query. If the computer has an IPv6 address then the application will perform a AAAA record DNS query.

If the application finds both an IPv4 and an IPv6 address for the host name then it will use the Prefer IPv6 setting to determine which IP address to use. If Prefer IPv6 is checked then the application will use the IPv6 address, else it will use the IPv4 address.

If the host portion is an IP endpoint then the application will not perform a DNS query.

The port parameter of the SIP URI is optional. If a port is not specified then the application will use 5060 for SIP and 5061 for SIPS (SIP over TLS).

### Starting From Number
This application uses a unique SIP From header for each INVITE request that it sends to the system under test. This setting specifies a 10-digit telephone number that will be used for the user part of the SIP URI in the From header of the first INVITE request. The telephone number will be incremented by one for each subsequent INVITE request.

For example, if the Starting From Number setting is 1000000000 and the local IPv4 address is 192.168.1.76, then the From header in the first INVITE request will be From: <sip:1000000000@192.168.1.76> and the From header for the second INVITE request will be From: <sip:1000000001@192.168.1.76>.

### Use urn:service:sos
If this checkbox is checked then the SIP Request URI in the request line of each INVITE request will be urn:service:sos. If this checkox is not checked then the SIP Request URI in the request line of each INVITE request will be set to the [SIP To URI](#ToSipUri).

## Number of Calls
The following settings specify the number and rate of call request that the application will send to the system under test.

### Total Calls
This setting specifies the number of call attempts that the application will perform. The application will stop sending INVITE requests when the number of call attempts reaches this limit.

### Simultaneous Calls
This setting specifies the maximum number of simultaneous call attempts to perform. For example, if this setting is 10, the application will send 10 INVITE requests to the call target. The application will not send another INVITE request until one or more of the calls is terminated or fails.

### Call Interval
The Call Interval setting specifies the interval to wait between sending consecutive INVITE requests in milliseconds. The minimum setting is 0 milliseconds. There is no upper limit for this setting.

When the call interval setting is below 10 milliseconds or so, the actual call interval is somewhat non-deterministic depending upon the operating system (Linux or Windows) and the SIP transport (UDP, TCP or TLS) being used. If using the UDP protocol for SIP, the minimum achieved call interval may be sub-millisecond. If using TCP or TLS, the minimum achieved call interval is typically on the order of milliseconds to 10 or 20 milliseconds.

### Call Duration
This application automatically terminates answered calls after the number of seconds specified by this setting. The minimum setting is 1 second. There is no upper limit for this setting.

If the call duration is less than 5 seconds call quality MOS scores will not be available.

## Location Settings
This application is capable of generating unique locations for each call and it will send a semi-random caller location for each call.

Semi-random locations are generated within a rectangle that is determined by the Starting Latitude, Starting Longitude, Delta Latitude and Delta Longitude settings.

### Send Location
If this checkbox is checked then the application will send a semi-random caller location with each call.

### Starting Latitude
This setting specifies the latitude in decimal degrees to use for generation of caller locations. The spacial reference is WGS-84.

The minimum value is -90 and the maximum value is 90 degrees.

### Starting Longitude
This setting specifies the longitude in decimal degrees to use for generation of caller locations. The spacial reference is WGS-84.

The minimum value is -180 and the maximum value is 180 degrees.

### Delta Latitude
All latitudes for the caller location will be between the Starting Latitude value and this value plus the Starting Latitude value.

The minimum value is 0 and the maximum value is limited to 10 degrees.

### Delta Longitude
All longitudes for the caller location will be between the Starting Longitude value and this value plus the Starting Longitude.

The minimum value is 0 and the maximum value is limited to 10 degrees.

## Network Settings
The network settings determine the IP addresses and port numbers used for SIP.

### Enable IPv4
If checked then IP version 4 (IPv4) will be used for SIP and media.

### IPv4 Address
This setting selects the local IPv4 address that the application will bind to for SIP and RTP.

### Enable IPv6
If checked then IP version 6 (IPv6) will be used for SIP and media.

### IPv6 Address
This setting selects the local IPv6 address that the application will bind to for SIP and RTP.

### Prefer IPv6
This setting determines which IP protocol to use (IPv4 or IPv6) if it performs a DNS host name lookup and finds both an IPv4 address and an IPv6 address for the hostname portion of the SIP To URI setting.

If the application finds both an IPv4 and an IPv6 address for the host name then it will use the Prefer IPv6 setting to determine which IP address to use. If Prefer IPv6 is checked then the application will use the IPv6 address, else it will use the IPv4 address.

## SIP Protocol Settings

### Local SIP Port
This setting specifies the local SIP port to bind to for UDP and TCP.

The default setting is 5060.

### Local SIPS Port
This setting specifies the local SIP port to bind to for SIPS (SIP over TLS).

The default setting is 5061.

### Audio Codec
This setting specifies which audio codec will be offered. This appication will offer only one audio codec in the INVITE request.

The available codecs are:

- PCMU
- PCMA
- G722
- G729
- AMR-WB

The default setting is PCMU.

### Audio Encryption
This setting specifies the type of audio media encryption to offer the call target in the INVITE request.

The following encryption methods are supported.

- None
- SDES-SRTP
- DTLS-SRTP

The default setting is None.

### Starting Audio Port
This setting specifies the first UDP port in the range of ports to used for audio media. The audio port range is from this port number to this port number plus the Number of Ports setting - 1.

This application manages ports within the audio port range. It allocates 2 ports for each call. The even ports are for audio media. The odd port numbers are for use the Real Time Control Protocol for the audio media.

### Number of Ports
The Number of Ports setting specifies the number of RTP ports to reserve for audio media.

This setting must be at least twice the Simultaneous Calls setting because two ports are allocated for each call.



