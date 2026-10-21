# Anicluster

<img src="Anicluster/Resources/Icon/Anicluster-64.png" alt="Anicluster icon" width="64" align="right">

**Inhalt / Contents**

- [Deutsch](#deutsch)
  - [Überblick](#überblick)
  - [Funktionen](#funktionen)
  - [Voraussetzungen](#voraussetzungen)
  - [Installation](#installation)
  - [Bauen und Veröffentlichen](#bauen-und-veröffentlichen)
  - [Daten und Speicherorte](#daten-und-speicherorte)
  - [Projektstruktur](#projektstruktur)
  - [Lizenz und Icon](#lizenz-und-icon)
- [English](#english)
  - [Overview](#overview)
  - [Features](#features)
  - [Requirements](#requirements)
  - [Installation (English)](#installation-english)
  - [Build and publish](#build-and-publish)
  - [Data and file locations](#data-and-file-locations)
  - [Project structure](#project-structure)
  - [License and icon](#license-and-icon)

---

## Deutsch

[⬆ Nach oben](#anicluster) · [Switch to English](#english)

### Überblick

Anicluster ist eine Windows-Desktopanwendung (WPF, .NET 8), mit der du deine Anime-Sammlung verwaltest: Bewertungen, Staffeln, OVAs, Tags, Status und Cover an einem Ort. Die Daten liegen lokal in einer SQLite-Datenbank, es ist kein Konto und keine Internetverbindung nötig.

### Funktionen

- **Bibliothek** mit Suche, Filtern (Status, Tier, Favoriten, Tags) und Sortierung. Die Darstellung wechselt zwischen **Karten**, **Tabelle** und **Bildern** (Kacheln mit Cover). Die Auswahl wird gespeichert.
- **Editor** zum Anlegen und Bearbeiten: Name, Originalname, Link, Status, Tier, Favorit, Empfehlung, Kommentar, Vorgänger/Nachfolger/Verwandtes, Medieninfos und Veröffentlichungszeitraum.
- **Bewertung** für Story, Animation, Effekte und Akustik (Soundtrack, Openings, Endings, Syncros) mit Gesamtwertung.
- **Staffeln und OVAs/Filme** mit Sortierung per Drag and Drop.
- **Bilder**: Cover per Dateiauswahl, aus der Zwischenablage oder per Drag and Drop einfügen. Bilder werden verkleinert in der Datenbank gespeichert, dazu entsteht eine zugeschnittene Vorschau (2:3).
- **Tags** verwalten und im **Tag-Netzwerk** ansehen: Die Größe folgt der Häufigkeit, oft gemeinsam vorkommende Tags liegen näher beieinander. Die Auswahl lässt sich als Filter in die Bibliothek übernehmen.
- **Geschmacksprofil**: Wertet aus, welche Tags bei deinen Lieblingen (Tier S bis D, Favoriten) auffällig oft oder selten vorkommen, und schlägt dazu passende Animes vor, wahlweise nach Status (z. B. Wunschliste). Zwei Schwellwerte trennen „passt zu dir“ von „passt leidlich zu dir“.
- **Zeitstrahl**: Zeigt alle Animes mit Veröffentlichungszeitraum als Balken auf einer Zeitachse, mit Staffeln, Unterbrechungen und Heute-Linie. Zoom (auch Strg + Mausrad), waagerechtes Scrollen, Tooltips mit Beginn, Ende und Dauer sowie Filter nach Name und Tags.
- **Anime-Netzwerk**: Animes als Knoten in drei Netzwerkarten, zwischen denen du umschaltest: **Vorgänger und Nachfolger** (aus den Feldern Vorgänger, Nachfolger, Verwandt, mit Pfeilen), **ähnliche Tags** (seltene gemeinsame Tags zählen mehr) und **ähnliche Bewertung** (Story, Animation, Effekte, Akustik). Cluster liegen auf einer gemeinsamen Fläche mit Beschriftung. Regler für Mindestähnlichkeit und Nachbarn pro Anime, Statusfilter, Namenssuche, Zoom (auch Strg + Mausrad). Ein Klick zeigt die Verbindungen des Animes, ein Doppelklick öffnet ihn.
- **Themes**: Hell, Dunkel und eigene Farbpalette mit Live-Vorschau.
- **Schnell hinzufügen** für Animes, die nur Name, Tier und Favorit brauchen.
- Versteckte **SQL-Konsole** (Rechtsklick auf die Versionsnummer) für Fortgeschrittene. Verändernde Befehle werden bestätigt und protokolliert.
- Automatische **Datenbankmigration** mit vorheriger Sicherung.

### Voraussetzungen

- Windows 10 oder 11 (64-Bit), zum Installieren sind Administratorrechte nötig
- Zum Bauen: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), optional [Inno Setup 6.3 oder neuer](https://jrsoftware.org/isinfo.php) für den Installer

### Installation

1. Den Installer `setup_Anicluster_v_<Version>.exe` ausführen.
2. Der Installer verlangt Administratorrechte und installiert für alle Benutzer nach `C:\Program Files\Anicluster`. Anicluster selbst läuft ohne Administratorrechte. Datenbank und Logs liegen im Installationsordner und sind für alle Benutzer beschreibbar, die Datenbank wird daher von allen Benutzern des Rechners gemeinsam genutzt. Die Einstellungen bleiben pro Benutzer.
3. Ein Update über eine bestehende Installation behält Datenbank und Einstellungen. Eine ältere Datenbank wird beim ersten Start automatisch migriert und vorher gesichert.
4. Umstieg von einer früheren Installation nur für den aktuellen Benutzer: Sie wird nicht automatisch übernommen. Nach der neuen Installation `data.sqlite` aus `%LocalAppData%\Programs\Anicluster\db` in `C:\Program Files\Anicluster\db` kopieren (Anicluster vorher schließen) und die alte Installation deinstallieren.

### Bauen und Veröffentlichen

```text
dotnet build Anicluster.sln
dotnet publish Anicluster/Anicluster.csproj -p:PublishProfile=FolderProfile
dotnet publish Anicluster/Anicluster.csproj -p:PublishProfile=FolderProfile -p:BuildSetup=true
```

- Der zweite Befehl veröffentlicht nach `Anicluster\bin\Publish`, der dritte baut zusätzlich den Installer (Inno Setup 6 muss installiert sein).
- Die Version steht nur in `Anicluster/Anicluster.csproj` (`<Version>`). Daraus entstehen Dateiversion, Anzeige in der Seitenleiste und Installername.
- Release-Ablauf: Version erhöhen → `CHANGELOG.md` und `InnoSetup/ReadMe.txt` ergänzen → veröffentlichen → Installer bauen.

### Daten und Speicherorte

| Was | Wo |
|---|---|
| Datenbank | `db\data.sqlite` neben der Anwendung (installiert: `C:\Program Files\Anicluster\db`, für alle Benutzer gemeinsam) |
| Sicherungen bei Migrationen | neben der Datenbank (`data.legacy-…`, `data.v1-…`) |
| Logs | `logs\` neben der Anwendung (tägliche Rotation) |
| Einstellungen | `%AppData%\Anicluster\settings.json` (pro Benutzer) |

Die Datenbank hat derzeit die Struktur **Version 2** (`PRAGMA user_version`). Neuere Datenbanken lassen sich mit älteren Programmversionen nicht öffnen.

### Projektstruktur

```text
Modells/                 Datenklassen (Anime, Tag, Rating, AnimeImage, …), keine Logik
Anicluster/
  Database/              DatabaseManager, DatabaseSession, Schema, Services
  ViewModels/            Zustand und Logik (MVVM, ohne Framework und ohne Commands)
  Views/                 UserControls (XAML + schlanker Code-Behind)
  Dialogs/               Dialog-Dienst und Dialogfenster
  Imaging/               Bildaufbereitung (Verkleinern, Vorschau erzeugen)
  Resources/             Styles, Templates, Icon
  Theming/ Settings/     Themes und Einstellungen
  Converters/            Wertkonverter
  TagNetwork/            Layout-Simulation des Tag-Netzwerks (reines C#)
  TasteProfile/          Berechnung des Geschmacksprofils (reines C#)
  Timeline/              Berechnung des Zeitstrahl-Layouts (reines C#)
  AnimeNetwork/          Verbindungen und Cluster des Anime-Netzwerks (reines C#)
InnoSetup/               Installer-Skript
SqlScripts/              Import- und Demodaten, Tag-Initialisierung
CHANGELOG.md             Änderungsverlauf
```

Die Coding-Regeln des Projekts (Stil, MVVM, Datenbank) stehen in der Datei *Konventionen*.

### Lizenz und Icon

Das Anicluster-Icon (`Anicluster/Resources/Icon`) ist eine eigene Gestaltung und gemeinfrei unter [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/deed.de). Eine Lizenz für den Quellcode ist hier noch nicht festgelegt.

---

## English

[⬆ Back to top](#anicluster) · [Zu Deutsch wechseln](#deutsch)

### Overview

Anicluster is a Windows desktop application (WPF, .NET 8) for managing your anime collection: ratings, seasons, OVAs, tags, status and cover images in one place. Data is stored locally in a SQLite database; no account and no internet connection are required.

### Features

- **Library** with search, filters (status, tier, favorites, tags) and sorting. Switch between **cards**, **table** and **images** (tiles with covers). Your choice is remembered.
- **Editor** for creating and editing entries: name, original name, link, status, tier, favorite, recommended by, comment, predecessor/successor/related, media info and publishing period.
- **Rating** for story, animation, effects and acoustics (soundtrack, openings, endings, dubs) with an overall score.
- **Seasons and OVAs/movies**, reorderable by drag and drop.
- **Images**: add a cover via file picker, from the clipboard or by drag and drop. Images are downscaled and stored in the database, together with a cropped 2:3 thumbnail.
- **Tags** management and the **tag network**: size follows frequency, tags that often appear together sit closer. A selection can be applied as a filter in the library.
- **Taste profile**: analyses which tags appear unusually often or rarely among your favourites (tier S to D, favourites) and suggests matching animes, optionally by status (e.g. wishlist). Two thresholds separate "fits you" from "fits you somewhat".
- **Timeline**: shows all animes with a publishing period as bars on a time axis, including seasons, interruptions and a today line. Zoom (also Ctrl + mouse wheel), horizontal scrolling, tooltips with start, end and duration, and filters by name and tags.
- **Anime network**: animes as nodes in three network types you can switch between: **predecessor and successor** (from the predecessor, successor and related fields, with arrows), **similar tags** (rare shared tags count more) and **similar ratings** (story, animation, effects, acoustics). Clusters sit on a shared background with a caption. Sliders for minimum similarity and neighbors per anime, status filter, name search, zoom (also Ctrl + mouse wheel). A click shows the connections of an anime, a double click opens it.
- **Themes**: light, dark and a custom palette with live preview.
- **Quick add** for entries that only need a name, tier and favorite flag.
- Hidden **SQL console** (right-click the version number) for advanced users. Modifying statements require confirmation and are logged.
- Automatic **database migration** with a backup beforehand.

### Requirements

- Windows 10 or 11 (64-bit); administrator rights are required to install
- To build: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), optionally [Inno Setup 6.3 or newer](https://jrsoftware.org/isinfo.php) for the installer

### Installation (English)

1. Run the installer `setup_Anicluster_v_<version>.exe`.
2. The installer requires administrator rights and installs for all users into `C:\Program Files\Anicluster`. Anicluster itself runs without administrator rights. The database and logs live in the installation folder (writable for all users), so all users of the computer share one database. Settings stay per user.
3. Updating over an existing installation keeps your database and settings. An older database is migrated automatically on first start, after a backup.
4. Switching from an earlier per-user installation: it is not taken over automatically. After the new installation, copy `data.sqlite` from `%LocalAppData%\Programs\Anicluster\db` to `C:\Program Files\Anicluster\db` (close Anicluster first) and uninstall the old installation.

### Build and publish

```text
dotnet build Anicluster.sln
dotnet publish Anicluster/Anicluster.csproj -p:PublishProfile=FolderProfile
dotnet publish Anicluster/Anicluster.csproj -p:PublishProfile=FolderProfile -p:BuildSetup=true
```

- The second command publishes to `Anicluster\bin\Publish`; the third also builds the installer (Inno Setup 6 must be installed).
- The version lives only in `Anicluster/Anicluster.csproj` (`<Version>`). File version, sidebar label and installer name derive from it.
- Release flow: bump version → update `CHANGELOG.md` and `InnoSetup/ReadMe.txt` → publish → build installer.

### Data and file locations

| What | Where |
|---|---|
| Database | `db\data.sqlite` next to the application (installed: `C:\Program Files\Anicluster\db`, shared by all users) |
| Migration backups | next to the database (`data.legacy-…`, `data.v1-…`) |
| Logs | `logs\` next to the application (daily rotation) |
| Settings | `%AppData%\Anicluster\settings.json` (per user) |

The database currently uses **schema version 2** (`PRAGMA user_version`). Newer databases cannot be opened by older program versions.

### Project structure

```text
Modells/                 Data classes (Anime, Tag, Rating, AnimeImage, …), no logic
Anicluster/
  Database/              DatabaseManager, DatabaseSession, schema, services
  ViewModels/            State and logic (MVVM, no framework, no commands)
  Views/                 UserControls (XAML + thin code-behind)
  Dialogs/               Dialog service and dialog window
  Imaging/               Image processing (downscaling, thumbnail creation)
  Resources/             Styles, templates, icon
  Theming/ Settings/     Themes and settings
  Converters/            Value converters
  TagNetwork/            Layout simulation of the tag network (plain C#)
  TasteProfile/          Calculation of the taste profile (plain C#)
  Timeline/              Calculation of the timeline layout (plain C#)
  AnimeNetwork/          Connections and clusters of the anime network (plain C#)
InnoSetup/               Installer script
SqlScripts/              Import and demo data, tag initialization
CHANGELOG.md             Change history
```

The project's coding rules (style, MVVM, database) are documented in the *Konventionen* file (German).

### License and icon

The Anicluster icon (`Anicluster/Resources/Icon`) is an original design released into the public domain under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). No license has been set for the source code yet.