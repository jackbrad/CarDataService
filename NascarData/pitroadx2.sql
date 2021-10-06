-- ==================================================
-- Create index template for Windows Azure SQL Database
-- ==================================================

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CarDataLocation ADD CONSTRAINT
	PK_CarDataLocation PRIMARY KEY CLUSTERED 
	(
	CarDataId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT



CREATE SPATIAL INDEX SIndx_CarDataLocation  ON CarDataLocation(Location);  

select * from cardatalocation where cardataid = 37467007

select * from (
select count(CarDataId) c, CarDataId from CarDataLocation 
group by CarDataId
) as exp1 
where c > 1
order by c desc

select count(*) from CarDataLocation where CarDataId not in (select id from cardata)

select distinct(cardataid) From cardatalocation l 
delete from cardatalocation
where cardataid not in (Select id from cardata)

delete from cardatalocation where 