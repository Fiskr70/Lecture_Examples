# LogHub

Учебный проект по практикуму по паттернам программирования, вариант 4. Начальный этап сохраняет формат разбора исходного примера `дата|уровень|сообщение`; Syslog RFC 5424 будет реализован в работе по импорту.

```powershell
dotnet restore LogHub.sln
dotnet build LogHub.sln --no-restore
dotnet test LogHub.sln --no-build
dotnet run --project LogHub -- путь-к-логу
```

Если `loghub.config` отсутствует, используются значения `sink=console`, `minSeverity=Info`, `format=plain`. Доступны приёмники `console`, `file`, `database` (демонстрационная имитация), форматы `plain`, `json`, `csv`.

Отчёт и ограничения: [docs/ЛР1.md](docs/ЛР1.md). Код создавался с помощью ChatGPT и основан на анализе [учебного проекта](https://github.com/clgray/Lecture_Examples); код класса-антипримера не скопирован в решение.
