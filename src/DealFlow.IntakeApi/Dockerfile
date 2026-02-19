FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files first (layer caching)
COPY DealFlow.slnx ./
COPY src/DealFlow.Contracts/DealFlow.Contracts.csproj src/DealFlow.Contracts/
COPY src/DealFlow.Data/DealFlow.Data.csproj src/DealFlow.Data/
COPY src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj src/DealFlow.IntakeApi/

RUN dotnet restore src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj

# Copy source
COPY src/DealFlow.Contracts/ src/DealFlow.Contracts/
COPY src/DealFlow.Data/ src/DealFlow.Data/
COPY src/DealFlow.IntakeApi/ src/DealFlow.IntakeApi/

RUN dotnet publish src/DealFlow.IntakeApi/DealFlow.IntakeApi.csproj \
    -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DealFlow.IntakeApi.dll"]
