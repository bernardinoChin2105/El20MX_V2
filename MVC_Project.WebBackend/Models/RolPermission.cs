namespace MVC_Project.WebBackend.Models
{
    public class RolPermission
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ControllerAction { get; set; }

        public string Section { get; set; }

        public string Role { get; set; }
        public bool Granted { get; set; }
    }
}