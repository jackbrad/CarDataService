USE [SportsVizRaceData]
GO

/****** Object:  Index [PK__CarData__3214EC275C8CBE2F]    Script Date: 7/5/2016 4:55:01 PM ******/
ALTER TABLE [dbo].[CarData] ADD  CONSTRAINT [PK__CarData__3214EC275C8CBE2F] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

select distinct(d), count(*) as c from 
(
	select concat(datepart(yyyy, stamp), '-', datepart(MM, stamp), '-', datepart(DD, stamp)) as d 
	from cardata where sessionid is null
) as exp1
group by d

select * into SpeedHistogram
from udf_GetSpeedHistogramBySession()

select * from RpmHistogram