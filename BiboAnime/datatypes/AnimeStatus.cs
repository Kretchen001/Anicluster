using System.Runtime.Serialization;

namespace BiboAnime.datatypes {

    public enum AnimeStatus {

        [EnumMember(Value = "dropped")]
        Abgebrochen,
        [EnumMember(Value = "started")]
        Angefangen,
        [EnumMember(Value = "finish")]
        Fertig,
        [EnumMember(Value = "stalled")]
        Unterbrochen,
        [EnumMember(Value = "want")]
        Wunschliste
    }
}