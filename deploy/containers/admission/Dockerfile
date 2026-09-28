# RideAudit admission container sketch.
# GPL-2.0-only. Build from the repository root:
#   docker build -f deploy/containers/admission/Dockerfile -t rideaudit-admission:local .
#
# The image listens on HTTP/2 port 8080. Production must terminate TLS 1.2+
# at the edge (RIDEAUDIT_EDGE_TLS=true) or mount a certificate. The documented
# OTS fixture and Play Integrity fixture are refused when ASPNETCORE_ENVIRONMENT=Production.

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
EXPOSE 8080
ENTRYPOINT ["dotnet", "RideAudit.Server.Admission.dll"]
