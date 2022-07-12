using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;

namespace TagAddInstall {

    internal class Program {

        public static void Main(string[] args) {

            databaseController dbController = new databaseController();

            List<AnimeTag> animeTags = new List<AnimeTag>();

            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Mecha"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Krieg"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Fantasy"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Sci-Fiction"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Romanze"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Harem"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Apokalypse"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Post-Apokalypse"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Action"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Magie"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Komödie"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Schwertkampf"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Demonen"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Drachen"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Over-Powered (OP)"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Reincarnation"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Mysterik"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Abenteuer"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Drama"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Tragödie"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Isekai"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Monster"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Mittelalter"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Virtuelle Realität (VR)"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Dungeon"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Übermäßige Gewaltdarstellung"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Politik"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Royal"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Management"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Gott"
            });
            animeTags.Add(new AnimeTag() {
                id = Guid.NewGuid(),
                tagDesignator = "Religion"
            });

            for (int i = 0; i < animeTags.Count; i += 1) {
                if (dbController.checkIfTagExistsByDesignator(animeTags[i])) {
                    animeTags.RemoveAt(i);
                }
            }

            dbController.addImportedTags(animeTags);
        }
    }
}