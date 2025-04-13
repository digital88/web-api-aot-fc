FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false
WORKDIR /build
COPY Directory.Packages.props .
COPY Test.Api/Test.Api.csproj ./Test.Api/
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet restore Test.Api/Test.Api.csproj --no-http-cache
COPY . .
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
  dotnet tool install --global dotnet-ef \
  && dotnet publish Test.Api/Test.Api.csproj \
  -c Release \
  -o /app \
  --os linux \
  --self-contained
  # && dotnet tool install --global dotnet-ef \
  # && dotnet ef migrations bundle \
  # --os linux \
  # --self-contained \
  # -o /var/efbundle

FROM busybox:glibc AS migrate
WORKDIR /app
COPY --from=build /var/efbundle .
CMD [ "sh", "-c", "./efbundle", "--verbose", "--connection", "${EF_BUNDLE_CONNECTION}" ]

FROM busybox:glibc AS final
HEALTHCHECK --interval=10s --timeout=30s --start-period=5s --retries=3 CMD [ "nc", "-vz", "-w", "1", "localhost/healthz", "8080" ]
RUN addgroup -g 10000 app \
    && adduser -G app -u 10000 app -D
WORKDIR /app
COPY --from=build --chown=app:app --chmod=0555 /app/Test.Api .
USER app
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT [ "./Test.Api" ]
EXPOSE 8080
