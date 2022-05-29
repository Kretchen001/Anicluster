namespace BiboAnime.datatypes {

    public class AnimeRow {
        public int id { get; set; }
        public bool favorite { get; set; }
        public string name { get; set; }
        public int generalRating { get; set; }
        public string tags { get; set; }
        public AnimeStatus status { get; set; }
    }
}
