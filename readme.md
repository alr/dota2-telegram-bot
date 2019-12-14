run db

```docker run --name=dota_db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=Qwerty_6 -e ALLOW_IP_RANGE=0.0.0.0/0 -p 5432:5432 -d postgres:11```