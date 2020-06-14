## Сборка образа
Необходимо *tag* вместо указать версию тега
```
docker build -t alr1wn0/dota-telegram-bot:tag -f Dota2Bot.WorkerService/Dockerfile .
```

## Запуск сервиса
```
docker-compose up -d
```