using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppInfonet.Models
{
    public class DashboardViewModel
    {
        public string UserEmail { get; set; }

        public DashboardViewModel()
        {
            // Qui recuperi la mail salvata dopo il login
            UserEmail = Preferences.Get("UserEmail", "utente@infonet.it");
        }
    }
}
