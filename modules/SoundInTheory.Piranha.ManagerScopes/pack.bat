:: Build the manager assets (Vite -> ./assets, embedded into the assembly)
call npm install
call npm run build

:: Clean and build in release
dotnet restore
dotnet clean
dotnet build -c Release

:: Create NuGet package
dotnet pack SoundInTheory.Piranha.ManagerScopes.csproj --no-build -c Release -o ./.nuget
