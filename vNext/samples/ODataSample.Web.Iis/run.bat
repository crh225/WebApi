@echo OFF
echo Restore packages and build if necessary
call dotnet restore > restore.txt
call dotnet build
call dnu restore
call dnu build
echo Copy the main project's views and general www files
onbeforerun
echo Announce our success to the world!
powershell -c (New-Object Media.SoundPlayer "%windir%\media\notify.wav").PlaySync();
REM call explorer http://localhost:3745/odata/Products
if "%1"=="d" (
	vsjitdebugger "bin\Debug\net451\win81-x64\ODataSample.Web.Iis.exe"
	)
if "%1"=="" (
	REM dotnet run
	"bin\Debug\net451\win81-x64\ODataSample.Web.Iis.exe"
)