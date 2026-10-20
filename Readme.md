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
- **Themes**: Hell, Dunkel und eigene Farbpalette mit Live-Vorschau.
- **Schnell hinzufügen** für Animes, die nur Name, Tier und Favorit brauchen.
- Versteckte **SQL-Konsole** (Rechtsklick auf die Versionsnummer) für Fortgeschrittene. Verändernde Befehle werden bestätigt und protokolliert.
- Automatische **Datenbankmigration** mit vorheriger Sicherung.

### Voraussetzungen

- Windows 10 oder 11
- Zum Bauen: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), optional [Inno Setup 6](https://jrsoftware.org/isinfo.php) für den Installer

### Installation

1. Den Installer `setup_Anicluster_v_<Version>.exe` ausführen.
2. Standardmäßig wird nur für den aktuellen Benutzer installiert (keine Administratorrechte nötig). Alternativ ist die Installation für alle Benutzer möglich.
3. Ein Update über eine bestehende Installation behält Datenbank und Einstellungen. Eine ältere Datenbank wird beim ersten Start automatisch migriert und vorher gesichert.

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
| Datenbank | `db\data.sqlite` neben der Anwendung |
| Sicherungen bei Migrationen | neben der Datenbank (`data.legacy-…`, `data.v1-…`) |
| Logs | `logs\` neben der Anwendung (tägliche Rotation) |
| Einstellungen | `%AppData%\Anicluster\settings.json` |

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
- **Themes**: light, dark and a custom palette with live preview.
- **Quick add** for entries that only need a name, tier and favorite flag.
- Hidden **SQL console** (right-click the version number) for advanced users. Modifying statements require confirmation and are logged.
- Automatic **database migration** with a backup beforehand.

### Requirements

- Windows 10 or 11
- To build: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), optionally [Inno Setup 6](https://jrsoftware.org/isinfo.php) for the installer

### Installation (English)

1. Run the installer `setup_Anicluster_v_<version>.exe`.
2. By default it installs for the current user only (no administrator rights needed). Installing for all users is possible as an option.
3. Updating over an existing installation keeps your database and settings. An older database is migrated automatically on first start, after a backup.

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
| Database | `db\data.sqlite` next to the application |
| Migration backups | next to the database (`data.legacy-…`, `data.v1-…`) |
| Logs | `logs\` next to the application (daily rotation) |
| Settings | `%AppData%\Anicluster\settings.json` |

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
InnoSetup/               Installer script
SqlScripts/              Import and demo data, tag initialization
CHANGELOG.md             Change history
```

The project's coding rules (style, MVVM, database) are documented in the *Konventionen* file (German).

### License and icon

The Anicluster icon (`Anicluster/Resources/Icon`) is an original design released into the public domain under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/). No license has been set for the source code yet.