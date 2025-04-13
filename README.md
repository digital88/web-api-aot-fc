EF Core models must be compiled. Run the following command:

``` sh
dotnet ef dbcontext optimize --project Test.Api/Test.Api.csproj --output-dir ./Migrations/EfModel --precompile-queries --nativeaot
```
