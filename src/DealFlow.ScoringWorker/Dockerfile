FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY DealFlow.slnx ./
COPY src/DealFlow.Contracts/DealFlow.Contracts.csproj src/DealFlow.Contracts/
COPY src/DealFlow.Data/DealFlow.Data.csproj src/DealFlow.Data/
COPY src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj src/DealFlow.ScoringWorker/

RUN dotnet restore src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj

COPY src/DealFlow.Contracts/ src/DealFlow.Contracts/
COPY src/DealFlow.Data/ src/DealFlow.Data/
COPY src/DealFlow.ScoringWorker/ src/DealFlow.ScoringWorker/

RUN dotnet publish src/DealFlow.ScoringWorker/DealFlow.ScoringWorker.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DealFlow.ScoringWorker.dll"]
