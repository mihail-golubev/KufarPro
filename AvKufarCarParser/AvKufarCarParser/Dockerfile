# Use .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj and restore the dependencies (via dotnet restore)
COPY ["AvKufarCarParser/AvKufarCarParser.csproj", "AvKufarCarParser/"]
RUN dotnet restore "AvKufarCarParser/AvKufarCarParser.csproj"

# Copy the rest of the application and build the application
COPY . ./
WORKDIR "/app/AvKufarCarParser"
RUN dotnet publish "AvKufarCarParser.csproj" -c Release -o /app/publish

# Use the runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Copy the published files from the build stage
COPY --from=build /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "AvKufarCarParser.dll"]