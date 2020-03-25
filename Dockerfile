### Build and Test the App
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build

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
### Our tests rely on Managed Identity
### In order to run in the docker file, you have to setup managed identity
### The tests run as part of CI-CD instead as part of docker build
#WORKDIR /src/tests
#RUN dotnet test /p:collectcoverage=true

### Build the release app
WORKDIR /src/app
RUN dotnet publish -c Release -o /app

###########################################################

### Build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime

### if port is changed, also update value in Constants.cs
EXPOSE 4120
WORKDIR /app

### create a user
RUN adduser -S helium && \
    ### dotnet needs a home directory
    mkdir -p /home/helium && \
    chown -R helium:helium /home/helium

### run as helium user
USER helium

### copy the app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "helium.dll" ]
