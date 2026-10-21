# Anicluster – Konventionen

Gilt für C#, XAML, WPF, MVVM und die Datenbankschicht. Die Regeln beschreiben den Stand von Version 2.3.0.
Bei neuem Code gilt: Erst nachsehen, wie es im Projekt bereits gelöst ist, und dem folgen.

## 1. Grundprinzipien

- **Sprache:** Kommentare und Dokumentation auf Deutsch, alle Bezeichner (Klassen, Member, Variablen) auf Englisch. Texte in der Oberfläche sind Deutsch.
- **Kommentare sparsam:** Eine `<summary>` pro Klasse und pro nicht offensichtlichem Member. Inline-Kommentare erklären das Warum, nicht das Was.
- **Keine Frameworks für Selbstverständliches:** kein Entity Framework, kein DI-Container, kein MVVM-Toolkit, kein RelayCommand. Das Verhalten der Datenbank und der Verdrahtung bleibt sichtbar.
- **Modelle bleiben POCOs:** `Modells` enthält keine Logik für Oberfläche oder Datenbank. Ihre Struktur ändert sich nur bewusst.
- **Eine Verantwortung pro Klasse:** Views stellen dar, ViewModels halten Zustand und Logik, Services lesen und schreiben Daten.
- **Online-Zugriffe sind optional:** Die Anwendung muss ohne Netz und ohne Konto vollständig nutzbar bleiben. Netzwerkzugriffe passieren nur auf ausdrücklichen Knopfdruck des Benutzers, immer einzeln und nie im Hintergrund oder als Massenabruf.

## 2. Projektstruktur

```
Modells/                  POCOs und einfache Records (Anime, Tag, TagUsage, AnimeListEntry, TimelineEntry, AnimeImage …)
Anicluster/
  Database/               DatabaseManager, DatabaseSession, Schema, Initializer
    Services/             I…Service + Implementierung, interne Teil-Services
  ViewModels/             ein ViewModel pro Datei, plus PropertyBaseChange und DialogViewModelBase
  Views/                  UserControls (je .xaml + .xaml.cs), Hilfsklassen für Views
  Dialogs/                DialogService, DialogWindow
  Backup/                 Export und Import der Bibliothek als JSON-Datensicherung, ohne WPF
  Imaging/                Bildaufbereitung (Verkleinern, Vorschau erzeugen), zustandslos
  Metadata/               Online-Metadaten (AniList, Jikan, Kitsu): Clients, Ergebnisse, Textaufbereitung, ohne WPF
  Resources/              Controls.xaml, Components.xaml, DataTemplates.xaml, TierColors.xaml
    Icon/                 Anwendungssymbol (SVG, PNG, ICO) mit Lizenzhinweis
  Theming/ Settings/      Themes, Einstellungen (settings.json in %AppData%\Anicluster)
  Converters/             ValueConverter
  TagNetwork/             Layout-Simulation (reines C#, ohne WPF-Abhängigkeit)
  TasteProfile/           Berechnung des Geschmacksprofils (reines C#)
  Timeline/               Layout-Berechnung des Zeitstrahls (reines C#)
  AnimeNetwork/           Verbindungen, Ähnlichkeiten und Cluster des Anime-Netzwerks (reines C#)
InnoSetup/                Installer (Skript, ReadMe)
SqlScripts/               Import- und Demodaten, Tag-Initialisierung
CHANGELOG.md              Änderungsverlauf
README.md                 Überblick auf Deutsch und Englisch
```

- Eine Klasse pro Datei, Dateiname gleich Klassenname.
- Kleine, zusammengehörige Typen (Records, Enums mit Hilfstypen) dürfen in einer Datei stehen, wenn die Datei nach dem Hauptthema benannt ist (`TagUsage.cs` enthält `TagUsage` und `TagLink`).
- Namespaces folgen dem Ordner: `Anicluster.ViewModels`, `Anicluster.Views`, `Anicluster.Database.Services`, `Anicluster.Imaging`, `Anicluster.Metadata`.
- Reine Berechnungen (Layout, Ähnlichkeit, Textaufbereitung, API-Antworten lesen) liegen in Ordnern **ohne WPF-Abhängigkeit**, damit sie ohne Oberfläche prüfbar bleiben.

## 3. Dateiformat

- Dateien in **UTF-8 mit BOM** und **CRLF** (gilt für `.cs`, `.xaml`, `.csproj`, `.iss`, `.sql`, `.md`, `.txt`).
- Einrückung 4 Leerzeichen in C# und XAML, in `.csproj` Tabs wie bisher.
- Namespaces **mit geschweiften Klammern** (block-scoped), nicht als file-scoped `namespace X;`. Der Inhalt wird eingerückt, nach der öffnenden Klammer folgt eine Leerzeile.
- Klammern immer setzen, auch bei einzeiligen `if`. Die öffnende Klammer steht in derselben Zeile. `else`, `else if` und `catch` beginnen in einer **neuen Zeile** nach der schließenden Klammer (kein `} else {`).
- Maximal etwa 140 Zeichen pro Zeile, lange Parameterlisten umbrechen (ein Parameter pro Zeile, schließende Klammer eigene Zeile).
- Binärdateien (Bilder, Icons) sind davon ausgenommen. Beim Bearbeiten per Skript müssen BOM und CRLF erhalten bleiben.

## 4. Benennung

| Element | Regel | Beispiel |
|---|---|---|
| Klasse, Record, Enum, Interface | PascalCase, Interface mit `I` | `LibraryViewModel`, `IAnimeService` |
| Öffentliche Property, Methode, Event | PascalCase | `SelectedTier`, `RequestEdit()` |
| Privates Feld | `_camelCase` | `_dialogService` |
| Lokale Variable, Parameter | camelCase | `animeToInsert` |
| Konstante, statisches Readonly | PascalCase | `MinValue` |
| Event | Verb in der Vergangenheit oder Anfrage | `EditRequested`, `CloseRequested` |
| Async-Methode (falls nötig) | Suffix `Async` | `LoadAsync` |

- ViewModels enden auf `ViewModel`, Views heißen wie ihr ViewModel ohne diese Endung plus `View` (`TagNetworkViewModel` → `TagNetworkView`). Eine zweite Darstellung desselben ViewModels bekommt einen eigenen Namen (`AnimeListItemViewModel` → `AnimeListItemView` als Karte, `AnimeTileView` als Kachel der Bilder-Ansicht).
- Booleans lesen sich als Aussage: `IsModified`, `HasTags`, `IsFavorite`, `HasImage`.
- Aufzählungswerte sind englisch und im Singular (`State.Wishlist`). Die deutsche Beschriftung liefert das ViewModel (`TierText`, `StateText`).
- Click-Handler in Views: `<Steuerelement>_<Ereignis>`, zum Beispiel `BtnOk_Click`, `TBInputNumber_KeyDown`, `ImageCard_Drop`. Steuerelemente, die der Code-Behind braucht, bekommen ein `x:Name` mit Präfix (`Btn`, `TB`, `LB`).

## 5. C#-Stil

- **Explizite Typen statt `var`** (`List<Tag> tags = [];`). Collection Expressions (`[]`) und Target-typed `new` sind erlaubt.
- **`this.` vor jedem Instanzmember** (Feld, Property, Methode, Event). Statische Member ohne Präfix.
- **Nullable ist aktiv.** Nullable Typen werden bewusst gesetzt (`string?`), `!` wird nicht verwendet. Erwartbares `null` wird geprüft und behandelt (Pattern Matching statt Annahme).
- **Records** für unveränderliche Datenträger (`AnimeListEntry`, `TagUsage`), **Klassen** für Zustand.
- Öffentliche Collections sind `ObservableCollection<T>` mit `{ get; }` und werden nicht ersetzt, sondern geleert und gefüllt.
- Properties, die sich nach außen ändern, nutzen `SetProperty` der Basisklasse. Abgeleitete Properties (zum Beispiel `ResultsText`) melden sich mit `OnPropertyChanged(nameof(...))`.
- Mehrere Rückgabewerte: Records oder Tupel mit Namen, keine `out`-Parameter außer bei `TryParse`-artigen Methoden.
- LINQ ist erlaubt, wenn es lesbar bleibt. In Schleifen mit großen Datenmengen (Simulation, Rendering) bewusst auf Allokationen achten.
- Kein `async void` außer in Ereignis-Handlern, die nur weiterleiten. Lange Arbeit und Netzwerkzugriffe gehören nicht in den UI-Thread, sondern laufen als `Task` mit `CancellationToken`.
- Ausnahmen werden spezifisch gefangen (mehrere `catch`-Blöcke statt `catch (Exception)`), siehe Abschnitt 11.

## 6. MVVM

### 6.1 Aufgabenverteilung

- **Model:** reine Daten (`Modells`).
- **ViewModel:** Zustand, Validierung, Logik, Navigation. **Kennt keine Fenster, Controls oder Brushes.** Kein `MessageBox`, kein `Clipboard`, kein `OpenFileDialog`, kein `Application.Current` außer im `DialogService`. Bilddaten reicht das ViewModel als `byte[]` weiter, nie als `BitmapImage`.
- **View:** `UserControl` mit XAML. Der Code-Behind enthält nur Weiterleitungen und rein visuelles Verhalten (Fokus, Drag and Drop, Mausinteraktion, Zwischenablage lesen und die Daten übergeben).

### 6.2 Basisklasse und Zustand

- Alle ViewModels erben von `PropertyBaseChange`. Dialoge erben von `DialogViewModelBase` (liefert `Title`, `FillsOwnerWindow`, `CloseRequested`, `RequestClose(bool)`).
- Zustand, der den Benutzer etwas ändern lässt, hat eine Property. Zustand, der nur berechnet ist, hat eine schreibgeschützte Property oder `=>`-Ausdruck.
- Schwere Daten werden **lazy** geladen: Eine Property liest beim ersten Zugriff und merkt sich das Ergebnis (`AnimeListItemViewModel.Thumbnail`). Listen tragen nur schlanke Angaben (`HasImage`), nicht die Daten selbst.

### 6.3 Aktionen ohne Commands

Dieses Projekt verwendet **keine ICommand/RelayCommand**. Stattdessen:

1. Das ViewModel bietet eine **öffentliche Methode** (`Confirm()`, `Execute()`, `RequestEdit()`, `SelectImage()`).
2. Die View hat einen **Click-Handler**, der nur diese Methode aufruft:

```csharp
private NumberInputViewModel? ViewModel => this.DataContext as NumberInputViewModel;

private void BtnOk_Click(object sender, RoutedEventArgs e) {
    this.ViewModel?.Confirm();
}
```

- Der Handler enthält keine Logik, keine Prüfung, keine Berechnung.
- Tastaturkürzel: `CommandBinding` auf Fensterebene mit `ApplicationCommands` (z. B. Strg+N) oder `KeyDown`-Handler, die ebenfalls nur weiterleiten.

### 6.4 Ereignisse vom ViewModel nach oben

- Ein Kind-ViewModel meldet sich über ein **Event** beim Elternteil (`EditRequested`, `DeleteRequested`). Das Elternteil abonniert in seinem Konstruktor. So kennt das Kind sein Elternteil nicht.
- ViewModels mit Timern, Abonnements oder Simulationen implementieren `IDisposable`. `MainViewModel.CurrentPage` ruft beim Seitenwechsel `Dispose` auf.

### 6.5 Navigation

- Die aktuelle Seite ist ein ViewModel (`CurrentPage`), die View dazu liefert das `DataTemplate` aus `Resources/DataTemplates.xaml`. Neue Seite: ViewModel anlegen, View anlegen, `DataTemplate` ergänzen, Eintrag in `NavigationTarget` und Seitenleiste.
- Wechsel mit ungespeicherten Änderungen fragt vorher nach (`ConfirmLeaveEditor`).
- Die Bibliothek bietet mehrere Darstellungen (`LibraryViewMode`: Karten, Tabelle, Bilder). Eine neue Darstellung = Wert im Enum, `IsXxxView`-Property im ViewModel, Umschalter in der View. Die Auswahl wird in den Einstellungen gespeichert.

### 6.6 Dialoge

- Dialoge sind **ViewModel + UserControl**, angezeigt vom `IDialogService.ShowDialog(viewModel)` im generischen `DialogWindow`. Das Ergebnis liest der Aufrufer aus dem ViewModel (`viewModel.Result`).
- Einfache Rückfragen und Meldungen: `Confirm`, `ShowMessage`, `RequestNumber` des Dialog-Service. Dateiauswahl: `PickImageFile` (liefert den Pfad oder `null`).
- Ein neuer Dialog: `…ViewModel : DialogViewModelBase`, `…View`, `DataTemplate`, Aufruf über den Dialog-Service. Das UserControl legt nur seine **Mindestgröße** fest (`MinWidth`/`MinHeight`, keine festen `Width`/`Height`), das `DialogWindow` passt sich an.
- **Größe:** Kleine Eingaben und Rückfragen behalten die Größe ihres Inhalts. Große Dialoge (Editoren, Auswahl, Konsole) überschreiben `FillsOwnerWindow => true`: Das Fenster ist dann etwa 92 % so groß wie das übergeordnete Fenster, nie kleiner als sein Inhalt, mittig darüber und auf dem Bildschirm. Der Inhalt eines solchen Dialogs muss sich strecken lassen (`Grid` mit `*`-Zeilen, keine festen Größen).

### 6.7 Verdrahtung

- **Keine DI.** Alles wird in `App.xaml.cs` von Hand erzeugt und über Konstruktoren übergeben (Datenbank → Services → ViewModels). Langlebige Hilfsobjekte mit `IDisposable` (`MetadataClient`) hält `App` und gibt sie beim Beenden frei.
- Abhängigkeiten werden als **Interface** übergeben (`IAnimeService`, `ITagService`, `IDialogService`, `IMetadataClient`), konkrete Typen nur an der Verdrahtungsstelle.
- Konstruktoren nehmen alles Nötige entgegen. Keine statischen Singletons, keine Service-Locator.

## 7. XAML und Views

- Jede View ist ein `UserControl` mit `d:DataContext="{d:DesignInstance Type=vm:…ViewModel}"`, damit die Bindings im Designer geprüft werden.
- **Bindings** gegen das ViewModel, `UpdateSourceTrigger=PropertyChanged` bei Eingabefeldern, die sofort wirken sollen (Suche, Zahleneingabe).
- **Kein Code-Behind für Darstellungslogik**, die ein Trigger, Converter oder DataTemplate leisten kann. Für Zustände der View (`IsMouseOver`, `IsSelected`) Trigger im Style verwenden.
- Attribute pro Zeile, wenn mehr als zwei, in der Reihenfolge: `x:Name`/`x:Key`, Layout (`Grid.Row`, Margin), Binding/Inhalt, Aussehen, Ereignisse.
- Margins und Abstände auf einem Raster von 4 px (4, 8, 12, 16, 20, 24). Ausnahme sind Maße, die sich aus Bildformaten ergeben (Kachel 160 × 240 für das Seitenverhältnis 2:3).
- Texte stehen direkt im XAML (keine Ressourcen-Dateien), Icons kommen aus der Schrift `IconFont` (Segoe MDL2 / Fluent) über Unicode-Entities (`&#xE713;`).
- `DataTemplate`s für ViewModels gehören nach `Resources/DataTemplates.xaml`, Templates nur für eine Liste dürfen lokal stehen (`TileTemplate` in `LibraryView`).
- Listen mit vielen Einträgen: Virtualisierung nicht abschalten (`ScrollViewer` nicht um `ItemsControl` wickeln, `VirtualizingPanel` beibehalten). Die Bilder-Ansicht nutzt das `VirtualizingWrapPanel` und füllt ihre Liste erst, wenn sie aktiv ist (`DataTrigger` setzt `ItemsSource`), Vorschauen werden lazy geladen.
- **Bilder anzeigen:** `Image.Source` bindet an `byte[]` und nutzt den `BytesToImageConverter`. Abgerundete Ecken entstehen über eine `Clip`-Geometrie. Ohne Bild zeigt die View einen Platzhalter (Trigger auf `HasImage`).
- **Anwendungssymbol:** `ApplicationIcon` in der `.csproj`, Fenstersymbol über `AppWindowStyle`, Logo in der Seitenleiste. Icon-Dateien sind `Resource`-Elemente.

## 8. Themes, Farben, Styles

- Farben kommen **ausschließlich aus Theme-Brushes per `DynamicResource`**, damit Live-Umschalten und die eigene Palette funktionieren: `BackgroundBrush`, `SurfaceBrush`, `SurfaceAltBrush`, `SurfaceHoverBrush`, `LineBrush`, `TextBrush`, `TextMutedBrush`, `AccentBrush`, `AccentTextBrush`, `AccentSoftBrush`, `DangerBrush`, `SuccessBrush`, `EdgeBrush`, `ScrollThumbBrush`, `HoverOverlayBrush`, `PressedOverlayBrush`, `ShadowBrush`.
- **Keine festen Farbwerte** (`#FF…`, `Red`) in Views. Ausnahmen sind fachliche Farben mit eigener Palette (`TierColors.xaml`) und Warnfarben, die in beiden Themes lesbar sind (Favoriten-Stern `#F5B301`). Text und Symbole auf Bildern liegen auf einer Fläche in Theme-Farbe, nicht direkt auf dem Bild.
- Eine Palette besteht aus vier Farben (Hintergrund, Fläche, Text, Akzent), alle weiteren Brushes leitet der `ThemeService` ab. Neue Brushes werden dort ergänzt, nicht in den Views.
- `StaticResource` nur für Dinge, die sich zur Laufzeit nicht ändern (Styles, Schriften, Converter). Alles Farbliche ist `DynamicResource`.
- Standardsteuerelemente erhalten ihr Aussehen durch **implizite Styles** in `Controls.xaml`. Varianten sind benannte Styles (`PrimaryButton`, `SecondaryButton`, `MutedText`, `ItemCard`). Neue Wiederverwendung kommt nach `Components.xaml`, nicht kopiert in Views.
- Neue Fenster verwenden `AppWindowStyle` und `WindowThemeHelper.Attach(this)` (dunkle Titelleiste).
- Jede neue Ansicht wird in **Hell, Dunkel und mit einer eigenen Palette** geprüft.

## 9. Converter

- Ein Converter pro Datei in `Converters/`, `IValueConverter`, registriert als Ressource in `App.xaml`.
- Nur für reine Darstellungsumwandlung (Enum → Brush, Leerstring → Sichtbarkeit, `byte[]` → Bild). Fachliche Umrechnung gehört ins ViewModel.
- Immer `object`-sicher: unerwartete Eingaben ergeben einen sinnvollen Standardwert (`null` bei unlesbaren Bilddaten), keine Exception.

## 10. Datenbank

- **ADO.NET mit `Microsoft.Data.Sqlite`, kein EF.** Der Zugriff läuft ausschließlich über `DatabaseManager` und `DatabaseSession`.
- **Eine Session = eine Transaktion** pro fachlichem Vorgang. Ohne `Commit()` wird beim `Dispose` zurückgerollt. Beteiligte Services bekommen dieselbe Session übergeben (`SelectByAnimeId(session, id)`).
- **Parameter immer als Tupel** `("@Name", wert)`, nie Werte in SQL-Text einsetzen. Ausnahme: feste, nicht vom Benutzer stammende Spalten- oder Tabellennamen.
- Typumwandlung zentral in der Session: Enum → Zahl, `bool` → 0/1, `DateTime` → `yyyy-MM-dd HH:mm:ss`, `null` → `NULL`, `byte[]` → BLOB.
- **Schema** liegt versioniert in `DatabaseSchema` (`PRAGMA user_version`, aktuell **4**). Änderungen am Schema = neue Version mit Migrationsskript, Sicherung und Rollback. Migrationen bauen aufeinander auf (0 → 1 → 2 → 3 → 4). `PRAGMA foreign_keys = ON` gilt pro Verbindung.
  - Version 1: Fremdschlüssel und Kaskadenlöschung, Migration in eine neue Datei.
  - Version 2: Tabelle `AnimeImage`. Die Migration ergänzt die vorhandene Datei, vorher entsteht eine Kopie (`data.v1-<Zeitstempel>.sqlite`), die bei einem Fehler zurückgespielt wird.
  - Version 3: Spalten `Description`, `AniListId`, `MyAnimeListId` und `KitsuId` in `Anime` (alle optional, per `ALTER TABLE`). Sicherung `data.v2-<Zeitstempel>.sqlite` wie oben. Version 4: Spalte `Uid` (feste Kennung, GUID als Text) mit eindeutigem Index und Trigger, der bei `INSERT` ohne Kennung eine vergibt (damit auch SQL-Skripte funktionieren), vorhandene Animes erhalten beim Migrieren eine Kennung. Sicherung `data.v3-<Zeitstempel>.sqlite`. Alle Schritte ab Version 1→2 teilen sich `MigrateWithBackup`.
- Beziehungen mit Fremdschlüssel und `ON DELETE CASCADE`. Listen (Seasons, Tags, Ovas …) werden beim Speichern ersetzt, nicht einzeln abgeglichen.
- **Bilder** liegen als BLOB in der eigenen Tabelle `AnimeImage` (1:1 zum Anime, `UNIQUE`), in zwei Größen: `Data` (höchstens 1024 px, JPEG) und `Thumbnail` (240 × 360, zugeschnitten). Sie werden mit dem Anime in derselben Transaktion gespeichert (`ImageService.Save`, Upsert, ohne Bild wird gelöscht). Listenabfragen laden nie die Bilddaten, sondern nur `HasImage`. Vorschauen holt `IAnimeService.SelectThumbnail(id)` bei Bedarf.
- **Lese-Services liefern fertige Modelle oder schlanke Records** für Listen (`AnimeListEntry` mit Tag-Ids und `HasImage`), nicht ganze Aggregate. Aggregierte Abfragen (Häufigkeiten, Co-Occurrence) laufen in SQL (`GROUP BY`, Self-Join), nicht in Schleifen in C#.
- Services fangen Datenbankfehler, protokollieren mit Serilog und liefern `false`/`null`/leere Liste zurück. Das ViewModel entscheidet, was der Benutzer sieht.
- Datenbank und Logs liegen neben der Anwendung (`db\data.sqlite`, `logs\`), Einstellungen unter `%AppData%\Anicluster`.

## 11. Fehlerbehandlung und Logging

- **Serilog** mit Datei-Sink (`logs/log_.txt`, täglich). Fehler mit Ausnahme loggen: `Log.Error(ex, "Anime '{AnimeName}' konnte nicht geladen werden.", name)`. Strukturierte Platzhalter statt String-Interpolation.
- Spezifische Ausnahmen fangen, wo Fehler erwartbar sind (`SqliteException`, `IOException`, `UnauthorizedAccessException`, `FileFormatException`, `NotSupportedException` bei Bildern, `COMException` bei der Zwischenablage, `HttpRequestException`, `TaskCanceledException` und `JsonException` bei Online-Abfragen). Kein leeres `catch`: Ein bewusst ignorierter Fall bekommt einen Kommentar mit dem Grund.
- Benutzermeldungen sind verständlich und deutsch, ohne Stacktrace. Nutzer-Eingaben werden im ViewModel validiert (z. B. Bilddatei höchstens 25 MB, unlesbare Bilder werden abgelehnt).
- Startfehler (Datenbank, Migration) beenden die App kontrolliert mit Meldung.

## 12. Besondere Bereiche

- **Tag-Netzwerk:** Die Physik (`ForceSimulation`) ist von WPF getrennt und testbar. Das ViewModel steuert den Takt (`DispatcherTimer`, 16 ms) und hält die Knoten als `TagNodeViewModel`. Gezeichnet werden Kanten und Cluster-Flächen in einer eigenen `FrameworkElement.OnRender`-Ebene, Knoten als Items eines `ItemsControl`. Cluster entstehen per Label Propagation auf den stärksten Verbindungen jedes Tags (`ClusterDetector`), die Flächen berechnet `ClusterHulls`. Zoom (Mausrad zum Mauszeiger bis 800 %), Verschieben per Ziehen und „Einpassen“ sind rein visuell in der View. Beim Verlassen der Seite wird die Simulation gestoppt (`IDisposable`).
- **Anime-Netzwerk:** `AnimeNetwork/` rechnet reine Verbindungen (`AnimeNetworkBuilder`) für drei Arten (Vorgänger/Nachfolger/Verwandt, ähnliche Tags mit Seltenheitsgewicht, ähnliche Bewertung aus den Teilnoten) und Cluster (`ClusterDetector`). Das ViewModel hält Knoten, Filter und Auswahl, die Ansicht nutzt dieselben Zoom-Regeln wie das Tag-Netzwerk. Die gewählte Art und die Regler werden in den Einstellungen gespeichert.
- **Bibliothek:** `LibraryViewModel.Refresh()` liest die Liste im Hintergrund (`Task.Run`) und ersetzt sie auf dem UI-Thread, ein neueres Laden macht ein früheres wirkungslos (`_refreshVersion`). Ein `LoadingNotice` blendet nach 350 ms Ladezeit einen dezenten Hinweis ein (Slide-in unten rechts, Slide-out danach), schnelle Vorgänge zeigen ihn nie. Vorschaubilder lädt `AnimeListItemViewModel.Thumbnail` bei Bedarf im Hintergrund, höchstens zwei gleichzeitig. Die Bilder-Ansicht nutzt das eigene `VirtualizingWrapPanel` (Views/): Es erzeugt nur sichtbare Kacheln, misst die Größe am ersten Eintrag (alle Kacheln müssen gleich hoch sein, Namen daher einzeilig mit `TextTrimming`) und braucht einen `ScrollViewer` mit `CanContentScroll="True"`. Der Schalter „Nur mit Bild“ (`OnlyWithImage`, in den Einstellungen gespeichert) wirkt nur in der Bilder-Ansicht.
- **Datensicherung:** `Backup/` (`IBackupService`, `BackupService`) exportiert alle Animes und Tags mit Newtonsoft in eine JSON-Datei (`BackupFile`: Kopf mit Format, Format- und Schemaversion, danach die Modelle selbst, Enums als Text, Datei erst als `.tmp`, dann umbenannt). Die Ids in der Datei gelten nur dort, ein Anime wird über `Anime.Uid` wiedererkannt. Import: erst `ReadAsync` (prüft Format, Versionen, Namen, ändert nichts), dann Bestätigung im ViewModel, dann `ImportAsync`. Vor dem Schreiben wird die Datenbankdatei kopiert (`….vor-import-<Zeitstempel>…`), bei jedem Fehler wird sie zurückgespielt. *Zusammenführen* erkennt Vorhandenes über Kennung, Quell-Ids oder Namen (Momentaufnahme `SelectAnimeIdentities`) und lässt es unverändert, *Ersetzen* leert zuerst alle Animes und Tags. Tags werden über den Namen zugeordnet. Vorgänger, Nachfolger und Verwandte werden erst nach dem Speichern aller Animes über eine Tabelle Datei-Id → Datenbank-Id gesetzt. Dateiauswahl liefert der `IDialogService` (`PickSaveFile`, `PickOpenFile`). Die Arbeit läuft mit `Task.Run`, das `SettingsViewModel` meldet den Fortschritt über `IProgress<string>`.
- **SQL-Konsole:** Nur als verstecktes Werkzeug (Rechtsklick auf die Version). Verändernde Befehle müssen bestätigt werden und werden protokolliert.
- **Bilder:** Die Aufbereitung liegt zustandslos in `Imaging/ImageProcessor` (`TryCreate(byte[])` liefert `AnimeImage` oder `null`). Sie dekodiert beliebige gängige Formate, verkleinert, legt Transparenz auf Weiß und kodiert als JPEG. Eingaben kommen aus Dateiauswahl, Zwischenablage und Drag and Drop, die View liest sie und übergibt Pfad oder Bytes an das `AnimeEditorViewModel` (`SetImageFromFile`, `SetImage`, `RemoveImage`). Das Bild wird erst mit dem Speichern des Animes in die Datenbank geschrieben.
- **Online-Metadaten:** `Metadata/` kapselt die Quellen AniList (GraphQL), Jikan (MyAnimeList) und Kitsu hinter `IMetadataProvider` und die Fassade `IMetadataClient` (`SearchAsync`, `GetByIdAsync`, `DownloadCoverAsync`). Alle Aufrufe liefern `MetadataResult<T>` mit Status (Erfolg, nicht gefunden, Limit erreicht, offline, Zeitüberschreitung, Dienstfehler, abgebrochen) statt Ausnahmen, das ViewModel übersetzt den Status in eine deutsche Meldung. Der Client hält pro Quelle einen Mindestabstand zwischen Anfragen ein und beachtet `Retry-After` bei 429. Es gibt genau einen `HttpClient` (`MetadataClient`, `IDisposable`, `User-Agent` mit Version), die Zeitgrenze gilt pro Versuch in `MetadataHttp` (15 s, bei Jikan 40 s), Jikan wird bei Zeitüberschreitung oder HTTP 5xx einmal wiederholt, AniList fragt bei einem Fehler noch einmal nur mit den Grundfeldern ab (ohne Studios und Mitarbeiter), der Anfang einer Fehlerantwort steht im Log, Cover werden nur über HTTPS und höchstens 10 MB geladen. Ausgelöst wird alles nur durch den Knopf „Metadaten laden“ im Editor, immer für genau einen Anime. Der Dialog (`MetadataImportViewModel`) lässt die Quelle wählen (Standard AniList, die letzte Wahl steht in `AppSettings.MetadataProvider`), zeigt Treffer und lässt Cover, Beschreibung, Zeitraum und (wenn die Quelle sie liefert) Originalname, Produzent, Veröffentlicher und Autor einzeln übernehmen. Vorhandene Daten werden nie stillschweigend überschrieben, unvollständige Angaben bleiben leer. Der Dialog gibt ein `MetadataImportResult` zurück, das `AnimeEditorViewModel` schreibt es in den Editor (wirkt erst mit „Speichern“) und merkt sich die Quell-Ids am Anime, damit eine spätere Abfrage ohne Suche auskommt. Beschreibungen sind meist englisch und werden von `MetadataText` von HTML und Zeilenumbrüchen bereinigt.
- **Geschmacksprofil:** `TasteProfileCalculator` (`TasteProfile/`) ist rein und bekommt nur `AnimeListEntry`, Tag-Namen und `TasteOptions` (Tiers, Favoriten, Status der bewerteten Animes). Als Lieblinge zählen gesehene Animes (Status nicht Wunschliste). Pro Tag wird ein geglätteter Lift berechnet, `Weight` ist sein Logarithmus; Vorschläge bewertet der Mittelwert der begrenzten Tag-Gewichte, die Passung in Prozent ist relativ zum besten Treffer. Die Schwellwerte „passt“ und „passt leidlich“ wendet das `TasteProfileViewModel` an, ohne neu zu rechnen.
- **Zeitstrahl:** `IAnimeService.SelectTimelineEntries()` liefert `TimelineEntry` (Gesamtzeitraum, Staffeln, Pausen, Tag-Ids) in drei Abfragen. Unbekannte Daten (`0001-01-01`) werden zu `null`. `TimelineLayout` (`Timeline/`) rechnet Daten in Pixel um (`pixelsPerYear`, Achse ab 1. Januar des frühesten Jahres) und filtert nach Name und Tags. Die View hat einen Hauptbereich mit beiden Scrollbalken, Jahresleiste und Namen folgen ihm per `ScrollChanged` im Code-Behind (nur visuelles Verhalten). Zoom hält die Mitte des sichtbaren Bereichs, Strg + Mausrad zoomt, Umschalt + Mausrad scrollt waagerecht. Inhalte in einem `ScrollViewer` mit fester Höhe oben ausrichten (`VerticalAlignment="Top"`), sonst werden sie zentriert.
- **Projektlink:** Der Link zum GitHub-Projekt in der Seitenleiste ist ein reiner Hinweis (`Hyperlink`, öffnet im Standardbrowser).

## 13. SQL-Skripte (`SqlScripts/`)

- Importskripte sind **wiederholbar** (idempotent): vorhandene Daten über den Namen prüfen, `INSERT OR IGNORE` oder `WHERE NOT EXISTS`.
- Ablauf: temporäre Tabelle mit den Rohdaten füllen → in die Zieltabellen übernehmen → temporäre Tabelle löschen.
- Demodaten (`Demo-Tags.sql`, `Demo-Zeitraeume.sql`) ordnen Animes über den Namen zu und überspringen nicht gefundene.
- Kopf mit Kommentar: Zweck, Bedeutung der Zahlencodes (`Tier`, `State`), Hinweise zur Reihenfolge.
- Enum-Werte werden als Zahl gespeichert. Die Zuordnung steht im Skriptkopf.

## 14. Versionierung, Changelog, Installer

- Die Version steht **nur** in `Anicluster.csproj` (`<Version>`). Daraus entstehen `version.txt` (beim Publish), Dateiversion, Anzeige in der Seitenleiste und der Name des Installers.
- **SemVer:** Major = inkompatibler Bruch (z. B. neue Datenbankgeneration ohne Migration), Minor = neue Funktion, Patch = Fehlerbehebung.
- `CHANGELOG.md` im Stil von Keep a Changelog: Abschnitte *Hinzugefügt*, *Geändert*, *Behoben*, *Entfernt*. Einträge beschreiben die Wirkung für den Benutzer, nicht den Code.
- Schemaänderung = Eintrag im Changelog **und** neue `user_version` mit Migration. Der Eintrag nennt auch, dass ältere Programmversionen die neue Datenbank nicht mehr öffnen.
- Release-Ablauf: Version erhöhen → Changelog ergänzen → `ReadMe.txt` des Installers anpassen → `dotnet publish` (Profil „Folder“, Ziel `bin\Publish`) → Installer bauen (`-p:BuildSetup=true` oder Inno Setup manuell) → Version taggen (`vX.Y.Z`).
- **Symbol und Lizenz:** Das Anwendungssymbol (`Resources/Icon`) ist eine eigene Gestaltung unter CC0. Fremde Grafiken, Symbolsammlungen oder Schriften werden nur übernommen, wenn ihre Lizenz die Weitergabe erlaubt und im Repository vermerkt ist.
- **Online-Quellen:** Die Nutzungsbedingungen der Schnittstellen sind einzuhalten (AniList: nicht kommerziell, kein Massenabruf, Ratenbegrenzung beachten). Es wird nur auf Anforderung des Benutzers für einen einzelnen Anime abgefragt, nie auf Vorrat.
- Die `README.md` ist zweisprachig (Deutsch und Englisch) mit Inhaltsverzeichnis oben. Änderungen an Funktionen, Speicherorten oder Struktur werden in beiden Sprachen nachgezogen.

## 15. Commits

- Betreff im Imperativ, Englisch, höchstens etwa 60 Zeichen (`Add hidden SQL console`).
- Der Text darunter sagt, was sich für den Benutzer oder die Architektur ändert und warum. Eine Änderung pro Commit, Formatierungs-Anpassungen getrennt.

## 16. Checkliste für neue Funktionen

1. Modell oder Record in `Modells` ergänzt (ohne Logik)?
2. Service-Interface erweitert, SQL parametrisiert, Fehler protokolliert?
3. ViewModel erbt von `PropertyBaseChange`, enthält keine UI-Typen (auch kein `Clipboard`, `BitmapImage`)?
4. Aktionen als öffentliche Methoden, View leitet nur weiter?
5. `DataTemplate` ergänzt, in `App.xaml.cs`/`MainViewModel` verdrahtet?
6. Nur Theme-Brushes verwendet, in Hell/Dunkel/Custom angesehen?
7. `IDisposable` nötig (Timer, Events auf langlebige Objekte, `HttpClient`)?
8. Dateien als UTF-8 mit BOM und CRLF gespeichert?
9. Changelog aktualisiert, bei Schemaänderung Migration ergänzt (mit Sicherung und Rollback)?
10. Große Daten (Bilder) nicht in Listenabfragen geladen, sondern bei Bedarf?
11. `README.md` (beide Sprachen) und Installer-`ReadMe.txt` angepasst?
12. Langsame Arbeit im Hintergrund, bei spürbarer Dauer mit `LoadingNotice`?
13. Neues Feld am Anime: auch in der Datensicherung (`Backup/`) und im Schema berücksichtigt?
14. Online-Zugriff nur auf Knopfdruck, mit Zeitlimit, Abbruch und deutschen Meldungen für offline, Limit und Fehler?
