namespace CodecControl.Client.Models
{
    public class VuValues
    {
        public int TxLeft { get; set; } // Sändning vänster
        public int TxRight { get; set; } // Sändning höger
        public int RxLeft { get; set; } // Mottagning vänster
        public int RxRight { get; set; } // Mottagning höger

        public override string ToString()
        {
            return $"Tx: {TxLeft} {TxRight} Rx: {RxLeft} {RxRight}";
        }

    }
}