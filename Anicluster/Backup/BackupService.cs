using Anicluster.Database;
using Anicluster.Database.Services;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using System.IO;
using System.Text;

namespace Anicluster.Backup {

    /// <summary>
    /// Standard-Implementierung von <see cref="IBackupService"/>. Die Arbeit läuft im Hintergrund und nutzt die vorhandenen
    /// Anime- und Tag-Dienste, damit alle Regeln beim Speichern (Tags, Bilder, Listen) dieselben bleiben wie sonst.
    /// </summary>
    public class BackupService : IBackupService {

        private readonly IAnimeService _animeService;
        private readonly ITagService _tagService;
        private readonly DatabaseManager _databaseManager;

        public BackupService(IAnimeService animeService, ITagService tagService, DatabaseManager databaseManager) {
            this._animeService = animeService;
            this._tagService = tagService;
            this._databaseManager = databaseManager;
        }

        public Task<BackupResult> ExportAsync(string path, bool includeImages, IProgress<string>? progress = null) {
            return Task.Run(() => this.Export(path, includeImages, progress));
        }

        public Task<BackupReadResult> ReadAsync(string path) {
            return Task.Run(() => this.Read(path));
        }

        public Task<BackupResult> ImportAsync(BackupFile file, BackupImportMode mode, IProgress<string>? progress = null) {
            return Task.Run(() => this.Import(file, mode, progress));
        }

        #region Export
        private BackupResult Export(string path, bool includeImages, IProgress<string>? progress) {
            string temporaryPath = path + ".tmp";
            try {
                List<AnimeListEntry> entries = this._animeService.SelectAnimeListEntries();
                BackupFile file = new BackupFile {
                    AppVersion = typeof(BackupService).Assembly.GetName().Version?.ToString(3) ?? string.Empty,
                    SchemaVersion = DatabaseSchema.CurrentVersion,
                    ExportedAt = DateTime.Now,
                    IncludesImages = includeImages,
                    Tags = this._tagService.SelectAllTags()
                };
                for (int i = 0; i < entries.Count; i++) {
                    progress?.Report($"Lese Anime {i + 1} von {entries.Count} …");
                    Anime? anime = this._animeService.SelectAnimeByX(entries[i].Id);
                    if (anime is null) {
                        return new BackupResult(false, $"Der Anime '{entries[i].Name}' konnte nicht gelesen werden. Details stehen im Log.");
                    }
                    if (!includeImages) {
                        anime.Image = null;
                    }
                    file.Animes.Add(anime);
                }
                progress?.Report("Schreibe Datei …");
                using (FileStream stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write)) {
                    using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false))) {
                        CreateSerializer().Serialize(writer, file);
                    }
                }
                File.Move(temporaryPath, path, true);
                Log.Information("Datensicherung exportiert: {Path} ({AnimeCount} Animes)", path, file.Animes.Count);
                return new BackupResult(true, $"{file.Animes.Count} Animes und {file.Tags.Count} Tags exportiert.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) {
                Log.Error(ex, "Die Datensicherung konnte nicht nach '{Path}' geschrieben werden.", path);
                TryDelete(temporaryPath);
                return new BackupResult(false, "Die Datei konnte nicht geschrieben werden: " + ex.Message);
            }
        }
        #endregion

        #region Lesen und Prüfen
        private BackupReadResult Read(string path) {
            BackupFile? file;
            try {
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8)) {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader)) {
                        file = CreateSerializer().Deserialize<BackupFile>(jsonReader);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                Log.Error(ex, "Die Datensicherung '{Path}' konnte nicht gelesen werden.", path);
                return new BackupReadResult(null, "Die Datei konnte nicht gelesen werden: " + ex.Message, 0);
            }
            catch (JsonException ex) {
                Log.Error(ex, "Die Datensicherung '{Path}' ist keine gültige JSON-Datei.", path);
                return new BackupReadResult(null, "Die Datei ist keine gültige Anicluster-Datensicherung.", 0);
            }
            string? problem = Validate(file);
            if (problem is not null || file is null) {
                return new BackupReadResult(null, problem ?? "Die Datei ist leer.", 0);
            }
            return new BackupReadResult(file, string.Empty, this._animeService.SelectAnimeListEntries().Count);
        }

        /// <summary>Prüft Format, Versionen und Pflichtangaben. Liefert den Grund bei einem Fehler, sonst <c>null</c>.</summary>
        private static string? Validate(BackupFile? file) {
            if (file is null || file.Format != BackupFile.FormatName) {
                return "Die Datei ist keine Anicluster-Datensicherung.";
            }
            if (file.FormatVersion > BackupFile.CurrentFormatVersion || file.SchemaVersion > DatabaseSchema.CurrentVersion) {
                return "Die Datensicherung stammt aus einer neueren Version von Anicluster. Bitte Anicluster aktualisieren.";
            }
            foreach (Anime anime in file.Animes) {
                if (string.IsNullOrWhiteSpace(anime.Name)) {
                    return "Die Datensicherung enthält einen Anime ohne Namen und wird nicht eingelesen.";
                }
            }
            return null;
        }
        #endregion

        #region Import
        private BackupResult Import(BackupFile file, BackupImportMode mode, IProgress<string>? progress) {
            string? safetyCopy = null;
            try {
                progress?.Report("Sichere die Datenbank …");
                safetyCopy = this.CreateSafetyCopy();
                BackupResult result = this.Apply(file, mode, progress);
                if (!result.Success) {
                    this.RestoreSafetyCopy(safetyCopy);
                    return result with { Message = result.Message + " Die Datenbank wurde nicht verändert." };
                }
                if (mode == BackupImportMode.Merge) {
                    TryDelete(safetyCopy); // beim Ergänzen geht nichts verloren, die Kopie wird nicht gebraucht
                }
                return result;
            }
            catch (Exception ex) when (
                ex is IOException || ex is UnauthorizedAccessException || ex is SqliteException || ex is InvalidOperationException
            ) {
                Log.Error(ex, "Der Import der Datensicherung ist fehlgeschlagen.");
                if (safetyCopy is not null) {
                    this.RestoreSafetyCopy(safetyCopy);
                }
                return new BackupResult(false, "Der Import ist fehlgeschlagen: " + ex.Message + " Die Datenbank wurde nicht verändert.");
            }
        }

        private BackupResult Apply(BackupFile file, BackupImportMode mode, IProgress<string>? progress) {
            if (mode == BackupImportMode.Replace) {
                progress?.Report("Entferne die vorhandenen Daten …");
                foreach (AnimeListEntry entry in this._animeService.SelectAnimeListEntries()) {
                    if (!this._animeService.DeleteAnime(entry.Id)) {
                        return new BackupResult(false, $"Der vorhandene Anime '{entry.Name}' konnte nicht entfernt werden.");
                    }
                }
                foreach (Tag tag in this._tagService.SelectAllTags()) {
                    if (!this._tagService.DeleteTagById(tag)) {
                        return new BackupResult(false, $"Der vorhandene Tag '{tag.Designation}' konnte nicht entfernt werden.");
                    }
                }
            }

            // Tags werden über den Namen zugeordnet, fehlende werden angelegt
            Dictionary<string, Tag> tags = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);
            foreach (Tag tag in this._tagService.SelectAllTags()) {
                tags[tag.Designation] = tag;
            }
            int createdTags = 0;
            IEnumerable<string> tagNames = file.Tags.Select(x => x.Designation)
                .Concat(file.Animes.SelectMany(anime => anime.Tags.Select(x => x.Designation)));
            foreach (string rawName in tagNames) {
                string name = (rawName ?? string.Empty).Trim();
                if (name.Length == 0 || tags.ContainsKey(name)) {
                    continue;
                }
                Tag newTag = new Tag(name);
                if (this._tagService.Insert(newTag) < 0) {
                    return new BackupResult(false, $"Der Tag '{name}' konnte nicht angelegt werden.");
                }
                tags[name] = newTag;
                createdTags++;
            }

            // Beim Ergänzen werden vorhandene Animes erkannt, beim Ersetzen ist die Datenbank leer
            IdentityIndex index = new IdentityIndex(
                mode == BackupImportMode.Merge ? this._animeService.SelectAnimeIdentities() : new List<AnimeIdentity>()
            );
            HashSet<string> usedUids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<int, int> idMap = [];
            List<PendingRelation> pending = [];
            int added = 0;
            int skipped = 0;
            List<Anime> animes = file.Animes.OrderBy(x => x.Id).ToList();
            for (int i = 0; i < animes.Count; i++) {
                progress?.Report($"Übernehme Anime {i + 1} von {animes.Count} …");
                Anime anime = animes[i];
                int fileId = anime.Id;
                int? existingId = index.Find(anime);
                if (existingId is int foundId) {
                    idMap[fileId] = foundId;
                    skipped++;
                    continue;
                }
                PendingRelation relation = new PendingRelation(anime, anime.Predecessor, anime.Successor, anime.Related);
                anime.Predecessor = -1;
                anime.Successor = -1;
                anime.Related = -1;
                anime.Id = 0;
                if (string.IsNullOrWhiteSpace(anime.Uid) || !usedUids.Add(anime.Uid)) {
                    anime.Uid = string.Empty; // fehlt oder kommt doppelt vor, der Dienst vergibt eine neue Kennung
                }
                anime.Tags = anime.Tags
                    .Select(x => (x.Designation ?? string.Empty).Trim())
                    .Where(x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(x => tags[x])
                    .ToList();
                if (anime.Image is not null && anime.Image.Data.Length == 0) {
                    anime.Image = null;
                }
                if (!this._animeService.Insert(anime)) {
                    return new BackupResult(false, $"Der Anime '{anime.Name}' konnte nicht gespeichert werden.");
                }
                usedUids.Add(anime.Uid);
                idMap[fileId] = anime.Id;
                added++;
                if (relation.HasAny) {
                    pending.Add(relation);
                }
            }

            // Vorgänger, Nachfolger und Verwandte erst jetzt, wenn alle Animes ihre neue Id haben
            progress?.Report("Verknüpfe Vorgänger und Nachfolger …");
            foreach (PendingRelation relation in pending) {
                relation.Anime.Predecessor = Map(idMap, relation.Predecessor);
                relation.Anime.Successor = Map(idMap, relation.Successor);
                relation.Anime.Related = Map(idMap, relation.Related);
                if (!this._animeService.UpdateAnime(relation.Anime)) {
                    return new BackupResult(false, $"Die Verknüpfungen von '{relation.Anime.Name}' konnten nicht gespeichert werden.");
                }
            }

            Log.Information(
                "Datensicherung eingelesen ({Mode}): {Added} ergänzt, {Skipped} vorhanden, {Tags} neue Tags",
                mode,
                added,
                skipped,
                createdTags
            );
            string message = mode == BackupImportMode.Replace
                ? $"Wiederhergestellt: {added} Animes, {createdTags} Tags angelegt."
                : $"Zusammengeführt: {added} Animes ergänzt, {skipped} waren schon vorhanden, {createdTags} neue Tags.";
            return new BackupResult(true, message);
        }

        /// <summary>Übersetzt eine Id aus der Datei in die Id in der Datenbank, -1 wenn es keinen Treffer gibt.</summary>
        private static int Map(Dictionary<int, int> idMap, int fileId) {
            return fileId != -1 && idMap.TryGetValue(fileId, out int databaseId) ? databaseId : -1;
        }

        private string CreateSafetyCopy() {
            string databasePath = this._databaseManager.DatabasePath;
            string copyPath = Path.Combine(
                Path.GetDirectoryName(databasePath) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(databasePath)}.vor-import-{DateTime.Now:yyyyMMdd-HHmmss}{Path.GetExtension(databasePath)}"
            );
            SqliteConnection.ClearAllPools(); // gepoolte Verbindungen halten die Datei sonst gesperrt
            File.Copy(databasePath, copyPath);
            Log.Information("Kopie der Datenbank vor dem Import: {CopyPath}", copyPath);
            return copyPath;
        }

        private void RestoreSafetyCopy(string copyPath) {
            SqliteConnection.ClearAllPools();
            File.Copy(copyPath, this._databaseManager.DatabasePath, true);
            Log.Warning("Die Datenbank wurde nach einem Fehler aus '{CopyPath}' wiederhergestellt.", copyPath);
        }

        /// <summary>Ein Anime, dessen Verknüpfungen (Ids aus der Datei) erst nach dem Speichern aller Animes gesetzt werden können.</summary>
        private sealed record PendingRelation(Anime Anime, int Predecessor, int Successor, int Related) {

            public bool HasAny => this.Predecessor != -1 || this.Successor != -1 || this.Related != -1;
        }

        /// <summary>Findet vorhandene Animes über Kennung, Ids der Online-Quellen oder Namen.</summary>
        private sealed class IdentityIndex {

            private readonly Dictionary<string, int> _byUid = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, int> _byName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<int, int> _byAniListId = [];
            private readonly Dictionary<int, int> _byMyAnimeListId = [];
            private readonly Dictionary<int, int> _byKitsuId = [];

            public IdentityIndex(IEnumerable<AnimeIdentity> identities) {
                foreach (AnimeIdentity identity in identities) {
                    if (!string.IsNullOrWhiteSpace(identity.Uid)) {
                        this._byUid.TryAdd(identity.Uid, identity.Id);
                    }
                    this._byName.TryAdd(identity.Name.Trim(), identity.Id);
                    if (identity.AniListId is int aniListId) {
                        this._byAniListId.TryAdd(aniListId, identity.Id);
                    }
                    if (identity.MyAnimeListId is int myAnimeListId) {
                        this._byMyAnimeListId.TryAdd(myAnimeListId, identity.Id);
                    }
                    if (identity.KitsuId is int kitsuId) {
                        this._byKitsuId.TryAdd(kitsuId, identity.Id);
                    }
                }
            }

            /// <summary>Die Id des vorhandenen Animes, der zu diesem passt, sonst <c>null</c>.</summary>
            public int? Find(Anime anime) {
                int id;
                if (!string.IsNullOrWhiteSpace(anime.Uid) && this._byUid.TryGetValue(anime.Uid, out id)) {
                    return id;
                }
                if (anime.AniListId is int aniListId && this._byAniListId.TryGetValue(aniListId, out id)) {
                    return id;
                }
                if (anime.MyAnimeListId is int myAnimeListId && this._byMyAnimeListId.TryGetValue(myAnimeListId, out id)) {
                    return id;
                }
                if (anime.KitsuId is int kitsuId && this._byKitsuId.TryGetValue(kitsuId, out id)) {
                    return id;
                }
                if (this._byName.TryGetValue(anime.Name.Trim(), out id)) {
                    return id;
                }
                return null;
            }
        }
        #endregion

        private static JsonSerializer CreateSerializer() {
            return JsonSerializer.Create(new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                Converters = { new StringEnumConverter() }
            });
        }

        private static void TryDelete(string path) {
            try {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) {
                Log.Warning(ex, "Die Datei '{Path}' konnte nicht gelöscht werden.", path); // nicht schlimm, bleibt nur liegen
            }
        }
    }
}
