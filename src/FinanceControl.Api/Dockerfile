FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/FinanceControl.Api/FinanceControl.Api.csproj", "src/FinanceControl.Api/"]
RUN dotnet restore "src/FinanceControl.Api/FinanceControl.Api.csproj"
COPY . .
WORKDIR "/src/src/FinanceControl.Api"
RUN dotnet build "FinanceControl.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceControl.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceControl.Api.dll"]
