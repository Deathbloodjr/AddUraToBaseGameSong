# AddUraToBaseGameSong
 A mod for TDMX to add ura charts to base game songs.

# How to use
 In the UraChartPath folder set in the config file, there will be a folder for each song getting an ura chart added\
 The folder will be the SongId, and within it will be a data.json file, and 3 chart files being <songId>_x.bin, <songId>_x_1.bin, and <songId>_x_2.bin\
 The json file will be formatted as
 ```json
 {
     "SongId": "weare0",
     "Branch": false,
     "Stars": 10,
     "Points": 1200,
     "PointsDuet": 1200,
     "Score": 1003200
 }
 ```
 using We Are's ura chart as an example
 
 It does not matter if the chart files are gzipped or not, but they can't be encrypted
