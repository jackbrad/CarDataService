select car, lap, gpstime,
percent_rank() over (partition by car order by RPM) as rank
from cardata

;with cte as (
select *, row_number() over (partition by car order by rank desc, rpm desc) as rn from 
(	
	select car, rpm, percent_rank() over (partition by car order by RPM) as rank 
	from CarData where stamp > '2016-06-26' and speed > 0 and speed < 225
)
as exp1 where rank < 0.99 and rank > 0.01 
group by car, rank, rpm
--order by rank desc
)
select * from cte where rn = 1
order by car asc



select top 1 stamp From cardata where stamp > '2016-06-26'
select count(*) from cardata
where stamp > '2016-06-26' and speed > 0 and speed < 225
w
select * from cardata where speed > 225

;with cte as (
	select *, row_number() over (partition by car order by rank desc, rpm desc) as rn from 
	(	
		select car, rpm, percent_rank() over (partition by car order by RPM) as rank 
		from CarData where stamp > '2016-06-26' and stamp < dateadd(d, 1, '2016-06-26') and speed > 0 and speed < 225
	)
	as exp1 --where rank < 0.99 and rank > 0.01 
	group by car, rank, rpm
	--order by rank desc
)
select * from cte where rn = 1
order by rpm desc

select * From cardata where car = 7 and stamp > '2016-06-26'

select max(lap) from cardata where stamp > '2016-06-26'

exec up_FreezeTheField