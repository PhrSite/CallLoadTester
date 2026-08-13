# Call Quality Page
The Call Quality page displays call quality statistics for all calls that have been completed.

Data for calls that have been completed but are less than 5 seconds in duration will not be included in the call quality statistics shown in this page.

# Call Quality Summary
This table shows a summary of the call quality statistics for all calls.

## Completed Calls
Shows the number of calls that have been completed and have a call duration of greater than 5 seconds.

## Maximum Jitter
The Maximum Jitter is the maximum smoothed RTP packet jitter in milliseconds for all completed calls.

## Out of Order Packets
Shows the total number of RTP audio packets that were received out of order for all completed calls.

## Minimum MOS
Shows the minimum Mean Opinion Score (MOS) for all completed calls.

## Dropped Packets
Shows the total number of dropped RTP audio packets for all completed calls.

## <a name="MaximumDelay">Maximum Network Delay</a>
The Maximum Network Delay is an estimate of the network delay between the computer running this application and the called party's computer in milliseconds.

This application estimates the network delay by comparing the NTP timestamp reported by the remote party's computer via a Sender Report RTCP message with this computer's current timestamp. The network delay will not be calculated if the called party does not send RTCP Sender Report packets and it will be set to 0.

Ideally, the network should be very low, on the order of milliseconds if the clocks on both computers are perfectly synchronized. A large network delay may be caused by one or more of the following reasons.

1. Delay due to the physical network
1. Delay or latency caused by the operating system or the way in which an application prioritizes and handles media packets
1. The clocks between the two computers are not synchronized with a NTP time server

The network delay can have a large impact on perceived media quality, especially with audio. The perceived delay in audio starts to become quite noticeable when the network delay exceeds 170 milliseconds.

# MOS Range Counts
This table splits MOS values into six ranges and reports the number of calls that fall within each range.

# Individual Call Statistics
This table shows the call quality statistics for each completed call. The call duration must be at least 5 seconds in order for it to be included in this table.

The Start Time is the time that the INVITE request was sent to the remote end point.

The Response Time is time between the Start Time and the time that the first interim response was received by this application. The units are milliseconds.

The Ring Time is the time between the time that the remote endpoint answered the call with a 200 OK response and the time that a 180 Ringing or a 183 Session Progress response was received from the remote endpoint. The units are milliseconds. If the remote endpoint answered the call without sending a 180 or a 183 response then this duration will be equal to 0 milliseconds.

The Duration column shows the duration of the call in seconds. This is measured as the interval between the time that the remote endpoint answered the call and the time that the call ended.

The Min. MOS column shows the minimum Mean Opinion Score (MOS) calculated during the call.

The Avg. MOS column shows the average Mean Opinion Score (MOS) calculated during the call.

The Max. Jitter column shows the maximum smoothed RTP packet jitter in milliseconds that was calculated during the call.

The Dropped Packets column shows the total number of dropped RTP audio packets detected during the call.

The Network Delay column shows the maximum network delay in milliseconds that was detected during the call. See [Maximum Delay](#MaximumDelay) for a description of how the network delay is calculated.

