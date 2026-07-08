#!/usr/bin/env bash
set -e

# Build the manager assets (Vite -> ./assets, embedded into the assembly)
npm install
npm run build

# Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

# Create NuGet package
dotnet pack SoundInTheory.Piranha.PageManagerExtensions.csproj --no-build -c Release -o ./.nuget
