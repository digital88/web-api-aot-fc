#!/bin/sh

dotnet clean
rm -rf ./app
rm -rf ./Test.Api/EfModel
dotnet publish --ucr Test.Api/Test.Api.csproj -c Release -o ./app
