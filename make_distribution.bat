@echo off
set pipo=%~dp0

if [%1]==[] (
	echo 1st arg: You must specify Release or Debug option to create a distribution.
	goto end
)

if [%2]==[] (
	echo 2nd arg: You must specify a Unity project path
	goto end
)

if [%3]==[] (
	echo 3rd arg: You must specify the Unity executable path
	goto end
)

if [%4]==[] (
	echo "4th arg: No unitypackage output specified. Using %pipo% for path"
	set exportPath=%pipo%
)
if not [%4]==[] (
	set exportPath=%4
)

set type=%1
set projectPath=%2
set unity=%3


:begin

if not [%type%]==[Release] (
	if not [%type%]==[Debug] (
		echo %type% is an unknown configuration...
		goto end
	)
)
if exist "build\dist" (
	echo Removing existing distribution dir
	RMDIR "build\dist" /s /q
)

REM Simply copy the generated build stuff
xcopy "build\%type%\*" "build\dist\" /F /R /Y /I /S

REM then remove all documentary files if on Release
if [%type%]==[Release] (
	echo Deleting documentary for production release
	del build\dist\*.xml /S
)

if exist "%projectPath%\Assets\Playblack" (
	echo Removing existing Asset data
	RMDIR "%projectPath%\Assets\Playblack" /s /q
)
REM Copy all files to the unity project path to prepare for a new unitypackage
xcopy "build\dist\*" "%projectPath%\Assets\Playblack" /F /R /Y /I /S

if [%type%]==[Debug] (
	echo Updating guiskin package for debug / editor build
	REM Now use unity to import the latest guiskin package
	%unity% -projectPath %projectPath% -importPackage "%pipo%build\dist\Editor\Resources\SequencerSkin.unitypackage" -batchmode -nographics -quit
)

REM And finally create a new distribution package.
echo Exporting PlayblackCore unity package
%unity% -projectPath %projectPath% -exportPackage Assets\Playblack %exportPath%PlayblackCore.unitypackage -batchmode -nographics -quit

:end
echo Done...