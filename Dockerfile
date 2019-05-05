FROM microsoft/dotnet:2.2-sdk

USER gitpod

ENV ASPNETCORE_ENVIRONMENT=Development   
ENV ASPNETCORE_URLS=http://0.0.0.0:3978

USER root
