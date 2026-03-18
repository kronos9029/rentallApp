FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["RentalApp.slnx", "./"]
COPY ["src/RentalApp.Web/RentalApp.Web.csproj", "src/RentalApp.Web/"]
COPY ["src/RentalApp.Application/RentalApp.Application.csproj", "src/RentalApp.Application/"]
COPY ["src/RentalApp.Domain/RentalApp.Domain.csproj", "src/RentalApp.Domain/"]
COPY ["src/RentalApp.Infrastructure/RentalApp.Infrastructure.csproj", "src/RentalApp.Infrastructure/"]

RUN dotnet restore "src/RentalApp.Web/RentalApp.Web.csproj"

COPY . .
RUN dotnet publish "src/RentalApp.Web/RentalApp.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RentalApp.Web.dll"]
