FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Bark.slnx Directory.Build.props Directory.Packages.props NuGet.Config global.json ./
COPY src/Bark/Bark.csproj src/Bark/
COPY tests/Bark.Tests/Bark.Tests.csproj tests/Bark.Tests/
RUN dotnet restore src/Bark/Bark.csproj

COPY docs/ docs/
COPY src/Bark/ src/Bark/
RUN dotnet publish src/Bark/Bark.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Bark.dll"]
