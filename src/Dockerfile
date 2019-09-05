### Build and Unit Test the App
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
RUN mkdir -p /src/app && \
    mkdir -p /src/unit-tests

WORKDIR /src/unit-tests

### copy the source and unit tests code 
COPY app /src/app
COPY unit-tests /src/unit-tests

### Build the app
RUN dotnet restore && \
    dotnet build --no-restore && \
### Run the unit tests
    dotnet test --no-build --logger:trx

### Build the release app
WORKDIR /src/app

RUN dotnet restore && \
    dotnet publish --no-restore -c Release -o /app


###########################################################

### Build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
EXPOSE 4120
WORKDIR /app

### create a user
RUN groupadd -g 4120 helium && \
    useradd -r  -u 4120 -g helium helium && \
### dotnet needs a home directory for the secret store
    mkdir -p /home/helium && \
    chown -R helium:helium /home/helium

### run as helium user
USER helium

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "helium.dll" ]
