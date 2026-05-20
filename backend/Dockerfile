# ============================================================
# SofyCRM Backend - .NET 8 multi-stage build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./SofyCRM.Api/SofyCRM.Api.csproj ./SofyCRM.Api/
RUN dotnet restore ./SofyCRM.Api/SofyCRM.Api.csproj

COPY ./SofyCRM.Api/ ./SofyCRM.Api/
RUN dotnet publish ./SofyCRM.Api/SofyCRM.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SofyCRM.Api.dll"]
