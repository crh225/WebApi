rd wwwroot /q /y
rd views /q /y
copy "..\ODataSample.Web\appsettings.json" "appsettings.json" /y
xcopy "..\ODataSample.Web\wwwroot\*" "wwwroot\" /O /X /E /H /K /Y
xcopy "..\ODataSample.Web\Views\*" "Views\" /O /X /E /H /K /Y