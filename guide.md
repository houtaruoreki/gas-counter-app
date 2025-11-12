# mobile app for gas counter locations 

## explanation
user should be able to add unit in app. that single unit is gas counter with location and id. idea is that user should be able to add all gas counters in the area and then see on the map afterwards. 

## problem this app should solve 
personel checking the counters sometimes miss counter. for example on given dead-end there are 4 counters and you check 3 and missed one you would not know it till total number of checked does not add up and you would not know which one coz they dont have a address just users name and street name. what im thinking is that if personel checks counter mark it on map and check for remaining on the map.

## business logic
using phones gps location and map api create offline map and save locations of manually added gas counters. and when map section is open display counters based on gps coordinates. 

## gas counter unit
counter unit should have location and id. that id is written on counter but mostly personel just wants pin on the map as an reminder and that should be optional. also it should have state field like if it leaks or something like that to be able mark on the map. 

## app UI/UX
- app must be in georgian but i can change that its not a problem. 
- it needs at leas 2 pages:
    - main page:
        - contains only map with dots on it. each dot it gas counter. 
        - on one location there could be multiple units
    - dashboard page:
        - add:
            - create new unit
        - search:
            - search unit with id
        - modify:
            after search or from map page user should be able to modify individual unit, like add id or any other parameter.

## suggestions
- counters does not change location so it should be 2 3 steps to delete it to avoid accidental deletion. 
- also database. if someone or me accidentally delete app it should have backup or data file on device.

## tech stack
use C# for android. phones in use mostly are samsung but all of them are at least androud 13. all you need is storage or same small db and location. since it will be offline for make it robust all user will install app manually and city is devided into secions and one section has like 800-1000 ish counters so it does not need big database.