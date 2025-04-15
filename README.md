How to start:

1. ```docker compose up```
2. ```./scripts/run-migrations.sh``` (check connection string).
3. curl or Postman some requests.
4. 

When debugging, EF Core models must be compiled. Run the following command before starting debug session:
```sh
dotnet ef dbcontext optimize --project ./Test.Api/Test.Api.csproj --output-dir ./EfModel --precompile-queries --nativeaot
```

Create Todo:
```sh
curl --location 'http://localhost:8080/todos' \
--header 'Content-Type: application/json' \
--data '{
    "title": "test",
    "isComplete": false
}'
```
Get Todo:
```sh
curl --location 'http://localhost:8080/todo/1'
```
