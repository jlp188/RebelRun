using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AngularJSAuthentication.API.Models
{
    class RebelModel
    {
        [Required]
        public int ID { get; set; }

        [Required]
        public int SeqNo { get; set; }

        public int Lvl { get; set; }
        public string Rank { get; set; }

        [Required]
        public string Name { get; set; }
        public int PrntID { get; set; }
        public int PassPrnt { get; set; }
        public int EnrollerID { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public string Phone { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string DeviceID { get; set; }
        public string TokenID { get; set; }

    }
}
