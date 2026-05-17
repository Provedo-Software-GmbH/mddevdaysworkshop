FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY Directory.Build.props Directory.Packages.props DevConfTicketing.slnx ./
COPY DevConfTicketing.Domain/DevConfTicketing.Domain.csproj DevConfTicketing.Domain/
COPY DevConfTicketing.Application/DevConfTicketing.Application.csproj DevConfTicketing.Application/
COPY DevConfTicketing.Infrastructure/DevConfTicketing.Infrastructure.csproj DevConfTicketing.Infrastructure/
COPY DevConfTicketing.Api/DevConfTicketing.Api.csproj DevConfTicketing.Api/
COPY DevConfTicketing.Tests/DevConfTicketing.Tests.csproj DevConfTicketing.Tests/

# Restore
RUN dotnet restore DevConfTicketing.slnx

# Copy everything and build
COPY . .
RUN dotnet publish DevConfTicketing.Api/DevConfTicketing.Api.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "DevConfTicketing.Api.dll"]
