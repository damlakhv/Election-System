using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectionSystem;

namespace ElectionSystem
{
    internal class VoterInfo
    {
        public string FullName { get; set; }      
        public DateTime Birthday { get; set; }  
        public int BallotBoxID { get; set; }     
        public string CityName { get; set; }     
    }
}
