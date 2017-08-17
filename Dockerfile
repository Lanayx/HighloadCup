FROM microsoft/dotnet:2.0.0-runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY artifacts .
ENTRYPOINT ["dotnet", "HCup.dll"]