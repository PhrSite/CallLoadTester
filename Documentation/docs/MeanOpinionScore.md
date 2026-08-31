# Mean Opinion Score Calculation
This application uses a method of calculating the estimated Mean Opinion Score that is described in the article entitled "[emos - Estimated Opinion Score](https://arimas.com/it/insight-news/emos-estimated-mean-opinion-score/)".


```
Effective Latency = Average Latency + (Jitter x 2) + 10

if Effective Latency < 160
    R = 93.2 - (Effective Latency / 40)
else
    R = 93.2 - (Effective Latency - 120) / 10

R = R - (Packet Loss * 2.5)

if R < 0
    MOS = 1.0
else if R >= 0 and R <= 100
    MOS = 1 + 0.035 * R + R * (R - 60) * (100 - R) * 7e-6
else
    MOS = 4.5

```



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

| Average Latency (ms) | Jitter (ms) | Packet Loss (%) | MOS |
|----------------------|-------------|-------------------|---|
| 0 | 0 | 0 | 4.40 |
| 0 | 20 | 0 | 4.38 |
| 0 | 40 | 0 | 4.36 |
| 0 | 60 | 0 | 4.33 |
| 0 | 80 | 0 | 4.29 |
| 0 | 100 | 0 | 4.17 |
| 0 | 120 | 0 | 4.03 |
| 0 | 140 | 0 | 3.87 |
| 0 | 160 | 0 | 3.70 |
| 0 | 170 | 0 | 3.61 |
| 0 | 180 | 0 | 3.51 |
| 0 | 190 | 0 | 3.41 |
| 0 | 200 | 0 | 3.31 |

| Average Latency (ms) | Jitter (ms) | Packet Loss (%) | MOS |
|----------------------|-------------|-------------------|---|
| 0 | 0 | 0 | 4.40 |
| 0 | 0 | 1 | 4.35 |
| 0 | 0 | 5 | 4.09 |
| 0 | 0 | 10 | 4.04 |
