IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'gmap_test')
BEGIN
  CREATE DATABASE gmap_test;
END;

GO

USE gmap_test;

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Markers' and xtype='U')
    CREATE TABLE Markers
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		marker_name VARCHAR(100),
		first_coordinate VARCHAR(100),
		second_coordinate VARCHAR(100),
		UNIQUE (marker_name)
	)

GO

INSERT INTO Markers (marker_name, first_coordinate, second_coordinate) VALUES ('Маркер #1', '54,9781305', '73,26137');
INSERT INTO Markers (marker_name, first_coordinate, second_coordinate) VALUES ('Маркер #2', '54,979956', '73,362481');
INSERT INTO Markers (marker_name, first_coordinate, second_coordinate) VALUES ('Маркер #3', '54,990120', '73,349995');

GO