@echo off

dotnet clean

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="./TestResults/"

dotnet reportgenerator "-reports:testresults\coverage.cobertura.xml" "-targetdir:./TestResults/html" -reporttypes:HTML;

TestResults\html\index.htm

REM https://www.tonyranieri.com/blog/2019/07/31/Measuring-.NET-Core-Test-Coverage-with-Coverlet/
