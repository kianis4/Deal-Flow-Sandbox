FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY DealFlow.slnx ./
COPY src/DealFlow.Contracts/DealFlow.Contracts.csproj src/DealFlow.Contracts/
COPY src/DealFlow.Data/DealFlow.Data.csproj src/DealFlow.Data/
COPY src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj src/DealFlow.ReportingApi/

RUN dotnet restore src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj

COPY src/DealFlow.Contracts/ src/DealFlow.Contracts/
COPY src/DealFlow.Data/ src/DealFlow.Data/
COPY src/DealFlow.ReportingApi/ src/DealFlow.ReportingApi/

RUN dotnet publish src/DealFlow.ReportingApi/DealFlow.ReportingApi.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DealFlow.ReportingApi.dll"]
