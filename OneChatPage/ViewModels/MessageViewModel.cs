using Microsoft.Identity.Client;
using System.Drawing.Printing;

namespace OneChatPage.ViewModels
{
    public class MSViewModel
    {
        public List<message> messages { get; set; }
    }

    public class message
    {
        public string role { get; set; }
        public string content { get; set; }
    }


}
