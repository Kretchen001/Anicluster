namespace BiboAnime.datatypes {

    public class AnimeRow {
        public int Id { get; set; }
        public bool Favorite { get; set; } = false;
        public string Name { get; set; }
        public int GeneralRating { get; set; }
        public string Tags { get; set; }
        public AnimeStatus Status { get; set; }
    }
}
