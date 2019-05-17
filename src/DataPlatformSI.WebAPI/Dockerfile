FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 5000

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY DataPlatformSI.WebAPI/DataPlatformSI.WebAPI.csproj DataPlatformSI.WebAPI/
RUN dotnet restore DataPlatformSI.WebAPI/DataPlatformSI.WebAPI.csproj
COPY . .
WORKDIR /src/DataPlatformSI.WebAPI
RUN dotnet build DataPlatformSI.WebAPI.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish DataPlatformSI.WebAPI.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "DataPlatformSI.WebAPI.dll"]
