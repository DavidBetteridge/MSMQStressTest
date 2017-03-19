USE MASTER
GO

DROP DATABASE SnapshotTest
GO

CREATE DATABASE SnapshotTest
GO

USE SnapshotTest
GO


SELECT is_read_committed_snapshot_on, snapshot_isolation_state_desc,snapshot_isolation_state FROM sys.databases WHERE name= db_name()

ALTER DATABASE SnapshotTest  
SET ALLOW_SNAPSHOT_ISOLATION ON  
GO

--Kick out other users
ALTER DATABASE SnapshotTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE
  
ALTER DATABASE SnapshotTest  
SET READ_COMMITTED_SNAPSHOT ON  WITH NO_WAIT  --fail fast
GO

ALTER DATABASE SnapshotTest SET MULTI_USER

SELECT is_read_committed_snapshot_on, snapshot_isolation_state_desc,snapshot_isolation_state FROM sys.databases WHERE name= db_name()
GO



CREATE TABLE dbo.MSMQTest
(
	ID		INTEGER IDENTITY(1,1)	NOT NULL,
	GUID	UNIQUEIDENTIFIER		NOT NULL,
	Status	TINYINT					NOT NULL,
CONSTRAINT pk_MSMQTest PRIMARY KEY (ID),
CONSTRAINT ak_MSMQTest UNIQUE (GUID)
)
GO


CREATE PROCEDURE dbo.usp_Insert AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	INSERT INTO dbo.MSMQTest (GUID, Status)
	SELECT NEWID(), 1

	SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID
END
GO

CREATE PROCEDURE dbo.usp_Get(@ID AS INTEGER) AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	--SET TRANSACTION ISOLATION LEVEL SERIALIZABLE  
    

	SELECT GUID 
	  FROM dbo.MSMQTest M
	 WHERE M.ID = @ID

END
GO

CREATE PROCEDURE dbo.usp_Update(@GUID AS UNIQUEIDENTIFIER) AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED

	UPDATE M
	   SET M.Status = 2
	  FROM dbo.MSMQTest M
	 WHERE M.GUID = @GUID
	   AND M.Status = 1

	IF @@ROWCOUNT != 1 
	BEGIN
		RAISERROR('No record was updated',16, 1)
		RETURN (-1)
	END

END
GO

select status, count(*) from MSMQTest group by status
go