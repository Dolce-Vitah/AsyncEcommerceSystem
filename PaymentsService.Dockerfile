FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PaymentsService/Payments.Web/Payments.Web.csproj", "PaymentsService/Payments.Web/"]
COPY ["PaymentsService/Payments.UseCases/Payments.UseCases.csproj", "PaymentsService/Payments.UseCases/"]
COPY ["PaymentsService/Payments.Domain/Payments.Domain.csproj", "PaymentsService/Payments.Domain/"]
COPY ["PaymentsService/Payments.Infrastructure/Payments.Infrastructure.csproj", "PaymentsService/Payments.Infrastructure/"]

COPY . .
WORKDIR "/src/PaymentsService/Payments.Web"

RUN dotnet restore "Payments.Web.csproj"

RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Payments.Web.dll"]