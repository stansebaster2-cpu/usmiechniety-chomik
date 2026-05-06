# Jak współtworzyć projekt / How to Contribute

Dziękujemy za zainteresowanie projektem! Poniżej znajdziesz wszystko co potrzebujesz aby zacząć.

*Thank you for your interest! Below you will find everything you need to get started.*

---

## Środowisko deweloperskie / Dev Environment

### Wymagania
- Windows 10/11 x64
- .NET 10 SDK (https://dotnet.microsoft.com/download)
- Visual Studio 2022 **lub** VS Code + rozszerzenie `ms-dotnettools.csdevkit`
- Git

### Pierwsze uruchomienie
```bash
git clone https://github.com/stansebaster2-cpu/usmiechniety-chomik.git
cd usmiechniety-chomik
dotnet restore
dotnet build
dotnet run --project ChomikApp/ChomikApp.csproj
```

---

## Konwencje kodu / Code Conventions

- Język kodu: **angielski** (nazwy zmiennych, metod, komentarze)
- Nazewnictwo: **PascalCase** dla klas/metod, **camelCase** dla zmiennych lokalnych
- MVVM pattern — logika w ViewModels, nie w code-behind
- Nowe teksty UI zawsze przez `Strings.cs` (obsługa PL/EN)
- Brak magicznych liczb — stałe w parametrach lub klasach danych
- Komentarze tylko gdy logika jest nieoczywista

---

## Struktura Pull Requesta

Tytuł: krótki, po angielsku, zaczyna się czasownikiem (`Add`, `Fix`, `Improve`, `Remove`)

Opis powinien zawierać:
- **Co** zostało zmienione
- **Dlaczego** (problem, motywacja)
- Screenshoty jeśli zmiana dotyczy UI

---

## Zgłaszanie Issues

Zanim zgłosisz — sprawdź czy podobny issue już nie istnieje.

Dobre zgłoszenie błędu zawiera:
1. Kroki do reprodukcji
2. Oczekiwane zachowanie
3. Rzeczywiste zachowanie
4. Wersja systemu i karty graficznej
5. Screenshot lub logi (folder `logs/` obok .exe)

Propozycje funkcji? Użyj labela `enhancement` i opisz **po co** ta funkcja — nie tylko co ma robić.

---

## Pytania?

Otwórz [Discussion](https://github.com/stansebaster2-cpu/usmiechniety-chomik/discussions) lub Issue z labelką `question`. Chętnie pomożemy!
