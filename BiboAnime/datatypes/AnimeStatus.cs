using System.Runtime.Serialization;

namespace BiboAnime.datatypes {

    public enum AnimeStatus {

        [EnumMember(Value = "want")]
        Wunschliste,
        [EnumMember(Value = "started")]
        Angefangen,
        [EnumMember(Value = "finish")]
        Fertig,
        [EnumMember(Value = "stalled")]
        Unterbrochen,
        [EnumMember(Value = "dropped")]
        Abgebrochen
    }
}