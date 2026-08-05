# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PsiArtigos.Domain/PsiArtigos.Domain.csproj PsiArtigos.Domain/
COPY src/PsiArtigos.Application/PsiArtigos.Application.csproj PsiArtigos.Application/
COPY src/PsiArtigos.Infrastructure/PsiArtigos.Infrastructure.csproj PsiArtigos.Infrastructure/
COPY src/PsiArtigos.Api/PsiArtigos.Api.csproj PsiArtigos.Api/

RUN dotnet restore PsiArtigos.Api/PsiArtigos.Api.csproj

COPY src/PsiArtigos.Domain/ PsiArtigos.Domain/
COPY src/PsiArtigos.Application/ PsiArtigos.Application/
COPY src/PsiArtigos.Infrastructure/ PsiArtigos.Infrastructure/
COPY src/PsiArtigos.Api/ PsiArtigos.Api/

RUN dotnet publish PsiArtigos.Api/PsiArtigos.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PsiArtigos.Api.dll"]
