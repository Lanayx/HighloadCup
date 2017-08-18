Set-Location src
.\build.cmd
Set-Location ..
get-childitem -path ".\kestrel" -recurse | copy-item -destination ".\artifacts"
docker build -t hcup/odin .
# docker run --rm -p 80:80 hcup/odin
# docker login stor.highloadcup.ru
docker tag hcup/odin stor.highloadcup.ru/travels/safe_pike
docker push stor.highloadcup.ru/travels/safe_pike