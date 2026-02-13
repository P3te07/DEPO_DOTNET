using Proiect_ASPDOTNET.Models.Entities;

namespace Proiect_ASPDOTNET.Models.ViewModels
{
    public class SuperAdminDashboardViewModel
    {
        public int TotalCompanii { get; set; }
        public int TotalDepozite { get; set; }
        public int TotalUtilizatori { get; set; }
        public int TotalTranzactii { get; set; }
        public List<CompanieGrowthData> CrestereCompanii { get; set; }
        public List<Companie> CompaniiRecente { get; set; }
    }

    public class CompanieGrowthData
    {
        public string Luna { get; set; }
        public int NumarCompanii { get; set; }
    }

    public class DirectorCompanieDashboardViewModel
    {
        public Companie Companie { get; set; }
        public List<DepozitStatistici> Depozite { get; set; }
        public decimal ValoareTotala { get; set; }
        public int TotalTranzactii { get; set; }
    }

    public class DepozitStatistici
    {
        public Depozit Depozit { get; set; }
        public int NumarTranzactii { get; set; }
        public decimal ValoareDepozit { get; set; }
        public int NumarMarfuri { get; set; }
    }

    public class ResponsabilDepozitDashboardViewModel
    {
        public Depozit Depozit { get; set; }
        public List<Marfa> Marfuri { get; set; }
        public int TotalMarfuri { get; set; }
        public decimal ValoareTotala { get; set; }
        public List<Tranzactii> TranzactiiRecente { get; set; }
    }

    public class MuncitorDashboardViewModel
    {
        public User User { get; set; }
        public List<Sarcina> SarciniAzi { get; set; }
        public int PuncteRewardTotal { get; set; }
        public int SarciniFinalizateAstazi { get; set; }
        public List<Sarcina> SarciniViitoare { get; set; }
    }
}