cd Simple.Data
call NugetPush.cmd %1
cd ..\Simple.Data.Ado
call NugetPush.cmd %1
cd ..\Simple.Data.Mocking
call NugetPush.cmd %1
cd ..\Simple.Data.SqlCe40
call NugetPush.cmd %1
cd ..\Simple.Data.SqlServer
call NugetPush.cmd %1
cd ..
