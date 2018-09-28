namespace CodecControl.Web.Cache
{
    public static class CacheKeys
    {
        public static string Codecinformationlist = "CodecInformationList";

        public static string CodecInformation(string sipAddress)
        {
            sipAddress = sipAddress.ToLower().Trim();
            return $"CodecInformation_{sipAddress}";
        }

    }
}