# RideAudit counsel container sketch.
# GPL-2.0-only. Build from the repository root:
#   docker build -f deploy/containers/counsel/Dockerfile -t rideaudit-counsel:local .
#
# This image publishes the same .NET 10 gRPC host as admission. Counsel, ingest,
# and privacy services are registered on that host. Production refuses fixture
# calendars and fixture Play Integrity. The image was not executed in the
# environment that added this sketch.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props nuget.config ./
COPY src ./src
RUN dotnet publish src/RideAudit.Server.Admission/RideAudit.Server.Admission.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV RIDEAUDIT_EDGE_TLS=true
ENV RIDEAUDIT_SERVICE_ROLE=counsel
EXPOSE 8080
ENTRYPOINT ["dotnet", "RideAudit.Server.Admission.dll"]
