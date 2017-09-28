# HighloadCup
Highload cup F# solution

To launch the solution
1) Clone or download the repository https://github.com/sat2707/hlcupdocs
2) Create a zip file data.zip from /data/TRAIN/data from step 1, copy to src/ folder (/data/FULL/ is similar to real data used in rating run)
3) Run dotnet restore and dotnet run

Test solutions:
1) https://github.com/AterCattus/highloadcup_tester - easiest way to test just speed
2) https://github.com/sat2707/hlcupdocs/blob/master/TANK.md - proper way to test with graphs (see https://github.com/sat2707/hlcupdocs/issues/59 to run on windows)

Latest results:

1st round 

![1st](http://clip2net.com/clip/m380071/88d2c-clip-105kb.png)

2nd round 

![2nd](http://clip2net.com/clip/m380071/7c400-clip-49kb.png)

3d round 

![3d](http://clip2net.com/clip/m380071/97bb6-clip-55kb.png)

Logs:


client_8320_1 |Locations 760409 Users 1000072 Visits: 10000720

client_8320_1 |Hosting environment: Production

client_8320_1 |Content root path: /app/

client_8320_1 |Now listening on: http://[::]:80

client_8320_1 |Application started. Press Ctrl+C to shut down.

client_8320_1 |{"id":8,"birth_date":-1257206400,"first_name":"Алина","last_name":"Колыканая","gender":"f","email":"awdufutfyubwani@mail.ru"}Gen0=12 Gen1=10 Gen2=9 Alloc=1211845096 Time=10:39:39.4986 ReqCount=8192

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1226746128 Time=10:40:08.5601 ReqCount=16384

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1241696360 Time=10:40:30.8575 ReqCount=24576

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1256695760 Time=10:40:49.6550 ReqCount=32768

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1271498648 Time=10:41:06.2145 ReqCount=40960

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1286522632 Time=10:41:21.1853 ReqCount=49152

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1301489248 Time=10:41:34.9529 ReqCount=57344

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1316406816 Time=10:41:47.7670 ReqCount=65536

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1331414448 Time=10:41:59.8026 ReqCount=73728

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1346250072 Time=10:42:11.1860 ReqCount=81920

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1361241360 Time=10:42:22.0133 ReqCount=90112

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1376175264 Time=10:42:32.3562 ReqCount=98304

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1391101048 Time=10:42:42.2783 ReqCount=106496

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1406100480 Time=10:42:51.8266 ReqCount=114688

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1420968752 Time=10:43:01.0388 ReqCount=122880

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1435951880 Time=10:43:09.9483 ReqCount=131072

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1450861176 Time=10:43:18.5828 ReqCount=139264

client_8320_1 |Gen0=12 Gen1=10 Gen2=9 Alloc=1465795104 Time=10:43:26.9679 ReqCount=147456

client_8320_1 |Running GC 150155 150155

client_8320_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1277975664 Time=10:44:46.5402 ReqCount=155648

client_8320_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1395440112 Time=10:45:46.4792 ReqCount=163840

client_8320_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1513194368 Time=10:46:31.3013 ReqCount=172032

client_8320_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1630840344 Time=10:47:08.7161 ReqCount=180224

client_8320_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1748556024 Time=10:47:41.5048 ReqCount=188416

client_8320_1 |Running GC 190155 190155

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1211981744 Time=10:48:19.4665 ReqCount=196608

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1226792848 Time=10:48:25.4671 ReqCount=204800

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1241800360 Time=10:48:30.0584 ReqCount=212992

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1256570416 Time=10:48:33.9239 ReqCount=221184

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1271422320 Time=10:48:37.3274 ReqCount=229376

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1286397136 Time=10:48:40.4035 ReqCount=237568

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1301257280 Time=10:48:43.2304 ReqCount=245760

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1316223896 Time=10:48:45.8623 ReqCount=253952

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1331174136 Time=10:48:48.3333 ReqCount=262144

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1346050696 Time=10:48:50.6700 ReqCount=270336

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1361017296 Time=10:48:52.8924 ReqCount=278528

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1375959376 Time=10:48:55.0165 ReqCount=286720

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1390893296 Time=10:48:57.0527 ReqCount=294912

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1405786240 Time=10:48:59.0120 ReqCount=303104

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1420712000 Time=10:49:00.9021 ReqCount=311296

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1435686784 Time=10:49:02.7308 ReqCount=319488

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1450538760 Time=10:49:04.5031 ReqCount=327680

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1465480792 Time=10:49:06.2236 ReqCount=335872

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1480455632 Time=10:49:07.8968 ReqCount=344064

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1495242088 Time=10:49:09.5266 ReqCount=352256

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1510241496 Time=10:49:11.1161 ReqCount=360448

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1525085192 Time=10:49:12.6687 ReqCount=368640

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1541117304 Time=10:49:14.1852 ReqCount=376832

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1556042984 Time=10:49:15.6701 ReqCount=385024

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1570935976 Time=10:49:17.1235 ReqCount=393216

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1585861568 Time=10:49:18.5491 ReqCount=401408

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1600754592 Time=10:49:19.9471 ReqCount=409600

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1615680240 Time=10:49:21.3190 ReqCount=417792

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1630679768 Time=10:49:22.6668 ReqCount=425984

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1645597152 Time=10:49:23.9912 ReqCount=434176

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1660498256 Time=10:49:25.2944 ReqCount=442368

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1675448560 Time=10:49:26.5763 ReqCount=450560

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1690382424 Time=10:49:27.8383 ReqCount=458752

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1705349104 Time=10:49:29.0816 ReqCount=466944

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1720143728 Time=10:49:30.3072 ReqCount=475136

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1735102184 Time=10:49:31.5141 ReqCount=483328

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1750068824 Time=10:49:32.7057 ReqCount=491520

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1765084712 Time=10:49:33.8806 ReqCount=499712

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1780035024 Time=10:49:35.0398 ReqCount=507904

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1794985304 Time=10:49:36.1848 ReqCount=516096

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1809706160 Time=10:49:37.3156 ReqCount=524288

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1824607312 Time=10:49:38.4329 ReqCount=532480

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1839582160 Time=10:49:39.5365 ReqCount=540672

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1854499584 Time=10:49:40.6271 ReqCount=548864

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1869433448 Time=10:49:41.7056 ReqCount=557056

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1884318200 Time=10:49:42.7719 ReqCount=565248

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1899293072 Time=10:49:43.8270 ReqCount=573440

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1914218904 Time=10:49:44.8710 ReqCount=581632

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1929193680 Time=10:49:45.9037 ReqCount=589824

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1944098480 Time=10:49:46.9265 ReqCount=598016

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1959007744 Time=10:49:47.9388 ReqCount=606208

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1973964488 Time=10:49:48.9410 ReqCount=614400

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1988832824 Time=10:49:49.9343 ReqCount=622592

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2003733928 Time=10:49:50.9176 ReqCount=630784

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2018692368 Time=10:49:51.8924 ReqCount=638976

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2035317440 Time=10:49:52.8581 ReqCount=647168

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2050193928 Time=10:49:53.8160 ReqCount=655360

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2065175624 Time=10:49:54.7640 ReqCount=663552

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2079921088 Time=10:49:55.7050 ReqCount=671744

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2094805840 Time=10:49:56.6371 ReqCount=679936

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2109773368 Time=10:49:57.5621 ReqCount=688128

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2124649944 Time=10:49:58.4794 ReqCount=696320

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2139600224 Time=10:49:59.3897 ReqCount=704512

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2154517688 Time=10:50:00.2921 ReqCount=712704

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2169418864 Time=10:50:01.1882 ReqCount=720896

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2184401824 Time=10:50:02.0771 ReqCount=729088

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2199360208 Time=10:50:02.9594 ReqCount=737280

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2214272280 Time=10:50:03.8345 ReqCount=745472

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2229156688 Time=10:50:04.7041 ReqCount=753664

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2244104008 Time=10:50:05.5669 ReqCount=761856

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2259067424 Time=10:50:06.4244 ReqCount=770048

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2273899328 Time=10:50:07.2746 ReqCount=778240

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2288872840 Time=10:50:08.1192 ReqCount=786432

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2303803432 Time=10:50:08.9587 ReqCount=794624

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2318623912 Time=10:50:09.7921 ReqCount=802816

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2333777432 Time=10:50:10.6205 ReqCount=811008

client_8320_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=2348941408 Time=10:50:11.4430 ReqCount=819200

client_8320_1 |Running GC 820154 820154
