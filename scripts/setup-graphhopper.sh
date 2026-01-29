#!/bin/bash
mkdir -p graphhopper-data
cd graphhopper-data
wget https://download.geofabrik.de/asia/vietnam-latest.osm.pbf
cd ..
docker-compose -f docker-compose.graphhopper.yml up -d
