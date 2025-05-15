using Microsoft.AspNetCore.Identity;

namespace FootBallStat
{
    public class User : IdentityUser
    {
        
        public int Year { get; set; }
    }
}
