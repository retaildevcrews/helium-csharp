# Change Log

## Sept 20, 2019

- Added logic to support CosmosDB key rotation
- Added middleware to handle /robots*.txt requests
- Added integration test (to dotnet.json) for robots*.txt
- added dockerfile to build dev environment (per Walmart request)
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
- renamed class IDal to DAL - not sure why it had the I preface
- added asserts to check /partitionKey in unit tests
- added architecture diagram
- added custom port instructions to readme
- added webhook setup command to readme

## Sept 5, 2019

- Initial commit
