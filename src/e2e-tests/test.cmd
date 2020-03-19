@echo off

dotnet clean
dotnet build

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="./TestResults/"

//coverlet bin\Debug\netcoreapp3.1\helium.dll --target "dotnet" --targetargs "test --no-build" --output "./coverage-reports/"

dotnet reportgenerator "-reports:testresults\coverage.cobertura.xml" "-targetdir:./TestResults/html" -reporttypes:HTML;

coverage-reports\html\index.htm

REM https://www.tonyranieri.com/blog/2019/07/31/Measuring-.NET-Core-Test-Coverage-with-Coverlet/
