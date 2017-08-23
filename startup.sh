#!/bin/bash
# service nginx start

warmup () {
    sleep 25
    curl -s -o /dev/null http://127.0.0.1/users/8
    curl -s -o /dev/null http://127.0.0.1/visits/8
    curl -s -o /dev/null http://127.0.0.1/locations/8
    curl -s -o /dev/null http://127.0.0.1/locations/605/avg?gender=f&fromDate=971568000&fromAge=27
    curl -s -o /dev/null http://127.0.0.1/users/8/visits?toDistance=20
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/visits/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/users/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/locations/new
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/visits/8
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/users/8
    curl -s -o /dev/null -H "Content-Type: application/json" -X POST -d '{"test":"test"}' http://127.0.0.1/locations/8
}
warmup &

dotnet HCup.dll