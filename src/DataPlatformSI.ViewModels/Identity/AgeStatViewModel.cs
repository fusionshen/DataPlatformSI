using DataPlatformSI.Entities.Identity;

namespace DataPlatformSI.ViewModels.Identity
{
    public class AgeStatViewModel
    {
        const char RleChar = (char)0x202B;

        public int UsersCount { set; get; }
        public int AverageAge { set; get; }
        public User MaxAgeUser { set; get; }
        public User MinAgeUser { set; get; }

        public string MinMax => $"{RleChar}成员: {MinAgeUser.DisplayName} ({MinAgeUser.BirthDate.Value}),: {MaxAgeUser.DisplayName} ({MaxAgeUser.BirthDate.Value})其中, {UsersCount}共";
    }
}