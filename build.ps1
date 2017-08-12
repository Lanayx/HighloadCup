Set-Location src
.\build.cmd
Set-Location ..
docker build -t hcup/odin:latest .
# docker login stor.highloadcup.ru
docker tag hcup/odin:latest stor.highloadcup.ru/travels/disabled_cat
docker push stor.highloadcup.ru/travels/disabled_cat