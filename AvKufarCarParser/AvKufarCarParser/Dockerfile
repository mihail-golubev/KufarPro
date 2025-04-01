## Step 1: Use the official .NET SDK image to build the application
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#
#WORKDIR /app
#
## Copy the all and restore any dependencies (via dotnet restore)
#COPY . ./
#RUN dotnet restore
#
## Publish the application to the /out directory (Release configuration)
#RUN dotnet publish -c Release -o /out
#
## Step 2: Use the official .NET Runtime image to run the application
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#
## Copy the built app from the previous stage
#COPY --from=build /out .
#
## Set the entry point for the container (this is the command to run your bot)
#ENTRYPOINT ["dotnet", "AvKufarCarParser.dll"]