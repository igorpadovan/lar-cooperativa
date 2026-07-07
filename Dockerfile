# ===== Desenvolvimento: hot reload com dotnet watch (código montado via volume) =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080
# --non-interactive: reinicia sozinho em edições que o hot reload não cobre (ex.: excluir classes)
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/LarCooperativa.Api", "--no-launch-profile", "--non-interactive"]

# ===== Build/publish =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY LarCooperativa.slnx ./
COPY src/LarCooperativa.Api/LarCooperativa.Api.csproj src/LarCooperativa.Api/
RUN dotnet restore src/LarCooperativa.Api
COPY . .
RUN dotnet publish src/LarCooperativa.Api -c Release -o /app/publish --no-restore

# ===== Runtime (produção) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "LarCooperativa.Api.dll"]
