# The Call Details Page

This page shows the call quality details for a single call. Each row in the table contains the data for a 5 second interval during the call. Each row contains the following columns.

## Sample Time
Shows the time of day of the sample interval. This represents the time of the end of the 5 second sample interval. The first sample time will be 5 seconds after the start of the call.

## MOS
This is the Mean Opinion Score (MOS) calculated during the sample interval.

## Packets Expected
Shows the number of RTP audio packets expected during the sample interval.

Since the sample interval is 5 seconds, this is expected to be 250 packets corresponding to 20 milliseconds per packet. However, the sample interval is determined by a software system timer and there may be slight variations in the sample interval due to timer jitter. For this reason, it is possible for there to be a +/- 1 packet variation in the number of expected packets during the interval.

## Packets Received
This column shows the number of RTP audio packets that were actually received during the sample interval.

Ideally, this should be exactly equal to the number of expected packets. However, the sample interval is determined by a software system timer and the timer runs asynchronously with respect to packets being sent over the network by the system under test. For this reason, it is possible that there will be a slight (+/- 1 packet or slightly more) difference between the packets expected and the number of packets actually received during the sample interval.

## Packets Dropped
Shows the difference between the packets expected and the packets received during the sample interval.

A positive value indicates that packets may have been dropped during the sample interval. A negative value indicates that more packets than expected were received during the sample interval.

Occasional low values in the range of +/- 1 or even +/- 2 during the sample interval may be due to the slight jitter in the software timer used to determine the sample interval and are generally no too significant.

Values of +/- 3 or greater generally indicate that packets were actually dropped during the sample interval.

## Jitter (ms)
This column shows the peak-to-peak instantaneous RTP packet jitter in milliseconds that was calculated during the sample interval. This is the jitter value that is used to calculate the MOS value.

## Max. Smoothed Jitter (ms)
This column shows the maximum smoothed RTP packet jitter in milliseconds that was calculated during the sample interval.

