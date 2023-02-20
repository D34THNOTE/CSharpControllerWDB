FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CSharpControllerWDB/CSharpControllerWDB.csproj", "CSharpControllerWDB/"]
RUN dotnet restore "CSharpControllerWDB/CSharpControllerWDB.csproj"
COPY . .
WORKDIR "/src/CSharpControllerWDB"
RUN dotnet build "CSharpControllerWDB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CSharpControllerWDB.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CSharpControllerWDB.dll"]
