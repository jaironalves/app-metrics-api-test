ARG RUNTIME_IMAGE_VERSION=3.1
ARG SDK_IMAGE_VERSION=3.1

FROM mcr.microsoft.com/dotnet/aspnet:${RUNTIME_IMAGE_VERSION} AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:${SDK_IMAGE_VERSION} AS build
WORKDIR /src
COPY ["AppMetrics.APIPost/AppMetrics.APIPost.csproj", "AppMetrics.APIPost/AppMetrics.APIPost.csproj"]
RUN dotnet restore "AppMetrics.APIPost/AppMetrics.APIPost.csproj"
COPY . .
WORKDIR /src
RUN dotnet build "AppMetrics.APIPost/AppMetrics.APIPost.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AppMetrics.APIPost/AppMetrics.APIPost.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AppMetrics.APIPost.dll"]