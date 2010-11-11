cd %1\Releases
set reldir=%DATE:~-4%%DATE:~3,2%%DATE:~0,2%%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%%TIME:~9,2%
mkdir %reldir%
copy ..\Simple.Data\bin\Release\*.dll %reldir%
copy ..\Simple.Data\bin\Release\*.pdb %reldir%
copy ..\Simple.Data.SqlServer\bin\Release\Simple.Data.SqlServer.dll %reldir%
copy ..\Simple.Data.SqlServer\bin\Release\Simple.Data.SqlServer.pdb %reldir%
copy ..\Simple.Data.SqlCe35\bin\Release\Simple.Data.SqlCe35.dll %reldir%
copy ..\Simple.Data.SqlCe35\bin\Release\Simple.Data.SqlCe35.pdb %reldir%
copy ..\Simple.Data.Mocking\bin\Release\Simple.Data.Mocking.dll %reldir%
copy ..\Simple.Data.Mocking\bin\Release\Simple.Data.Mocking.pdb %reldir%
