FROM microsoft/dotnet:2.0.0-runtime
 
RUN apt-get update
RUN apt-get install -y nginx
 
WORKDIR /app
COPY artifacts .
ENV ASPNETCORE_URLS=http://+:80

COPY ./startup.sh .
RUN chmod 755 /app/startup.sh

EXPOSE 80

CMD ["sh", "/app/startup.sh"]
