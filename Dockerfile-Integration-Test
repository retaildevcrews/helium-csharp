### build the app
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build

### Optional: Set Proxy Variables
# ENV http_proxy {value}
# ENV https_proxy {value}
# ENV HTTP_PROXY {value}
# ENV HTTPS_PROXY {value}
# ENV no_proxy {value}
# ENV NO_PROXY {value}

# Copy the source
COPY src /src

WORKDIR /src/integration-test
 
RUN dotnet publish -c Release -o /app

###########################################################

### build the runtime container
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime

# create a user
RUN groupadd -g 4120 helium && \
    useradd -r -u 4120 -g helium helium

# run as the helium user
USER helium

EXPOSE 4122

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "integration-test.dll" ]
