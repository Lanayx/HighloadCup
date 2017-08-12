FROM microsoft/dotnet
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY artifacts .
ENTRYPOINT ["dotnet", "HCup.dll"]