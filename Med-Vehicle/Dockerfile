# ---------------------------------------
# Stage 1: Build the Application
# ---------------------------------------
# usage: mcr.microsoft.com/dotnet/sdk:10.0
# We use the SDK image to compile and publish the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Med-Vehicle.csproj", "./"]
RUN dotnet restore "Med-Vehicle.csproj"

# Copy the rest of the source code
COPY . .

# Publish the app (This generates blazor.web.js and optimizes files)
RUN dotnet publish "Med-Vehicle.csproj" -c Release -o /app/publish

# ---------------------------------------
# Stage 2: Run the Application
# ---------------------------------------
# usage: mcr.microsoft.com/dotnet/aspnet:10.0
# We use the lighter ASP.NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published files from Stage 1
COPY --from=build /app/publish .

# Expose the port Railway expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the app
# Note: We use Med_Vehicle.dll because your .csproj replaces spaces with underscores
ENTRYPOINT ["dotnet", "Med_Vehicle.dll"]