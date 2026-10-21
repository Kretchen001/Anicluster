# Changelog

Alle nennenswerten Änderungen an Anicluster. Das Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.1.0/).
Für Versionen vor 2.1.0 gibt es keine Tags im Verlauf, sie sind deshalb nach Zeiträumen gruppiert.

## [2.3.0] – 2026-10-20

### Hinzugefügt
- **Geschmacksprofil**: Neuer Bereich in der Seitenleiste unter dem Tag-Netzwerk. Aus den gesehenen Animes, die du als Lieblinge wählst, wird berechnet, welche Tags dort auffällig häufiger oder seltener vorkommen als im Rest der Bibliothek. Als Liebling können die Tiers S bis D sowie Favoriten zählen. „Das magst du“ und „Eher nicht dein Ding“ zeigen Balken, Faktor und Begründung, „Zeigen“ filtert die Bibliothek nach dem Tag.
- **Vorschläge zum Profil**: Animes werden nach Passung zum Profil sortiert, mit den passenden Tags. Der Status der bewerteten Animes ist wählbar (Wunschliste, Begonnen, Beendet, Unterbrochen, Abgebrochen oder alle). Zwei Schwellwerte teilen sie in „Passt zu dir“ und „Passt leidlich zu dir“. „In Bibliothek filtern“ übernimmt die stärksten Tags des Profils und den gewählten Status.
- **Zeitstrahl**: Neuer Bereich in der Seitenleiste unter dem Geschmacksprofil. Alle Animes mit eingetragenem Veröffentlichungszeitraum erscheinen als Balken auf einer gemeinsamen Zeitachse: Staffeln einzeln, Unterbrechungen als Lücken, dazu eine Heute-Linie. Die Farbe folgt wahlweise dem Tier oder dem Status.
  - Zoom per Regler oder Strg + Mausrad, waagerechtes Scrollen per Scrollbalken oder Umschalt + Mausrad. Jahresleiste und Namen laufen beim Scrollen mit.
  - Ein Tooltip zeigt beim Überfahren eines Zeitraums oder einer Pause Beginn, Ende und Dauer.
  - Filter nach Namen und nach Tags (alle Tags oder ein Tag genügt). Doppelklick öffnet den Anime. Animes ohne Zeitraum werden nur gezählt.
- **Anime-Netzwerk**: Neuer Bereich in der Seitenleiste unter dem Tag-Netzwerk. Animes sind die Knoten, verbundene Animes rücken zusammen, Cluster erhalten eine gemeinsame Fläche mit Beschriftung (häufigste Tags, Durchschnittsnote oder Anzahl). Drei Netzwerkarten lassen sich umschalten, die Wahl wird gespeichert:
  - **Vorgänger und Nachfolger**: Verbindungen aus den Feldern Vorgänger, Nachfolger und Verwandt. Pfeile zeigen vom Vorgänger zum Nachfolger, gestrichelte Linien verbinden Verwandte. Die Cluster sind die zusammenhängenden Gruppen (Franchises).
  - **Ähnliche Tags**: Je mehr Tags zwei Animes gemeinsam haben, desto ähnlicher sind sie, seltene Tags zählen mehr als häufige (mindestens zwei gemeinsame Tags nötig). Animes ohne Tags erscheinen nicht.
  - **Ähnliche Bewertung**: Ähnlichkeit aus den Teilnoten Story, Animation, Effekte und Akustik, gewichtet wie die Gesamtnote. Unbewertete Animes erscheinen nicht.
  - Regler für Mindestähnlichkeit und die höchste Zahl ähnlicher Nachbarn pro Anime, Schalter für Animes ohne Verbindung (standardmäßig ausgeblendet), Filter nach Status und Hervorheben per Namenssuche.
  - Klick wählt einen Anime und zeigt seine Verbindungen mit Art oder Ähnlichkeit, Doppelklick öffnet ihn im Editor, „In Bibliothek suchen“ springt zur Bibliothek. Zoom per Mausrad zum Mauszeiger oder Regler (bis 800 %, „Einpassen“ zeigt wieder alles), Verschieben des Ausschnitts durch Ziehen auf freier Fläche, jeder Cluster mit eigener Farbe (vom Akzent des Designs abgeleitet), Knoten lassen sich ziehen, „Neu anordnen“ startet das Layout neu.
- `SqlScripts/Demo-Zeitraeume.sql`: Veröffentlichungszeiträume, Staffeln und Pausen für viele Animes der Beispielliste, zum Ausprobieren des Zeitstrahls.

### Geändert
- **Installer**: Das Setup verlangt jetzt Administratorrechte und installiert für alle Benutzer nach „Programme“ (`C:\Program Files\Anicluster`, 64-Bit). Die Auswahl „nur für mich“ entfällt. Anicluster selbst läuft weiterhin ohne Administratorrechte.
  - Datenbank und Logs bleiben im Installationsordner und werden für alle Benutzer beschreibbar angelegt. Alle Benutzer eines Rechners teilen sich damit eine Datenbank, die Einstellungen bleiben pro Benutzer.
  - Eine bisher nur für den Benutzer installierte Version wird nicht automatisch übernommen: Die Datenbank (`%LocalAppData%\Programs\Anicluster\db`) muss nach dem Umstieg in den neuen `db`-Ordner kopiert werden.
  - Beim Deinstallieren gilt die Rückfrage zu den Einstellungen nur für das Benutzerkonto, unter dem die Deinstallation läuft.
- **Tag-Netzwerk**: Zoom per Mausrad zum Mauszeiger oder Regler (bis 800 %, „Einpassen“ zeigt wieder alles) und Verschieben des Ausschnitts durch Ziehen auf freier Fläche. Eng zusammenhängende Tags liegen auf einer Fläche in eigener Farbe (vom Akzent des Designs abgeleitet), Strg + Mausrad ist nicht mehr nötig.

### Hinweis
- Die Datenbankstruktur bleibt bei Version 2, ein Update braucht keine Migration.

## [2.2.0] – 2026-10-20

### Hinzugefügt
- **Bilder**: Jeder Anime kann ein Cover erhalten. Im Editor lässt es sich per Dateiauswahl, aus der Zwischenablage oder per Drag and Drop einfügen und wieder entfernen. Das Bild wird verkleinert (längste Seite höchstens 1024 Pixel) in der Datenbank gespeichert.
- **Bilder-Ansicht** in der Bibliothek neben Karten und Tabelle: Kacheln mit Vorschaubild, Tier und Favorit auf dem Bild, darunter Name und Status. Animes ohne Bild zeigen einen Platzhalter. Die Auswahl der Ansicht wird gespeichert.
- Link zum Projekt auf GitHub über der Versionsnummer in der Seitenleiste.
- **Anicluster-Symbol** für Fenster, Programmdatei, Seitenleiste und Installer (eigene Gestaltung, gemeinfrei unter CC0).

### Geändert
- **Datenbank**: Struktur Version 2 mit neuer Tabelle für die Bilder. Eine vorhandene Datenbank wird beim ersten Start automatisch ergänzt, vorher entsteht eine Sicherung neben der Datenbankdatei (`data.v1-<Zeitstempel>.sqlite`). Eine Datenbank der neuen Version lässt sich mit älteren Programmversionen nicht mehr öffnen.

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
