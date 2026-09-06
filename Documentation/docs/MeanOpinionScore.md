# Mean Opinion Score Calculation
This application uses a method of calculating the estimated Mean Opinion Score that is described in the article entitled "[emos - Estimated Mean Opinion Score](https://arimas.com/it/insight-news/emos-estimated-mean-opinion-score/)".

The calculation of the MOS uses the following inputs.
1. Jitter in milliseconds
1. Packet Loss in percent
1. Average Latency in milliseconds

The following code snippet shows the pseudo code for the algorithm used to calculate the MOS.

```
Effective Latency = Average Latency + (Jitter x 2) + 10

if Effective Latency < 160
    R = 93.2 - (Effective Latency / 40)
else
    R = 93.2 - (Effective Latency - 120) / 10

Packet Loss Percent = Abs((1.0 - Received Packets / Expected Packets) * 100)

R = R - (Packet Loss Percent * 2.5)

if R < 0
    MOS = 1.0
else if R >= 0 and R <= 100
    MOS = 1 + 0.035 * R + R * (R - 60) * (100 - R) * 7e-6
else
    MOS = 4.5

```

This application uses the peak-to-peak jitter within each sample interval.

The Average Latency term represents the network latency between this application and the remote endpoint under test. It is calculated by comparing the NTP timestamps reported by the remote endpoint point in RTCP Sender Reports that it sends to this application with the NTP timestamp calculated by this application.
This application ingores the Average Latency (i.e. it is set to 0) in the calculation of the MOS because this number is often not accurate because the remote endpoint's clock is not syncronized with the network's NTP server.

Note: The equation for the R factor shown in step 4 of the EMOS article is incorrect.
The R factor shown in the above code snippet is calculated using Equation B-4 of Annex B of the ITU-T G.107 standard.

The following table shows the effect of of jitter on the MOS.

| Average Latency (ms) | Jitter (ms) | Packet Loss (%) | MOS |
|----------------------|-------------|-------------------|---|
| 0 | 0 | 0 | 4.40 |
| 0 | 40 | 0 | 4.36 |
| 0 | 80 | 0 | 4.29 |
| 0 | 120 | 0 | 4.03 |
| 0 | 160 | 0 | 3.70 |
| 0 | 200 | 0 | 3.31 |

The following table shows the effect of packet loss on the MOS.

| Average Latency (ms) | Jitter (ms) | Packet Loss (%) | MOS |
|----------------------|-------------|-------------------|---|
| 0 | 0 | 0 | 4.40 |
| 0 | 0 | 1 | 4.35 |
| 0 | 0 | 5 | 4.04 |
| 0 | 0 | 10 | 3.50 |
| 0 | 0 | 15 | 2.86 |
| 0 | 0 | 20 | 2.21 |

The following table shows the effect of Average Latency on the MOS. This software does not use the Average Latency for calculating the MOS. This table is provided for reference only.

| Average Latency (ms) | Jitter (ms) | Packet Loss (%) | MOS |
|----------------------|-------------|-------------------|---|
| 0  | 0 | 0 | 4.40 |
| 100 | 0 | 0 | 4.35 |
| 200 | 0 | 0 | 4.12 |
| 300 | 0 | 0 | 3.79 |
| 400 | 0 | 0 | 3.36 |
| 500 | 0 | 0 | 2.79 |
| 600 | 0 | 0 | 2.27 |
| 700 | 0 | 0 | 1.79 |
| 800 | 0 | 0 | 1.39 |
| 900 | 0 | 0 | 1.11 |
| 1000 | 0 | 0 | 0.99 |
