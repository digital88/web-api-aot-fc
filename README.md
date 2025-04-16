How to start:

1. `docker compose up`
2. `./scripts/run-migrations.sh` (check connection string). This is one off step.
3. curl or Postman some requests.
4. Open redis UI at `http://localhost:8001/redis-stack/browser` and see than cache works.

Configuration builder (slim) ignores or not supports(?) configuration from `appsettings.json`. Use env variables to pass configuration.

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
Get Todos with paging:
```sh
curl --location 'http://localhost:8080/todos/?recordId=0&pageSize=5'
```
