select top 100 * From cardata where stamp > '2016-07-14'
select count(*) from cardatalocation where cardataid > 37751224

select top 100 * from cardata 
where stamp > '2016-07-17 17:41:00' 
and RPM > 0 and lap = 1
order by stamp asc


select car, gpstime, stamp, lat, lon, Location from cardata c 
join CarDataLocation l on c.id = l.cardataid
join pitroads p on geography::STPolyFromText(p.pitroad.MakeValid().STUnion(p.pitroad.STStartPoint()).STAsText(), 4326).STContains(l.Location) = 1
where stamp > '2016-07-17 17:41:00'
order by stamp asc

select * from cardatalocation c,
pitroads p
where p.Pitroad.STWithin(c.Location) = 1
join pitroads p on p.Pitroad.STContains(c.Location)


DECLARE @gm AS Geometry;
DECLARE @gg AS Geography;
SET @gg = geography::STGeomFromText(@gm.ToString(), 4326);

select geography::STGeomFromText(Pitroad.STAsText(), 4326) as gr from PitRoads

select pitroad.STAsText() from pitroads

select top 1 *, geography::STPolyFromText(Pitroad.STAsText(), 4326) as gr from PitRoads


select * from Pitroadpoly
where Pitroad.STContains(geography::STPointFromText('POINT(35.1310 -80.8560)', 4326)) = 1


select * from Pitroads
where Pitroad.STContains(geometry::STPointFromText('POINT(43.362545	-71.458833)', 4326)) = 1 

select * from Pitroads

select geography::STPolyFromText(pitroad.MakeValid().STUnion(pitroad.STStartPoint()).STAsText(), 4326).ReorientObject() from pitroadpoly

select pitroad from pitroads

--update pitroadpoly set pitroad = pitroad.ReorientObject()

--select * From cardata where stamp > '2016-07-20'
select Location from CarDataLocation
where CarDataId in( 
	select top 50000 id from cardata c
	where stamp > '2016-07-17 17:41:00'
	) 


select top 50000 l.location From cardata c
join CarDataLocation l on l.CarDataId = c.Id

where stamp > '2016-07-17 17:41:00'
tablesample(5000 rows);

) as exp1
join Pitroads p on (p.Pitroad.STContains(l.Location) = 1)

select * From tracks