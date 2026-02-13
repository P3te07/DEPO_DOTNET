namespace Proiect_ASPDOTNET.Helpers
{
    public static class IdGenerator
    {
        public static string GenerateUserId()
        {
            return $"USR{DateTime.Now:yyyyMMdd}{GenerateRandomString(6)}";
        }

        public static string GenerateCompanieId()
        {
            return $"CMP{DateTime.Now:yyyyMMdd}{GenerateRandomString(6)}";
        }

        public static string GenerateDepozitId()
        {
            return $"DEP{DateTime.Now:yyyyMMdd}{GenerateRandomString(6)}";
        }

        public static string GenerateMarfaId()
        {
            return $"MRF{DateTime.Now:yyyyMMdd}{GenerateRandomString(6)}";
        }

        public static string GenerateTranzactieId()
        {
            return $"TRZ{DateTime.Now:yyyyMMdd}{GenerateRandomString(8)}";
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}