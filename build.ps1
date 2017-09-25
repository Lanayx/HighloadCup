$container = if ($env:COMPUTERNAME -eq 'CTHULHU-3') { 'hcup/fornever' } else { 'hcup/odin' }
$tag = if ($env:COMPUTERNAME -eq 'CTHULHU-3') { 'stor.highloadcup.ru/travels/nice_toucan' } else { 'stor.highloadcup.ru/travels/safe_pike' }

Set-Location src
.\build.cmd
Set-Location ..
get-childitem -path ".\kestrel" -recurse | copy-item -destination ".\artifacts"
docker build -t $container .
# docker run -it --rm -p 80:80 $container
# docker login stor.highloadcup.ru
docker tag $container $tag
docker push $tag