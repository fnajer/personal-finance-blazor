# Мои финансы

Небольшое приложение для учёта личных доходов и расходов. Проект демонстрирует работу с Blazor Interactive Server, Entity Framework Core, SQLite, миграциями, формами и валидацией.

## Возможности

- сводка доходов, расходов и баланса за текущий месяц;
- диаграмма динамики расходов за последние шесть месяцев;
- распределение расходов по категориям;
- месячные лимиты по категориям расходов с индикаторами использования;
- предупреждения при приближении к лимиту и его превышении;
- создание, редактирование и удаление операций;
- поиск и фильтрация по типу и периоду;
- экспорт отфильтрованных операций в CSV;
- управление категориями доходов и расходов;
- регистрация и вход через ASP.NET Core Identity;
- подтверждение email и повторная отправка письма;
- восстановление пароля по одноразовой ссылке;
- полная изоляция операций и категорий между пользователями;
- защита от удаления используемой категории;
- начальный набор категорий для каждого нового пользователя.

## Технологии

- .NET 10 LTS;
- ASP.NET Core Blazor Web App;
- Interactive Server rendering;
- Entity Framework Core 10;
- ASP.NET Core Identity;
- SQLite.

## Запуск

Установите [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), затем выполните:

```powershell
dotnet restore
dotnet run
```

Откройте адрес, указанный в терминале. При первом запуске приложение автоматически создаст файл `finance.db` и применит миграции. Зарегистрируйте аккаунт — для него будет создан отдельный набор категорий.

### Настройка email

Для отправки писем заполните секцию `Email` в `appsettings.json` или передайте те же параметры через переменные окружения (`Email__Host`, `Email__Port`, `Email__Username`, `Email__Password`, `Email__FromAddress`). Пароль SMTP не следует сохранять в репозитории — для локальной разработки используйте user secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Email:Host" "smtp.example.com"
dotnet user-secrets set "Email:Username" "user@example.com"
dotnet user-secrets set "Email:Password" "smtp-password"
dotnet user-secrets set "Email:FromAddress" "user@example.com"
```

Если SMTP не настроен в Development, ссылки подтверждения и восстановления выводятся в консоль приложения. В остальных окружениях SMTP обязателен.

## Работа с миграциями

В проект добавлен локальный инструмент EF Core. Восстановите его один раз:

```powershell
dotnet tool restore
```

Создание новой миграции после изменения моделей:

```powershell
dotnet tool run dotnet-ef migrations add НазваниеМиграции
dotnet tool run dotnet-ef database update
```

База SQLite и каталоги сборки исключены из Git. Строка подключения находится в `appsettings.json`.

## Следующие этапы

1. Автоматические тесты бизнес-логики и компонентов.
