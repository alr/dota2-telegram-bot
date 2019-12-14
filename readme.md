run db
```
docker run -d \
    --name=dota_db \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=Qwerty_6 \
    -e ALLOW_IP_RANGE=0.0.0.0/0 \
    -p 5432:5432 \
    -v ~/docker/dota2bot:/var/lib/postgresql/data \
    postgres:11
```