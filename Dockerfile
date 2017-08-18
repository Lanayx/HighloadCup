# FROM microsoft/dotnet:2.0-sdk AS build-env
# WORKDIR /app

# # copy csproj and restore as distinct layers
# COPY ./src/*.fsproj ./
# RUN dotnet restore

# # copy everything else and build
# COPY ./src ./
# RUN dotnet publish -c Release -r linux-x64 -o out
# # COPY ./src/data.zip ./out

# # build runtime image
# FROM microsoft/dotnet:2.0-runtime-deps 
# WORKDIR /app
# ENV ASPNETCORE_URLS=http://+:80
# EXPOSE 80
# COPY --from=build-env /app/out ./
   
# ENTRYPOINT ["./HCup"]


FROM microsoft/dotnet:2.0-runtime-deps
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
COPY artifacts .
ENTRYPOINT ["./HCup"]