FROM microsoft/dotnet:1.1.1-runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY artifacts .
ENTRYPOINT ["dotnet", "HCup.dll"]