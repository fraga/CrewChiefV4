@echo off
cls
setlocal

rem *******************************************************
rem Main user-configurable items:

set gamesList=AC,ACC,AMS,AMS2,DR2,GTR2,PCARS2,RF2
set CrewChiefPath="%ProgramFiles(x86)%\Britton IT Ltd\CrewChiefV4"

rem *******************************************************

rem *******************************************************
rem but you may need to edit these
	set AC_process=assettocorsa.exe
	set ACC_process=acc.exe
	set AMS_process=AMS.exe
	set AMS2_process=AMS2AVX.exe
	set GTR2_process=GTR2.exe
	set DR2_process=dirtrally2.exe
	set PCARS2_process=pcars2avx.exe
	set RF2_process=rfactor2.exe
rem *******************************************************

rem Logging may help if there are problems
set logging=off


rem ******************* HERE BE DRAGONS *******************	
rem Shouldn't need to touch anything below

	setlocal enableDelayedExpansion
	call :logSetup

title %~nx0
:Loop	
	@echo Program to check if one of a list of games has been started and
	@echo start Crew Chief for that game.
	@echo Also closes it down when the game finishes.
	@echo.
	@echo Press Ctrl/C to stop
	@echo.
	for %%g in (%gamesList%) do call :doGame %%g
	rem wait before checking again
	timeout /t 5 > nul
	cls
	goto :Loop


:doGame
		set game=%1
		set "process=%1_process"
		@echo Checking for %game% starting (!%process%!)...
		%SystemRoot%\System32\qprocess.exe !%process%! >nul 2>&1
		if errorlevel 1 (
			goto :NextGame
			)
		@call :cmnt %game% started
		@call :doCrewChief
		@call :cmnt Now waiting for %game% to close down after playing...
	:WaitStop
		%SystemRoot%\System32\qprocess.exe !%process%! >nul 2>&1
		if errorlevel 1 (
			call :cmnt %game% finished
			goto :shutDown
			) else (
			timeout /t 2 > nul
			goto :WaitStop
			)
	:shutDown
		call :cmnt Shut down CC
		@call :log start /d %CrewChiefPath% CrewChiefV4.exe -c_exit
		start /d %CrewChiefPath% CrewChiefV4.exe -c_exit
	:NextGame

	


	goto :eof


:doCrewChief
	call :CrewChief  -game %game% -profile %game%
	goto :eof
:CrewChief
	@call :log start /d %CrewChiefPath% CrewChiefV4.exe %*
	if /i "%2" == "None" goto :eof
	start /d %CrewChiefPath% CrewChiefV4.exe %*
	goto :eof


::::::::::::::::::::::::::::::::::::::::::::::::::
:logSetup
	if /i '%logging%' == 'off' GOTO :eof
	
	rem Log file timestamp
	set CUR_YYYY=%date:~10,4%
	set CUR_MM=%date:~7,2%
	set CUR_DD=%date:~4,2%
	set CUR_HH=%time:~0,2%
	if %CUR_HH% lss 10 (set CUR_HH=0%time:~1,1%)

	set CUR_NN=%time:~3,2%
	set CUR_SS=%time:~6,2%
	set CUR_MS=%time:~9,2%

	if not exist %USERPROFILE%\documents\CrewChief_AutoSwitcher mkdir %USERPROFILE%\documents\CrewChief_AutoSwitcher > nul
	set log=%USERPROFILE%\documents\CrewChief_AutoSwitcher\%CUR_YYYY%%CUR_MM%%CUR_DD%-%CUR_HH%%CUR_NN%%CUR_SS%.log
	call :cmnt Log file %log%

	echo.%date% > %log%
	goto :eof
::::::::::::::::::::::::::::::::::::::::::::::::
rem Echo and log comments	
:cmnt
	echo %*
	set _type=CMT
	goto :logIt
rem Log logs
:log
	set _type=LOG
:logIt	
	if /i '%logging%' == 'off' GOTO :eof
	echo %time% - %_type% %* >> %log%
	goto :eof