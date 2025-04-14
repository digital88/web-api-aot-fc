#!/bin/sh

export ASPNETCORE_HTTP_PORTS=8080
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=todos_db;Username=todos_db_user;Password=todos_db_user_password;"
export FusionCache__RedisCacheOptions__EndPoints__0=""
export FusionCache__RedisCacheOptions__Ssl=""
export FusionCache__RedisCacheOptions__Password=""
export FusionCache__RedisBackplaneOptions__EndPoints__0=""
export FusionCache__RedisBackplaneOptions__Ssl=""
export FusionCache__RedisBackplaneOptions__Password=""

./app/Test.Api
