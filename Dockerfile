FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src

COPY ./Warehouse.Components/*.csproj ./Warehouse.Components/
COPY ./Warehouse.Contracts/*.csproj ./Warehouse.Contracts/
COPY ./Warehouse.Startup/*.csproj ./Warehouse.Startup/
RUN dotnet restore ./Warehouse.Startup/Warehouse.Startup.csproj -r linux-musl-x64

COPY . .
RUN dotnet publish ./Warehouse.Startup/Warehouse.Startup.csproj -c Release -o /app -r linux-musl-x64 

FROM masstransit/platform:7.0
WORKDIR /app

COPY --from=build /app .


