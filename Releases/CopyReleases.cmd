@echo off
cd %1\Releases
set reldir="%DATE:~-4%%DATE:~4,2%%DATE:~7,2%%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%%TIME:~9,2%"
mkdir %reldir%
copy ..\Simple.Data\bin\Release\Simple.Data.dll %reldir%
copy ..\Simple.Data\bin\Release\Simple.Data.pdb %reldir%
copy ..\Simple.Data.AdapterApi\bin\Release\Simple.Data.AdapterApi.dll %reldir%
copy ..\Simple.Data.AdapterApi\bin\Release\Simple.Data.AdapterApi.pdb %reldir%
copy ..\Simple.Data.Ado\bin\Release\Simple.Data.Ado.dll %reldir%
copy ..\Simple.Data.Ado\bin\Release\Simple.Data.Ado.pdb %reldir%
copy ..\Simple.Data.MongoDb\bin\Release\*.dll %reldir%
copy ..\Simple.Data.MongoDb\bin\Release\Simple.Data.MongoDb.pdb %reldir%
copy ..\Simple.Data.SqlServer\bin\Release\Simple.Data.SqlServer.dll %reldir%
copy ..\Simple.Data.SqlServer\bin\Release\Simple.Data.SqlServer.pdb %reldir%
copy ..\Simple.Data.SqlCe35\bin\Release\Simple.Data.SqlCe35.dll %reldir%
copy ..\Simple.Data.SqlCe35\bin\Release\Simple.Data.SqlCe35.pdb %reldir%
copy ..\Simple.Data.SqlCe40\bin\Release\Simple.Data.SqlCe40.dll %reldir%
copy ..\Simple.Data.SqlCe40\bin\Release\Simple.Data.SqlCe40.pdb %reldir%
copy ..\Simple.Data.Mocking\bin\Release\Simple.Data.Mocking.dll %reldir%
copy ..\Simple.Data.Mocking\bin\Release\Simple.Data.Mocking.pdb %reldir%
