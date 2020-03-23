### Build and Test the App
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

### Optional: Set Proxy Variables
# ENV http_proxy {value}
# ENV https_proxy {value}
# ENV HTTP_PROXY {value}
# ENV HTTPS_PROXY {value}
# ENV no_proxy {value}
# ENV NO_PROXY {value}

### copy the source and tests
COPY src /src

### Run the tests
WORKDIR /src/tests
RUN dotnet test /p:collectcoverage=true

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
