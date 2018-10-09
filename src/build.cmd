rmdir data /s /q
REM dotnet restore
dotnet publish -c Release -r linux-x64 -o ../artifacts
rmdir obj /s /q