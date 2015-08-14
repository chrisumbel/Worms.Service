using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using Norm;
using Norm.BSON;
using Norm.Collections;
using Norm.Commands.Modifiers;
using Norm.Responses;

namespace Worms.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class ScoreService : IScoreService
    {
        private string GenerateConnectionString()
        {
            if (Environment.GetEnvironmentVariable("VCAP_SERVICES") == null)
                return "mongodb://localhost/worms";

            Dictionary<string, object> vcapServices = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                Environment.GetEnvironmentVariable("VCAP_SERVICES"));
            var credentials = ((Newtonsoft.Json.Linq.JArray)vcapServices["p-mongodb"]).First()["credentials"];
            return credentials["uri"].ToString();
        }

        private const int HALL_THRESHOLD = 10;

        public int GetHallThreshold() 
        {
            return HALL_THRESHOLD;
        }

        private String MongoURL {
            get {
                return GenerateConnectionString();
            }
        }

        public bool QualifiesForHall(int score)
        {
            bool qualifies = false;

            using (IMongo mongo = Mongo.Create(this.MongoURL))
            {
                IEnumerable<Score> scores = mongo.GetCollection<Score>("Score").Find(new { }, 
                    new { Value = Norm.OrderBy.Descending, CreatedAt = Norm.OrderBy.Descending }, HALL_THRESHOLD, 0).ToList<Score>();

                qualifies = scores.Count() == 0 || (scores.Last<Score>().Value <= score || scores.Count() < HALL_THRESHOLD);
            }

            return qualifies;
        }
        
        public List<Score> GetHighScores(int count)
        {
            List<Score> scores = null;

            using (IMongo mongo = Mongo.Create(this.MongoURL))
            {
                scores = mongo.GetCollection<Score>("Score").Find(new { }, 
                    new { Value = Norm.OrderBy.Descending, CreatedAt = Norm.OrderBy.Descending }, count, 0).ToList<Score>();
            }

            return scores;
        }

        public void AddHighScore(Score score) 
        {
            using (IMongo mongo = Mongo.Create(this.MongoURL))
            {
                mongo.GetCollection<Score>("Score").Insert(score);
            } 
        }

        Stream StringToStream(string result)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        public Stream GetSilverlightPolicy()
        {
            string result = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <access-policy>
                <cross-domain-access>
                    <policy>
                        <allow-from http-request-headers=""*"">
                            <domain uri=""*""/>
                        </allow-from>
                        <grant-to>
                            <resource path=""/"" include-subpaths=""true""/>
                        </grant-to>
                    </policy>
                </cross-domain-access>
            </access-policy>";
            return StringToStream(result);
        }
    }
}
