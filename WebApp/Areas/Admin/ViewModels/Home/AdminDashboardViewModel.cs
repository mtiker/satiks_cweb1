namespace WebApp.Areas.Admin.ViewModels.Home;

public class AdminDashboardViewModel
{
    public int TotalEquipment { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ActiveBookings { get; set; }
}