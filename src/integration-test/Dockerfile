### build the app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
RUN mkdir -p /src
WORKDIR /src
 
COPY . /src

RUN dotnet restore && \
    dotnet publish -c Release -o /app

###########################################################

### build the runtime container

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime

# create a user
RUN groupadd -g 4120 helium && \
    useradd -r -u 4120 -g helium helium

# run as teh helium user
USER helium

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT [ "dotnet",  "integration-test.dll" ]

# default to localhost
CMD [ "-h","http://localhost:4120" ]
