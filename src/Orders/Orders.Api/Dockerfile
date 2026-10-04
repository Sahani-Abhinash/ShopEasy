# Build from the repository root:
#   docker build -f src/Orders/Orders.Api/Dockerfile -t shopeasy/orders-api:dev .
#   docker build -f src/Orders/Orders.Api/Dockerfile --target migrator -t shopeasy/orders-migrator:dev .

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first -> restore is cached until dependencies change
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/BuildingBlocks/ShopEasy.ServiceDefaults/*.csproj src/BuildingBlocks/ShopEasy.ServiceDefaults/
COPY src/Orders/Orders.Domain/*.csproj         src/Orders/Orders.Domain/
COPY src/Orders/Orders.Application/*.csproj    src/Orders/Orders.Application/
COPY src/Orders/Orders.Infrastructure/*.csproj src/Orders/Orders.Infrastructure/
COPY src/Orders/Orders.Api/*.csproj            src/Orders/Orders.Api/
RUN dotnet restore src/Orders/Orders.Api/Orders.Api.csproj

# Now copy the source and publish
COPY src/ src/
RUN dotnet publish src/Orders/Orders.Api/Orders.Api.csproj \
      -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- EF Core migration bundle (only built for --target migrator) ----------
FROM build AS bundle
RUN dotnet tool install --global dotnet-ef --version 10.0.12
ENV PATH="${PATH}:/root/.dotnet/tools"
# Creating the bundle starts the app to build the EF model; it never connects, so placeholders are enough.
# Set only for this RUN (not stored in the image); the real values come from Compose at runtime.
RUN ConnectionStrings__OrdersDb="Host=placeholder" Services__Catalog="http://placeholder/" \
    dotnet ef migrations bundle \
      --project src/Orders/Orders.Infrastructure --startup-project src/Orders/Orders.Api \
      --configuration Release --output /app/efbundle

# ---------- Migrator: runs the bundle once, then exits ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS migrator
WORKDIR /app
COPY --from=bundle /app/efbundle .
# The bundle is a single-file app that extracts itself; /app is not writable for the non-root user
ENV DOTNET_BUNDLE_EXTRACT_BASE_DIR=/tmp
USER $APP_UID
ENTRYPOINT ["./efbundle"]

# ---------- Runtime (default target) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
