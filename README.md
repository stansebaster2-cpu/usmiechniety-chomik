# 🐹 Uśmiechnięty Chomik — Generator Kołowrotka

**Generator kołowrotka 3D dla chomików** | **3D Hamster Wheel Generator**

> Projekt stworzony z troski o dobrostan chomików. Odpowiednio duże, dobrze zaprojektowane koło może realnie wydłużyć życie i ochronić kręgosłup Twojego zwierzęcia. Budujemy świadomość — dobre koło to nie luksus, to potrzeba każdego chomika.

> *Created out of care for hamster welfare. A properly sized wheel can genuinely extend your hamster's life and protect its spine. Good wheels are not a luxury — they are every hamster's need.*

---

## ✨ Funkcje / Features

- 🖨️ **Generator kół 3D** gotowych do druku (STL, OBJ, PLY)
- 🔩 **Dwa typy podstawy**: bieżnia łożyskowa lub podwieszane koło na śrubie
- 🐾 **Profile gatunków** z zalecanymi wymiarami (chomik syryjski, dżungar, Roborovski, chiński)
- ⚠️ **Walidacja zdrowotna** — ostrzeżenie gdy koło jest za małe dla wybranego gatunku
- 🔧 **Dobór łożysk** z automatycznym promieniem rozmieszczenia
- 🖥️ **Podgląd 3D** w czasie rzeczywistym (Direct3D 11)
- 💾 **Preset system** — zapis i wczytywanie konfiguracji
- 🌐 **PL / EN** — pełna dwujęzyczność
- 📐 Walidacja pola roboczego drukarki

---

## 🛠️ Wymagania / Requirements

- **Windows 10/11** x64
- **.NET 10 SDK** — https://dotnet.microsoft.com/download
- **GPU** z obsługą Direct3D 11
- Visual Studio 2022 lub VS Code z rozszerzeniem C# Dev Kit

---

## 🚀 Uruchomienie / Getting Started

```bash
git clone https://github.com/stansebaster2-cpu/usmiechniety-chomik.git
cd usmiechniety-chomik
dotnet build
dotnet run --project ChomikApp/ChomikApp.csproj
```

---

## 🏗️ Architektura projektu

```
kołowrotek/
├── ChomikApp/
│   ├── Data/           # Gatunki, łożyska, profile V-slot
│   ├── Design/         # Dobór łożysk, kalkulator zdrowia kręgosłupa
│   ├── Dialogs/        # Okna dialogowe (wybór gatunku, O programie)
│   ├── Export/         # Eksport STL/OBJ/PLY
│   ├── Geometry/       # Generator koła i podstawy, import STL
│   ├── Localization/   # System tłumaczeń PL/EN
│   ├── Models/         # Mesh, Vector3
│   ├── Parameters/     # WheelParameters, BaseParameters, PrinterParameters
│   ├── Services/       # DonationService
│   └── ViewModels/     # MainViewModel (MVVM)
├── openscad/           # Skrypty OpenSCAD (referencja)
└── exports/            # Pliki STL wygenerowane przez OpenSCAD
```

---

## 🤝 Jak współtworzyć / How to Contribute

Projekt jest otwartoźródłowy i **aktywnie szuka współpracowników**! Każda pomoc jest mile widziana.

**Obszary gdzie pomoc jest najbardziej potrzebna:**

| Obszar | Opis |
|--------|------|
| 🎨 **UI/UX** | Poprawa interfejsu, lepszy podgląd 3D, animacje |
| 🔩 **Generator podstawy** | Dopracowanie modelu 3D bieżni łożyskowej i wersji podwieszanej |
| 🧮 **Algorytmy** | Lepsza optymalizacja siatki, generowanie wsporników |
| 🌐 **Tłumaczenia** | Dodanie kolejnych języków (DE, FR, CZ...) |
| 🐾 **Baza gatunków** | Dodanie nowych gatunków i weryfikacja danych zdrowotnych |
| 🖨️ **Slicer integration** | Integracja z PrusaSlicer / Bambu Studio |
| 📖 **Dokumentacja** | Wiki, tutoriale, opisy parametrów |
| 🐛 **Bugfixing** | Znajdowanie i naprawianie błędów |

### Kroki / Steps

1. **Fork** repozytorium
2. Utwórz branch: `git checkout -b feature/nazwa-funkcji`
3. Wprowadź zmiany i napisz commit po angielsku
4. Wyślij **Pull Request** z opisem co i dlaczego zmieniłeś

### Zgłaszanie błędów / Reporting bugs

Użyj zakładki [Issues](https://github.com/stansebaster2-cpu/usmiechniety-chomik/issues) i opisz:
- Co zrobiłeś
- Czego się spodziewałeś
- Co się stało zamiast tego
- Wersja systemu i karty graficznej

---

## 📋 Roadmap

- [ ] Generator podstawy — pełny model 3D bieżni łożyskowej
- [ ] Generator podstawy — wersja podwieszana na śrubie
- [ ] Eksport do OpenSCAD (parametryczny)
- [ ] Więcej gatunków gryzoni (szczur, myszka, gerbil)
- [ ] Kalkulator materiału i kosztu druku
- [ ] Automatyczna propozycja łożysk po wpisaniu średnicy
- [ ] Integracja z serwisami 3D (Printables, MakerWorld)
- [ ] Wersja webowa (Blazor?)

---

## 🙏 Podziękowania / Credits

- Stworzony przez **Sebastian Stańczykowski**
- Zbudowany przy pomocy **[Claude](https://claude.ai)** (Anthropic) — asystenta AI
- UI: [ModernWpfUI](https://github.com/Kinnara/ModernWpf)
- 3D rendering: [Vortice.Windows](https://github.com/amerkoleci/vortice.windows) (Direct3D 11)
- MVVM: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)

---

## 📄 Licencja / License

MIT — szczegóły w pliku [LICENSE](LICENSE).
