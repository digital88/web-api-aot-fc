EF Core models must be compiled. Run the following command:

```sh
dotnet ef dbcontext optimize --project Test.Api/Test.Api.csproj --output-dir ./Migrations/EfModel --precompile-queries --nativeaot
```

```sh
dotnet publish --ucr Test.Api/Test.Api.csproj -c Release -o ./app
```

```sh
cd ./Test.Api

dotnet ef migrations bundle --self-contained -o ./efbundle --force

./efbundle --verbose --connection "Host=localhost;Port=5432;Database=todos_db;Username=todos_db_user;Password=todos_db_user_password;"
```

Create TODO:
```sh
curl --location 'http://localhost:8080/todos' \
--header 'Content-Type: application/json' \
--data '{
    "title": "test",
    "isComplete": false
}'
```
