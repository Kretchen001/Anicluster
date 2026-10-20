namespace Modells.Anime {

    /// <summary>
    /// Das Bild (Cover) eines Animes in zwei Größen. Beide Varianten sind fertig kodierte Bilddaten (JPEG),
    /// die Aufbereitung übernimmt die Anwendung, das Modell enthält nur die Bytes.
    /// </summary>
    public class AnimeImage {

        /// <summary>Das Bild in voller (verkleinerter) Größe für die Detailansicht</summary>
        public byte[] Data { get; set; } = [];

        /// <summary>Zugeschnittene Vorschau für die Bilder-Ansicht der Bibliothek</summary>
        public byte[] Thumbnail { get; set; } = [];
    }
}
