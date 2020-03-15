@echo off
REM %1 should be the $(ProjectDir) macro in msbuild.
REM %2 should be the $(OutDir) macro in msbuild.

SET _about=About
IF EXIST %~dp1%_about% (
    xcopy /i /e /d /y %~dp1%_about% %~dp2..\%_about%
)
