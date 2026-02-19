FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY DealFlow.slnx ./
COPY src/DealFlow.Contracts/DealFlow.Contracts.csproj src/DealFlow.Contracts/
COPY src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj src/DealFlow.NotifyWorker/

RUN dotnet restore src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj

COPY src/DealFlow.Contracts/ src/DealFlow.Contracts/
COPY src/DealFlow.NotifyWorker/ src/DealFlow.NotifyWorker/

RUN dotnet publish src/DealFlow.NotifyWorker/DealFlow.NotifyWorker.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DealFlow.NotifyWorker.dll"]
