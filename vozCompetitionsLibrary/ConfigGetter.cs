using System.Configuration;

namespace vozCompetitionsLibrary
{
    public class ConfigGetter
    {
        public static string API()
        {
            return ConfigurationManager.ConnectionStrings["BotAPI"].ConnectionString;
        }

        public static string CompetitionsPath()
        {
            return ConfigurationManager.ConnectionStrings["Competitions"].ConnectionString;
        }

        public static string SubmissionsPath()
        {
            return ConfigurationManager.ConnectionStrings["Submissions"].ConnectionString;
        }

        public static string VotesPath()
        {
            return ConfigurationManager.ConnectionStrings["Votes"].ConnectionString;
        }
    }
}
