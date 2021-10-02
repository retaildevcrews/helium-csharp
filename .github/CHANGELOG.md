# Change Log

## 1.1.1

- Oct 2021
- updated dev container / Codespaces build

## v1.1.0

- March 2020
- added authentication type environment varible and command line requirements
  - supports MSI, Azure CLI or Visual Studio credentials
- converted Swagger to not auto-generate swagger.json for fidelity

## v1.0.0

- Dec 2019
- converted /healthz to use dotnet HealthCheck
- added IETF draft render to /healthz/ietf

## Dec 2019

- updated md docs

## Nov 2019

- Created a separate repo for the application
- Upgraded to dotnet core 3.0

## Oct 2019

- added retry for AKS pod identity (it takes ~ 30 seconds to spin up the first time in the cluster)
- added /version endpoint
- added additional movie APIs
  - /api/movies?genre=action
  - /api/movies?year=1999
  - /api/movies?rating=8.5 (this returns >= 8.5)
  - /api/movies?actorid=nm0000206
  - /api/movies?toprated=true (returns the 10 top rated movies sorted by rating desc)
  - /api/featured/movie (returns a random movie from the featured movies)

## Oct 2019

- Enhanced exception logging to include CosmosDB information where available
- Simplified dockerfile

## Sept 2019

- Added logic to support CosmosDB key rotation
- Added middleware to handle /robots*.txt requests
- Added integration test (to dotnet.json) for robots*.txt
- added dockerfile to build dev environment
- added optional proxy variables in dockerfile
- added .dockerignore
- simplified dockerfile
- renamed appsettings.json and made it required
- removed unused assemblies
- removed and sorted usings
- updated to new IMDb extract
- added totalScore to Movie for upcoming "voting version"
- removed "profession" from Movies.Roles for clarity
- renamed /key to /partitionKey for clarity
- added call to dal.Healthz() on DAL create and reload
  - if CreateDal() fails, the app will exit(-1) - "fail early"
  - if ReloadDal() fails, an error is logged and the old DAL is used until the next check
- renamed class IDal to DAL for clarity
- added asserts to check /partitionKey in unit tests

## Sept 5, 2019

- Initial commit
