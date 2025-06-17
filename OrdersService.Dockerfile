FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["OrdersService/Orders.Web/Orders.Web.csproj", "OrdersService/Orders.Web/"]
COPY ["OrdersService/Orders.UseCases/Orders.UseCases.csproj", "OrdersService/Orders.UseCases/"]
COPY ["OrdersService/Orders.Domain/Orders.Domain.csproj", "OrdersService/Orders.Domain/"]
COPY ["OrdersService/Orders.Infrastructure/Orders.Infrastructure.csproj", "OrdersService/Orders.Infrastructure/"]

COPY . .
WORKDIR "/src/OrdersService/Orders.Web"

RUN dotnet restore "Orders.Web.csproj"

RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Orders.Web.dll"]