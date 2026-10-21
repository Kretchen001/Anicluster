Anicluster 2.3.0

Installation
- Der Installer verlangt Administratorrechte (Windows fragt beim Start nach) und installiert für alle Benutzer nach
  "Programme" (C:\Program Files\Anicluster). Anicluster selbst startet weiterhin ohne Administratorrechte.
- Datenbank (db) und Logs (logs) liegen im Installationsordner und werden für alle Benutzer beschreibbar angelegt.
  Alle Benutzer dieses Rechners arbeiten daher mit derselben Datenbank. Die Einstellungen (Farben, Ansicht) bleiben
  pro Benutzer unter %AppData%\Anicluster.
- Ein Update über eine vorhandene Installation behält Datenbank und Einstellungen. Eine ältere Datenbank wird beim
  ersten Start automatisch migriert und vorher gesichert.
- Umstieg von einer Installation nur für den aktuellen Benutzer: Diese Installation wird nicht automatisch übernommen.
  Anicluster schließen, nach der neuen Installation die Datei data.sqlite aus
  %LocalAppData%\Programs\Anicluster\db in C:\Program Files\Anicluster\db kopieren und die alte Installation
  danach über "Apps und Features" deinstallieren (Daten dabei behalten).

Neu in 2.3.0
- Der Installer verlangt jetzt Administratorrechte und installiert für alle Benutzer nach "Programme"
- Geschmacksprofil: zeigt, welche Tags bei deinen Lieblingen (Tier S bis D, Favoriten) auffällig oft oder selten vorkommen
- Animes mit wählbarem Status werden nach Passung vorgeschlagen ("Passt zu dir" und "Passt leidlich zu dir" über zwei
  Schwellwerte) und lassen sich in der Bibliothek filtern
- Zeitstrahl: Animes nach Veröffentlichungszeitraum auf einer Zeitachse, mit Staffeln, Unterbrechungen und Heute-Linie,
  Zoom, waagerechtem Scrollen, Tooltips und Filter nach Name und Tags
- Anime-Netzwerk: Animes als Netzwerk in drei Arten (Vorgänger/Nachfolger/Verwandt, ähnliche Tags, ähnliche Bewertung)
  mit Clustern, Reglern für die Ähnlichkeit, Auswahl mit Verbindungen und Zoom
- Die Datenbankstruktur bleibt unverändert, es ist keine Migration nötig

Neu in 2.2.0
- Bilder: Jeder Anime kann ein Cover erhalten (Datei wählen, aus der Zwischenablage einfügen oder per Drag and Drop)
- Neue Bibliotheksansicht "Bilder" mit Vorschaubildern, neben Karten und Tabelle
- Neues Anicluster-Symbol für Fenster, Programmdatei und Installer
- Link zum Projekt auf GitHub über der Versionsnummer in der Seitenleiste
- Die Datenbank wird beim ersten Start automatisch auf die neue Struktur gebracht und vorher gesichert

Neu in 2.1.0
- Neue Oberfläche mit Seitenleiste (Bibliothek, Tags, Tag-Netzwerk, Einstellungen)
- Hell, Dunkel und eigene Farbpalette mit Live-Vorschau
- Bibliothek als Karten oder Tabelle, mit Suche, Filter und Sortierung
- Tag-Netzwerk: Tags als Netzwerk nach Häufigkeit und gemeinsamem Auftreten, Auswahl zeigt passende Animes
  und lässt sich als Tag-Filter in die Bibliothek übernehmen
- Neues Datenbankschema mit automatischer Migration, Transaktionen und Fremdschlüsseln
- Versteckte SQL-Konsole (Rechtsklick auf die Versionsnummer)

Den vollständigen Verlauf enthält die Datei CHANGELOG.md.
Projektseite: https://github.com/Kretchen001/Anicluster
