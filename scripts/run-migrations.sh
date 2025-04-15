#!/bin/sh

export ConnectionStrings__Default="Host=localhost;Port=5432;Database=todos_db;Username=todos_db_user;Password=todos_db_user_password;"
dotnet clean
rm -rf ./Test.Api/EfModel
dotnet ef migrations bundle --project ./Test.Api/Test.Api.csproj --self-contained -o ./Test.Api/efbundle --force --verbose
./Test.Api/efbundle --verbose
rm -f ./Test.Api/efbundle
export ConnectionStrings__Default=
