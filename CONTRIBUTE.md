# How to publish

To publish changes to the NuGet package, follow these steps:

1. Ensure you have the latest version of the code and have committed all your changes.
2. Update the version number in the `.csproj` file if necessary.
3. Run the following command to create the NuGet package:
   ```bash
   dotnet pack -c Release
   ```
4. Publish the package to NuGet.org using the following commands:
   ```bash
   dotnet nuget push "bin/Release/kvandijk.AI.<VERSION>.nupkg" --api-key <JOUW_API_KEY> --source https://api.nuget.org/v3/index.json
   ```

   ```bash
   dotnet nuget push "bin/Release/kvandijk.AI.<VERSION>.snupkg" --api-key <JOUW_API_KEY> --source https://api.nuget.org/v3/index.json
   ```