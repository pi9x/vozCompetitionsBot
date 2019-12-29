using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using vozCompetitionsLibrary.Model;

namespace vozCompetitionsLibrary.DataAccess
{
    public class AccessSubmission
    {
        public static List<Submission> Get()
        {
            using var reader = File.OpenText(ConfigGetter.SubmissionsPath());
            return JsonConvert.DeserializeObject<List<Submission>>(reader.ReadToEnd());
        }

        public static void Add(Submission submission)
        {
            List<Submission> Submissions = Get();
            Submissions.Add(submission);

            using var writer = File.CreateText(ConfigGetter.SubmissionsPath());
            writer.Write(JsonConvert.SerializeObject(Submissions, Formatting.Indented));
        }

        public static void ChangeVote(int messageId, bool upVote)
        {
            List<Submission> Submissions = Get();

            for (int i = 0; i < Submissions.Count; i++)
                if (Submissions[i].MessageId == messageId)
                {
                    if (upVote)
                    {
                        Submissions[i].Point++;
                    }
                    else
                    {
                        Submissions[i].Point--;
                    }
                    break;
                }

            using var writer = File.CreateText(ConfigGetter.SubmissionsPath());
            writer.Write(JsonConvert.SerializeObject(Submissions, Formatting.Indented));
        }
    }
}
