### Build and Unit Test the App
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

### Optional: Set Proxy Variables
# ENV http_proxy {value}
# ENV https_proxy {value}
# ENV HTTP_PROXY {value}
# ENV HTTPS_PROXY {value}
# ENV no_proxy {value}
# ENV NO_PROXY {value}

### copy the source and unit tests code 
COPY src /src

### Run the unit tests
# WORKDIR /src/unit-tests
WORKDIR /src/e2e-tests
RUN dotnet test --logger:trx

### Build the release app
WORKDIR /src/app
RUN dotnet publish -c Release -o /app

###########################################################

### Build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime

### if port is changed, also update value in Constants.cs
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
