FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["RentalApp.slnx", "./"]
COPY ["src/RentalApp.Worker/RentalApp.Worker.csproj", "src/RentalApp.Worker/"]
COPY ["src/RentalApp.Application/RentalApp.Application.csproj", "src/RentalApp.Application/"]
COPY ["src/RentalApp.Domain/RentalApp.Domain.csproj", "src/RentalApp.Domain/"]
COPY ["src/RentalApp.Infrastructure/RentalApp.Infrastructure.csproj", "src/RentalApp.Infrastructure/"]

RUN dotnet restore "src/RentalApp.Worker/RentalApp.Worker.csproj"

COPY . .
RUN dotnet publish "src/RentalApp.Worker/RentalApp.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RentalApp.Worker.dll"]
