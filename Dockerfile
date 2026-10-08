# syntax=docker/dockerfile:1
# Build either host:  docker build --build-arg PROJECT=src/AMGH.ITInventory.Web --build-arg ENTRY=AMGH.ITInventory.Web.dll .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
ARG PROJECT=src/AMGH.ITInventory.API
RUN dotnet publish ${PROJECT} -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ARG ENTRY=AMGH.ITInventory.API.dll
ENV ENTRY=${ENTRY} ASPNETCORE_URLS=http://+:8080
RUN useradd -m app
COPY --from=build /app .
USER app
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "exec dotnet \"$ENTRY\""]
