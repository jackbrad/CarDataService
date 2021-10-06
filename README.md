# CarDataService

This is the complete code... by Jack Bradham circa 2018 for the NASCAR CarDataService and test harnesses. 
The primary deployable unit is in the NascarTelnetServer in the dir named CarDataService. Which is a windows service that aggregates and format the flow of CSV messages into Json documents. 

NASCAR Data reveals example SQL Statements for aggregates and Geopoisitioning queries: NascarData

NASCAR needed a way to retrieve data from race cars traveling around a track at 200 MPH with no WIFI or Cellular coverage. After being received from the serial provider on a network port (Telnet) the messages needed to be parsed and transformed. 

Impact: Built as a field gateway device to take raw data from the Network port in raw bits into json Messages sent to any application subscriber. 
