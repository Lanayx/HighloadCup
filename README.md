# HighloadCup
Highload cup F# solution

To launch the solution
1) Clone or download the repository https://github.com/sat2707/hlcupdocs
2) Create a zip file data.zip from /data/TRAIN/data from step 1, copy to src/ folder (/data/FULL/ is similar to real data used in rating run)
3) Run dotnet restore and dotnet run

Test solutions:
1) https://github.com/AterCattus/highloadcup_tester - easiest way to test just speed
2) https://github.com/sat2707/hlcupdocs/blob/master/TANK.md - proper way to test with graphs (see https://github.com/sat2707/hlcupdocs/issues/59 to run on windows)

Latest results (326.17633 s):

1st round 

![1st](http://clip2net.com/clip/m380071/6c332-clip-52kb.png)

2nd round 

![2nd](http://clip2net.com/clip/m380071/32121-clip-89kb.png)

3d round 

![3d](http://clip2net.com/clip/m380071/0e8fd-clip-70kb.png)


Avg method execution, mcs (name, 1st round, 3d round):

getUser 394,6414685 458,8085383

getVisit 389,3863371 368,7419097

getLocation 309,3342555 368,7853166

getAvgMark 344,0486825 392,5768781

getUserVisits 357,0944646 403,1762794


Logs:

client_8400_1 |Locations 760409 Users 1000072 Visits: 10000720

client_8400_1 |Hosting environment: Production

client_8400_1 |Content root path: /app/

client_8400_1 |Now listening on: http://[::]:80

client_8400_1 |Application started. Press Ctrl+C to shut down.

client_8400_1 |{"id":8,"birth_date":-1257206400,"first_name":"Алина","last_name":"Колыканая","gender":"f","email":"awdufutfyubwani@mail.ru"}Running GC 11 12:24:18.3989 gu:1 gv:1 gl:1 ga:1 gvs:1

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1217032560 Time=12:32:56.9646 ReqCount=8192

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1231360368 Time=12:33:27.2462 ReqCount=16384

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1245745384 Time=12:33:50.0683 ReqCount=24576

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1260179688 Time=12:34:09.1751 ReqCount=32768

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1274482832 Time=12:34:25.9445 ReqCount=40960

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1288908944 Time=12:34:41.0698 ReqCount=49152

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1303212176 Time=12:34:54.9568 ReqCount=57344

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1317621736 Time=12:35:07.8679 ReqCount=65536

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1332031464 Time=12:35:19.9835 ReqCount=73728

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1346285480 Time=12:35:31.4344 ReqCount=81920

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1360736168 Time=12:35:42.3177 ReqCount=90112

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1375088552 Time=12:35:52.7143 ReqCount=98304

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1389432744 Time=12:36:02.6805 ReqCount=106496

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1403858856 Time=12:36:12.2675 ReqCount=114688

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1418129296 Time=12:36:21.5147 ReqCount=122880

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1432555408 Time=12:36:30.4559 ReqCount=131072

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1446915984 Time=12:36:39.1213 ReqCount=139264

client_8400_1 |Gen0=13 Gen1=11 Gen2=9 Alloc=1461265936 Time=12:36:47.5333 ReqCount=147456

client_8400_1 |Running GC 151067 12:36:51.8992 gu:29473 gv:30023 gl:31072 ga:22657 gvs:30657

client_8400_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1245693464 Time=12:37:58.9591 ReqCount=155648

client_8400_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1363092680 Time=12:39:01.8291 ReqCount=163840

client_8400_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1480856984 Time=12:39:47.8017 ReqCount=172032

client_8400_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1598531848 Time=12:40:25.8742 ReqCount=180224

client_8400_1 |Gen0=14 Gen1=12 Gen2=9 Alloc=1716188584 Time=12:40:59.1011 ReqCount=188416

client_8400_1 |Running GC 191082 12:41:09.8987 gu:29473 gv:30023 gl:31087 ga:22657 gvs:30657

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1206567576 Time=12:41:38.6517 ReqCount=196608

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1220829768 Time=12:41:44.9263 ReqCount=204800

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1235280320 Time=12:41:49.6332 ReqCount=212992

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1249452288 Time=12:41:53.5674 ReqCount=221184

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1263739064 Time=12:41:57.0165 ReqCount=229376

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1278140480 Time=12:42:00.1265 ReqCount=237568

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1292427272 Time=12:42:02.9813 ReqCount=245760

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1306804176 Time=12:42:05.6329 ReqCount=253952

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1321164568 Time=12:42:08.1225 ReqCount=262144

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1335508696 Time=12:42:10.4741 ReqCount=270336

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1349869144 Time=12:42:12.7088 ReqCount=278528

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1364221504 Time=12:42:14.8433 ReqCount=286720

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1378557384 Time=12:42:16.8901 ReqCount=294912

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1392917848 Time=12:42:18.8575 ReqCount=303104

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1407221056 Time=12:42:20.7564 ReqCount=311296

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1421630664 Time=12:42:22.5919 ReqCount=319488

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1435901056 Time=12:42:24.3699 ReqCount=327680

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1450277912 Time=12:42:26.0965 ReqCount=335872

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1464671184 Time=12:42:27.7750 ReqCount=344064

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1478925176 Time=12:42:29.4104 ReqCount=352256

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1493301992 Time=12:42:31.0040 ReqCount=360448

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1507613328 Time=12:42:32.5613 ReqCount=368640

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1521859104 Time=12:42:34.0821 ReqCount=376832

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1536186736 Time=12:42:35.5697 ReqCount=385024

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1550547232 Time=12:42:37.0271 ReqCount=393216

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1564899496 Time=12:42:38.4557 ReqCount=401408

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1579218992 Time=12:42:39.8572 ReqCount=409600

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1593587656 Time=12:42:41.2320 ReqCount=417792

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1607980920 Time=12:42:42.5827 ReqCount=425984

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1622283968 Time=12:42:43.9105 ReqCount=434176

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1636693584 Time=12:42:45.2160 ReqCount=442368

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1651062200 Time=12:42:46.4999 ReqCount=450560

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1665398080 Time=12:42:47.7645 ReqCount=458752

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1679766728 Time=12:42:49.0098 ReqCount=466944

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1694028888 Time=12:42:50.2366 ReqCount=475136

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1708372912 Time=12:42:51.4468 ReqCount=483328

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1722782560 Time=12:42:52.6397 ReqCount=491520

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1737069296 Time=12:42:53.8166 ReqCount=499712

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1751396936 Time=12:42:54.9776 ReqCount=507904

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1765795688 Time=12:42:56.1238 ReqCount=516096

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1779975912 Time=12:42:57.2565 ReqCount=524288

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1794279072 Time=12:42:58.3750 ReqCount=532480

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1808697624 Time=12:42:59.4799 ReqCount=540672

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1823000768 Time=12:43:00.5722 ReqCount=548864

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1837344848 Time=12:43:01.6521 ReqCount=557056

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1851697104 Time=12:43:02.7200 ReqCount=565248

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1866057640 Time=12:43:03.7770 ReqCount=573440

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1880401760 Time=12:43:04.8219 ReqCount=581632

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1894827776 Time=12:43:05.8566 ReqCount=589824

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1909122760 Time=12:43:06.8813 ReqCount=598016

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1923459096 Time=12:43:07.8936 ReqCount=606208

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1937823800 Time=12:43:08.8975 ReqCount=614400

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1952167792 Time=12:43:09.8914 ReqCount=622592

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1966446424 Time=12:43:10.8761 ReqCount=630784

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1980831448 Time=12:43:11.8520 ReqCount=638976

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=1995224560 Time=12:43:12.8181 ReqCount=647168

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2009486760 Time=12:43:13.7765 ReqCount=655360

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2023904456 Time=12:43:14.7266 ReqCount=663552

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2038098168 Time=12:43:15.6682 ReqCount=671744

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2052384928 Time=12:43:16.6015 ReqCount=679936

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2066745424 Time=12:43:17.5275 ReqCount=688128

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2081056696 Time=12:43:18.4456 ReqCount=696320

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2095441696 Time=12:43:19.3566 ReqCount=704512

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2109802240 Time=12:43:20.2598 ReqCount=712704

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2124103392 Time=12:43:21.1566 ReqCount=720896

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2138526480 Time=12:43:22.0464 ReqCount=729088

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2152861616 Time=12:43:22.9293 ReqCount=737280

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2167192872 Time=12:43:23.8062 ReqCount=745472

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2181538032 Time=12:43:24.6757 ReqCount=753664

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2195865032 Time=12:43:25.5396 ReqCount=761856

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2210279944 Time=12:43:26.3971 ReqCount=770048

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2224528520 Time=12:43:27.2485 ReqCount=778240

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2238889344 Time=12:43:28.0938 ReqCount=786432

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2253318528 Time=12:43:28.9339 ReqCount=794624

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2267553616 Time=12:43:29.7682 ReqCount=802816

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2281941576 Time=12:43:30.5966 ReqCount=811008

client_8400_1 |Gen0=15 Gen1=13 Gen2=9 Alloc=2296276976 Time=12:43:31.4205 ReqCount=819200

client_8400_1 |Running GC 821126 12:43:32.3989 gu:155822 gv:155328 gl:157161 ga:117476 gvs:156168
