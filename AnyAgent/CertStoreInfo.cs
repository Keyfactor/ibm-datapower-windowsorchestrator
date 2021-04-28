namespace DataPower
{
    internal class CertStoreInfo
    {
        public string Domain { get; set; }
        public string CertificateStore { get; set; }
        public string CryptoCertObjectPrefix { get; set; }
        public string CryptoKeyObjectPrefix { get; set; }
        public string CertFilePrefix { get; set; }
        public string KeyFilePrefix { get; set; }

    }
}