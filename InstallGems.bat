@ECHO OFF
SETLOCAL

@call bundle -v
IF ERRORLEVEL 1 (@call gem install bundler)

@call bundle install
