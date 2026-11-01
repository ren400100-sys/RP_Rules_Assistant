# RP Rules Assistant

Готовый проект для сборки одного Windows `.exe` через GitHub Actions.

## Как собрать

1. Создай новый репозиторий на GitHub.
2. Загрузи **всё содержимое этой папки**, включая скрытую папку `.github`.
3. Открой вкладку **Actions**.
4. Выбери workflow **Build Windows EXE**.
5. Нажми **Run workflow** → **Run workflow**.
6. После завершения открой выполненный запуск.
7. Внизу страницы в разделе **Artifacts** скачай `RP_Rules_Assistant_Windows`.
8. Внутри архива будет один файл `RP_Rules_Assistant.exe`.

Программа собирается как self-contained `win-x64` приложение. Устанавливать .NET на компьютер для запуска готового EXE не требуется.
