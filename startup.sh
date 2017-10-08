#!/bin/bash

warmup () {
    sleep 155
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"id": 8, "user": 2131, "visited_at": 1165595891, "location": 3905, "mark": 3}' http://127.0.0.1/visits/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"first_name": "\u0412\u0430\u0441\u0438\u043b\u0438\u0439", "last_name": "\u041a\u043b\u0435\u0440\u0430\u0448\u0435\u043b\u043e", "gender": "m", "id": 8, "birth_date": -952041600, "email": "uhehsoun@me.com"}' http://127.0.0.1/users/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"id": 8, "distance": 87, "place": "\u0414\u0430\u0447\u0430", "city": "\u0421\u0430\u043d\u043a\u0442\u044f\u0440\u0441\u043a", "country": "\u0411\u0435\u043b\u043e\u0440\u0443\u0441\u0441\u0438\u044f"}' http://127.0.0.1/locations/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"location": 6364}' http://127.0.0.1/visits/2000000
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"gender": "f", "first_name": "\u0417\u043b\u0430\u0442\u0430", "last_name": "\u041b\u0435\u0431\u043e\u043b\u043e\u0447\u0430\u043d"}' http://127.0.0.1/users/200000
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"distance": 1, "place": "\u0414\u0435\u0440\u0435\u0432\u043e"}' http://127.0.0.1/locations/200000
    curl -s http://127.0.0.1/users/8
    curl -s -o /dev/null http://127.0.0.1/visits/8
    curl -s -o /dev/null http://127.0.0.1/locations/8
    curl -s -o /dev/null http://127.0.0.1/locations/8/avg?gender=f&fromDate=971568000&fromAge=27
    curl -s -o /dev/null http://127.0.0.1/users/8/visits?toDistance=20
}
warmup &

dotnet HCup.dll