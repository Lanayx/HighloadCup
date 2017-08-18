dotnet restore
dotnet publish -c Release -r linux-x64 -o ../artifacts
REM rmdir bin /s /q
REM rmdir obj /s /q