FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY SyncroBE.API.csproj ./
COPY SyncroBE.Application/SyncroBE.Application.csproj ./SyncroBE.Application/
COPY SyncroBE.Domain/SyncroBE.Domain.csproj ./SyncroBE.Domain/
COPY SyncroBE.Infrastructure/SyncroBE.Infrastructure.csproj ./SyncroBE.Infrastructure/
RUN dotnet restore SyncroBE.API.csproj

# Copy everything and clean any stale build artifacts
COPY . .
RUN rm -rf bin obj SyncroBE.Application/bin SyncroBE.Application/obj \
    SyncroBE.Domain/bin SyncroBE.Domain/obj \
    SyncroBE.Infrastructure/bin SyncroBE.Infrastructure/obj

RUN dotnet publish SyncroBE.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy certificates from build stage
COPY --from=build /src/Certificates/ ./Certificates/

# Render uses PORT env var
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SyncroBE.API.dll"]
