```

BenchmarkDotNet v0.15.8, macOS 27.0 (26A428) [Darwin 27.0.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT armv8.0-a
  Suite  : .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT armv8.0-a

Job=Suite  IterationCount=15  IterationTime=500ms  
LaunchCount=1  WarmupCount=6  

```
| Method  | Case                                            | Mean            | Error           | StdDev          | Gen0      | Gen1      | Gen2      | Allocated  |
|-------- |------------------------------------------------ |----------------:|----------------:|----------------:|----------:|----------:|----------:|-----------:|
| **Execute** | **basic/large/Binary/load**                         |    **767,966.7 ns** |     **5,548.49 ns** |     **4,918.59 ns** |  **234.7561** |  **112.8049** |         **-** |  **1478648 B** |
| **Execute** | **basic/large/Binary/roundtrip**                    |  **2,838,099.2 ns** |    **47,292.71 ns** |    **41,923.76 ns** |  **604.1667** |  **312.5000** |   **20.8333** |  **3974606 B** |
| **Execute** | **basic/large/Binary/save**                         |    **736,870.2 ns** |     **3,176.63 ns** |     **2,652.63 ns** |  **158.4302** |   **40.6977** |   **39.2442** |  **1095378 B** |
| **Execute** | **basic/large/deserialize**                         |    **550,296.0 ns** |     **2,639.18 ns** |     **2,339.56 ns** |   **58.1140** |   **12.0614** |         **-** |   **368024 B** |
| **Execute** | **basic/large/serialize**                           |    **346,803.7 ns** |     **4,842.82 ns** |     **4,293.03 ns** |  **156.9293** |   **57.7446** |         **-** |   **984656 B** |
| **Execute** | **basic/large/to-string**                           |    **558,462.2 ns** |    **14,173.57 ns** |    **11,835.57 ns** |  **291.2946** |  **219.8661** |  **158.4821** |  **1495684 B** |
| **Execute** | **basic/large/to-string-fragment**                  |    **563,434.4 ns** |    **11,542.96 ns** |    **10,797.29 ns** |  **280.1339** |  **212.0536** |  **147.3214** |  **1490829 B** |
| **Execute** | **basic/large/Xml/load**                            |  **1,380,482.5 ns** |    **14,267.46 ns** |    **12,647.73 ns** |  **214.6739** |  **105.9783** |         **-** |  **1356704 B** |
| **Execute** | **basic/large/Xml/roundtrip**                       |  **3,367,337.6 ns** |    **73,032.30 ns** |    **64,741.23 ns** |  **562.5000** |  **270.8333** |  **270.8333** |  **3959094 B** |
| **Execute** | **basic/large/Xml/save**                            |    **781,396.4 ns** |    **20,786.87 ns** |    **19,444.05 ns** |  **285.1563** |  **285.1563** |  **285.1563** |  **1225808 B** |
| **Execute** | **basic/small/Binary/load**                         |        **808.0 ns** |         **8.63 ns** |         **7.65 ns** |    **0.3751** |    **0.0016** |         **-** |     **2360 B** |
| **Execute** | **basic/small/Binary/roundtrip**                    |      **3,113.7 ns** |        **52.40 ns** |        **49.02 ns** |    **1.0781** |    **0.0062** |         **-** |     **6776 B** |
| **Execute** | **basic/small/Binary/save**                         |      **1,146.2 ns** |        **52.91 ns** |        **49.50 ns** |    **0.4686** |    **0.0022** |         **-** |     **2952 B** |
| **Execute** | **basic/small/deserialize**                         |        **615.5 ns** |        **15.26 ns** |        **13.52 ns** |    **0.0619** |         **-** |         **-** |      **392 B** |
| **Execute** | **basic/small/serialize**                           |        **338.9 ns** |         **3.43 ns** |         **2.86 ns** |    **0.1772** |         **-** |         **-** |     **1112 B** |
| **Execute** | **basic/small/to-string**                           |        **633.1 ns** |        **26.58 ns** |        **24.86 ns** |    **0.4620** |    **0.0025** |         **-** |     **2904 B** |
| **Execute** | **basic/small/to-string-fragment**                  |        **596.8 ns** |         **7.13 ns** |         **6.32 ns** |    **0.3832** |    **0.0012** |         **-** |     **2408 B** |
| **Execute** | **basic/small/Xml/load**                            |      **2,393.3 ns** |        **25.57 ns** |        **22.67 ns** |    **1.0718** |    **0.0193** |         **-** |     **6728 B** |
| **Execute** | **basic/small/Xml/roundtrip**                       |      **4,314.8 ns** |        **43.06 ns** |        **40.28 ns** |    **2.0338** |    **0.0342** |         **-** |    **12792 B** |
| **Execute** | **basic/small/Xml/save**                            |        **895.6 ns** |        **15.83 ns** |        **14.80 ns** |    **0.7354** |    **0.0108** |         **-** |     **4624 B** |
| **Execute** | **binary-only-nodes/Binary/roundtrip**              |        **454.0 ns** |        **14.24 ns** |        **11.89 ns** |    **0.2241** |         **-** |         **-** |     **1408 B** |
| **Execute** | **collections/large/Binary/roundtrip**              | **37,477,949.3 ns** | **3,010,123.12 ns** | **2,815,671.03 ns** | **4500.0000** | **1750.0000** |  **500.0000** | **26655544 B** |
| **Execute** | **collections/large/Xml/roundtrip**                 | **33,929,163.4 ns** | **1,823,040.80 ns** | **1,705,273.50 ns** | **3833.3333** | **2000.0000** |  **666.6667** | **24800285 B** |
| **Execute** | **collections/small/Binary/roundtrip**              |     **19,516.7 ns** |       **460.61 ns** |       **408.32 ns** |    **5.0791** |    **0.1163** |         **-** |    **31998 B** |
| **Execute** | **collections/small/Xml/roundtrip**                 |     **22,678.0 ns** |       **572.36 ns** |       **535.39 ns** |    **5.8263** |    **0.1766** |         **-** |    **37161 B** |
| **Execute** | **expando/Binary/roundtrip**                        |        **918.2 ns** |         **9.61 ns** |         **8.52 ns** |    **0.4867** |         **-** |         **-** |     **3064 B** |
| **Execute** | **expando/Xml/roundtrip**                           |      **1,918.6 ns** |        **21.17 ns** |        **19.80 ns** |    **1.0601** |    **0.0077** |         **-** |     **6664 B** |
| **Execute** | **imported-conversions/Binary/load-deserialize**    |      **3,819.9 ns** |        **37.88 ns** |        **35.43 ns** |    **0.8465** |    **0.0075** |         **-** |     **5337 B** |
| **Execute** | **imported-conversions/Xml/load-deserialize**       |      **7,143.7 ns** |       **114.36 ns** |        **95.49 ns** |    **1.9152** |    **0.0439** |         **-** |    **12098 B** |
| **Execute** | **integer-widths/Binary/roundtrip**                 |      **1,746.9 ns** |        **32.15 ns** |        **28.50 ns** |    **0.7081** |    **0.0035** |         **-** |     **4448 B** |
| **Execute** | **integer-widths/Xml/roundtrip**                    |      **3,409.3 ns** |       **128.12 ns** |       **119.84 ns** |    **1.6799** |    **0.0202** |         **-** |    **10576 B** |
| **Execute** | **large-data/1MiB/Binary/roundtrip**                |    **262,148.9 ns** |     **6,587.67 ns** |     **6,162.12 ns** |  **395.8333** |  **394.7368** |  **394.7368** |  **4198319 B** |
| **Execute** | **large-data/1MiB/Xml/roundtrip**                   |  **3,534,977.5 ns** |    **65,264.96 ns** |    **61,048.88 ns** | **1430.5556** | **1215.2778** |  **958.3333** | **16481990 B** |
| **Execute** | **legacy-dictionary/Binary/roundtrip**              |      **1,206.3 ns** |        **15.68 ns** |        **14.66 ns** |    **0.5453** |         **-** |         **-** |     **3432 B** |
| **Execute** | **legacy-dictionary/Xml/roundtrip**                 |      **2,271.7 ns** |        **29.18 ns** |        **25.87 ns** |    **1.1294** |    **0.0045** |         **-** |     **7104 B** |
| **Execute** | **legacy-list/Binary/roundtrip**                    |        **891.4 ns** |        **14.49 ns** |        **13.55 ns** |    **0.4193** |         **-** |         **-** |     **2632 B** |
| **Execute** | **legacy-list/Xml/roundtrip**                       |      **1,953.4 ns** |        **24.91 ns** |        **22.08 ns** |    **1.0027** |    **0.0079** |         **-** |     **6304 B** |
| **Execute** | **members/large/Binary/roundtrip**                  |  **3,976,678.1 ns** |   **235,834.56 ns** |   **220,599.79 ns** |  **812.5000** |  **406.2500** |         **-** |  **5283416 B** |
| **Execute** | **members/large/Xml/roundtrip**                     |  **5,271,772.5 ns** |   **208,300.12 ns** |   **173,940.03 ns** |  **843.7500** |  **343.7500** |  **281.2500** |  **5209421 B** |
| **Execute** | **members/small/Binary/roundtrip**                  |      **4,067.3 ns** |        **36.49 ns** |        **32.35 ns** |    **1.3433** |    **0.0081** |         **-** |     **8464 B** |
| **Execute** | **members/small/Xml/roundtrip**                     |      **5,394.5 ns** |        **27.45 ns** |        **22.92 ns** |    **2.2936** |    **0.0441** |         **-** |    **14416 B** |
| **Execute** | **nested-shared/large/Binary/roundtrip**            | **15,288,904.3 ns** |   **300,833.63 ns** |   **281,399.97 ns** | **2444.4444** | **1111.1111** |  **111.1111** | **15563276 B** |
| **Execute** | **nested-shared/large/Xml/roundtrip**               | **14,059,354.8 ns** |    **72,411.57 ns** |    **60,466.94 ns** | **2300.0000** | **1400.0000** |  **500.0000** | **13604462 B** |
| **Execute** | **nested-shared/small/Binary/roundtrip**            |      **8,221.8 ns** |        **38.87 ns** |        **34.46 ns** |    **2.6837** |    **0.0329** |         **-** |    **16881 B** |
| **Execute** | **nested-shared/small/Xml/roundtrip**               |     **11,494.5 ns** |       **164.66 ns** |       **145.96 ns** |    **3.9587** |    **0.1200** |         **-** |    **24962 B** |
| **Execute** | **nesting/128/Binary/roundtrip**                    |     **56,559.2 ns** |       **778.23 ns** |       **727.96 ns** |   **20.8701** |    **2.6502** |         **-** |   **131048 B** |
| **Execute** | **nesting/128/Xml/roundtrip**                       |     **91,480.9 ns** |     **2,279.80 ns** |     **2,132.53 ns** |   **30.1662** |    **5.9610** |         **-** |   **189688 B** |
| **Execute** | **nesting/16/Binary/roundtrip**                     |      **6,097.2 ns** |        **22.48 ns** |        **19.93 ns** |    **2.6376** |    **0.0486** |         **-** |    **16600 B** |
| **Execute** | **nesting/16/Xml/roundtrip**                        |      **8,560.6 ns** |       **116.98 ns** |       **109.42 ns** |    **3.6409** |    **0.1208** |         **-** |    **22864 B** |
| **Execute** | **nesting/512/Binary/roundtrip**                    |    **265,958.6 ns** |     **1,170.54 ns** |     **1,094.93 ns** |   **83.6864** |   **25.9534** |         **-** |   **528424 B** |
| **Execute** | **nesting/512/Xml/roundtrip**                       |    **889,209.7 ns** |    **24,707.96 ns** |    **23,111.84 ns** |  **281.2500** |  **281.2500** |  **281.2500** |  **1540264 B** |
| **Execute** | **nodes/large/Binary/roundtrip**                    |  **5,001,668.8 ns** |   **104,015.63 ns** |    **97,296.28 ns** | **1093.7500** |  **562.5000** |   **31.2500** |  **7032772 B** |
| **Execute** | **nodes/large/Xml/roundtrip**                       |  **7,332,162.0 ns** |   **136,808.96 ns** |   **127,971.19 ns** | **1218.7500** |  **718.7500** |  **468.7500** |  **7739412 B** |
| **Execute** | **nodes/small/Binary/roundtrip**                    |      **4,662.5 ns** |        **67.57 ns** |        **63.21 ns** |    **1.7486** |    **0.0187** |         **-** |    **11016 B** |
| **Execute** | **nodes/small/Xml/roundtrip**                       |      **7,316.4 ns** |       **971.78 ns** |       **811.48 ns** |    **2.6510** |    **0.0536** |         **-** |    **16672 B** |
| **Execute** | **nullable/large/Binary/roundtrip**                 | **14,013,591.1 ns** | **1,447,815.65 ns** | **1,354,287.66 ns** | **2000.0000** | **1000.0000** |  **100.0000** | **12875992 B** |
| **Execute** | **nullable/large/Xml/roundtrip**                    | **16,483,409.3 ns** |   **274,963.77 ns** |   **257,201.28 ns** | **2111.1111** | **1185.1852** |  **481.4815** | **12640625 B** |
| **Execute** | **nullable/small/Binary/roundtrip**                 |     **11,541.8 ns** |       **104.35 ns** |        **92.50 ns** |    **2.9406** |    **0.0467** |         **-** |    **18545 B** |
| **Execute** | **nullable/small/Xml/roundtrip**                    |     **14,485.9 ns** |       **127.82 ns** |       **106.74 ns** |    **3.8740** |         **-** |         **-** |    **24596 B** |
| **Execute** | **pair-enumerable/Binary/roundtrip**                |      **1,168.7 ns** |        **14.11 ns** |        **13.20 ns** |    **0.5493** |         **-** |         **-** |     **3456 B** |
| **Execute** | **pair-enumerable/Xml/roundtrip**                   |      **2,220.3 ns** |        **24.33 ns** |        **22.76 ns** |    **1.1252** |    **0.0133** |         **-** |     **7064 B** |
| **Execute** | **payloads/large/Binary/roundtrip**                 | **14,146,845.9 ns** |   **166,981.29 ns** |   **139,436.94 ns** | **2307.6923** | **1076.9231** |  **230.7692** | **14203973 B** |
| **Execute** | **payloads/large/Xml/roundtrip**                    | **15,876,272.3 ns** |   **237,667.70 ns** |   **210,686.23 ns** | **2700.0000** | **1800.0000** | **1000.0000** | **20475496 B** |
| **Execute** | **payloads/small/Binary/roundtrip**                 |     **17,939.1 ns** |       **228.11 ns** |       **202.22 ns** |    **4.3729** |    **0.1067** |         **-** |    **27552 B** |
| **Execute** | **payloads/small/Xml/roundtrip**                    |     **13,440.2 ns** |       **170.43 ns** |       **151.08 ns** |    **7.7160** |    **0.7716** |         **-** |    **49040 B** |
| **Execute** | **real-special-values/Binary/roundtrip**            |      **1,349.0 ns** |         **5.40 ns** |         **4.79 ns** |    **0.5743** |         **-** |         **-** |     **3608 B** |
| **Execute** | **real-special-values/Xml/roundtrip**               |      **3,699.9 ns** |        **81.94 ns** |        **72.64 ns** |    **1.5097** |    **0.0145** |         **-** |     **9512 B** |
| **Execute** | **reference-widths/127/Binary/roundtrip**           |     **44,689.6 ns** |       **271.22 ns** |       **240.43 ns** |   **15.3805** |    **1.0669** |         **-** |    **96595 B** |
| **Execute** | **reference-widths/127/Xml/roundtrip**              |     **54,973.5 ns** |     **1,037.30 ns** |       **919.54 ns** |   **15.2057** |    **1.2299** |         **-** |    **95627 B** |
| **Execute** | **reference-widths/128/Binary/roundtrip**           |     **47,020.2 ns** |       **634.61 ns** |       **593.62 ns** |   **16.4918** |    **1.3118** |         **-** |   **103596 B** |
| **Execute** | **reference-widths/128/Xml/roundtrip**              |     **55,548.1 ns** |     **1,111.18 ns** |     **1,039.40 ns** |   **15.1879** |    **1.3112** |         **-** |    **95867 B** |
| **Execute** | **reference-widths/14/Binary/roundtrip**            |      **5,450.4 ns** |        **79.25 ns** |        **74.13 ns** |    **1.7667** |    **0.0109** |         **-** |    **11112 B** |
| **Execute** | **reference-widths/14/Xml/roundtrip**               |      **7,470.7 ns** |        **92.41 ns** |        **81.92 ns** |    **2.6168** |    **0.0456** |         **-** |    **16481 B** |
| **Execute** | **reference-widths/15/Binary/roundtrip**            |      **5,814.4 ns** |        **86.33 ns** |        **80.76 ns** |    **1.8198** |    **0.0118** |         **-** |    **11472 B** |
| **Execute** | **reference-widths/15/Xml/roundtrip**               |      **7,865.1 ns** |        **79.15 ns** |        **70.16 ns** |    **2.6799** |    **0.0313** |         **-** |    **16841 B** |
| **Execute** | **reference-widths/255/Binary/roundtrip**           |     **96,415.6 ns** |     **2,178.40 ns** |     **2,037.68 ns** |   **33.6085** |    **4.3239** |         **-** |   **211328 B** |
| **Execute** | **reference-widths/255/Xml/roundtrip**              |    **106,686.9 ns** |     **2,859.78 ns** |     **2,675.04 ns** |   **27.7778** |    **3.4722** |         **-** |   **179174 B** |
| **Execute** | **reference-widths/256/Binary/roundtrip**           |     **92,922.3 ns** |       **689.73 ns** |       **645.17 ns** |   **34.2440** |    **5.2395** |         **-** |   **215824 B** |
| **Execute** | **reference-widths/256/Xml/roundtrip**              |    **108,266.8 ns** |     **2,892.67 ns** |     **2,705.81 ns** |   **28.4091** |    **3.4091** |         **-** |   **179415 B** |
| **Execute** | **reference-widths/32768/Binary/roundtrip**         | **23,943,010.1 ns** |   **655,440.91 ns** |   **581,031.30 ns** | **2800.0000** | **1600.0000** | **1100.0000** | **23043203 B** |
| **Execute** | **reference-widths/32768/Xml/roundtrip**            | **22,825,474.7 ns** |   **619,382.11 ns** |   **549,066.11 ns** | **3000.0000** | **1900.0000** | **1500.0000** | **19014358 B** |
| **Execute** | **rejections/Binary-nesting-limit**                 |    **436,636.3 ns** |    **19,557.42 ns** |    **17,337.14 ns** |   **33.3904** |    **4.2808** |         **-** |   **213784 B** |
| **Execute** | **rejections/collection-cycle**                     |    **307,655.3 ns** |     **5,174.25 ns** |     **4,586.83 ns** |   **28.1250** |    **0.6250** |         **-** |   **181120 B** |
| **Execute** | **rejections/deserialize-conflicting-keys**         |      **5,772.9 ns** |        **39.69 ns** |        **35.18 ns** |    **0.4274** |         **-** |         **-** |     **2712 B** |
| **Execute** | **rejections/integer-overflow**                     |      **3,538.4 ns** |        **32.88 ns** |        **30.75 ns** |    **0.0853** |         **-** |         **-** |      **544 B** |
| **Execute** | **rejections/invalid-enum**                         |      **3,692.2 ns** |        **40.97 ns** |        **38.32 ns** |    **0.1023** |         **-** |         **-** |      **680 B** |
| **Execute** | **rejections/mismatched-value-type**                |     **15,067.3 ns** |       **458.34 ns** |       **406.31 ns** |    **0.0311** |         **-** |         **-** |      **272 B** |
| **Execute** | **rejections/non-string-dictionary-key**            |     **28,356.1 ns** |       **326.38 ns** |       **305.29 ns** |    **0.3381** |         **-** |         **-** |     **2304 B** |
| **Execute** | **rejections/null-root**                            |     **15,493.1 ns** |       **382.98 ns** |       **358.24 ns** |    **0.0615** |         **-** |         **-** |      **488 B** |
| **Execute** | **rejections/object-cycle**                         |    **343,404.7 ns** |    **48,960.41 ns** |    **43,402.13 ns** |   **28.8915** |         **-** |         **-** |   **185200 B** |
| **Execute** | **rejections/serialize-conflicting-keys**           |      **6,318.2 ns** |       **255.25 ns** |       **238.76 ns** |    **0.4679** |         **-** |         **-** |     **3008 B** |
| **Execute** | **rejections/xml-fill**                             |      **3,658.8 ns** |        **58.56 ns** |        **54.78 ns** |    **0.1428** |         **-** |         **-** |      **912 B** |
| **Execute** | **rejections/Xml-nesting-limit**                    |    **268,943.1 ns** |     **5,336.48 ns** |     **4,730.65 ns** |   **35.2564** |    **9.6154** |         **-** |   **223096 B** |
| **Execute** | **rejections/xml-null**                             |      **3,531.0 ns** |        **42.10 ns** |        **37.32 ns** |    **0.1394** |         **-** |         **-** |      **912 B** |
| **Execute** | **resolvers/large/Binary/roundtrip**                | **19,518,992.2 ns** |   **346,861.86 ns** |   **307,484.01 ns** | **3000.0000** | **1375.0000** |  **250.0000** | **18184204 B** |
| **Execute** | **resolvers/large/Xml/roundtrip**                   | **16,544,093.7 ns** |   **282,975.87 ns** |   **236,297.67 ns** | **2555.5556** | **1333.3333** |  **555.5556** | **16660025 B** |
| **Execute** | **resolvers/small/Binary/roundtrip**                |     **10,393.4 ns** |       **178.07 ns** |       **139.03 ns** |    **3.1804** |    **0.0616** |         **-** |    **20017 B** |
| **Execute** | **resolvers/small/Xml/roundtrip**                   |     **13,364.8 ns** |       **311.43 ns** |       **291.31 ns** |    **4.2476** |    **0.1517** |         **-** |    **27106 B** |
| **Execute** | **root-integer/Binary/roundtrip**                   |        **282.5 ns** |         **3.55 ns** |         **3.32 ns** |    **0.2418** |    **0.0006** |         **-** |     **1520 B** |
| **Execute** | **root-integer/Xml/roundtrip**                      |      **1,226.7 ns** |        **18.03 ns** |        **16.86 ns** |    **0.8334** |    **0.0049** |         **-** |     **5240 B** |
| **Execute** | **root-string/Binary/roundtrip**                    |        **287.7 ns** |         **1.22 ns** |         **1.08 ns** |    **0.2354** |         **-** |         **-** |     **1480 B** |
| **Execute** | **root-string/Xml/roundtrip**                       |      **1,252.1 ns** |        **19.70 ns** |        **17.46 ns** |    **0.8293** |    **0.0074** |         **-** |     **5216 B** |
| **Execute** | **scalars/large/Binary/roundtrip**                  | **12,860,773.4 ns** |   **111,759.32 ns** |    **93,324.10 ns** | **2100.0000** | **1000.0000** |  **200.0000** | **13976141 B** |
| **Execute** | **scalars/large/Xml/roundtrip**                     | **16,968,740.6 ns** |   **294,057.47 ns** |   **260,674.28 ns** | **2583.3333** | **1750.0000** | **1000.0000** | **14808100 B** |
| **Execute** | **scalars/small/Binary/roundtrip**                  |     **11,320.7 ns** |       **189.87 ns** |       **177.60 ns** |    **3.1772** |    **0.0444** |         **-** |    **20042 B** |
| **Execute** | **scalars/small/Xml/roundtrip**                     |     **13,882.6 ns** |        **88.87 ns** |        **74.21 ns** |    **4.0533** |         **-** |         **-** |    **25562 B** |
| **Execute** | **uid-widths/Binary/roundtrip**                     |      **1,011.6 ns** |        **13.03 ns** |        **12.19 ns** |    **0.4028** |         **-** |         **-** |     **2536 B** |
| **Execute** | **uid-widths/Xml/roundtrip**                        |      **4,850.7 ns** |        **52.89 ns** |        **49.47 ns** |    **2.1836** |    **0.0096** |         **-** |    **13728 B** |
| **Execute** | **unsupported-collections/Binary/load-deserialize** |        **717.7 ns** |         **2.41 ns** |         **2.01 ns** |    **0.3055** |         **-** |         **-** |     **1920 B** |
| **Execute** | **unsupported-collections/Xml/load-deserialize**    |      **1,876.6 ns** |        **20.94 ns** |        **19.59 ns** |    **0.8768** |    **0.0112** |         **-** |     **5512 B** |
| **Execute** | **untyped-null/Binary/load-deserialize**            |        **109.5 ns** |         **1.85 ns** |         **1.55 ns** |    **0.0918** |         **-** |         **-** |      **576 B** |
| **Execute** | **untyped/large/Binary/roundtrip**                  |  **6,611,798.3 ns** |    **89,243.83 ns** |    **83,478.74 ns** | **1156.2500** |  **562.5000** |   **62.5000** |  **7584770 B** |
| **Execute** | **untyped/large/Xml/roundtrip**                     |  **6,995,237.5 ns** |    **71,266.28 ns** |    **63,175.70 ns** | **1000.0000** |  **343.7500** |  **281.2500** |  **6927258 B** |
| **Execute** | **untyped/small/Binary/roundtrip**                  |      **5,070.0 ns** |        **40.90 ns** |        **38.25 ns** |    **1.8033** |    **0.0201** |         **-** |    **11353 B** |
| **Execute** | **untyped/small/Xml/roundtrip**                     |      **6,726.5 ns** |        **29.47 ns** |        **26.12 ns** |    **2.6227** |    **0.0538** |         **-** |    **16489 B** |
