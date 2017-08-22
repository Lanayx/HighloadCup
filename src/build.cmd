rmdir data /s /q
dotnet restore
dotnet publish -c Release -r linux-x64 -o ../artifacts
REM rmdir obj /s /q