using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Norm;

namespace Worms.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IScoreService
    {
        [OperationContract, WebGet(UriTemplate = "/crossdomain.xml")]
        List<Score> GetHighScores(int count);

        [OperationContract, WebGet(UriTemplate = "/crossdomain.xml")]
        bool QualifiesForHall(int count);

        [OperationContract, WebGet(UriTemplate = "/crossdomain.xml")]
        int GetHallThreshold();

        [OperationContract, WebGet(UriTemplate = "/crossdomain.xml")]
        void AddHighScore(Score score);
    }

    [DataContract]
    public class Score
    {
        public ObjectId Id { get; set; }

        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public DateTime CreatedAt { get; set; }
        [DataMember]
        public int Value { get; set; }
    }
}
