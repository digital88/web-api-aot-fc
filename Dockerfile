FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
    clang zlib1g-dev
WORKDIR /build
COPY Directory.Packages.props .
COPY Test.Api/Test.Api.csproj ./Test.Api/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet restore Test.Api/Test.Api.csproj --no-http-cache --ucr
COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet publish Test.Api/Test.Api.csproj \
  -c Release \
  -o /app \
  --ucr

FROM busybox:glibc AS final
HEALTHCHECK --interval=10s --timeout=30s --start-period=5s --retries=3 CMD [ "nc", "-vz", "-w", "1", "localhost", "8080" ]
RUN addgroup -g 10000 app \
    && adduser -G app -u 10000 app -D
WORKDIR /app
COPY --from=build --chown=app:app --chmod=0555 /app/Test.Api .
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT [ "./Test.Api" ]
EXPOSE 8080
