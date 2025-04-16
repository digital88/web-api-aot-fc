#!/bin/sh

ASPNETCORE_HTTP_PORTS=8080 \
ConnectionStrings__Default="Host=localhost;Port=5432;Database=todos_db;Username=todos_db_user;Password=todos_db_user_password;" \
FusionCache__RedisCacheOptions__EndPoints__0="" \
FusionCache__RedisCacheOptions__Ssl="false" \
FusionCache__RedisCacheOptions__Password="" \
FusionCache__RedisBackplaneOptions__EndPoints__0="" \
FusionCache__RedisBackplaneOptions__Ssl="false" \
FusionCache__RedisBackplaneOptions__Password="" \
./app/Test.Api
