# HighloadCup
Highload cup F# solution

To launch the solution
1) Clone or download the repository https://github.com/sat2707/hlcupdocs
2) Create a zip file data.zip from /data/TRAIN/data from step 1, copy to src/ folder
3) Run dotnet restore and dotnet run

Test solutions:
1) https://github.com/AterCattus/highloadcup_tester - easiest way to test just speed
2) https://github.com/sat2707/hlcupdocs/blob/master/TANK.md - proper way to test with graphs

Latest results:

1st round 

![1st](http://clip2net.com/clip/m380071/f12db-clip-56kb.png)

2nd round 

![2nd](http://clip2net.com/clip/m380071/8a28d-clip-52kb.png)

3d round 

![3d](http://clip2net.com/clip/m380071/038e5-clip-58kb.png)

Logs:

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1246701000 Time=17:12:39.7432 ReqCount=8192

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1268589824 Time=17:13:08.7989 ReqCount=16384

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1290655024 Time=17:13:31.0935 ReqCount=24576

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1312801272 Time=17:13:49.8897 ReqCount=32768

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1334583704 Time=17:14:06.4469 ReqCount=40960

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1356620032 Time=17:14:21.4178 ReqCount=49152

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1378533544 Time=17:14:35.1847 ReqCount=57344

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1400570232 Time=17:14:47.9982 ReqCount=65536

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1422639424 Time=17:15:00.0333 ReqCount=73728

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1444479088 Time=17:15:11.4154 ReqCount=81920

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1466507256 Time=17:15:22.2425 ReqCount=90112

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1488379672 Time=17:15:32.5872 ReqCount=98304

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1510465456 Time=17:15:42.5079 ReqCount=106496

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1532493624 Time=17:15:52.0559 ReqCount=114688

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1554374336 Time=17:16:01.2673 ReqCount=122880

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1576345192 Time=17:16:10.1774 ReqCount=131072

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1598225928 Time=17:16:18.8125 ReqCount=139264

client_8233_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1620320008 Time=17:16:27.1967 ReqCount=147456

client_8233_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1306720152 Time=17:17:47.0402 ReqCount=155648

client_8233_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1425372720 Time=17:18:46.9644 ReqCount=163840

client_8233_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1544307064 Time=17:19:31.7800 ReqCount=172032

client_8233_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1663096048 Time=17:20:09.1916 ReqCount=180224

client_8233_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1782003488 Time=17:20:41.9785 ReqCount=188416

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1245151160 Time=17:21:21.1732 ReqCount=196608

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1266360160 Time=17:21:27.1728 ReqCount=204800

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1287822864 Time=17:21:31.7634 ReqCount=212992

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1309015336 Time=17:21:35.6285 ReqCount=221184

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1330314344 Time=17:21:39.0315 ReqCount=229376

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1351801752 Time=17:21:42.1072 ReqCount=237568

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1373133640 Time=17:21:44.9358 ReqCount=245760

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1394539208 Time=17:21:47.5663 ReqCount=253952

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1415862664 Time=17:21:50.0374 ReqCount=262144

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1437137120 Time=17:21:52.3743 ReqCount=270336

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1458534144 Time=17:21:54.5970 ReqCount=278528

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1479775776 Time=17:21:56.7203 ReqCount=286720

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1501009376 Time=17:21:58.7563 ReqCount=294912

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1522382112 Time=17:22:00.7160 ReqCount=303104

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1543697632 Time=17:22:02.6066 ReqCount=311296

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1565070264 Time=17:22:04.4349 ReqCount=319488

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1586443040 Time=17:22:06.2069 ReqCount=327680

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1607848592 Time=17:22:07.9274 ReqCount=335872

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1629221448 Time=17:22:09.6030 ReqCount=344064

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1650364864 Time=17:22:11.2304 ReqCount=352256

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1671803096 Time=17:22:12.8202 ReqCount=360448

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1693159384 Time=17:22:14.3723 ReqCount=368640

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1714401112 Time=17:22:15.8891 ReqCount=376832

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1735814768 Time=17:22:17.3740 ReqCount=385024

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1757187608 Time=17:22:18.8281 ReqCount=393216

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1778560072 Time=17:22:20.2532 ReqCount=401408

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1799867360 Time=17:22:21.6504 ReqCount=409600

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1821231184 Time=17:22:23.0226 ReqCount=417792

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1842587624 Time=17:22:24.3704 ReqCount=425984

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1863869520 Time=17:22:25.6955 ReqCount=434176

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1885094704 Time=17:22:26.9986 ReqCount=442368

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1906472456 Time=17:22:28.2802 ReqCount=450560

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1927787800 Time=17:22:29.5424 ReqCount=458752

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1949185184 Time=17:22:30.7853 ReqCount=466944

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1970426936 Time=17:22:32.0108 ReqCount=475136

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1991930656 Time=17:22:33.2182 ReqCount=483328

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2013295240 Time=17:22:34.4092 ReqCount=491520

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2034545104 Time=17:22:35.5840 ReqCount=499712

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2055889552 Time=17:22:36.7440 ReqCount=507904

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2077319720 Time=17:22:37.8891 ReqCount=516096

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2098399008 Time=17:22:39.0192 ReqCount=524288

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2119747248 Time=17:22:40.1356 ReqCount=532480

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2141256424 Time=17:22:41.2393 ReqCount=540672

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2162615344 Time=17:22:42.3303 ReqCount=548864

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2183931672 Time=17:22:43.4088 ReqCount=557056

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2205259472 Time=17:22:44.4756 ReqCount=565248

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2226612368 Time=17:22:45.5309 ReqCount=573440

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2247984848 Time=17:22:46.5749 ReqCount=581632

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2269254152 Time=17:22:47.6075 ReqCount=589824

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2290501336 Time=17:22:48.6298 ReqCount=598016

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2311818072 Time=17:22:49.6427 ReqCount=606208

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2333231416 Time=17:22:50.6448 ReqCount=614400

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2354516536 Time=17:22:51.6378 ReqCount=622592

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2375941864 Time=17:22:52.6220 ReqCount=630784

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2397284472 Time=17:22:53.5962 ReqCount=638976

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2418647520 Time=17:22:54.5615 ReqCount=647168

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2439916848 Time=17:22:55.5189 ReqCount=655360

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2461347896 Time=17:22:56.4674 ReqCount=663552

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2482491960 Time=17:22:57.4088 ReqCount=671744

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2503802656 Time=17:22:58.3408 ReqCount=679936

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2525336744 Time=17:22:59.2653 ReqCount=688128

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2546647264 Time=17:23:00.1828 ReqCount=696320

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2567995104 Time=17:23:01.0933 ReqCount=704512

client_8233_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2589374264 Time=17:23:01.9957 ReqCount=712704

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1243610008 Time=17:23:02.8921 ReqCount=720896

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1265031600 Time=17:23:03.7806 ReqCount=729088

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1286281336 Time=17:23:04.6632 ReqCount=737280

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1307522976 Time=17:23:05.5387 ReqCount=745472

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1328879144 Time=17:23:06.4077 ReqCount=753664

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1350227232 Time=17:23:07.2703 ReqCount=761856

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1371575320 Time=17:23:08.1277 ReqCount=770048

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1392923496 Time=17:23:08.9782 ReqCount=778240

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1414369784 Time=17:23:09.8234 ReqCount=786432

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1435717984 Time=17:23:10.6622 ReqCount=794624

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1456894152 Time=17:23:11.4953 ReqCount=802816

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1478357104 Time=17:23:12.3235 ReqCount=811008

client_8233_1 |Gen0=16 Gen1=13 Gen2=9 Alloc=1500073952 Time=17:23:13.1467 ReqCount=819200
