# Changelog

Alle nennenswerten Änderungen an Anicluster. Das Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.1.0/).
Für Versionen vor 2.1.0 gibt es keine Tags im Verlauf, sie sind deshalb nach Zeiträumen gruppiert.

## [2.1.0] – 2026-10-20

### Hinzugefügt
- **Tag-Netzwerk**: Tags werden als kräftegesteuertes Netzwerk dargestellt. Die Schriftgröße folgt der Häufigkeit, Tags, die oft gemeinsam vorkommen, liegen näher beieinander. Knoten lassen sich ein- und ausschalten und auswählen, die passenden Animes erscheinen neben dem Netzwerk und können als Tag-Filter in die Bibliothek übernommen werden.
- **Neue Oberfläche** mit Seitenleiste (Bibliothek, Tags, Tag-Netzwerk, Einstellungen).
- **Themes**: Hell, Dunkel und eine eigene Palette aus vier Farben mit Live-Vorschau, dazu ein dunkler Fenstertitel. Die Auswahl wird gespeichert.
- **Bibliothek** wahlweise als Karten oder Tabelle mit Suche, Filtern und Sortierung.
- **Versteckte SQL-Konsole** per Rechtsklick auf die Versionsnummer: führt SQL aus, zeigt das Ergebnis als Tabelle und fragt bei verändernden Befehlen nach.
- Anime können direkt aus der Liste gelöscht werden. Soundtrack-Stücke werden im Bewertungseditor gespeichert.
- Hilfs-Scripts im Ordner `SqlScripts` für Import und Demodaten.

### Geändert
- **Architektur auf MVVM umgestellt**: ViewModels, wiederverwendbare Views und ein Dialog-Service ersetzen die bisherigen Fenster. Anlegen und Bearbeiten eines Animes nutzen denselben Editor.
- **Datenbank**: versioniertes Schema mit automatischer Migration (inklusive Sicherung), Transaktionen pro Vorgang, Fremdschlüsseln und Kaskadenlöschung. Die Services laden und speichern einen kompletten Anime atomar, die Datenbankinitialisierung ist ausgelagert.
- **Installer** modernisiert: moderner Assistent, Installation pro Benutzer oder für alle, Version automatisch aus der Anwendung, beschreibbare Daten- und Log-Ordner, Rückfrage beim Deinstallieren. Das Umbenennen per Skript entfällt.

## [2.0.x] – Februar 2025 bis März 2026

### Hinzugefügt
- OVA- und Film-Verwaltung mit Hinzufügen, Löschen und Sortieren per Drag and Drop (März 2026).
- Fließender Farbverlauf der Fortschrittsbalken in Bewertungsdialogen (März 2026).
- Platzhaltertexte in Kommentarfeldern (März 2026).
- Aktualisierungsfunktion für Animes, Tiefenkopie von Animes und Vergleichslogik, Bearbeitungsfenster (April und September 2025).
- Tag-Suche (April 2025).
- Link zum Anime einfügbar, Sortierung auf- und absteigend in der Hauptansicht (Februar 2025).

### Geändert
- Der Editor ist ein eingebettetes Steuerelement statt eines eigenen Fensters, Zahleneingabe mit Startwert (März 2026).
- Der Editor zeigt geladene Daten an und reagiert auf die erste Änderung (Dezember 2025).
- Bewertung, Akustik (Soundtrack, Opening, Ending, Syncro), Status, Medieninfos, Staffeln und Tags werden aus der Datenbank geladen (Februar 2025).
- Veröffentlichungs- und Installer-Prozess vereinfacht (Januar bis April 2025).

### Behoben
- Tags verschwanden beim Bearbeiten eines Animes (März 2026).
- Fehler beim Laden der Oberfläche (Dezember 2025).
- Falscher Tabellenname bei Syncro, Typumwandlungen im Bewertungsdienst (März 2026).
- Einfügen von Video-Animationen bei Staffeln (März 2025).
- Syncros wurden nicht korrekt ersetzt, Hauptfenster aktualisiert sich nach dem Hinzufügen (Februar 2025).

### 2.0.4
- Titelbildschirm zeigt die Version.
- Popup informiert über das Entfernen per Rechtsklick im Bewertungsbildschirm.

### 2.0.3
- Eingetragene Openings, Endings und Syncros lassen sich entfernen.
- Bewertungsbildschirm mit besserer Buttonbeschriftung.
- Die Berechnung erlaubt es, Opening, Ending und Syncros nicht einzutragen, sie werden dann nicht gewichtet.

## Frühe Entwicklung – November 2024 bis Januar 2025

- Grundlage: Einfügen von Animes samt Tags, Hauptfenster mit Sternewertung, Tag-Auswahl und SQL-Script für die Tag-Tabelle.
- Formeldarstellung der Berechnung mit WpfMath (Januar 2025).
- Fenster für Veröffentlichungszeiträume und Unterbrechungen, alphabetisch sortierte Tags.
- Fehlerbehebungen bei Initialisierung, Tag-Bindung und Eingabe von Unterbrechungen.
