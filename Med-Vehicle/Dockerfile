# ---------------------------------------
# Stage 1: Build the Application
# ---------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Med-Vehicle.csproj", "./"]
RUN dotnet restore "Med-Vehicle.csproj"

# Copy the rest of the source code
COPY . .

# Publish the app
RUN dotnet publish "Med-Vehicle.csproj" -c Release -o /app/publish

# ---------------------------------------
# Stage 2: Run the Application
# ---------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published files from Stage 1
COPY --from=build /app/publish .

# DIAGNOSTIC: List files to confirm the DLL name in the logs
RUN ls -la

# Expose the port Railway expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Start the app
# UPDATED: Changed Med_Vehicle.dll to Med-Vehicle.dll
ENTRYPOINT ["dotnet", "Med-Vehicle.dll"]