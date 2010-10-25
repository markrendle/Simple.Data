ALTER DATABASE [$(DatabaseName)]
    ADD LOG FILE (NAME = [SimpleTest_log], FILENAME = 'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\SimpleTest_log.ldf', SIZE = 1024 KB, MAXSIZE = 2097152 MB, FILEGROWTH = 10 %);

