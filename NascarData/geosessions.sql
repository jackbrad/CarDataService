SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE usp_GetRawRaceDataByDate
	@startdate datetime,
	@enddate datetime
AS
BEGIN
	SET NOCOUNT ON;
	SELECT Car, GpsTime, Stamp, Lat, Lon, Alt, Lap, Speed, RPM, Throttle, Brake from CarData where stamp > @startdate and stamp < @enddate
END
GO

exec usp_GetRawRaceDataByDate @startdate = '2016-06-26', @enddate = '2016-06-27'

select count(*) from cardata where stamp > '2016-06-26'

with x as (
select row_number() over(order by stamp) as rownum, stamp
  from CarData
)
select a.rownum, b.rownum, a.stamp, b.stamp
  from x a
  join x b
    on b.rownum = a.rownum - 1
 where datediff(minute, a.stamp, b.stamp) > 1
 
;with cte as
	(
		select gpstime, stamp, 
		datediff(minute, stamp, lead(stamp, 1, 0) over (order by stamp)) leaddiff,
		datediff(minute, stamp, lag(stamp, 1, 0) over (order by stamp)) lagdiff,
		row_number() over (order by stamp) rn
		from cardata
	)

select gpstime, leaddiff, lagdiff, stamp from cte
where leaddiff > 5 or lagdiff < -5
--and rn <> 1
order by stamp

select * from cardata where gpstime in (1465570594.600000)
order by id, stamp 

select top 1000 id, stamp, gpstime From cardata where id > 297649
order by stamp, id

select stamp, count(*) as sc from cardata group by stamp
order by sc desc

select distinct(stamp) from cardata

update cardata set Stamp = dateadd(millisecond, (gpstime % 1) * 1000, dateadd(second, gpstime, '1970-01-01'))

select top 10 gpstime, stamp, gpstimestring, dateadd(millisecond,  (gpstime % 1) * 1000, dateadd(second, gpstime, '1970-01-01')) as parsed
from (Select distinct(gpstime), stamp, gpstimestring from cardata) as p1

--update cardata set stamp = 

select count(*) as c, * from (
select CONCAT(ROUND(Lat, 2), ',', round(lon, 2)) as loc from CarData
where gpstime > 
) as exp1
group by loc
order by c desc

select charindex(convert(varchar(20), lat), '.') from cardata

select distinct(ok) from (
select concat(
	substring(convert(varchar(20), lat), 0, (charindex('.', convert(varchar(20), lat)) + 3)),
	',', 
	substring(convert(varchar(20), lon), 0, (charindex('.', convert(varchar(20), lon)) + 3))
	) as ok
from cardata
) as exp1

select count (*) from cardata


select distinct(CONVERT(DATE, Stamp)) from CarData

select CONCAT(Lat, ',', lon) from cardata where lat > 45.320000

select charindex('.', convert(varchar(20), lat)) + 2 from cardata

select distinct(car) from cardata
where stamp > '2016-06-11 14:00:00' and stamp < '2016-06-11 15:00:00'
and speed > 0 and rpm > 0

--1465570293.400000	1465570594.600000

select count(*) as c, * from (
select CONCAT(ROUND(Lat, 2), ',', round(lon, 2)) as loc from CarData
where gpstime >= 1465570293.400000 and gpstime <= 1465570594.600000
) as exp1
group by loc
order by c desc

select * from cardata where sessionid is not null