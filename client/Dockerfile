FROM mcr.microsoft.com/dotnet/runtime:6.0-focal AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["client/client.csproj", "client/"]
RUN dotnet restore "client/client.csproj"
COPY . .
WORKDIR "/src/client"
RUN dotnet build "client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ENV DEVICE_ID="${DEVICE_ID}" \
    GATE_NUMBER="${GATE_NUMBER}" \
    HUB_URL="${HUB_URL}"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "client.dll"]
