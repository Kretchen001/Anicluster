using LiteDB;

namespace BiboAnime.datatypes {

    public class AnimePoster {

        [BsonId]
        public Guid Id { get; set; }
        public byte[]? Picture { get; set; }

        public AnimePoster () {
        }

        public AnimePoster (Guid id, byte[] picture) {
            Id = id;
            Picture = picture;
        }
    }
}
