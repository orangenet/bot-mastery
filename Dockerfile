FROM microsoft/dotnet:2.2.100-sdk-alpine3.8

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://0.0.0.0:3978

USER root
