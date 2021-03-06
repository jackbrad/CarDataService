/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP 1000 [Column 0]
      ,[Column 1]
      ,[Column 2]
      ,[Column 3]
      ,[Column 4]
      ,[Column 5]
      ,[Column 6]
      ,[Column 7]
      ,[Column 8]
      ,[Column 9]
      ,[Column 10]
      ,[Column 11]
      ,[Column 12]
      ,[Column 13]
      ,[Column 14]
      ,[Column 15]
      ,[Column 16]
      ,[Column 17]
      ,[Column 18]
      ,[Column 19]
      ,[Column 20]
  FROM [dbo].[SV-Data-5202]

  select dateadd(SECOND, , '1970-1-1' )

  select top 10 * from martinsville2016
  select top 10 * from martinsville2016
 (select top 1000 dateadd(SECOND, CAST(GpsTime as decimal(18,6)) - 17, '1970-1-1') as Stamp, GpsTimeString, CarNumber, geography::STGeomFromText('POINT(' + Latitude + ' ' + Longitude +')',4326) as pointdata
 from martinsville2016)
  as exp1
 join martinsville m on m.placemark.STContains(exp1.pointdata) = 1
 
 select top 10 * from martinsvillerace2016

 select * from martinsvillerace2016 
 where Lap = 19 and speed > 35
 order by Stamp asc
 
 select top 10 * from martinsvillerace2016
 order by rpm desc

 select Car, Lap, max(Stamp) from martinsvillerace2016
 group by car, lap
 order by car, lap
 having max(stamp)

 select max(lat), max(lon) from (
 select avg(Lat), avg(Lon) from (
select a.racetime, b.Car, b.Lap, b.Stamp, b.Lat, b.Lon From (
 select max(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap
 union
 select min(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap) as a
 left join martinsvillerace2016 b on a.car = b.car and a.lap = b.lap and a.racetime = b.gpstime
--order by a.car, a.lap, a.racetime
where speed > 40
) as exp1
select avg(speed) from martinsvillerace2016 where speed > 35





select 

 select a.*
 from martinsvillerace2016 a
 left join martinsvillerace2016 b
 on a.car = b.car and a.lap = b.lap and a.stamp < b.stamp
 --where b.stamp = null 
 
  select min(Stamp) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap
 union
 select max(Stamp) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap
 order by car, lap, racetime


select a.racetime, b.Car, b.Lap, b.Stamp, b.Lat, b.Lon From (
 select max(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap
 union
 select min(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap) as a
 left join martinsvillerace2016 b on a.car = b.car and a.lap = b.lap and a.racetime = b.gpstime
--order by a.car, a.lap, a.racetime
where speed > 40
) as exp1


--2535694

select * From martinsvillerace2016 where car = 18 and lap = 379
select count(*) from martinsvillerace2016
select count(distinct(GPStime)) from martinsvillerace2016

select * From martinsvillerace2016
where car = 18 and lap = 379
order by gpstime

with 

with d as 
	(
		select gpstime, location, location.STBuffer(5) as buffer from martinsvillerace2016 where car = 18 and lap = 379
		--order by gpstime
	)
select row_number() over (order by gpstime), *
from d

select count(*) from martinsvillerace2016
where location.STIntersects(d.buffer) = 0

drop table #buffer;
with d as 
	(
		select gpstime, location, location.STBuffer(7) as buffer from martinsvillerace2016 where car = 18 and lap = 379
		--order by gpstime
	)
select row_number() over (order by gpstime) as segment, * into #buffer
from d

select m.car, max(m.lap), max(b.segment)--, m.location, b.buffer
from martinsvillerace2016 m 
join #buffer b on m.location.STIntersects(b.buffer) = 1
where m.gpstime = 1459712688.000000
group by lap, car, segment
order by lap desc, segment desc

drop table #buffer5
select count(*) From martinsvillerace2016 m
join (
	select gpstime, location, location.STBuffer(5) as buffer into #buffer5 from martinsvillerace2016 
	where car = 18 and lap = 379  ) as exp1 on m.location.STIntersects(exp1.buffer) = 0

where location.STIntersects(select 

select count(*) From martinsvillerace2016
with 
where location.STIntersects() = 0

select count(*) From martinsvillerace2016 m
join #buffer5 b on m.location.STIntersects(b.buffer) = 0



select 
select lap, car, lap, gpstime, stamp from martinsvillerace2016
order by 

select top 1000 lap, car, stamp, gpstime From martinsvillerace2016 
group by lap, car, stamp, gpstime
where count(max(lap)) = 1

select * from (
select top 1 car, lap, gpstime, stamp from martinsvillerace2016 
where lap = 273 order by gpstime
) as exp1
group by car, lap, gpstime, stamp

select count(*), max(Distinct(lap)) From martinsvillerace2016
where stamp = '2016-04-03 19:04:09.000'

 select max(Lap) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap
 union
 select min(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap) as a
 left join martinsvillerace2016 b on a.car = b.car and a.lap = b.lap and a.racetime = b.gpstime
 select * from @t
 
DECLARE @t TABLE(Point varchar(Max), Id int Identity(1,1) NOT NULL)

INSERT INTO @t (Point)
              SELECT top 98
              CAST(Lat AS varchar(15)) +  ' '  + CAST(Lon AS varchar(15)) as Loc
              FROM [MartinsvilleRace2016]
              WHERE lap=379 and car=88
              order by GpsTime
declare @max as int
select @max = MAX(ID) from @t

INSERT INTO @t (Point)
              SELECT Point
              FROM @t
              WHERE Id=1 

DECLARE @Loc VARCHAR(max)
SELECT @Loc = COALESCE(@Loc + ', ', '') + Point FROM @t order by id
Select 'CURVEPOLYGON(CIRCULARSTRING(' + @Loc + '))';
DECLARE @Track varchar(max) =  'CURVEPOLYGON(CIRCULARSTRING(' + @Loc + '))';

DECLARE @g1 geography = @Track
SELECT @g1


select m.car, max(m.lap) as lap, max(b.segment) as tracksegment--, m.location, b.buffer
from martinsvillerace2016 m 
join #buffer b on m.location.STIntersects(b.buffer) = 1
where m.gpstime = 1459712688.000000
group by lap, car, segment
order by lap desc, segment desc

select b.Car, b.Lap From (
 select max(Lap) from martinsvillerace2016
 group by car, lap
 union
 select min(GpsTime) as racetime, Car, Lap from martinsvillerace2016
 group by car, lap) as a
 left join martinsvillerace2016 b on a.car = b.car and a.lap = b.lap and a.racetime = b.gpstime
--order by a.car, a.lap, a.racetime
where speed > 40

---ftf---

drop table #buffer;
with d as 
	(
		select gpstime, location, location.STBuffer(7) as buffer from martinsvillerace2016 where car = 18 and lap = 379
		--order by gpstime
	)
select row_number() over (order by gpstime) as segment, * into #buffer
from d

;with cte
as
(
	select m.Car,
	max(m.Lap) over (partition by m.Car) as maxlap,
	max(b.Segment) over (partition by m.Car) as maxsegment
	from martinsvillerace2016 m join #buffer b on m.location.STIntersects(b.buffer) = 1 
	where m.gpstime = 1459715793.000000 --race end
)
select *
from cte
group by car, maxlap, maxsegment
order by maxlap desc, maxsegment desc

---ftf---


select top 1000 car, lap, gpstime from martinsvillerace2016
where car = 18 and lap = 500 
order by gpstime asc

;with cte
as
(
	select m.Car,
	max(m.Lap) over (partition by m.Car) as maxlap,
	max(b.Segment) over (partition by m.Car) as maxsegment,
	c.Location.STAsText() as point
	from martinsvillerace2016 m
	join tvf_BuildHotLap(379, 18, 7) b on m.location.STIntersects(b.buffer) = 1 
	join martinsvillerace2016 c on c.id = m.id
	where m.gpstime = 1459715793.000000 --race end
)
select car, maxlap, maxsegment, geography::STGeomFromText(point, 4326) as spot
from cte
group by car, maxlap, maxsegment, point
order by maxlap desc, maxsegment desc


select m.Car,
		max(m.Lap) over (partition by m.Car) as maxlap,
		max(b.Segment) over (partition by m.Car) as maxsegment
		from martinsvillerace2016 m 
		join tvf_BuildHotLap(379, 18, 7) b on m.location.STIntersects(b.buffer) = 1
		join martinsvillerace2016 b on m.id = b.id

;with cte
as
(
	select m.Car,
	max(m.Lap) over (partition by m.Car) as maxlap,
	max(b.Segment) over (partition by m.Car) as maxsegment,
	c.Location.STAsText() as point
	from martinsvillerace2016 m
	join tvf_BuildHotLap(379, 18, 7) b on m.location.STIntersects(b.buffer) = 1 
	join martinsvillerace2016 c on c.id = m.id
	where m.gpstime = 1459715793.000000 --race end
)
select car, maxlap, maxsegment, geography::STGeomFromText(point, 4326) as spot
from cte
group by car, maxlap, maxsegment, point
order by maxlap desc, maxsegment desc

declare @thing datetime = '2016-05-02 01:00:14.2000000'
select top 10 gpstime, dateadd(millisecond,  (gpstime % 1) * 1000, dateadd(second, gpstime - 17, '1970-01-01')) as parsed
from (Select distinct(gpstime) from martinsvillerace2016) as p1

select top 10 (gpstime % 1) * 1000 from (Select distinct(gpstime) from martinsvillerace2016) as p1

update MartinsvilleRace2016
set Stamp = dateadd(millisecond, (gpstime % 1) * 1000, dateadd(second, gpstime, '1970-01-01'))

select top 100 * from cardata where sessionid = null order by stamp desc

select * from streamsession order by startutc asc

select count(*) from cardata where id not in (
select id from cardata c
join streamsession s on c.Stamp >= s.StartUTC and c.Stamp <= s.EndUTC)

update c set c.SessionId = s.SessionId from CarData c
join StreamSession s on c.Stamp >= s.StartUTC and c.Stamp <= s.EndUTC

;with cte as	
(
    select gpstime, stamp, datediff(minute, stamp, lead(stamp, 1, 0) over (order by stamp)) leaddiff,
    datediff(minute, stamp, lag(stamp, 1, 0) over (order by stamp)) lagdiff,
	row_number() over (order by stamp) rn from cardata
    --where stamp > '2016-06-28'
)
select gpstime, leaddiff, lagdiff, stamp from cte
where leaddiff > 5 or lagdiff < -5
--and rn <> 1
order by stamp

select * from streamsession where startgpstime in (1456001376.400000,
1456007039.600000,1465570293.400000,1465570594.600000,1465571319.000000,1465579641.000000,1465584517.000000,1465588637.200000,1465589260.200000,1465594349.800000,
1465649390.600000,1465658656.400000,1465660483.000000,1465664253.800000,1465665317.800000,1465673136.400000,1465750282.800000,1465763078.600000,1466794818.400000,
1466801887.600000,1466807221.000000,1466812755.000000,1466877688.000000,1466881527.600000,1466967630.800000,1466967736.200000,1466968183.000000,1466978689.000000,
1467309414.400000,1467321525.000000,1467380431.400000,1467383470.800000,1467395049.400000,1467405457.800000,1467405959.600000,1467411478.600000,1467415602.000000,
1467425458.000000,1467503048.000000)


;with cte as (
select count(speed) as c, * from 
(	
	select speed, percent_rank() over (order by speed) as rank 
	from CarData where stamp > '2016-07-17 17:41:00' and speed > 0 and speed < 225
)
as exp1 --where rank < 0.99 and rank > 0.01 
group by rank, speed
order by speed desc
)
select * from cte --where rn = 1
order by speed desc

select count(distinct(Speed)) from CarData where stamp > '2016-07-17 17:41:00' and speed > 0 and speed < 225

SELECT   
		PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY speed) over (PARTITION BY SessionID) AS MedianDisc
FROM [dbo].[CarData] where stamp > '2016-07-17 17:41:00' and speed > 0 and speed < 225
Group By SessionID, Speed
Order By SessionID 

select


select count(speed) as c, * from 
(	
	select speed, percent_rank() over (order by speed) as rank 
	from CarData where stamp > '2016-07-17 17:41:00' and speed > 0 and speed < 225
)
as exp1 --where rank < 0.99 and rank > 0.01 
group by rank, speed
order by speed desc

select count(speed) c, speed from cardata
where stamp > '2016-07-17 17:41:00' and speed > 0 and speed < 225
group by speed
order by speed asc

select count(round(speed, 0)) c, round(speed, 0) from cardata
where stamp > '2016-07-17 17:41:00' and speed > 1 and speed < 225
group by round(speed, 0)
order by c desc
