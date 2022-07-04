ARG RUNTIME_IMAGE_VERSION=3.1
ARG SDK_IMAGE_VERSION=3.1

FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_IMAGE_VERSION} AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:${SDK_IMAGE_VERSION} AS build
WORKDIR /src
COPY ["AppMetricsTest.API/AppMetricsTest.API.csproj", "AppMetricsTest.API/AppMetricsTest.API.csproj"]
RUN dotnet restore "AppMetricsTest.API/AppMetricsTest.API.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "AppMetricsTest.API/AppMetricsTest.API.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AppMetricsTest.API/AppMetricsTest.API.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AppMetricsTest.API.dll"]